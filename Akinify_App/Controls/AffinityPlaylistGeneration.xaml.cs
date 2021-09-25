using SpotifyAPI.Web;
using System.Windows;
using System.Windows.Controls;

namespace Akinify_App {
	/// <summary>
	/// Interaction logic for AffinityPlaylistGeneration.xaml
	/// </summary>
	public partial class AffinityPlaylistGeneration : UserControl {

		private AffinityPlaylistGenerationVM m_ViewModel;

		public AffinityPlaylistGeneration() {
			InitializeComponent();
			m_ViewModel = new AffinityPlaylistGenerationVM(SearchProgressBar, SearchProgressBarText, LogScrollViewer);
			DataContext = m_ViewModel;
		}

		private void QueryTextBox_Update(object sender, RoutedEventArgs e) {
			m_ViewModel.SearchText = (sender as TextBox).Text;
		}

		private void UserTextBox_Update(object sender, RoutedEventArgs e) {
			m_ViewModel.UserSearchText = (sender as TextBox).Text;
		}

		private void AddUser_Click(object sender, RoutedEventArgs e) {
			m_ViewModel.SearchQuery.ConfirmSearchText();
		}
		private void GeneratePlaylist_Click(object sender, RoutedEventArgs e) {
			m_ViewModel.SearchQuery.Search();
		}

		private void CreatePlaylist_Click(object sender, RoutedEventArgs e) {
			m_ViewModel.CreatePlaylist();
		}
	}
}
