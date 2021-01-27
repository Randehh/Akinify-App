using SpotifyAPI.Web;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Akinify_App {
	public partial class MainWindow : Window {

		private MainWindowVM m_ViewModel;
		private AuthenticationWindow m_AuthenticationWindow;

		public MainWindow() {
			InitializeComponent();
			m_ViewModel = new MainWindowVM(SearchProgressBar, SearchProgressBarText, LogScrollViewer);
			DataContext = m_ViewModel;
		}

		protected override void OnActivated(EventArgs e) {
			base.OnActivated(e);

			if (m_ViewModel.IsLoggedIn) return;

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
			m_ViewModel.SetActiveClient(client);

			if (m_AuthenticationWindow != null) {
				Dispatcher.Invoke(m_AuthenticationWindow.Close);
			}
		}

		private void ArtistTextBox_Update(object sender, RoutedEventArgs e) {
			m_ViewModel.FindArtistsByName((sender as TextBox).Text);
		}

		private void GeneratePlaylist_Click(object sender, RoutedEventArgs e) {
			m_ViewModel.GeneratePlaylist();
		}

		private void CreatePlaylist_Click(object sender, RoutedEventArgs e) {
			m_ViewModel.CreatePlaylist();
		}
	}
}
