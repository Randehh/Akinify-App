using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Akinify_App {
	class SpotifyAuthenticator {

		private static readonly Uri m_CallbackUri = new Uri("http://localhost:5000/callback");
		private static EmbedIOAuthServer m_AuthServer;

		private static string m_TokenPath = "token";
		private static string m_ClientId = "2bd4f113bcaf4e3f84b7c7fdf65ed706";

		public static bool StartCached(Action<SpotifyClient> onComplete) {
			PKCETokenResponse token = FileUtilities.LoadJson<PKCETokenResponse>(m_TokenPath);
			if(token == null) {
				return false;
			}

			PKCEAuthenticator authenticator = new PKCEAuthenticator(m_ClientId, token);
            authenticator.TokenRefreshed += (sender, refreshedToken) => FileUtilities.SaveJson(m_TokenPath, refreshedToken);

			SpotifyClientConfig config = SpotifyClientConfig.CreateDefault().WithAuthenticator(authenticator);
			SpotifyClient client = new SpotifyClient(config);

			onComplete(client);
			return true;
		}

		public static async Task StartRemote(Action<SpotifyClient> onComplete) {
            (string verifier, string challenge) = PKCEUtil.GenerateCodes();

			m_AuthServer = new EmbedIOAuthServer(m_CallbackUri, 5000);
			await m_AuthServer.Start();

			m_AuthServer.AuthorizationCodeReceived += async (sender, response) =>
            {
                await m_AuthServer.Stop();
                PKCETokenResponse token = await new OAuthClient().RequestToken(
                  new PKCETokenRequest(m_ClientId, response.Code, m_AuthServer.BaseUri, verifier)
                );

				FileUtilities.SaveJson(m_TokenPath, token);
				StartCached(onComplete);
			};

            var request = new LoginRequest(m_AuthServer.BaseUri, m_ClientId, LoginRequest.ResponseType.Code) {
                CodeChallenge = challenge,
                CodeChallengeMethod = "S256",
                Scope = new List<string> { Scopes.PlaylistModifyPublic }
			};

            Uri uri = request.ToUri();
            BrowserUtil.Open(uri);
        }
    }
}
