using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MiiverseArchive.Context;
using MiiverseArchive.Entities.Token;
using MiiverseArchive.Tools.Constants;
using MiiverseArchive.Tools.Extensions;
using MiiverseArchive.Entities.Response;

namespace MiiverseArchive.Managers
{
    public class MiiverseOAuthClient : IDisposable
    {
        private const string AUTH_FORWARD_URI = "https://miiverse.nintendo.net/auth/forward";
        private const string AUTHORIZE_URI = "https://id.nintendo.net/oauth/authorize";

        public Task<NintendoNetworkSessionToken> GetTokenAsync()
        {
            return this.Client.PostAsync(AUTH_FORWARD_URI, new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                ["location"] = "https://miiverse.nintendo.net/",
            })).ContinueWith(r =>
            {
                var location = r.Result.Headers.Location;
                var queries = location.QueryToKeyValuePair();

                string clientID = null, responseType = null, redirectUri = null, state = null;
                foreach (var query in queries)
                {
                    switch (query.Key)
                    {
                        case "client_id":
                            clientID = query.Value;
                            break;

                        case "response_type":
                            responseType = query.Value;
                            break;

                        case "redirect_uri":
                            redirectUri = query.Value;
                            break;

                        case "state":
                            state = query.Value;
                            break;
                    }
                }

                if (clientID == null || responseType == null || redirectUri == null || state == null)
                {
                    throw new Exception();
                }

                return new NintendoNetworkSessionToken(clientID, responseType, redirectUri, state);
            });
        }

        public Task<MiiverseContext> Authorize(NintendoNetworkSessionToken sessionToken, NintendoNetworkAuthenticationToken authenticationToken, string language = "en-US", int region = ViewRegion.America)
        {
            // TODO: Handle authentication errors (bad passwords, network down) better.
            try
            {
                return this.Client.PostAsync(AUTHORIZE_URI, new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    ["client_id"] = sessionToken.ClientID,
                    ["response_type"] = sessionToken.ResponseType,
                    ["redirect_uri"] = sessionToken.RedirectUri.ToString(),
                    ["state"] = sessionToken.State,
                    ["nintendo_authenticate"] = string.Empty,
                    ["nintendo_authorize"] = string.Empty,
                    ["scope"] = string.Empty,
                    ["lang"] = "ja-JP",
                    ["username"] = authenticationToken.UserName,
                    ["password"] = authenticationToken.Password,
                })).ContinueWith(r => this.Client.GetAsync(r.Result.Headers.Location)).Unwrap()
            .ContinueWith(r =>
            {
                var cookie = this._clientHandler.CookieContainer.GetCookies(MiiverseConstantValues.MIIVERSE_DOMAIN_URI)
                    .Cast<Cookie>()
                    .Where(c => c.Name == "ms" && c.Path == "/" && c.Secure && c.HttpOnly)
                    .OrderByDescending(c => c.Expires.Ticks)
                    .First();
                return new MiiverseContext(authenticationToken.UserName, sessionToken.ClientID, cookie.Value, language, region);
            });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to Authenticate", ex);
            }
        }

        public void Dispose()
        {
            if (this._Client != null)
            {
                this._Client.Dispose();
                this._Client = null;
            }
        }

        private HttpClient Client
        {
            get
            {
                if (this._Client == null)
                {
                    this._clientHandler = new HttpClientHandler()
                    {
                        AllowAutoRedirect = false,
                        AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                    };

                    this._Client = new HttpClient(this._clientHandler, true);
                }
                return this._Client;
            }
        }
        private HttpClientHandler _clientHandler = null;
        private HttpClient _Client = null;
    }
}