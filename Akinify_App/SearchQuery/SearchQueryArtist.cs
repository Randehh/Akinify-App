using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akinify_App {
	public class SearchQueryArtist : SearchQueryBase {

		private FullArtist m_SelectedArtist = null;
		public FullArtist SelectedArtist { 
			get {
				return m_SelectedArtist;
			}
			set {
				m_SelectedArtist = value;
				m_ViewModel.OnPropertyChanged(nameof(m_ViewModel.SearchQuery));
			}
		}
		public bool CanGeneratePlaylist => SelectedArtist != null;

		public SearchQueryArtist(MainWindowVM vm) : base(vm) {

		}

		protected override async void OnUpdateSearchText(string s) {
			if (string.IsNullOrEmpty(s)) {
				BaseArtistList = new ObservableCollection<FullArtist>();
			} else {
				SearchResponse response = await m_ViewModel.CurrentUser.Search.Item(new SearchRequest(SearchRequest.Types.Artist, s));
				BaseArtistList = new ObservableCollection<FullArtist>(response.Artists.Items);
				m_ViewModel.OnPropertyChanged(nameof(m_ViewModel.SearchQuery));
			}
		}

		public override SearchDepth GetSearchDepth() {
			return m_ViewModel.CurrentSearchDepth;
		}

		public override List<FullArtist> GetArtistsToGenerateFrom() {
			List<FullArtist> list = new List<FullArtist>();
			list.Add(m_SelectedArtist);
			return list;
		}
	}
}
