using SpotifyAPI.Web;
using System;
using System.Collections.Generic;

namespace Akinify_App {
	public class SpotifyPlaylistSelectorRandom : SpotifyPlaylistSelector {

		public SpotifyPlaylistSelectorRandom(List<FullTrack> tracks, int count) : base(tracks, count) { }

		public override List<List<string>> SelectTracks() {
			Random rand = new Random(DateTime.Now.ToString().GetHashCode());
			while (!IsDoneSelecting) {
				int index = rand.Next(0, m_Tracks.Count);
				SelectTrack(index);
			}
			return GetResult();
		}
	}
}
