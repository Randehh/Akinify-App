using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Akinify_App {
	public class MainWindowVM : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;
		public EnumBindingSourceExtension SearchDepthEnumBindingSource { get; } = new EnumBindingSourceExtension(typeof(SearchDepth));
		public RequestStaggerer RequestStaggerer { get; set; } = new RequestStaggerer();
		public ProgressBarWrapper SearchProgressBar { get; private set; }

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
		public PrivateUser CurrentUserProfile {
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
		 * Search query
		 */
		private SearchQueryType m_CurrentSearchType = SearchQueryType.Artist;
		private Dictionary<SearchQueryType, SearchQueryBase> m_SearchQueries = new Dictionary<SearchQueryType, SearchQueryBase>();
		public SearchQueryBase SearchQuery => m_SearchQueries[m_CurrentSearchType];

		public bool IsSearchQueryArtist {
			get {
				return m_CurrentSearchType == SearchQueryType.Artist;
			}
			set {
				if (value == true) {
					UpdateSearchQuery(SearchQueryType.Artist);
				}
				OnPropertyChanged(nameof(IsSearchQueryArtist));
			}
		}

		public bool IsSearchQueryPlaylist {
			get {
				return m_CurrentSearchType == SearchQueryType.Playlist;
			}
			set {
				if (value == true) {
					UpdateSearchQuery(SearchQueryType.Playlist);
				}
				OnPropertyChanged(nameof(IsSearchQueryPlaylist));
			}
		}

		public string SearchText {
			set {
				SearchQuery.SearchText = value;
				OnPropertyChanged(nameof(SearchText));
			}
			get {
				return SearchQuery.SearchText;
			}
		}

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
		public VisualLogger VisualLogger { get; private set; }
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
			VisualLogger = new VisualLogger();
			VisualLogger.OnUpdated += (logText) => { LogText = logText; };
			VisualLogger.AddLine("Standing by.");

			SearchProgressBar = new ProgressBarWrapper(searchProgressBar, searchProgressText, () => {
				OnPropertyChanged(nameof(SearchProgressBar));
				if (SearchProgressBar.IsCompleted) {
					VisualLogger.AddLine("Search completed.");
				}
			});

			UpdateSearchQuery(SearchQueryType.Artist);
		}

		public void UpdatePlaylistStats() {
			OnPropertyChanged(nameof(Playlist));
		}

		public void UpdateSearchQuery(SearchQueryType queryType) {
			m_CurrentSearchType = queryType;

			if (!m_SearchQueries.ContainsKey(queryType)) {
				switch (queryType) {
					case SearchQueryType.Artist:
						m_SearchQueries.Add(queryType, new SearchQueryArtist(this));
						break;

					case SearchQueryType.Playlist:
						m_SearchQueries.Add(queryType, new SearchQueryPlaylist(this));
						break;
				}
			}

			OnPropertyChanged(nameof(SearchQuery));
		}

		/*
		 * Spotify API calls
		 */
		public async void SetActiveClient(SpotifyClient client) {
			VisualLogger.AddLine("Succesfully logged in, requesting user profile...");
			CurrentUser = client;
			CurrentUserProfile = await client.UserProfile.Current();
			VisualLogger.AddLine("User profile received.");
		}

		public async void CreatePlaylist(string artistName) {
			VisualLogger.AddLine("Creating playlist with " + PlaylistSize + " tracks...");
			PlaylistCreateRequest request = new PlaylistCreateRequest("Akinify - " + artistName);
			request.Description = "A playlist based artists whose listeners listen to " + artistName + ".";
			FullPlaylist playlist = await CurrentUser.Playlists.Create(CurrentUserProfile.Id, request);
			List<List<string>> trackUriBatches = Playlist.SelectTracks(PlaylistSize, CurrentSelectionType);
			foreach (List<string> trackUris in trackUriBatches) {
				await Task.Delay(100);
				await CurrentUser.Playlists.AddItems(playlist.Id, new PlaylistAddItemsRequest(trackUris));
			}
		}

		public void OnPropertyChanged([CallerMemberName] string name = null) {
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

	public enum SearchQueryType {
		Artist,
		Playlist
	}
}
