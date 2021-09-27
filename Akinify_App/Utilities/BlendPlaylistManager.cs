using Newtonsoft.Json;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace Akinify_App {
	public class BlendPlaylistManager : BaseVM, IDataTableContext<BlendPlaylistGroup> {
		
		public ObservableCollection<BlendPlaylistGroup> BlendGroups { get; set; } = new ObservableCollection<BlendPlaylistGroup>();

		public ObservableCollection<BlendPlaylistGroup> Items => BlendGroups;

		private BlendPlaylistGroup m_SelectedItem;
		public BlendPlaylistGroup SelectedItem {
			get => m_SelectedItem;
			set {
				m_SelectedItem = value;
				OnPropertyChanged(nameof(SelectedItem));
				OnPropertyChanged(nameof(HasItemSelected));
				OnSelectedItemChanged();
			}
		}

		public bool HasItemSelected => SelectedItem != null;

		public Action OnSelectedItemChanged { get; set; } = delegate { };

		public BlendPlaylistGroup AddGroup(string name, string generatedPlaylistId) {
			BlendPlaylistGroup newGroup = new BlendPlaylistGroup(name, generatedPlaylistId);
			BlendGroups.Add(newGroup);
			OnPropertyChanged(nameof(Items));
			return newGroup;
		}

		public static BlendPlaylistManager Load() {
			BlendPlaylistManager manager = new BlendPlaylistManager();
			ObservableCollection<BlendPlaylistGroup> loadedGroups = FileUtilities.LoadJson<ObservableCollection<BlendPlaylistGroup>>("BlendPlaylistData");
			if(loadedGroups != null && loadedGroups.Count != 0) {
				manager.BlendGroups = loadedGroups;
			}
			return manager;
		}

		public void Save() {
			FileUtilities.SaveJson("BlendPlaylistData", BlendGroups);
		}
	}

	public class BlendPlaylistGroup {
		public string Name { get; set; }
		public string GeneratedPlaylistId { get; set; }
		public ObservableCollection<BlendPlaylistUser> Users { get; set; } = new ObservableCollection<BlendPlaylistUser>();

		public BlendPlaylistGroup(string name, string generatedPlaylistId) {
			Name = name;
			GeneratedPlaylistId = generatedPlaylistId;
		}

		public void AddUser(string userId, string playlistId, string playlistName) {
			Users.Add(new BlendPlaylistUser(userId) {
				PlaylistId = playlistId,
				PlaylistName = playlistName,
			});
		}
	}

	public class BlendPlaylistUser {
		public string UserId { get; set; }
		private string m_PlaylistId = "";
		public string PlaylistId {
			get => m_PlaylistId;
			set {
				m_PlaylistId = value;
				UpdatePlaylistName();
			}
		}
		public string PlaylistName { get; set; } = "-";
		[JsonIgnore]
		public ObservableCollection<BlendPlaylistWrapper> Playlists { get; set; } = new ObservableCollection<BlendPlaylistWrapper>();
		public BlendPlaylistUser(string userId) {
			UserId = userId;
			GetPlaylistsForUser();
		}

		private void GetPlaylistsForUser() {
			if (string.IsNullOrWhiteSpace(UserId)) return;

			Task.Run(async () => {
				List<BlendPlaylistWrapper> playlists = await FindPlaylistsTask();
				Application.Current.Dispatcher.Invoke(() => {
					bool playlistFound = false;
					foreach (BlendPlaylistWrapper wrapper in playlists) {
						Playlists.Add(wrapper);

						if (wrapper.Id == PlaylistId) playlistFound = true;
					}

					if (!playlistFound && !string.IsNullOrWhiteSpace(PlaylistId)) {
						Playlists.Add(new BlendPlaylistWrapper(PlaylistName, PlaylistId));
					}
				});
			});
		}

		private async Task<List<BlendPlaylistWrapper>> FindPlaylistsTask() {
			PublicUser profile = await Endpoint.Client.UserProfile.Get(UserId);
			List<BlendPlaylistWrapper> allPlaylists = new List<BlendPlaylistWrapper>();

			int currentPlaylistOffset = 0;
			while (true) {
				try {
					Paging<SimplePlaylist> playlists = await Endpoint.Client.Playlists.GetUsers(profile.Id, new PlaylistGetUsersRequest() {
						Limit = 50,
						Offset = currentPlaylistOffset,
					});

					if (playlists.Items.Count == 0) break;

					foreach(SimplePlaylist playlist in playlists.Items) {
						allPlaylists.Add(new BlendPlaylistWrapper(playlist));
					}

					currentPlaylistOffset += playlists.Items.Count;
				} catch {}
			}

			return allPlaylists;
		}

		private void UpdatePlaylistName() {
			bool nameFound = false;
			foreach(BlendPlaylistWrapper playlist in Playlists) {
				if(playlist.Id == PlaylistId) {
					PlaylistName = playlist.Name;
					nameFound = true;
					break;
				}
			}
			if (!nameFound) {
				PlaylistName = "-";
			}
		}
	}

	public class BlendPlaylistWrapper {
		public string Name { get; set; } = "";
		[JsonIgnore]
		public string DisplayName => IsHidden ? $"{Name} (hidden)" : Name;
		public string Id { get; set; }
		public bool IsHidden { get; set; }

		public BlendPlaylistWrapper(SimplePlaylist playlist, bool isHidden = false) {
			Name = playlist.Name;
			Id = playlist.Id;
			IsHidden = isHidden;
		}

		/// <summary>
		/// Represents a hidden playlist
		/// </summary>
		public BlendPlaylistWrapper(string name, string id) {
			Name = name;
			Id = id;
			IsHidden = true;
		}
	}
}
