using SpotifyAPI.Web;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Akinify_App {
	public class MainWindowVM : BaseVM {
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
	}
}
