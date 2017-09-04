using System;

namespace MiiverseArchive.Entities.Token
{
	public  class NintendoNetworkSessionToken
	{
		public NintendoNetworkSessionToken(string clientID, string responseType, string redirectUri, string state)
		{
			this.ClientID = clientID;
			this.ResponseType = responseType;
			this.RedirectUri = new Uri(Uri.UnescapeDataString(redirectUri));
			this.State = state;
		}

		public string ClientID { get; set; }
		public string ResponseType { get; set; }
		public Uri RedirectUri { get; set; }
		public string State { get; set; }
	}
}
