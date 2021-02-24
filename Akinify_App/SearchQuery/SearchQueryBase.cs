using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Akinify_App {

	public partial class SearchQueryBase<T> : ISearchQuery, IDataTableContext<T> {

		protected MainWindowVM m_ViewModel;

		private ObservableCollection<T> m_Items = new ObservableCollection<T>();
		public ObservableCollection<T> Items {
			get { return m_Items; }
			set {
				m_Items = value;
				OnPropertyChanged(nameof(Items));
			}
		}

		private T m_SelectedItem;
		public T SelectedItem {
			get {
				return m_SelectedItem;
			}
			set {
				m_SelectedItem = value;
				OnPropertyChanged(nameof(SelectedItem));
				OnPropertyChanged(nameof(CanGeneratePlaylist));
			}
		}
		public bool CanGeneratePlaylist => SelectedItem != null;

		private string m_SearchText = "";

		public event PropertyChangedEventHandler PropertyChanged;

		public string SearchText {
			get {
				return m_SearchText;
			}
			set {
				m_SearchText = value;
				OnUpdateSearchText(value);
			}
		}

		public SearchQueryBase(MainWindowVM vm) {
			m_ViewModel = vm;
		}

		protected virtual void OnUpdateSearchText(string s) {
			throw new NotImplementedException();
		}

		public virtual SearchDepth GetSearchDepth() {
			throw new NotImplementedException();
		}

		public virtual Task<List<string>> GetArtistsToGenerateFrom() {
			return Task.Run(() => { return new List<string>(); });
		}

		public async void Search() {
			m_ViewModel.Playlist = new SpotifyPlaylist(m_ViewModel.UpdatePlaylistStats);

			int depth = -1;
			switch (GetSearchDepth()) {
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

			m_ViewModel.RequestStaggerer.Reset();
			m_ViewModel.SearchProgressBar.Reset();

			m_ViewModel.VisualLogger.AddLine("Gathering related artists...");

			List<string> artistIds = await GetArtistsToGenerateFrom();

			int estimatedTaskSize = 20 * artistIds.Count;
			for (int i = 1; i < depth; i++) {
				estimatedTaskSize *= 8;
			}

			m_ViewModel.VisualLogger.AddLine($"Getting an estimated maximum of {estimatedTaskSize} related artists...");

			ProgressBarWrapperTask artistSearchTask = m_ViewModel.SearchProgressBar.AddTask(1, estimatedTaskSize);
			ProgressBarWrapperTask trackSearchTask = m_ViewModel.SearchProgressBar.AddTask(3, 1);
			int trackCountEstimate = 0;

			Dictionary<string, Tuple<FullArtist, int>> uniqueArtists = new Dictionary<string, Tuple<FullArtist, int>>();
			Dictionary<int, List<string>> artistIdsToGet = new Dictionary<int, List<string>>();
			for (int i = 1; i <= depth; i++) {
				if(depth == i) {
					artistIdsToGet[i] = new List<string>(artistIds);
				} else {
					artistIdsToGet[i] = new List<string>();
				}
			}

			int currentProgress = 0;
			List<string> currentArtistsToGet = artistIdsToGet[depth];
			while (currentArtistsToGet.Count != 0) {
				string nextArtistId = currentArtistsToGet[0];
				await Task.Delay(100);

				if (!string.IsNullOrEmpty(nextArtistId)) {
					ArtistsRelatedArtistsResponse relatedArtists = await m_ViewModel.CurrentUser.Artists.GetRelatedArtists(nextArtistId);
					foreach (FullArtist artist in relatedArtists.Artists) {
						if (!uniqueArtists.ContainsKey(artist.Uri)) {
							currentProgress++;
							if (currentProgress % 10 == 0) {
								m_ViewModel.VisualLogger.AddLine($"Related artists remaining: {estimatedTaskSize - currentProgress}");
							}

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
				if (currentArtistsToGet.Count == 0) {
					depth--;
					if (artistIdsToGet.ContainsKey(depth)) {
						currentArtistsToGet = artistIdsToGet[depth];
					}
				}
			}

			artistSearchTask.ForceComplete();
			trackSearchTask.UpdateSectionCount(trackCountEstimate);

			m_ViewModel.VisualLogger.AddLine($"Gathering top track data for {uniqueArtists.Count} artists...");
			foreach (Tuple<FullArtist, int> artistData in uniqueArtists.Values) {
				GetAndAddTopTracks(artistData.Item1, artistData.Item2, trackSearchTask);
			}
		}

		private int GetTrackCountForDepth(int currentDepth) {
			switch (m_ViewModel.CurrentSearchDepth) {
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
			int nextDelay = m_ViewModel.RequestStaggerer.GetNextDelay();
			bool printLog = nextDelay % (RequestStaggerer.STAGGER_TIME * 10) == 0;
			await Task.Delay(nextDelay);
			if(printLog) {
				int remainingArtists = (m_ViewModel.RequestStaggerer.CurrentWait - nextDelay) / RequestStaggerer.STAGGER_TIME;
				m_ViewModel.VisualLogger.AddLine($"Top songs of artists remaining: {remainingArtists}");
			}
			ArtistsTopTracksResponse response = await m_ViewModel.CurrentUser.Artists.GetTopTracks(artist.Id, new ArtistsTopTracksRequest("DE"));
			trackSearchTask.CompleteSection(trackCount);

			for (int i = 0; i < trackCount && i < response.Tracks.Count; i++) {
				m_ViewModel.Playlist.AddTrack(response.Tracks[i]);
			}
		}

		public void OnPropertyChanged([CallerMemberName] string name = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}