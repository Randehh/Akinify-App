using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Akinify_App {
	public class MainWindowVM : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;
		public EnumBindingSourceExtension SearchDepthEnumBindingSource { get; } = new EnumBindingSourceExtension(typeof(SearchDepth));
		private RequestStaggerer m_RequestStaggerer = new RequestStaggerer();
		private ProgressBarWrapper m_SearchProgressBar;

		/*
		 * User functions
		 */
		private SpotifyClient m_CurrentUser = null;
		public SpotifyClient CurrentUser {
			get { return m_CurrentUser; }
			set {
				m_CurrentUser = value;
				OnPropertyChanged(nameof(CurrentUser));
				OnPropertyChanged(nameof(IsLoggedIn));
			}
		}

		private PrivateUser m_CurrentUserProfile;
		private PrivateUser CurrentUserProfile {
			get { return m_CurrentUserProfile; }
			set {
				m_CurrentUserProfile = value;
				OnPropertyChanged(nameof(CurrentUserProfile));
				OnPropertyChanged(nameof(UserDisplayName));
			}
		}

		public string UserDisplayName => CurrentUserProfile != null ? $"Logged in as: {CurrentUserProfile.DisplayName}" : "Not logged in";
		public bool IsLoggedIn => CurrentUser != null;

		/*
		 * Artist search
		 */
		private ObservableCollection<FullArtist> m_ArtistSearchResults = new ObservableCollection<FullArtist>();
		public ObservableCollection<FullArtist> ArtistSearchResults {
			get { return m_ArtistSearchResults; }
			set {
				m_ArtistSearchResults = value;
				OnPropertyChanged(nameof(ArtistSearchResults));
				SelectedArtist = value.Count != 0 ? value[0] : null;
				OnPropertyChanged(nameof(CanGeneratePlaylist));
			}
		}

		private FullArtist m_SelectedArtist;
		public FullArtist SelectedArtist {
			get { return m_SelectedArtist; }
			set {
				m_SelectedArtist = value;
				OnPropertyChanged(nameof(SelectedArtist));
			}
		}

		public string SelectedArtistId { get; set; }
		public bool CanGeneratePlaylist => SelectedArtist != null;
		public bool IsSearchComplete => m_SearchProgressBar.IsCompleted;

		/*
		 * Depth
		 */
		private SearchDepth m_CurrentSearchDepth = SearchDepth.Familiar;
		public SearchDepth CurrentSearchDepth {
			get { return m_CurrentSearchDepth; }
			set {
				m_CurrentSearchDepth = value;
				OnPropertyChanged(nameof(CurrentSearchDepth));
			}
		}

		/*
		 * Playlist
		 */
		private SpotifyPlaylist m_Playlist;
		public SpotifyPlaylist Playlist {
			get { return m_Playlist; }
			set {
				m_Playlist = value;
				OnPropertyChanged(nameof(Playlist));
				OnPropertyChanged(nameof(PlaylistTracks));
			}
		}

		public ObservableCollection<FullTrack> PlaylistTracks {
			get {
				if(Playlist == null) {
					return new ObservableCollection<FullTrack>();
				} else {
					return new ObservableCollection<FullTrack>(Playlist.Tracks);
				}
			}
		}

		public int m_PlaylistSize = 100;
		public int PlaylistSize {
			get { return m_PlaylistSize; }
			set {
				m_PlaylistSize = value;
				OnPropertyChanged(nameof(PlaylistSize));
			}
		}

		private SelectionType m_CurrentSelectionType = SelectionType.Random;
		public SelectionType CurrentSelectionType {
			get { return m_CurrentSelectionType; }
			set {
				m_CurrentSelectionType = value;
				OnPropertyChanged(nameof(CurrentSelectionType));
			}
		}

		/*
		 * Logger
		 */
		private VisualLogger m_VisualLogger;
		private ScrollViewer m_LogScrollViewer;
		private string m_LogText = "";
		public string LogText {
			get {
				return m_LogText;
			}
			set {
				m_LogText = value;
				OnPropertyChanged(nameof(LogText));
				m_LogScrollViewer.Dispatcher.Invoke(m_LogScrollViewer.ScrollToBottom);
			}
		}

		public MainWindowVM(ProgressBar searchProgressBar, TextBlock searchProgressText, ScrollViewer logScrollViewer) {
			m_LogScrollViewer = logScrollViewer;
			m_VisualLogger = new VisualLogger();
			m_VisualLogger.OnUpdated += (logText) => { LogText = logText; };
			m_VisualLogger.AddLine("Standing by.");

			m_SearchProgressBar = new ProgressBarWrapper(searchProgressBar, searchProgressText, () => {
				OnPropertyChanged(nameof(IsSearchComplete));
				if (IsSearchComplete) {
					m_VisualLogger.AddLine("Search completed.");
				}
			});
		}

		public void UpdatePlaylistStats() {
			OnPropertyChanged(nameof(Playlist));
		}

		/*
		 * Spotify API calls
		 */
		public async void SetActiveClient(SpotifyClient client) {
			m_VisualLogger.AddLine("Succesfully logged in, requesting user profile...");
			CurrentUser = client;
			CurrentUserProfile = await client.UserProfile.Current();
			m_VisualLogger.AddLine("User profile received.");
		}

		public async void FindArtistsByName(string name) {
			if (string.IsNullOrEmpty(name)) {
				ArtistSearchResults = new ObservableCollection<FullArtist>();
			} else {
				SearchResponse response = await CurrentUser.Search.Item(new SearchRequest(SearchRequest.Types.Artist, name));
				ArtistSearchResults = new ObservableCollection<FullArtist>(response.Artists.Items);
			}
		}

		public void GeneratePlaylist() {
			Playlist = new SpotifyPlaylist(UpdatePlaylistStats);

			int depth = -1;
			switch (CurrentSearchDepth) {
				case SearchDepth.Unfamiliar:
					depth = 3;
					break;

				case SearchDepth.Normal:
					depth = 2;
					break;

				case SearchDepth.Familiar:
					depth = 1;
					break;
			}

			m_RequestStaggerer.Reset();
			m_SearchProgressBar.Reset();
			GetRelatedArtistsAndAddTopTracks(SelectedArtistId, depth);
		}

		public async void GetRelatedArtistsAndAddTopTracks(string artistId, int depth) {
			m_VisualLogger.AddLine("Gathering related artists...");

			int estimatedTaskSize = 20;
			for(int i = 1; i < depth; i++) {
				estimatedTaskSize *= 8;
			}
			ProgressBarWrapperTask artistSearchTask =  m_SearchProgressBar.AddTask(1, estimatedTaskSize);
			ProgressBarWrapperTask trackSearchTask = m_SearchProgressBar.AddTask(3, 1);
			int trackCountEstimate = 0;

			Dictionary<string, Tuple<FullArtist, int>> uniqueArtists = new Dictionary<string, Tuple<FullArtist, int>>();
			Dictionary<int, List<string>> artistIdsToGet = new Dictionary<int, List<string>>();
			for(int i = 1; i <= depth; i++) {
				artistIdsToGet[i] = new List<string>();
			}

			artistIdsToGet[depth].Add(artistId);

			List<string> currentArtistsToGet = artistIdsToGet[depth];
			while (currentArtistsToGet.Count != 0) {
				string nextArtistId = currentArtistsToGet[0];
				await Task.Delay(100);

				if (!string.IsNullOrEmpty(nextArtistId)) {
					ArtistsRelatedArtistsResponse relatedArtists = await CurrentUser.Artists.GetRelatedArtists(nextArtistId);
					foreach (FullArtist artist in relatedArtists.Artists) {
						if (!uniqueArtists.ContainsKey(artist.Uri)) {
							int trackCount = GetTrackCountForDepth(depth);
							trackCountEstimate += trackCount;
							uniqueArtists.Add(artist.Uri, new Tuple<FullArtist, int>(artist, trackCount));
							artistSearchTask.CompleteSection();
							if (depth != 1) {
								artistIdsToGet[depth - 1].Add(artist.Id);
							}
						}
					}
				}

				currentArtistsToGet.RemoveAt(0);
				if(currentArtistsToGet.Count == 0) {
					depth--;
					if (artistIdsToGet.ContainsKey(depth)) {
						currentArtistsToGet = artistIdsToGet[depth];
					}
				}
			}

			artistSearchTask.ForceComplete();
			trackSearchTask.UpdateSectionCount(trackCountEstimate);

			m_VisualLogger.AddLine($"Gathering track data for {uniqueArtists.Count} artists...");
			foreach (Tuple<FullArtist, int> artistData in uniqueArtists.Values) {
				GetAndAddTopTracks(artistData.Item1, artistData.Item2, trackSearchTask);
			}
		}

		private int GetTrackCountForDepth(int currentDepth) {
			switch (CurrentSearchDepth) {
				case SearchDepth.Unfamiliar:
					if (currentDepth == 3) return 20;
					else if (currentDepth == 2) return 10;
					else return 5;

				case SearchDepth.Normal:
					if (currentDepth == 2) return 20;
					else return 10;

				case SearchDepth.Familiar:
					return 20;
			}
			return 20;
		}

		public async void GetAndAddTopTracks(FullArtist artist, int trackCount, ProgressBarWrapperTask trackSearchTask) {
			await Task.Delay(m_RequestStaggerer.GetNextDelay());
			ArtistsTopTracksResponse response = await CurrentUser.Artists.GetTopTracks(artist.Id, new ArtistsTopTracksRequest("DE"));
			trackSearchTask.CompleteSection(trackCount);

			for (int i = 0; i < trackCount && i < response.Tracks.Count; i++) {
				Playlist.AddTrack(response.Tracks[i]);
			}
			OnPropertyChanged(nameof(PlaylistTracks));
		}

		public async void CreatePlaylist() {
			m_VisualLogger.AddLine("Creating playlist with " + PlaylistSize + " tracks...");
			PlaylistCreateRequest request = new PlaylistCreateRequest("Akinify - " + SelectedArtist.Name);
			request.Description = "A playlist based artists whose listeners listen to " + SelectedArtist.Name + ".";
			FullPlaylist playlist = await CurrentUser.Playlists.Create(CurrentUserProfile.Id, request);
			List<List<string>> trackUriBatches = Playlist.SelectTracks(PlaylistSize, CurrentSelectionType);
			foreach(List<string> trackUris in trackUriBatches) {
				await Task.Delay(100);
				await CurrentUser.Playlists.AddItems(playlist.Id, new PlaylistAddItemsRequest(trackUris));
			}
		}

		protected void OnPropertyChanged([CallerMemberName] string name = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}

	public enum SearchDepth {
		Familiar,
		Normal,
		Unfamiliar
	}

	public enum SelectionType {
		Random,
		Most_Popular
	}
}
