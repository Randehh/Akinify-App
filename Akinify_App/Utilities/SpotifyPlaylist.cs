﻿using SpotifyAPI.Web;
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

		public List<List<string>> SelectTracks(int count) {
			List<List<string>> batches = new List<List<string>>();
			Random rand = new Random(DateTime.Now.ToString().GetHashCode());
			List<FullTrack> uniqueTracks = new List<FullTrack>(m_Tracks);
			List<string> selectedTracks = new List<string>();
			int totalSelectedTracks = 0;
			while (uniqueTracks.Count > 0 && totalSelectedTracks < count) {
				int index = rand.Next(0, uniqueTracks.Count);
				selectedTracks.Add(uniqueTracks[index].Uri);
				uniqueTracks.RemoveAt(index);
				totalSelectedTracks++;

				if (selectedTracks.Count == 100) {
					batches.Add(selectedTracks);
					selectedTracks = new List<string>();
				}
			}
			
			if(selectedTracks.Count != 0) {
				batches.Add(selectedTracks);
			}
			return batches;
		}
	}
}
