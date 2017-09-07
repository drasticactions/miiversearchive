using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MiiverseArchive.Entities.Community;
using MiiverseArchive.Entities.Feeling;
using MiiverseArchive.Entities.Post;
using MiiverseArchive.Entities.Response;
using MiiverseArchive.Entities.User;
using MiiverseArchive.Tools.Constants;
using MiiverseArchive.Tools.Extensions;
using System.Diagnostics;

namespace MiiverseArchive.Context
{
    public class MiiverseContext : IDisposable
    {
        private bool _isEnabled = true;

        public MiiverseContext(string userName, string clientID, string sessionValue, string language = "en-US", int region = ViewRegion.America)
        {
            UserName = userName;
            ClientID = clientID;
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            };
            handler.CookieContainer.Add(MiiverseConstantValues.MIIVERSE_DOMAIN_URI, new Cookie("ms", sessionValue, "/", MiiverseConstantValues.MIIVERSE_DOMAIN)
            {
                Secure = true,
                HttpOnly = true,
            });
            handler.CookieContainer.Add(MiiverseConstantValues.MIIVERSE_DOMAIN_URI, new Cookie("lang", language, "/", MiiverseConstantValues.MIIVERSE_DOMAIN)
            {
                Secure = true,
                HttpOnly = true,
            });
            handler.CookieContainer.Add(MiiverseConstantValues.MIIVERSE_DOMAIN_URI, new Cookie("view_region_id", region.ToString(), "/", MiiverseConstantValues.MIIVERSE_DOMAIN)
            {
                Secure = true,
                HttpOnly = true,
            });
            var client = new HttpClient(handler, true);
            client.DefaultRequestHeaders.Add("Accept-Language", $"{language},en;q=0.5");
            Client = client;

        }

        public async Task<WebApiResponse> GetPostResponse(string postId, WebApiType type, double lastPostTime = 0, double currentDate = 0)
        {
            AccessCheck();

            var baseUrl = $"https://miiverse.nintendo.net/posts/{postId}";
            if (currentDate == 0)
            {
                currentDate = ReturnEpochTime(DateTime.Now);
            }

            switch (type)
            {
                case WebApiType.Replies:
                    baseUrl += "/replies";
                    break;
            }

            if (lastPostTime != 0)
            {
                baseUrl += $"?page_param=%7B%22upinfo%22%3A%22{lastPostTime * -1 }%2C{(int)currentDate}%2C{currentDate}%22%2C%22reftime%22%3A%22{lastPostTime}%22%2C%22per_page%22%3A120%2C%22order%22%3A%22asc%22%7D&selected=all";
            }
            else
            {
                baseUrl += $"?page_param=%7B%22per_page%22%3A120%2C%22order%22%3A%22asc%22%7D&selected=all";
            }

            var req = new HttpRequestMessage(HttpMethod.Get, baseUrl);
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using (var result = await Client.SendAsync(req))
            using(var content = await result.Content.ReadAsStreamAsync())
            {
                var doc = new HtmlDocument();
                doc.Load(content, System.Text.Encoding.UTF8);

                var posts = new List<Post>();

                switch (type)
                {
                    case WebApiType.Replies:
                        posts = ParsePosts(type, doc);
                        break;
                    case WebApiType.Posts:
                        posts.Add(ParsePost(doc.GetElementbyId("main-body").GetElementByClassName("post-list").ChildNodes.FirstOrDefault(n => n.HasClassName("post") && !n.HasClassName("none"))));
                        posts.Select(n => n.InReplyToId = postId);
                        break;
                }
                double postTime = 0;
                if (posts.Any())
                {
                    postTime = -(ReturnEpochTime(posts.Last().PostedDate));
                }

                return new WebApiResponse(currentDate, postTime, posts);
            }
        }

