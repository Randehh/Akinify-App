using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Akinify_App {
	public class SpotifyPlaylistSelectorMostPopular : SpotifyPlaylistSelector {
		public SpotifyPlaylistSelectorMostPopular(List<FullTrack> tracks, int count) : base(tracks, count) {
			m_Tracks = m_Tracks.OrderByDescending(o => o.Popularity).ToList();
		}

		public override List<List<string>> SelectTracks() {
			Random rand = new Random(DateTime.Now.ToString().GetHashCode());
			while (!IsDoneSelecting) {
				int lastIndex = 1;
				int popularity = -1;
				for(int i = 0; i < m_Tracks.Count; i++) {
					if(popularity == -1) {
						popularity = m_Tracks[i].Popularity;
					} else if(popularity == m_Tracks[i].Popularity){
						lastIndex = i + 1;
					}
				}
				SelectTrack(rand.Next(0, lastIndex));
			}
			return GetResult();
		}
	}
}
