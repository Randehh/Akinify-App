using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Akinify_App {
	public class BlendPlaylistManager : BaseVM, IDataTableContext<BlendPlaylistGroup> {
		
		public List<BlendPlaylistGroup> BlendGroups { get; set; } = new List<BlendPlaylistGroup>();

		public ObservableCollection<BlendPlaylistGroup> Items => new ObservableCollection<BlendPlaylistGroup>(BlendGroups);

		private BlendPlaylistGroup m_SelectedItem;
		public BlendPlaylistGroup SelectedItem {
			get => m_SelectedItem;
			set {
				m_SelectedItem = value;
				OnPropertyChanged(nameof(SelectedItem));
				OnPropertyChanged(nameof(HasItemSelected));
			}
		}

		public bool HasItemSelected => SelectedItem != null;

		public BlendPlaylistGroup AddGroup(string name, string generatedPlaylistId) {
			BlendPlaylistGroup newGroup = new BlendPlaylistGroup(name, generatedPlaylistId);
			BlendGroups.Add(newGroup);
			return newGroup;
		}

		public static BlendPlaylistManager Load() {
			BlendPlaylistManager loadedManager = FileUtilities.LoadJson<BlendPlaylistManager>("BlendPlaylistData");
			if(loadedManager != null) {
				return loadedManager;
			}
			return new BlendPlaylistManager();
		}

		public void Save() {
			FileUtilities.SaveJson("BlendPlaylistData", this);
		}
	}

	public class BlendPlaylistGroup {
		public string Name { get; set; }
		public string GeneratedPlaylistId { get; set; }
		public List<string> UserIds { get; set; } = new List<string>();
		public List<string> PlaylistIds { get; set; } = new List<string>();
		public List<string> PlaylistNames { get; set; } = new List<string>();

		public BlendPlaylistGroup(string name, string generatedPlaylistId) {
			Name = name;
			GeneratedPlaylistId = generatedPlaylistId;
		}

		public void AddUser(string userId, string playlistId, string playlistName) {
			UserIds.Add(userId);
			PlaylistIds.Add(playlistId);
			PlaylistNames.Add(playlistName);
		}
	}
}