        public async Task<WebApiResponse> GetWebApiResponse(Game game, WebApiType type, double lastPostTime = 0, double currentDate = 0)
        {
            AccessCheck();

            var baseUrl = "https://miiverse.nintendo.net";
            if (currentDate == 0)
            {
                currentDate = ReturnEpochTime(DateTime.Now);
            }

            switch(type)
            {
                case WebApiType.Diary:
                    baseUrl += game.TitleUrl + "/diary";
                    break;
                case WebApiType.Drawing:
                    baseUrl += game.TitleUrl + "/artwork";
                    break;
                case WebApiType.Discussion:
                    baseUrl += game.TitleUrl + "/topic";
                    break;
                case WebApiType.InGame:
                    baseUrl += game.TitleUrl + "/in_game";
                    break;
                case WebApiType.OldGame:
                    baseUrl += game.TitleUrl + "/old";
                    break;
                case WebApiType.Replies:
                    baseUrl += game.TitleUrl.Replace("title", "posts") + "/replies";
                    break;
                case WebApiType.Posts:
                    baseUrl += game.TitleUrl.Replace("title", "posts");
                    break;
            }

            if (lastPostTime != 0 && (type != WebApiType.Replies || type != WebApiType.Posts))
            {
                // I know it's JSON encoded and it would be better to just decode/encode it.
                // But the service it going away so screw it.
                baseUrl += $"?page_param=%7B%22upinfo%22%3A%22{lastPostTime * -1 }%2C{(int)currentDate}%2C{currentDate}%22%2C%22reftime%22%3A%22{lastPostTime}%22%2C%22order%22%3A%22desc%22%2C%22per_page%22%3A%2250%22%7D ";
            }
            else if (lastPostTime != 0)
            {
                baseUrl += $"?page_param=%7B%22upinfo%22%3A%22{lastPostTime * -1 }%2C{(int)currentDate}%2C{currentDate}%22%2C%22reftime%22%3A%22{lastPostTime}%22%2C%22per_page%22%3A120%2C%22order%22%3A%22asc%22%7D&selected=all";
            }

            var req = new HttpRequestMessage(HttpMethod.Get, baseUrl);
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");
            using (var result = await Client.SendAsync(req))
            using (var content = await result.Content.ReadAsStreamAsync())
            {
                var doc = new HtmlDocument();
                doc.Load(content, System.Text.Encoding.UTF8);

                var posts = ParsePosts(type, doc);
                double postTime = 0;
                if (posts.Any())
                {
                    postTime = -(ReturnEpochTime(posts.Last().PostedDate));
                }

                return new WebApiResponse(currentDate, postTime, posts);
            }
        }

        private List<Post> ParsePosts(WebApiType type, HtmlDocument doc)
        {
            var posts = new List<Post>();

            switch (type)
            {
                case WebApiType.Replies:
                    var replyListNode = doc.DocumentNode.Descendants("ul")
                        .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("list reply-list"));
                    var replyNodes = replyListNode?.ChildNodes.Where(n => n.HasClassName("post") && !n.HasClassName("none"));
                    posts = replyNodes == null ? new List<Post>() : replyNodes.Select(ParsePost).ToList();
                    break;
                case WebApiType.Diary:
                case WebApiType.Drawing:
                case WebApiType.InGame:
                case WebApiType.OldGame:
                    var postListNode = doc.DocumentNode.Descendants("div")
                        .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("list post-list"));

