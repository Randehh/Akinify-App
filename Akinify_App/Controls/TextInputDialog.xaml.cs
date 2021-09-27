using System;
using System.Windows;

namespace Akinify_App {
	/// <summary>
	/// Interaction logic for TextInputDialog.xaml
	/// </summary>
	public partial class TextInputDialog : Window {
		public TextInputDialog() {
			InitializeComponent();
		}

		public static string DisplayDialog(string headerText, string description, Func<string, bool> prerequisite = null) {
			TextInputDialogVM vm = new TextInputDialogVM(headerText, description, prerequisite);
			TextInputDialog dialog = new TextInputDialog();
			dialog.DataContext = vm;

			vm.OnRequestClose += () => {
				dialog.Close();
			};

			dialog.ShowDialog();
			return vm.ResultText;
		}
	}
}
