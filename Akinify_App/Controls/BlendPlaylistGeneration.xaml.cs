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

		private void EditBlends_Click(object sender, RoutedEventArgs e) {
			m_ViewModel.OpenBlendGroupEditor();
		}
	}
}
