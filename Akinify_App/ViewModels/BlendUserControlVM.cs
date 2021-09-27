namespace Akinify_App {
	public class BlendUserControlVM : BaseVM {

		private BlendPlaylistUser m_User;
		public BlendPlaylistUser User {
			get => m_User;
			set {
				SetProperty(ref m_User, value);
				OnPropertyChanged(nameof(Username));
				OnPropertyChanged(nameof(SelectedPlaylistId));
				OnPropertyChanged(nameof(SelectedPlaylistName));
			}
		}

		public string Username => User.UserId;
		public string SelectedPlaylistId => User.PlaylistId;
		public string SelectedPlaylistName => User.PlaylistName;

		public BlendUserControlVM(BlendPlaylistUser user) {
			User = user;


		}
	}
}
