using SpotifyAPI.Web;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace Akinify_App {
	public class BlendGroupEditorVM : BaseVM {

		private const string PROFILE_LINK_FORMAT = @"https://open.spotify.com/user/";

		private BlendPlaylistManager m_BlendPlaylistManager;
		public BlendPlaylistManager BlendPlaylistManager {
			get => m_BlendPlaylistManager;
			set => SetProperty(ref m_BlendPlaylistManager, value);
		}

		public SimpleCommand CreateNewBlendCommand { get; private set; }
		public SimpleCommand RemoveSelectedBlendCommand { get; private set; }
		public SimpleCommand AddNewUserCommand { get; private set; }
		public SimpleCommand SaveCommand { get; private set; }

		public BlendGroupEditorVM(BlendPlaylistManager manager) {
			BlendPlaylistManager = manager;
			BlendPlaylistManager.OnSelectedItemChanged += UpdateUsers;

			CreateNewBlendCommand = new SimpleCommand(CreateNewBlendGroup);
			RemoveSelectedBlendCommand = new SimpleCommand(DeleteSelectedBlendGroup, () => BlendPlaylistManager.HasItemSelected);
			AddNewUserCommand = new SimpleCommand(AddNewUserToBlendGroup, () => BlendPlaylistManager.HasItemSelected);
			SaveCommand = new SimpleCommand(BlendPlaylistManager.Save);
		}

		~BlendGroupEditorVM() {
			BlendPlaylistManager.OnSelectedItemChanged -= UpdateUsers;
		}

		private void UpdateUsers() {
			OnPropertyChanged(nameof(BlendPlaylistManager));
		}

		private void CreateNewBlendGroup() {
			string result = TextInputDialog.DisplayDialog("New blend group", "Enter new blend group name");
			if (string.IsNullOrWhiteSpace(result)) return;

			Task.Run(async () => {
				FullPlaylist playlist = await CreatePlaylist(result);

				Application.Current.Dispatcher.Invoke(() => {
					BlendPlaylistManager.AddGroup(playlist.Name, playlist.Id);
					BlendPlaylistManager.Save();
				});
			});
		}

		private void DeleteSelectedBlendGroup() {
			BlendPlaylistManager.BlendGroups.Remove(BlendPlaylistManager.SelectedItem);
			BlendPlaylistManager.Save();
		}

		private void AddNewUserToBlendGroup() {
			string result = TextInputDialog.DisplayDialog("Add new user", "Enter the username or profile link (in Spotify: Copy link to profile)");
			if (string.IsNullOrWhiteSpace(result)) return;

			Task.Run(async () => {
				try {
					string searchText = result;
					if (searchText.StartsWith(PROFILE_LINK_FORMAT)) {
						int userIdLength = searchText.IndexOf("?") - PROFILE_LINK_FORMAT.Length;
						searchText = searchText.Substring(PROFILE_LINK_FORMAT.Length, userIdLength);
					}
					PublicUser response = await Endpoint.Client.UserProfile.Get(searchText);
					Application.Current.Dispatcher.Invoke(() => {
						BlendPlaylistManager.SelectedItem.AddUser(response.Id, "", "");
						BlendPlaylistManager.Save();
					});
				} catch { }
			});
		}

		public async Task<FullPlaylist> CreatePlaylist(string name) {
			PlaylistCreateRequest request = new PlaylistCreateRequest(name);
			return await Endpoint.Client.Playlists.Create(Endpoint.UserProfile.Id, request);
		}
	}
}
