using System.Runtime.Serialization;

namespace MiiverseArchive.Entities.Post
{
	[DataContract]
	public  class PostTag
	{
        public PostTag()
        {

        }

		public PostTag(TagType tagType, string tagID, string tag)
		{
			this.TagType = tagType;
			this.TagID = tagID;
			this.Tag = tag;
		}

		/// <summary>
		/// Tag Type
		/// </summary>
		[DataMember(Name = "tag_type")]
		public TagType TagType { get; set; }

		/// <summary>
		/// Tag ID
		/// </summary>
		[DataMember(Name = "tag_id")]
		public string TagID { get; set; }

		/// <summary>
		/// Tag
		/// </summary>
		[DataMember(Name = "tag")]
		public string Tag { get; set; }
	}
}