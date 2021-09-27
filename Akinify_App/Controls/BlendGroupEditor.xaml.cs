using System.Windows;

namespace Akinify_App {
	/// <summary>
	/// Interaction logic for BlendGroupEditor.xaml
	/// </summary>
	public partial class BlendGroupEditor : Window {
		private BlendGroupEditor() {
			InitializeComponent();
		}

		public static void Show(BlendPlaylistManager manager) {
			BlendGroupEditor newDialog = new BlendGroupEditor() {
				DataContext = new BlendGroupEditorVM(manager),
			};
			newDialog.ShowDialog();
		}
	}
}
