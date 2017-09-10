using System;
using System.Runtime.Serialization;

namespace MiiverseArchive.Entities.Post
{
	public  class PostUser
	{
        public PostUser()
        {

        }

		public PostUser(string name, string screenName, Uri iconUri, bool isOfficial)
		{
			this.Name = name;
			this.ScreenName = screenName;
			this.IconUri = iconUri;
            this.IsOfficial = isOfficial;
		}

		public string Name { get; set; }

		public string ScreenName { get; set; }

		public Uri IconUri { get; set; }

        public bool IsOfficial { get; set; }
	}
}
