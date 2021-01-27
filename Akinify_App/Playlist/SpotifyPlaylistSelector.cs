using SpotifyAPI.Web;
using System;
using System.Collections.Generic;

namespace Akinify_App {
	public partial class SpotifyPlaylistSelector {

		protected List<FullTrack> m_Tracks;
		private int m_TargetCount = 0;
		protected bool IsDoneSelecting => m_CurrentSongCount == m_TargetCount || m_Tracks.Count == 0;

		private List<List<string>> m_Batches = new List<List<string>>();
		private List<string> m_CurrentBatch = new List<string>();
		private int m_CurrentSongCount = 0;

		public SpotifyPlaylistSelector(List<FullTrack> tracks, int count) {
			m_Tracks = new List<FullTrack>(tracks);
			m_TargetCount = count;
		}

		public virtual List<List<string>> SelectTracks() {
			throw new Exception("SelectTracks function not implemented");
		}

		protected void SelectTrack(int trackIndex) {
			FullTrack track = m_Tracks[trackIndex];
			m_Tracks.RemoveAt(trackIndex);
			if(m_CurrentBatch.Count == 100) {
				m_Batches.Add(m_CurrentBatch);
				m_CurrentBatch = new List<string>();
			}
			m_CurrentBatch.Add(track.Uri);
			m_CurrentSongCount++;
		}

		protected List<List<string>> GetResult() {
			m_Batches.Add(m_CurrentBatch);
			return m_Batches;
		}
	}
}
