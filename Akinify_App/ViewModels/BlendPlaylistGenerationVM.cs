using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Akinify_App {
	public class BlendPlaylistGenerationVM : BaseGenerationVM {
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
		public SearchQueryUser SearchQuery { get; private set; }

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
		 * Blend groups
		 */
		private string m_BlendName;
		public string BlendName {
			get => m_BlendName;
			set {
				m_BlendName = value;
				OnPropertyChanged(nameof(BlendName));
			}
		}

		private BlendPlaylistManager m_BlendPlaylistManager;
		public BlendPlaylistManager BlendPlaylistManager {
			get => m_BlendPlaylistManager;
			set {
				m_BlendPlaylistManager = value;
				OnPropertyChanged(nameof(BlendPlaylistManager));
			}
		}

		public bool CanUpdatePlaylist => BlendPlaylistManager.SelectedItem != null;
		public int PlaylistSize { get; set; }

		public BlendPlaylistGenerationVM(ProgressBar searchProgressBar, TextBlock searchProgressText, ScrollViewer logScrollViewer) {
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

			SearchQuery = new SearchQueryUser(this);
			OnPropertyChanged(nameof(SearchQuery));

			BlendPlaylistManager = BlendPlaylistManager.Load();
		}

		public async void CreateNewBlend() {
			FullPlaylist playlist = await CreatePlaylist(BlendName);
			BlendPlaylistGroup group = BlendPlaylistManager.AddGroup(BlendName, playlist.Id);
			foreach(PublicUser user in SearchQuery.Items) {
				SimplePlaylist onRepeatPlaylist = await SearchQuery.GetOnRepeatPlaylist(user.Id);
				group.AddUser(user.Id, onRepeatPlaylist?.Id, onRepeatPlaylist?.Name);
			}

			BlendPlaylistManager.Save();
			OnPropertyChanged(nameof(BlendPlaylistManager));
		}

		public async void UpdateBlend() {
			BlendPlaylistGroup toUpdate = BlendPlaylistManager.SelectedItem;
			await SearchQuery.UpdatePlaylist(toUpdate.GeneratedPlaylistId, toUpdate.PlaylistIds.ToArray());
		}

		/*
		 * Spotify API calls
		 */
		public async Task<FullPlaylist> CreatePlaylist(string name) {
			PlaylistCreateRequest request = new PlaylistCreateRequest(name);
			//request.Description = Playlist.Description;
			return await CurrentUser.Playlists.Create(CurrentUserProfile.Id, request);
		}
	}
}
