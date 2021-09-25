using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Akinify_App {
	public class SearchQueryArtist : SearchQueryArtistBase<FullArtist> {
		public SearchQueryArtist(AffinityPlaylistGenerationVM vm) : base(vm) {

		}

		protected override async void OnUpdateSearchText(string s) {
			if (string.IsNullOrEmpty(s)) {
				Items = new ObservableCollection<FullArtist>();
			} else {
				SearchResponse response = await m_ViewModel.CurrentUser.Search.Item(new SearchRequest(SearchRequest.Types.Artist, s));
				Items = new ObservableCollection<FullArtist>(response.Artists.Items);
				m_ViewModel.OnPropertyChanged(nameof(m_ViewModel.SearchQuery));
			}
		}

		public override SearchDepth GetSearchDepth() {
			return m_ViewModel.CurrentSearchDepth;
		}

		public override Task<List<string>> GetArtistsToGenerateFrom() {
			return Task.Run(() => {
				return new List<string>() { SelectedItem.Id };
			});
		}
	}
}
