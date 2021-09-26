using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Akinify_App {
	public class SearchQueryPlaylist : SearchQueryArtistBase<SimplePlaylist, AffinityPlaylistGenerationVM> {
		public SearchQueryPlaylist(AffinityPlaylistGenerationVM vm) : base(vm) {
		}

		protected override async void OnUpdateSearchText(string s) {
			if (string.IsNullOrEmpty(s)) {
				Items = new ObservableCollection<SimplePlaylist>();
			} else {
				SearchResponse response = await m_ViewModel.CurrentUser.Search.Item(new SearchRequest(SearchRequest.Types.Playlist, s));
				Items = new ObservableCollection<SimplePlaylist>(response.Playlists.Items.ToList());
				m_ViewModel.OnPropertyChanged(nameof(m_ViewModel.SearchQuery));
			}
		}

		public override SearchDepth GetSearchDepth() {
			return SearchDepth.Familiar;
		}

		public override Task<List<string>> GetArtistsToGenerateFrom() {
			return Task.Run(async () => {
				FullPlaylist response = await m_ViewModel.CurrentUser.Playlists.Get(SelectedItem.Id);
				HashSet<string> artists = new HashSet<string>();
				foreach (PlaylistTrack<IPlayableItem> item in response.Tracks.Items) {
					if (!(item.Track is FullTrack)) continue;

					FullTrack track = item.Track as FullTrack;
					foreach (SimpleArtist artist in track.Artists) {
						artists.Add(artist.Id);
					}
				}
				return artists.ToList();
			});
		}
	}
}
