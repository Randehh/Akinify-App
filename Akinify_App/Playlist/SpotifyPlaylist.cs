using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Akinify_App {
	public class SpotifyPlaylist {

		private Action m_OnUpdate;
		private List<FullTrack> m_Tracks = new List<FullTrack>();
		public ObservableCollection<FullTrack> Tracks => new ObservableCollection<FullTrack>(m_Tracks);

		private HashSet<string> m_ArtistIdSet = new HashSet<string>();
		private HashSet<string> m_TrackIdSet = new HashSet<string>();
		private HashSet<string> m_AlbumIdSet = new HashSet<string>();

		public int TrackCount => m_TrackIdSet.Count;
		public int ArtistCount => m_ArtistIdSet.Count;
		public int AlbumCount => m_AlbumIdSet.Count;

		public SpotifyPlaylist(Action onUpdate) {
			m_OnUpdate = onUpdate;
		}

		public void AddTrack(FullTrack track) {
			if (m_TrackIdSet.Contains(track.Uri + track.Id)) {
				return;
			}

			foreach(SimpleArtist artist in track.Artists) {
				if (!m_ArtistIdSet.Contains(artist.Id)) {
					m_ArtistIdSet.Add(artist.Id);
				}
			}

			if (!m_AlbumIdSet.Contains(track.Album.Id)) {
				m_AlbumIdSet.Add(track.Album.Id);
			}

			m_TrackIdSet.Add(track.Uri + track.Id);
			m_Tracks.Add(track);

			m_OnUpdate();
		}

		public List<List<string>> SelectTracks(int count, SelectionType selectionType) {
			SpotifyPlaylistSelector selector;
			switch (selectionType) {
				default:
				case SelectionType.Random:
					selector = new SpotifyPlaylistSelectorRandom(m_Tracks, count);
					break;

				case SelectionType.Most_Popular:
					selector = new SpotifyPlaylistSelectorMostPopular(m_Tracks, count);
					break;
			}

			return selector.SelectTracks();
		}
	}
}
