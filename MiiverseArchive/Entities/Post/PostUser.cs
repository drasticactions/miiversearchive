using System;
using System.Runtime.Serialization;

namespace MiiverseArchive.Entities.Post
{
	[DataContract]
	public  class PostUser
	{
        public PostUser()
        {

        }

		public PostUser(string name, string screenName, Uri iconUri)
		{
			this.Name = name;
			this.ScreenName = screenName;
			this.IconUri = iconUri;
		}

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "screen_name")]
		public string ScreenName { get; set; }

		[DataMember(Name = "icon_uri")]
		public Uri IconUri { get; set; }
	}
}
