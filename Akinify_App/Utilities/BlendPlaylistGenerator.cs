using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Akinify_App {
	public class BlendPlaylistGenerator {

		private BlendPlaylistGenerationVM m_ViewModel;

		public BlendPlaylistGenerator(BlendPlaylistGenerationVM vm){
			m_ViewModel = vm;
		}

		public async Task UpdatePlaylist(string generatedPlaylistId, params string[] playlists) {
			m_ViewModel.Playlist = new SpotifyPlaylist(m_ViewModel.UpdatePlaylistStats);
			m_ViewModel.RequestStaggerer.Reset();
			m_ViewModel.SearchProgressBar.Reset();

			List<string> trackUris = new List<string>();

			ProgressBarWrapperTask userSearchTask = m_ViewModel.SearchProgressBar.AddTask(1, playlists.Length);
			ProgressBarWrapperTask trackSearchTask = m_ViewModel.SearchProgressBar.AddTask(1, playlists.Length);

			m_ViewModel.VisualLogger.AddLine($"Gathering top tracks by {playlists.Length} users...");

			Dictionary<string, int> trackCounts = new Dictionary<string, int>();
			Dictionary<int, HashSet<string>> sortedTrackCounts = new Dictionary<int, HashSet<string>>();
			Dictionary<string, FullTrack> fullTracks = new Dictionary<string, FullTrack>();

			foreach (string playlistId in playlists) {
				if (!string.IsNullOrEmpty(playlistId)) {
					await Task.Delay(m_ViewModel.RequestStaggerer.GetNextDelay());

					int currentTrackPlaylistOffset = 0;
					List<PlaylistTrack<IPlayableItem>> tracksInPlaylist = new List<PlaylistTrack<IPlayableItem>>();
					while (true) {
						await Task.Delay(100);
						Paging<PlaylistTrack<IPlayableItem>> tracks = await Endpoint.Client.Playlists.GetItems(playlistId, new PlaylistGetItemsRequest() {
							Limit = 100,
							Offset = currentTrackPlaylistOffset,
						});

						if (tracks.Items.Count == 0) break;

						tracksInPlaylist.AddRange(tracks.Items);

						currentTrackPlaylistOffset += tracks.Items.Count;
					}

					//Sort by date, then filter to the last 30
					int originalTrackCount = tracksInPlaylist.Count;
					tracksInPlaylist = tracksInPlaylist.OrderByDescending((track) => track.AddedAt).Take(30).ToList();

					if (originalTrackCount == tracksInPlaylist.Count) {
						m_ViewModel.VisualLogger.AddLine($"{tracksInPlaylist.Count} tracks found");
					} else {
						m_ViewModel.VisualLogger.AddLine($"{originalTrackCount} tracks found, truncated to {tracksInPlaylist.Count}");
					}
					foreach (PlaylistTrack<IPlayableItem> item in tracksInPlaylist) {
						if (!(item.Track is FullTrack)) continue;

						FullTrack fullTrack = item.Track as FullTrack;
						if (fullTrack.IsLocal) continue;

						if (trackCounts.ContainsKey(fullTrack.Id)) {
							trackCounts[fullTrack.Id]++;
						} else {
							trackCounts[fullTrack.Id] = 1;
						}

						if (!fullTracks.ContainsKey(fullTrack.Id)) {
							fullTracks.Add(fullTrack.Id, fullTrack);
						}
					}
				}
				trackSearchTask.CompleteSection();

				userSearchTask.CompleteSection();
			}

			int maxTrackCount = 0;
			foreach (KeyValuePair<string, int> keyValuePair in trackCounts) {
				if (!sortedTrackCounts.ContainsKey(keyValuePair.Value)) {
					sortedTrackCounts[keyValuePair.Value] = new HashSet<string>();

					if (keyValuePair.Value > maxTrackCount) {
						maxTrackCount = keyValuePair.Value;
					}
				}
				sortedTrackCounts[keyValuePair.Value].Add(keyValuePair.Key);
			}

			for (int i = maxTrackCount; i > 0; i--) {
				foreach (string trackId in sortedTrackCounts[i]) {
					fullTracks[trackId].Popularity = (int)(((float)i / playlists.Length) * 100);
					m_ViewModel.Playlist.AddTrack(fullTracks[trackId]);
				}
			}

			m_ViewModel.PlaylistSize = Math.Max(playlists.Length * 15, 100);

			List<string> selectedTracks = m_ViewModel.Playlist.SelectTracks(100, SelectionType.Most_Popular)[0];

			await Endpoint.Client.Playlists.ReplaceItems(generatedPlaylistId, new PlaylistReplaceItemsRequest(selectedTracks));
		}

		public async Task<SimplePlaylist> GetOnRepeatPlaylist(string userId) {
			int delay = m_ViewModel.RequestStaggerer.GetNextDelay();
			await Task.Delay(delay);

			PublicUser profile = await Endpoint.Client.UserProfile.Get(userId);
			SimplePlaylist onRepeatPlaylist = null;

			int currentPlaylistOffset = 0;
			while (true) {
				await Task.Delay(m_ViewModel.RequestStaggerer.GetNextDelay());

				try {
					Paging<SimplePlaylist> playlists = await Endpoint.Client.Playlists.GetUsers(profile.Id, new PlaylistGetUsersRequest() {
						Limit = 50,
						Offset = currentPlaylistOffset,
					});

					if (playlists.Items.Count == 0) break;

					foreach (SimplePlaylist playlist in playlists.Items) {
						if (playlist.Name == ("On Repeat")) {
							onRepeatPlaylist = playlist;
							m_ViewModel.VisualLogger.AddLine($"User's {userId} \"On Repeat\" playlist found");
							break;
						}
					}

					if (onRepeatPlaylist != null) break;

					currentPlaylistOffset += playlists.Items.Count;
				} catch (Exception e) {
					m_ViewModel.VisualLogger.AddLine($"Error: {e.Message}");
					break;
				}
			}

			return onRepeatPlaylist;
		}
	}
}
