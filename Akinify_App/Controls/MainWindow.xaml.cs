using SpotifyAPI.Web;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Akinify_App {
	public partial class MainWindow : Window {
		private AuthenticationWindow m_AuthenticationWindow;

		public MainWindow() {
			InitializeComponent();
			DataContext = new MainWindowVM();
		}

		protected override void OnActivated(EventArgs e) {
			base.OnActivated(e);

			if (Endpoint.IsLoggedIn) return;

			if (m_AuthenticationWindow == null) {
				if (SpotifyAuthenticator.StartCached(OnLoggedIn)) {
					return;
				}

				m_AuthenticationWindow = new AuthenticationWindow(OnLoggedIn);
				m_AuthenticationWindow.Owner = this;
				m_AuthenticationWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				m_AuthenticationWindow.Show();
			}

			if (!m_AuthenticationWindow.IsVisible) {
				m_AuthenticationWindow.Show();
			}
		}

		private void OnLoggedIn(SpotifyClient client) {
			Endpoint.Client = client;
			Endpoint.UserProfile = Task.Run(() => client.UserProfile.Current()).Result;
			Endpoint.OnLoggedIn();

			if (m_AuthenticationWindow != null) {
				Dispatcher.Invoke(m_AuthenticationWindow.Close);
			}
		}
    }
}
