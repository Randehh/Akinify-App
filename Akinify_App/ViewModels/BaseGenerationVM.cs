
using SpotifyAPI.Web;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Akinify_App {
	public class BaseGenerationVM : BaseVM {
		public RequestStaggerer RequestStaggerer { get; set; } = new RequestStaggerer();
		public ProgressBarWrapper SearchProgressBar { get; protected set; }

		private SpotifyPlaylist m_Playlist;
		public SpotifyPlaylist Playlist {
			get { return m_Playlist; }
			set {
				m_Playlist = value;
				OnPropertyChanged(nameof(Playlist));
				OnPropertyChanged(nameof(PlaylistTracks));
			}
		}

		public ObservableCollection<FullTrack> PlaylistTracks {
			get {
				if (Playlist == null) {
					return new ObservableCollection<FullTrack>();
				} else {
					return new ObservableCollection<FullTrack>(Playlist.Items);
				}
			}
		}

		public void UpdatePlaylistStats() {
			OnPropertyChanged(nameof(Playlist));
		}

		/*
		* Logger
		*/
		public VisualLogger VisualLogger { get; protected set; }
		protected ScrollViewer m_LogScrollViewer;
		private string m_LogText = "";
		public string LogText {
			get {
				return m_LogText;
			}
			set {
				m_LogText = value;
				OnPropertyChanged(nameof(LogText));
				m_LogScrollViewer.Dispatcher.Invoke(m_LogScrollViewer.ScrollToBottom);
			}
		}
	}
}
