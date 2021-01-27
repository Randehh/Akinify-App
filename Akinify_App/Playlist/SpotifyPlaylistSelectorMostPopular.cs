using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Linq;

namespace Akinify_App {
	public class SpotifyPlaylistSelectorMostPopular : SpotifyPlaylistSelector {
		public SpotifyPlaylistSelectorMostPopular(List<FullTrack> tracks, int count) : base(tracks, count) {
			m_Tracks = m_Tracks.OrderByDescending(o => o.Popularity).ToList();
		}

		public override List<List<string>> SelectTracks() {
			while (!IsDoneSelecting) {
				SelectTrack(0);
			}
			return GetResult();
		}
	}
}
