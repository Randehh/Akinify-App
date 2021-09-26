using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akinify_App {
	public class SearchQueryUser : SearchQueryBase<PublicUser, BlendPlaylistGenerationVM> {

		private const string PROFILE_LINK_FORMAT = @"https://open.spotify.com/user/";

		private Dictionary<string, PublicUser> m_AllUsers = new Dictionary<string, PublicUser>();

		public SearchQueryUser(BlendPlaylistGenerationVM vm) : base(vm) {

		}

		public override bool CanGeneratePlaylist => m_AllUsers.Count >= 1;

		protected override void OnUpdateSearchText(string s) { }

		public override async void ConfirmSearchText() {
			string s = SearchText;
			if (string.IsNullOrEmpty(s)) {
				Items = new ObservableCollection<PublicUser>();
			} else {
				try {
					string searchText = m_ViewModel.UserSearchText;
					if (searchText.StartsWith(PROFILE_LINK_FORMAT)) {
						int userIdLength = searchText.IndexOf("?") - PROFILE_LINK_FORMAT.Length;
						searchText = searchText.Substring(PROFILE_LINK_FORMAT.Length, userIdLength);
					}
					PublicUser response = await m_ViewModel.CurrentUser.UserProfile.Get(searchText);
					if (!m_AllUsers.ContainsKey(response.Id)) {
						m_AllUsers.Add(response.Id, response);
					}
				} catch { }

				Items = new ObservableCollection<PublicUser>(m_AllUsers.Values);

				m_ViewModel.OnPropertyChanged(nameof(m_ViewModel.SearchQuery));
			}
		}

		public override async void Search() {
			base.Search();

			List<string> userIdsToCheck = new List<string>(Items.Select((user) => user.Id));
			userIdsToCheck.Add(m_ViewModel.CurrentUserProfile.Id);

			ProgressBarWrapperTask userSearchTask = m_ViewModel.SearchProgressBar.AddTask(1, userIdsToCheck.Count);
			ProgressBarWrapperTask trackSearchTask = m_ViewModel.SearchProgressBar.AddTask(1, userIdsToCheck.Count);

			m_ViewModel.VisualLogger.AddLine($"Gathering top tracks by {userIdsToCheck.Count} users...");

			List<string> usersWithPlaylist = new List<string>();
			Dictionary<string, int> trackCounts = new Dictionary<string, int>();
			Dictionary<int, HashSet<string>> sortedTrackCounts = new Dictionary<int, HashSet<string>>();
			Dictionary<string, FullTrack> fullTracks = new Dictionary<string, FullTrack>();

			foreach(string userId in userIdsToCheck) {
				SimplePlaylist onRepeatPlaylist = await GetOnRepeatPlaylist(userId);

				if (onRepeatPlaylist == null) {
					m_ViewModel.VisualLogger.AddLine($"User {userId} does not have their \"On Repeat\" playlist set to public.");

					trackSearchTask.CompleteSection();
				} else {
					await Task.Delay(m_ViewModel.RequestStaggerer.GetNextDelay());
					FullPlaylist onRepeatFullPlaylist = await m_ViewModel.CurrentUser.Playlists.Get(onRepeatPlaylist.Id);
					foreach (PlaylistTrack<IPlayableItem> item in onRepeatFullPlaylist.Tracks.Items) {
						if (!(item.Track is FullTrack)) continue;

						FullTrack fullTrack = item.Track as FullTrack;
						if (trackCounts.ContainsKey(fullTrack.Id)) {
							trackCounts[fullTrack.Id]++;
						} else {
							trackCounts[fullTrack.Id] = 1;
						}

						if (!fullTracks.ContainsKey(fullTrack.Id)) {
							fullTracks.Add(fullTrack.Id, fullTrack);
						}
					}

					usersWithPlaylist.Add(userId);
					trackSearchTask.CompleteSection();
				}

				userSearchTask.CompleteSection();
			}

			int maxTrackCount = 0;
			foreach(KeyValuePair<string, int> keyValuePair in trackCounts) {
				if (!sortedTrackCounts.ContainsKey(keyValuePair.Value)) {
					sortedTrackCounts[keyValuePair.Value] = new HashSet<string>();

					if(keyValuePair.Value > maxTrackCount) {
						maxTrackCount = keyValuePair.Value;
					}
				}
				sortedTrackCounts[keyValuePair.Value].Add(keyValuePair.Key);
			}

			for(int i = maxTrackCount; i > 0; i--) {
				foreach(string trackId in sortedTrackCounts[i]) {
					fullTracks[trackId].Popularity = (i / maxTrackCount) * 100;
					m_ViewModel.Playlist.AddTrack(fullTracks[trackId]);
				}
			}

			StringBuilder description = new StringBuilder();
			description.Append("Blended by Akinify for the following users: ");
			for (int i = 0; i < usersWithPlaylist.Count; i++) {
				description.Append(usersWithPlaylist[i]);

				if (i != usersWithPlaylist.Count - 1) {
					description.Append(", ");
				}
			}
			m_ViewModel.Playlist.Description = description.ToString();

			m_ViewModel.PlaylistSize = usersWithPlaylist.Count * 15;
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
				await Task.Delay(m_ViewModel.RequestStaggerer.GetNextDelay());
				FullPlaylist onRepeatFullPlaylist = await m_ViewModel.CurrentUser.Playlists.Get(playlistId);
				foreach (PlaylistTrack<IPlayableItem> item in onRepeatFullPlaylist.Tracks.Items) {
					if (!(item.Track is FullTrack)) continue;

					FullTrack fullTrack = item.Track as FullTrack;
					if (trackCounts.ContainsKey(fullTrack.Id)) {
						trackCounts[fullTrack.Id]++;
					} else {
						trackCounts[fullTrack.Id] = 1;
					}

					if (!fullTracks.ContainsKey(fullTrack.Id)) {
						fullTracks.Add(fullTrack.Id, fullTrack);
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

			PublicUser profile = await m_ViewModel.CurrentUser.UserProfile.Get(userId);
			SimplePlaylist onRepeatPlaylist = null;

			int currentPlaylistOffset = 0;
			while (true) {
				await Task.Delay(m_ViewModel.RequestStaggerer.GetNextDelay());

				try {
					Paging<SimplePlaylist> playlists = await m_ViewModel.CurrentUser.Playlists.GetUsers(profile.Id, new PlaylistGetUsersRequest() {
						Limit = 50,
						Offset = currentPlaylistOffset,
					});

					if (playlists.Items.Count == 0) break;

					int localCount = 0;
					foreach (SimplePlaylist playlist in playlists.Items) {
						if (playlist.Name == ("On Repeat")) {
							onRepeatPlaylist = playlist;
							m_ViewModel.VisualLogger.AddLine($"User's {userId} \"On Repeat\" playlist found");
							break;
						}
						localCount++;
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
