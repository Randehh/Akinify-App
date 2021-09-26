using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Akinify_App {
	public class AffinityPlaylistGenerationVM : BaseGenerationVM {
		public EnumBindingSourceExtension SearchDepthEnumBindingSource { get; } = new EnumBindingSourceExtension(typeof(SearchDepth));

		/*
		 * User functions
		 */
		public SpotifyClient CurrentUser => Endpoint.Client;
		public PrivateUser CurrentUserProfile => Endpoint.UserProfile;

		public string UserDisplayName => CurrentUserProfile != null ? $"Logged in as: {CurrentUserProfile.DisplayName}" : "Not logged in";
		public bool IsLoggedIn => Endpoint.IsLoggedIn;

		/*
		 * Search query
		 */
		private SearchQueryType m_CurrentSearchType = SearchQueryType.Artist;
		private Dictionary<SearchQueryType, ISearchQuery> m_SearchQueries = new Dictionary<SearchQueryType, ISearchQuery>();
		public ISearchQuery SearchQuery => m_SearchQueries[m_CurrentSearchType];

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

		public string UserSearchText {
			set {
				SearchQuery.SearchText = value;
				OnPropertyChanged(nameof(UserSearchText));
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

		public AffinityPlaylistGenerationVM(ProgressBar searchProgressBar, TextBlock searchProgressText, ScrollViewer logScrollViewer) {
			Endpoint.OnLoggedIn += () => {
				OnPropertyChanged(nameof(CurrentUser));
				OnPropertyChanged(nameof(CurrentUserProfile));
				OnPropertyChanged(nameof(IsLoggedIn));
			};
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
		public async void CreatePlaylist() {
			VisualLogger.AddLine($"Creating playlist with {PlaylistSize} tracks...");
			PlaylistCreateRequest request = new PlaylistCreateRequest(Playlist.Name);
			request.Description = Playlist.Description;
			FullPlaylist playlist = await CurrentUser.Playlists.Create(CurrentUserProfile.Id, request);
			List<List<string>> trackUriBatches = Playlist.SelectTracks(PlaylistSize, CurrentSelectionType);
			foreach (List<string> trackUris in trackUriBatches) {
				await Task.Delay(100);
				await CurrentUser.Playlists.AddItems(playlist.Id, new PlaylistAddItemsRequest(trackUris));
			}
			VisualLogger.AddLine($"Playlist created: {Playlist.Name}");
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
