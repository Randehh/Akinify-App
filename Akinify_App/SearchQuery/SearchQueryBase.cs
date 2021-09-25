using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Akinify_App {

	public partial class SearchQueryBase<T> : ISearchQuery, IDataTableContext<T> {

		protected MainWindowVM m_ViewModel;

		private ObservableCollection<T> m_Items = new ObservableCollection<T>();
		public ObservableCollection<T> Items {
			get { return m_Items; }
			set {
				m_Items = value;
				OnPropertyChanged(nameof(Items));
			}
		}

		private T m_SelectedItem;
		public T SelectedItem {
			get {
				return m_SelectedItem;
			}
			set {
				m_SelectedItem = value;
				OnPropertyChanged(nameof(SelectedItem));
				OnPropertyChanged(nameof(CanGeneratePlaylist));
			}
		}
		public virtual bool CanGeneratePlaylist => SelectedItem != null;

		private string m_SearchText = "";

		public event PropertyChangedEventHandler PropertyChanged;

		public string SearchText {
			get {
				return m_SearchText;
			}
			set {
				m_SearchText = value;
				OnUpdateSearchText(value);
			}
		}

		public SearchQueryBase(MainWindowVM vm) {
			m_ViewModel = vm;
		}

		protected virtual void OnUpdateSearchText(string s) {
			throw new NotImplementedException();
		}

		public virtual async void Search() {
			m_ViewModel.Playlist = new SpotifyPlaylist(m_ViewModel.UpdatePlaylistStats);
			m_ViewModel.RequestStaggerer.Reset();
			m_ViewModel.SearchProgressBar.Reset();
		}

		public virtual async void ConfirmSearchText() {
			throw new NotImplementedException();
		}

		public void OnPropertyChanged([CallerMemberName] string name = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}