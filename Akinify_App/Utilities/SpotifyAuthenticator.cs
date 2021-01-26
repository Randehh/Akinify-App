using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace Akinify_App {
	class SpotifyAuthenticator {

		private static readonly Uri m_CallbackUri = new Uri("http://localhost:5000/callback");
		private static EmbedIOAuthServer m_AuthServer;

		private static Action<SpotifyClient> m_OnComplete;

		private static string m_ClientId = "YOUR_ID_HERE";
		private static string m_ClientSecret = "YOUR_SECRET_HERE";

		public static async Task Start(Action<SpotifyClient> onComplete) {
			m_OnComplete = onComplete;
			m_AuthServer = new EmbedIOAuthServer(m_CallbackUri, 5000);
			await m_AuthServer.Start();

			m_AuthServer.AuthorizationCodeReceived += OnAuthorizationCodeReceived;

			LoginRequest request = new LoginRequest(m_AuthServer.BaseUri, m_ClientId, LoginRequest.ResponseType.Code) {
				Scope = new List<string> { Scopes.PlaylistModifyPublic }
			};
			BrowserUtil.Open(request.ToUri());
		}

		private static async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response) {
			await m_AuthServer.Stop();

			SpotifyClientConfig config = SpotifyClientConfig.CreateDefault();
			AuthorizationCodeTokenRequest tokenRequest = new AuthorizationCodeTokenRequest(m_ClientId, m_ClientSecret, response.Code, m_CallbackUri);
			AuthorizationCodeTokenResponse tokenResponse = await new OAuthClient(config).RequestToken(tokenRequest);

			SpotifyClient client = new SpotifyClient(tokenResponse.AccessToken);
			m_OnComplete(client);
		}
	}
}
