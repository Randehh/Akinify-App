using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Akinify_App {

	public partial class AuthenticationWindow : Window {

		private AuthenticationWindowVM m_ViewModel;
		private Action<SpotifyClient> m_OnLoggedIn;
		private bool m_AllowClosing = false;

		public AuthenticationWindow(Action<SpotifyClient> onLoggedIn) {
			InitializeComponent();

			m_ViewModel = new AuthenticationWindowVM();
			DataContext = m_ViewModel;
			WindowStyle = WindowStyle.None;

			m_OnLoggedIn = onLoggedIn;
		}

		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing(e);
			e.Cancel = !m_AllowClosing;
		}

		private async void LoginButton_Click(object sender, RoutedEventArgs e) {
			await SpotifyAuthenticator.Start(OnLoggedIn);
		}

		private void OnLoggedIn(SpotifyClient client) {
			m_AllowClosing = true;
			m_OnLoggedIn(client);
		}

		private void AboutButton_Click(object sender, RoutedEventArgs e) {
			string url = "https://github.com/Randehh/Akinify-App";
			var process = new ProcessStartInfo {
				FileName = url,
				UseShellExecute = true
			};
			Process.Start(process);
		}
	}
}
