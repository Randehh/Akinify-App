using SpotifyAPI.Web;
using System;

namespace Akinify_App {

	public static class Endpoint {

		public static SpotifyClient Client { get; set; }
		public static PrivateUser UserProfile { get; set; }
		public static bool IsLoggedIn => Client != null;
		public static Action OnLoggedIn { get; set; } = delegate { };
	}
}
