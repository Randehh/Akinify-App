using SpotifyAPI.Web;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Akinify_App {
	public class BlendPlaylistGenerationVM : BaseGenerationVM {
		public EnumBindingSourceExtension SearchDepthEnumBindingSource { get; } = new EnumBindingSourceExtension(typeof(SearchDepth));
		public bool IsLoggedIn => Endpoint.IsLoggedIn;

		public SimpleCommand OnGenerateBlendCommand { get; set; }
		public bool HasBlendSelected => BlendPlaylistManager != null ? BlendPlaylistManager.HasItemSelected : false;

		/*
		 * Blend groups
		 */
		private string m_BlendName;
		public string BlendName {
			get => m_BlendName;
			set {
				m_BlendName = value;
				OnPropertyChanged(nameof(BlendName));
			}
		}

		private BlendPlaylistManager m_BlendPlaylistManager;
		public BlendPlaylistManager BlendPlaylistManager {
			get => m_BlendPlaylistManager;
			set {
				m_BlendPlaylistManager = value;
				OnPropertyChanged(nameof(BlendPlaylistManager));
			}
		}

		public bool CanUpdatePlaylist => BlendPlaylistManager.SelectedItem != null;
		public int PlaylistSize { get; set; }

		public BlendPlaylistGenerationVM(ProgressBar searchProgressBar, TextBlock searchProgressText, ScrollViewer logScrollViewer) {
			Endpoint.OnLoggedIn += () => {
				OnPropertyChanged(nameof(IsLoggedIn));
				BlendPlaylistManager = BlendPlaylistManager.Load();
				OnPropertyChanged(nameof(HasBlendSelected));
			};

			m_LogScrollViewer = logScrollViewer;
			VisualLogger = new VisualLogger();
			VisualLogger.OnUpdated += (logText) => { LogText = logText; };
			VisualLogger.AddLine("Standing by.");

			SearchProgressBar = new ProgressBarWrapper(searchProgressBar, searchProgressText, () => {
				OnPropertyChanged(nameof(SearchProgressBar));
				if (SearchProgressBar.IsCompleted) {
					VisualLogger.AddLine("Search completed.");
				}
			});

			OnGenerateBlendCommand = new SimpleCommand(GeneratePlaylist, () => HasBlendSelected);
		}

		public void OpenBlendGroupEditor() {
			BlendGroupEditor.Show(BlendPlaylistManager);
		}

		private async void GeneratePlaylist() {
			BlendPlaylistGenerator generator = new BlendPlaylistGenerator(this);
			//Task.Run(async () => {
				await generator.UpdatePlaylist(BlendPlaylistManager.SelectedItem.GeneratedPlaylistId, BlendPlaylistManager.SelectedItem.Users.Select((user) => user.PlaylistId).ToArray());
			//});
		}
	}
}
