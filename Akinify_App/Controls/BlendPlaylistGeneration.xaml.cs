using System.Windows;
using System.Windows.Controls;

namespace Akinify_App {
	/// <summary>
	/// Interaction logic for AffinityPlaylistGeneration.xaml
	/// </summary>
	public partial class BlendPlaylistGeneration : UserControl {

		private BlendPlaylistGenerationVM m_ViewModel;

		public BlendPlaylistGeneration() {
			InitializeComponent();
			m_ViewModel = new BlendPlaylistGenerationVM(SearchProgressBar, SearchProgressBarText, LogScrollViewer);
			DataContext = m_ViewModel;
		}

		private void UserTextBox_Update(object sender, RoutedEventArgs e) {
			m_ViewModel.UserSearchText = (sender as TextBox).Text;
		}

		private void AddUser_Click(object sender, RoutedEventArgs e) {
			m_ViewModel.SearchQuery.ConfirmSearchText();
		}
		private void GeneratePlaylist_Click(object sender, RoutedEventArgs e) {
			m_ViewModel.UpdateBlend();
		}

		private void SaveBlend_Click(object sender, RoutedEventArgs e) {
			m_ViewModel.CreateNewBlend();
		}
	}
}
