using System;
using System.Runtime.Serialization;

namespace MiiverseArchive.Entities.Post
{
	public  class PostCommunity
	{
        public PostCommunity()
        {

        }

		public PostCommunity(ulong titleID, ulong id, string name, Uri iconUri)
		{
			this.TitleID = titleID;
			this.ID = id;
			this.Name = name;
			this.IconUri = iconUri;
		}

		/// <summary>
		/// Title ID
		/// </summary>
		[DataMember(Name = "title_id")]
		public ulong TitleID { get; set; }

		/// <summary>
		/// Community ID
		/// </summary>
		[DataMember(Name = "id")]
		public ulong ID { get; set; }

		/// <summary>
		/// Community Name
		/// </summary>
		[DataMember(Name = "name")]
		public string Name { get; set; }

		/// <summary>
		/// Community Icon Uri
		/// </summary>
		[DataMember(Name = "icon_uri")]
		public Uri IconUri { get; set; }
	}
}
