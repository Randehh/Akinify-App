using SpotifyAPI.Web;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Akinify_App {
	public class MainWindowVM : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;
		/*
		 * User functions
		 */
		public PrivateUser CurrentUserProfile => Endpoint.UserProfile;

		public string UserDisplayName => CurrentUserProfile != null ? $"Logged in as: {CurrentUserProfile.DisplayName}" : "Not logged in";

		public MainWindowVM() {
			Endpoint.OnLoggedIn += () => {
				OnPropertyChanged(nameof(CurrentUserProfile));
				OnPropertyChanged(nameof(UserDisplayName));
			};
		}

		
		public void OnPropertyChanged([CallerMemberName] string name = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
