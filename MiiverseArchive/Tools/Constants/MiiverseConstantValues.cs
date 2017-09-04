using System;

namespace MiiverseArchive.Tools.Constants
{
    public enum WebApiType
    {
        Drawing,
        Diary,
        Discussion,
        InGame,
        OldGame
    }

	public static class MiiverseConstantValues
	{
		public const string MIIVERSE_DOMAIN = "miiverse.nintendo.net";
		public const string MIIVERSE_DOMAIN_URI_STRING = "https://miiverse.nintendo.net/";
		public static readonly Uri MIIVERSE_DOMAIN_URI = new Uri(MIIVERSE_DOMAIN_URI_STRING);

		public const string MIIVERSE_SIGN_OUT_URI_STRING = "https://id.nintendo.net/oauth/logout?client_id={0}";

		public const string MIIVERSE_ACTIVITY_URI_STRING = MIIVERSE_DOMAIN_URI_STRING + "activity";
		public const string MIIVERSE_USER_PAGE_URI_STRING = MIIVERSE_DOMAIN_URI_STRING + "users/{0}";

		public const string MIIVERSE_COMMUNITIES_FAVORITES_URI_STRING = MIIVERSE_DOMAIN_URI_STRING + "communities/favorites";
		public const string MIIVERSE_COMMUNITIES_PLAYED_URI_STRING = MIIVERSE_DOMAIN_URI_STRING + "communities/played";
	}
}
