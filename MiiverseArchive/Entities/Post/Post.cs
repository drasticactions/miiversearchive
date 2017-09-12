using System;
using MiiverseArchive.Entities.Feeling;
using MiiverseArchive.Entities.Community;

namespace MiiverseArchive.Entities.Post
{
	public class Post
	{
        public Post()
        {

        }

        public Post(string id, bool isDeleted)
        {
            this.ID = id;
            this.IsDeleted = isDeleted;
        }

        public Post(string id, bool accept, string discussionType, DateTime time, string text, uint replyCount, uint empathyCount, bool isPlayed, bool isSpoiler, PostUser user, FeelingType feeling, PostCommunity community, string inReplyToId)
            : this(id, accept, discussionType, time, text, replyCount, empathyCount, isPlayed, isSpoiler, null, user, feeling, community)
        {
            this.InReplyToId = inReplyToId;
        }

        public Post(string id, bool accept, string discussionType, DateTime time, string text, uint replyCount, uint empathyCount, bool isPlayed, bool isSpoiler, PostUser user, FeelingType feeling, PostCommunity community)
			: this(id, accept, discussionType, time, text, replyCount, empathyCount, isPlayed, isSpoiler, null, user, feeling, community)
		{ }

		public Post(string id, bool accept, string discussionType, DateTime time, string text, uint replyCount, uint empathyCount, bool isPlayed, bool isSpoiler, Uri screenShotUri, PostUser user, FeelingType feeling, PostCommunity community)
			: this(id, accept, discussionType, time, null, text, replyCount, empathyCount, isPlayed, isSpoiler, screenShotUri, user, feeling, community)
		{
			this.Text = text;
			this.ImageUri = null;
		}

		public Post(string id, bool accept, string discussionType, DateTime time, PostTag tag, string text, uint replyCount, uint empathyCount, bool isPlayed, bool isSpoiler, PostUser user, FeelingType feeling, PostCommunity community)
			: this(id, accept, discussionType, time, tag, replyCount, empathyCount, isPlayed, isSpoiler, null, user, feeling, community)
		{
			this.Text = text;
			this.ImageUri = null;
		}

		public Post(string id, bool accept, string discussionType, DateTime time, PostTag tag, string text, uint replyCount, uint empathyCount, bool isPlayed, bool isSpoiler, Uri screenShotUri, PostUser user, FeelingType feeling, PostCommunity community)
			: this(id, accept, discussionType, time, tag, replyCount, empathyCount, isPlayed, isSpoiler, screenShotUri, user, feeling, community)
		{
			this.Text = text;
			this.ImageUri = null;
		}

		public Post(string id, bool accept, string discussionType, DateTime time, Uri imageUri, uint replyCount, uint empathyCount, bool isPlayed, bool isSpoiler, PostUser user, FeelingType feeling, PostCommunity community)
			: this(id, accept, discussionType, time, imageUri, replyCount, empathyCount, isPlayed, isSpoiler, null, user, feeling, community)
		{ }

		public Post(string id, bool accept, string discussionType, DateTime time, Uri imageUri, uint replyCount, uint empathyCount, bool isPlayed, bool isSpoiler, Uri screenShotUri, PostUser user, FeelingType feeling, PostCommunity community)
			: this(id, accept, discussionType, time, null, imageUri, replyCount, empathyCount, isPlayed, isSpoiler, screenShotUri, user, feeling, community)
		{
			this.Text = string.Empty;
			this.ImageUri = imageUri;
		}

		public Post(string id, bool accept, string discussionType, DateTime time, PostTag tag, Uri imageUri, uint replyCount, uint empathyCount, bool isPlayed, bool isSpoiler, PostUser user, FeelingType feeling, PostCommunity community)
			: this(id, accept, discussionType, time, tag, replyCount, empathyCount, isPlayed, isSpoiler, null, user, feeling, community)
		{
			this.Text = null;
			this.ImageUri = imageUri;
		}

		public Post(string id, bool accept, string discussionType, DateTime time, PostTag tag, Uri imageUri, uint replyCount, uint empathyCount, bool isPlayed, bool isSpoiler, Uri screenShotUri, PostUser user, FeelingType feeling, PostCommunity community)
			: this(id, accept, discussionType, time, tag, replyCount, empathyCount, isPlayed, isSpoiler, screenShotUri, user, feeling, community)
		{
			this.Text = null;
			this.ImageUri = imageUri;
		}

		private Post(string id, bool accept, string discussionType, DateTime time, PostTag tag, uint replyCount, uint empathyCount, bool isPlayed, bool isSpoiler, Uri screenShotUri, PostUser user, FeelingType feeling, PostCommunity community)
		{
			this.ID = id;
            this.IsAcceptingResponse = accept;
            this.DiscussionType = discussionType;
            this.PostedDate = time;
			this.Tag = tag;
			this.ReplyCount = replyCount;
			this.EmpathyCount = empathyCount;
			this.IsPlayed = isPlayed;
			this.IsSpoiler = isSpoiler;
			this.ScreenShotUri = screenShotUri;
			this.User = user;
			this.Feeling = feeling;
			this.Community = community;
		}

		/// <summary>
		/// Post ID
		/// </summary>
		public string ID { get; set; }

		/// <summary>
		/// Tag
		/// </summary>
		public PostTag Tag { get; set; }

		/// <summary>
		/// Text content
		/// </summary>
		public string Text { get; set; }

        /// <summary>
		/// The date of the post
		/// </summary>
        public DateTime PostedDate { get; set; }

		/// <summary>
		/// Image content
		/// </summary>
		public Uri ImageUri { get; set; }

		/// <summary>
		/// Reply count
		/// </summary>
		public uint ReplyCount { get; set; }

		/// <summary>
		/// Empathy count
		/// </summary>
		public uint EmpathyCount { get; set; }

		/// <summary>
		/// Played or not
		/// </summary>
		public bool IsPlayed { get; set; }

		/// <summary>
		/// Spoiler or not
		/// </summary>
		public bool IsSpoiler { get; set; }

        /// <summary>
		/// Is Accepting Responses
		/// </summary>
        public bool IsAcceptingResponse { get; set; }

        /// <summary>
		/// Topic Type
		/// </summary>
        public string DiscussionType { get; set; }

		/// <summary>
		/// Screen Shot
		/// </summary>
		public Uri ScreenShotUri { get; set; }

		/// <summary>
		/// User
		/// </summary>
		public PostUser User { get; set; }

		/// <summary>
		/// Feeling
		/// </summary>
		public FeelingType Feeling { get; set; }

		/// <summary>
		/// Community
		/// </summary>
		public PostCommunity Community { get; set; }

        /// <summary>
		/// Is Message Deleted
		/// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
		/// In Reply To Id
		/// </summary>
        public string InReplyToId { get; set; }

        public CommunityItem GameCommunity { get; set; }
	}
}
