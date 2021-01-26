using SpotifyAPI.Web;
using System.Windows;
using System.Windows.Controls;

namespace Akinify_App {
	public partial class MainWindow : Window {

		private MainWindowVM m_ViewModel;

		public MainWindow() {
			InitializeComponent();
			m_ViewModel = new MainWindowVM(SearchProgressBar, SearchProgressBarText, LogScrollViewer);
			DataContext = m_ViewModel;
		}

		private void LoginButton_Click(object sender, RoutedEventArgs e) {
			m_ViewModel.Login();
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