                    var postNodes = postListNode?.ChildNodes.Where(n => n.HasClassName("post") && !n.HasClassName("none"));
                    posts = postNodes == null ? new List<Post>() : postNodes.Select(ParsePost).ToList();
                    break;
                case WebApiType.Discussion:
                    var postListNodeDiscuess = doc.DocumentNode.Descendants("div")
                       .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("list multi-timeline-post-list"));

                    var postNodes2 = postListNodeDiscuess?.ChildNodes.Where(n => n.HasClassName("post") && !n.HasClassName("none"));
                    posts = postNodes2 == null ? new List<Post>() : postNodes2.Select(ParsePost).ToList();
                    break;
            }

            return posts;
        }



        public async Task<GameResponse> GetGameAsync(Game game, string nextPageUrl = "")
        {
            AccessCheck();

            var baseUrl = "https://miiverse.nintendo.net/";
            if (!string.IsNullOrEmpty(nextPageUrl))
            {
                baseUrl += nextPageUrl;
            }
            else
            {
                baseUrl += game.TitleUrl;
            }

            var req = new HttpRequestMessage(HttpMethod.Get, baseUrl);
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using (var result = await Client.SendAsync(req))
            using (var content = await result.Content.ReadAsStreamAsync())
            {
                var doc = new HtmlDocument();
                doc.Load(content, System.Text.Encoding.UTF8);

                var favoriteButton =
                    doc.DocumentNode.Descendants("button")
                        .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("favorite-button"));

                var isFavorite = favoriteButton != null && favoriteButton.GetAttributeValue("class", string.Empty).Contains("checked");

                var textArea =
                    doc.DocumentNode.Descendants("textarea")
                        .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("textarea-text"));

                var canPost = textArea != null;

                var postListNode = doc.DocumentNode.Descendants("div")
                           .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("before-renewal"));

                //var nextPage = postListNode?.GetAttributeValue("data-next-page-url", string.Empty);
                //   var postNodes = postListNode?.ChildNodes.Where(n => n.HasClassName("post") && !n.HasClassName("none"));
                var posts = new List<Post>();
                return new GameResponse(canPost, isFavorite, "", postListNode != null, posts);
            }
        }

        public async Task<CommunityListResponse> GetCommunityGameListAsync(GameSearchList searchOption, GamePlatformSearch platformSearch, int offset)
        {
            AccessCheck();

            var baseUrl = "https://miiverse.nintendo.net/communities/categories/";

            switch (platformSearch)
            {
                case GamePlatformSearch.Nintendo3ds:
                    baseUrl += "3ds";
                    break;
                case GamePlatformSearch.Wiiu:
                    baseUrl += "wiiu";
                    break;
            }
            switch (searchOption)
            {
                case GameSearchList.All:
                    baseUrl += "_all?offset=" + offset;
                    break;
                case GameSearchList.Game:
                    baseUrl += "_game?offset=" + offset;
                    break;
                case GameSearchList.Other:
                    baseUrl += "_other?offset=" + offset;
                    break;
                case GameSearchList.VirtualConsole:
                    baseUrl += "_virtualconsole?offset=" + offset;
                    break;
            }

            var req = new HttpRequestMessage(HttpMethod.Get, baseUrl);
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");
            using (var result = await Client.SendAsync(req))
            using (var content = await result.Content.ReadAsStreamAsync())
            {
                var doc = new HtmlDocument();
                doc.Load(content, System.Text.Encoding.UTF8);
                var gameListNode =
                    doc.DocumentNode.Descendants("ul")
                        .FirstOrDefault(
                            node =>
                                node.GetAttributeValue("class", string.Empty) == "list community-list");
                if (gameListNode == null)
                {
                    return new CommunityListResponse(null);
                }

                var gamesList = gameListNode.Descendants("li");
                var output = new List<Game>();
                foreach (var game in gamesList)
                {
                    var id = game.GetAttributeValue("id", string.Empty);
                    var titleUrl = game.GetAttributeValue("data-href", string.Empty);

                    var communityIconImageNode = game.Descendants("img").FirstOrDefault(n => n.GetAttributeValue("class", string.Empty) == "community-list-cover");
                    var communityListIcon = communityIconImageNode?.GetAttributeValue("src", string.Empty);

                    var iconImg = game.Descendants("img").FirstOrDefault(n => n.GetAttributeValue("class", string.Empty) == "icon");
                    var icon = iconImg?.GetAttributeValue("src", string.Empty);

                    var body =
                        game.Descendants("div")
                            .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty) == "body");

                    var titleNode =
                        body.Descendants("a")
                            .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty) == "title");
                    var title = titleNode?.InnerText;

                    var imageNode = body.Descendants("img").FirstOrDefault();
                    var image = imageNode?.GetAttributeValue("src", string.Empty);
                    var imageFilename = string.Empty;
                    if (!string.IsNullOrEmpty(image))
                    {
                        var uri = new Uri(image);
                        imageFilename = System.IO.Path.GetFileName(uri.LocalPath).Split('?').FirstOrDefault();
                    }

                    var gameTextSpan = body.Descendants("span").LastOrDefault();
                    var gameText = gameTextSpan.InnerText;
                    if (string.IsNullOrEmpty(communityListIcon))
                    {
                        output.Add(new Game(id, title, titleUrl, new Uri(icon), imageFilename, gameText));
                    }
                    else
                    {
                        output.Add(new Game(id, title, titleUrl, new Uri(icon), new Uri(communityListIcon), imageFilename, gameText));
                    }
                }
                return new CommunityListResponse(output);
            }

        }

        public async Task<UserProfileFeedResponse> GetUserProfileFeedAsync(string username, UserProfileFeedType type, int offset = 0)
        {
            // The max per page is 50.
            AccessCheck();

            var baseUrl = $"https://miiverse.nintendo.net/users/{username}/";

            switch (type)
            {
                case UserProfileFeedType.Followers:
                    baseUrl += "followers";
                    break;
                case UserProfileFeedType.Following:
                    baseUrl += "following";
                    break;
                case UserProfileFeedType.Friends:
                    baseUrl += "friends";
                    break;
            }

            if (offset > 0)
            {
                baseUrl += $"?offset={offset}";
            }

            var req = new HttpRequestMessage(HttpMethod.Get, baseUrl);
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using (var result = await Client.SendAsync(req))
            using (var content = await result.Content.ReadAsStreamAsync())
            {
                var friendList = new List<string>();
                var doc = new HtmlDocument();
                doc.Load(content, System.Text.Encoding.UTF8);
                try
                {
                    // NOTE: For the "friends" feed, it may contain people not on Miiverse.
                    // We are only counting Miiverse friends, since those are the only ones we can
                    // parse.
                    var friendListNode = doc.GetElementbyId("friend-list-content");

                    var friendFeedNodes = friendListNode.Descendants("li").Where(node => node.GetAttributeValue("data-href", string.Empty) != string.Empty);

                    if (friendFeedNodes.Any())
                    {
                        foreach (var friendFeed in friendFeedNodes)
                        {
                            friendList.Add(friendFeed.GetAttributeValue("data-href", string.Empty).Replace("/users/", ""));
                        }
                    }
                }
                catch (Exception)
                {
                    // We are going to assume if this fails, that the list is empty.
                    // Most liekly, this is the result of "user-page-no-content"
                    Debug.WriteLine($"Failed to parse for {username}");
                }

                return new UserProfileFeedResponse(username, friendList, type);
            }
        }

        public async Task<UserProfileResponse> GetUserProfileAsync(string username)
        {
            AccessCheck();

            var req = new HttpRequestMessage(HttpMethod.Get, "https://miiverse.nintendo.net/users/" + username);
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");
            using (var result = await Client.SendAsync(req))
            using (var content = await result.Content.ReadAsStreamAsync())
            {
                var doc = new HtmlDocument();
                doc.Load(content, System.Text.Encoding.UTF8);
                var mainNode = doc.GetElementbyId("main-body");

                if (mainNode == null)
                {
                    // Sometimes, the user profile feed with return JSON. Because Nintendo be weird. 
                    // Not gonna throw, gonna assume hidden user for now.
                    return new UserProfileResponse(new User()
                    {
                        ScreenName = username,
                        IsHidden = true,
                        IsError = true
                    });
                }


                var isHiddenNode = mainNode.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty) == "no-content");
                if (isHiddenNode != null)
                {
                    return new UserProfileResponse(new User()
                    {
                        ScreenName = username,
                        IsHidden = true
                    });
                }

                var sidebarImageNode = mainNode.Descendants("img").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("sidebar-cover-image"));
                var sidebarImageUrl = sidebarImageNode?.GetAttributeValue("src", string.Empty);

                var friendCountNode = mainNode.Descendants("span").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("test-friend-count"));
                var friendInnerText = friendCountNode.InnerText.Split('/').First();
                var friendCount = 0;
                Int32.TryParse(friendInnerText, out friendCount);

                var postCountNode = mainNode.Descendants("span").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("test-post-count"));
                var postCount = 0;
                Int32.TryParse(postCountNode.InnerText, out postCount);

                var empathyCountNode = mainNode.Descendants("span").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("test-empathy-count"));
                var empathyCount = 0;
                Int32.TryParse(empathyCountNode.InnerText, out empathyCount);

                var followingCountNode = mainNode.Descendants("span").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("number test-following-count"));
                var followingCount = 0;
                Int32.TryParse(followingCountNode.InnerText, out followingCount);

                var followersCountNode = mainNode.Descendants("span").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("number test-follower-count"));
                var followersCount = 0;
                Int32.TryParse(followersCountNode.InnerText, out followersCount);

                var avatarUrlNode = mainNode.Descendants("img").FirstOrDefault();
                var avatarUri = new Uri(avatarUrlNode?.GetAttributeValue("src", string.Empty));
                var nickNameNode = mainNode.Descendants("a").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty) == "nick-name");
                var nickName = nickNameNode?.InnerText;

                var userNameNode = mainNode.Descendants("p").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty) == "id-name");
                var userName = userNameNode?.InnerText;

                var followButton =
                    mainNode.Descendants("button")
                        .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("follow-button button symbol"));

                // If "none" is contained in the button, then it's hidden, meaning the user is being followed. 
                var isFollowing = followButton == null;

                var userNode = mainNode.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty) == "user-data");
                var countryNode = userNode.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("note"));

                var bioNode = mainNode.Descendants("p").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("js-full-text"));
                if (bioNode == null)
                {
                    bioNode = mainNode.Descendants("p").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("js-truncated-text"));
                }

                var birthdayNode = userNode.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("birthday"));

                var gameSkillNode = userNode.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("game-skill"));
                var gameSkillSpan = gameSkillNode?.Descendants("span").LastOrDefault();
                var gameSkill = gameSkillSpan?.GetAttributeValue("class", string.Empty).Replace("test-game-skill", "").Trim();

                var gameSystemsNode = userNode.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("game data-content"));
                var gameSystems = gameSystemsNode?.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("note"));

                var systems = new List<string>();
                var htmlNodes2 = gameSystems?.Descendants("div");
                if (htmlNodes2 != null)
                {
                    systems.AddRange(htmlNodes2.Select(div => div.GetAttributeValue("class", string.Empty)));
                }

                var favoriteGameGenreNode = userNode.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("favorite-game-genre"));
                var favoriteGameGenre = favoriteGameGenreNode?.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("note"));
                var genres = new List<string>();
                var htmlNodes = favoriteGameGenre?.Descendants("span");
                if (htmlNodes != null)
                {
                    genres.AddRange(htmlNodes.Select(node => node.InnerText));
                }

                return new UserProfileResponse(new User(nickName, userName, avatarUri,
                    countryNode?.InnerText, birthdayNode?.InnerText, gameSkill, systems, genres,
                    bioNode?.InnerText, (int)followersCount, (int)followingCount, friendCount,
                    (int)postCount, (int)empathyCount, sidebarImageUrl));
            }
        }

        public async Task<PostResponse> GetPostAsync(string id)
        {
            AccessCheck();

            var req = new HttpRequestMessage(HttpMethod.Get, "https://miiverse.nintendo.net/posts/" + id);
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");
            using (var result = await Client.SendAsync(req))
            using (var content = await result.Content.ReadAsStreamAsync())
            {
                var doc = new HtmlDocument();
                doc.Load(content, System.Text.Encoding.UTF8);
                var postNode = doc.GetElementbyId("post-content");
                var post = ParsePost(postNode);
                return new PostResponse(post);
            }
        }

        public async Task<UserFeedResponse> GetUserFeedAsync(string username)
        {
            AccessCheck();

            var req = new HttpRequestMessage(HttpMethod.Get, $"https://miiverse.nintendo.net/users/{username}/posts");
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");
            using (var result = await Client.SendAsync(req))
            using (var content = await result.Content.ReadAsStreamAsync())
            {
                var doc = new HtmlDocument();
                doc.Load(content, System.Text.Encoding.UTF8);

                var mainBody = doc.GetElementbyId("main-body");
                var postList = mainBody.Descendants("div").Where(node => node.GetAttributeValue("id", string.Empty).Contains("post-"));
                var posts = postList.Select(ParsePost).ToList();
                return new UserFeedResponse(posts);
            }
        }

        public async Task<ActivityResponse> GetActivityAsync(bool friendsOnly = false)
        {
            AccessCheck();
            var url = "https://miiverse.nintendo.net/activity?fragment=activityfeed";
            if (friendsOnly)
            {
                url += "&filter=friend";
            }
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");
            using (var result = await Client.SendAsync(req))
            using (var content = await result.Content.ReadAsStreamAsync())
            {
                var doc = new HtmlDocument();
                doc.Load(content, System.Text.Encoding.UTF8);

                var postsNode = doc.GetElementbyId("main-body").GetElementByClassName("post-list").ChildNodes.Where(n => n.HasClassName("post") && !n.HasClassName("none"));
                var posts = postsNode.Select(ParsePost).ToList();
                return new ActivityResponse(posts);
            }
        }

        public Task SignOutAsync()
        {
            AccessCheck();
            return Client.HeadAsync(string.Format(MiiverseConstantValues.MIIVERSE_SIGN_OUT_URI_STRING, ClientID));
        }

        private void AccessCheck()
        {
            if (!_isEnabled)
            {
                throw new Exception();
            }
        }

        private double ReturnEpochTime(DateTime postedDate)
        {
            var epoch = postedDate - new DateTime(1970, 1, 1);
            return epoch.TotalSeconds;
        }

        public void Dispose()
        {
            Client.Dispose();
            _isEnabled = false;
        }

        public string UserName { get; set; }
        public string ClientID { get; set; }
        public string SessionValue { get; set; }

        private HttpClient Client { get; set; }

        private Post ParsePost(HtmlNode postNode)
        {
            DateTime dateTime;
            try
            {
                var timestampAnchorNode = postNode.Descendants("p").Where(n => n.GetAttributeValue("class", string.Empty).Contains("timestamp")).FirstOrDefault();
                if (timestampAnchorNode == null)
                    timestampAnchorNode = postNode.Descendants("a").Where(n => n.GetAttributeValue("class", string.Empty).Contains("timestamp")).FirstOrDefault();
                dateTime = this.ConvertRelativeTime(timestampAnchorNode.InnerText.ToLower().Replace("&middot;", "").Replace("spoilers", "").Trim());
            }
            catch (Exception)
            {
                // Set to min if we fail.
                dateTime = DateTime.MinValue;
            }
            HtmlNode postContentNode;
            try
            {
                // Post List Page
                var bodyNode = postNode.Descendants("div").Where(n => n.GetAttributeValue("class", string.Empty).Contains("body")).FirstOrDefault();
                postContentNode = bodyNode.GetElementByClassName("post-content");
            }
            catch (InvalidOperationException)
            {
                // Individual Post Page
                postContentNode = postNode.Descendants("div").Where(n => n.GetAttributeValue("class", string.Empty).Contains("body")).FirstOrDefault();
            }


            var postTopicCategory = postNode.Descendants("a").Where(n => n.GetAttributeValue("class", string.Empty).Contains("post-topic-category")).FirstOrDefault();
            string topic = postTopicCategory != null ? postTopicCategory.InnerText : "";

            var isReply = false;
            var postMetaNode = postContentNode.Descendants("div").Where(n => n.GetAttributeValue("class", string.Empty).Contains("post-meta")).FirstOrDefault();
            if (postMetaNode == null)
            {
                isReply = true;
                postMetaNode = postContentNode.Descendants("div").Where(n => n.GetAttributeValue("class", string.Empty).Contains("reply-meta")).FirstOrDefault();
            }

            var id = isReply ? postNode.Id.Substring(6) : postNode.Id.Substring(5);

            var isDeletedNode = postContentNode.Descendants("p").Where(n => n.GetAttributeValue("class", string.Empty).Contains("deleted-message")).FirstOrDefault();
            if (isDeletedNode != null)
            {
                return new Post(id, true);
            }

            uint replyCount = 0;
            if (!isReply)
            {
                replyCount = postMetaNode.GetElementByClassName("reply").GetElementByClassName("reply-count").GetInnerTextAsUInt32();
            }
            var empathyCount = postMetaNode.GetElementByClassName("empathy").GetElementByClassName("empathy-count").GetInnerTextAsUInt32();
            var isPlayed = postMetaNode.GetElementsByClassName("played").Count() != 0;
            var isSpoiler = postNode.HasClassName("hidden");

            string text = null;
            Uri imageUri = null;
        
            var textNodes = postContentNode.GetElementsByClassName(isReply ? "reply-content-text" : "post-content-text");
            var isImagePost = !textNodes.Any();
            if (isImagePost)
            {
                imageUri = postContentNode.GetElementByClassName(isReply ? "reply-content-memo" : "post-content-memo").GetImageSource();
            }
            else
            {
                text = textNodes.Single().InnerText;
            }

            var tagType = TagType.None;
            var tagID = string.Empty;
            var tag = string.Empty;
            postContentNode.ChildNodes.MatchClassName("post-tag",
                some: n =>
                {
                    var hrefText = n.GetAttributeValue("href", string.Empty);
                    var questionMarkIndex = hrefText.IndexOf('?');
                    var equalMarkIndex = hrefText.IndexOf('=');
                    var tagTypeText = hrefText.Substring(questionMarkIndex + 1, equalMarkIndex - questionMarkIndex - 1);
                    if (tagTypeText == "official_tag_id")
                    {
                        tagType = TagType.Official;
                        tagID = hrefText.Substring(equalMarkIndex + 1);
                        tag = n.InnerText;
                    }
                    else if (tagTypeText == "topic_tag_id")
                    {
                        tagType = TagType.Topic;
                        tagID = hrefText.Substring(equalMarkIndex + 1);
                        tag = n.InnerText;
                    }
                });

            Uri screenShotUri = null;
            postContentNode.ChildNodes.MatchClassName("screenshot-container",
                some: n => screenShotUri = n.GetImageSource());

            var userNameAnchorNode = postNode.Descendants("p").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty) == "user-name").FirstChild;
            var userName = userNameAnchorNode.GetAttributeValue("href", string.Empty).Substring(7);
            var screenName = userNameAnchorNode.InnerText;
            HtmlNode userIconContainer;
            try
            {
                userIconContainer = postNode.GetElementByClassName("icon-container");
            }
            catch (Exception)
            {
                userIconContainer = postNode.Descendants("a").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty) == "icon-container");
            }
            var userIconUri = userIconContainer.Descendants("img").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty) == "icon").GetAttributeValue("src", "");
            var feeling = FeelingTypeHelpers.DetectFeelingTypeFromIconUri(new Uri(userIconUri));
            var normalUserIconUri = FeelingTypeHelpers.GetNormalFaceIconUri(new Uri(userIconUri), feeling);

            HtmlNode communityAnchorNode = communityAnchorNode = postNode.Descendants("a").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty) == "test-community-link");
            if (communityAnchorNode == null)
            {
                var testNode = postNode.Descendants("h1").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty) == "community-container-heading");
                if (testNode != null)
                {
                    communityAnchorNode = testNode.Descendants("a").FirstOrDefault();
                }
            }

            ulong titleID = 0;
            ulong communityID = 0;
            Uri communityIconUri = new Uri("https://miiverse.nintendo.com");
            string communityName = "";

            if (communityAnchorNode != null)
            {
                var communityIconImageNode = communityAnchorNode.GetElementByTagName("img");
                var comInfo = communityAnchorNode.GetAttributeValue("href", string.Empty).Substring(1).Split('/');
                titleID = Convert.ToUInt64(comInfo[1]);
                communityID = Convert.ToUInt64(comInfo[2]);
                communityIconUri = communityAnchorNode.GetImageSource();
                communityName = communityAnchorNode.InnerText;
            }



            var acceptNode = postNode.Descendants("div").Where(n => n.GetAttributeValue("class", string.Empty).Contains("test-topic-answer-accepting-status")).FirstOrDefault();
            bool accept = false;
            if (acceptNode != null)
            {
                var status = Convert.ToInt32(acceptNode.GetAttributeValue("data-test-accepting-status", string.Empty));
                accept = status == 1;
            }

            if (isImagePost)
            {
                return new Post(
                    id,
                    accept,
                    topic,
                    dateTime,
                    new PostTag(tagType, tagID, tag),
                    imageUri,
                    replyCount,
                    empathyCount,
                    isPlayed,
                    isSpoiler,
                    screenShotUri,
                    new PostUser(userName, screenName, normalUserIconUri),
                    feeling,
                    new PostCommunity(titleID, communityID, communityName, communityIconUri));
            }

            return new Post(
                id,
                accept,
                topic,
                dateTime,
                new PostTag(tagType, tagID, tag),
                text,
                replyCount,
                empathyCount,
                isPlayed,
                isSpoiler,
                screenShotUri,
                new PostUser(userName, screenName, normalUserIconUri),
                feeling,
                new PostCommunity(titleID, communityID, communityName, communityIconUri));
        }

        private DateTime ConvertRelativeTime(string input)
        {
            DateTime result = DateTime.MinValue;
            int minutesMultiplier = 0;

            if (input.Contains("less than a minute ago"))
            {
                return DateTime.UtcNow;
            }

            if (input.Contains("minute"))
                minutesMultiplier = 1;
            else
                if (input.Contains("hour"))
                minutesMultiplier = 60;
            else
                    if (input.Contains("day"))
                minutesMultiplier = 1440;
            else
            {
                try
                {
                    result = DateTime.Parse(input);
                }
                catch (Exception)
                {
                    throw new Exception("Couldn't parse time format");
                }
            }

            string numberStr = input.Split(' ')[0];
            int number;
            if (int.TryParse(numberStr, out number))
                result = DateTime.Now.AddMinutes(-number * minutesMultiplier);
            // We assume Now instead of UTC, because the site is configured for your local
            Debug.WriteLine($"Input: {input} - Result: {result} - UTCNow: {DateTime.UtcNow}");
            // Now some of you may be wondering. Why -9?
            // Great question... The timestamps given on the web forum are encoded, from what I can tell,
            // To the users region. That's done on the server and passed down. I'm not 100% on which timezone it's set to,
            // but when looking at the results from the website, it seemed to always be 9 hours off? And it wasn't exactly UTC.
            // So for now, it's this. Until I think of something better or see how other accounts handle it.
            return result.AddHours(-9);
        }
    }
}
