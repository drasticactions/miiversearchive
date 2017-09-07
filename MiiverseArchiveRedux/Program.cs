using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using MiiverseArchive.Entities.Community;
using MiiverseArchive.Entities.Token;
using MiiverseArchive.Managers;
using System.Threading.Tasks;
using System.Net;
using MiiverseArchive.Entities.Post;
using System.IO;
using MiiverseArchive.Entities.Response;
using Newtonsoft.Json;
using MiiverseArchive.Entities.User;
using MiiverseArchive.Context;

namespace MiiverseArchiveRedux
{
    class Program
    {
        static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
            //DownloadAsync().GetAwaiter().GetResult();
        }

        static async Task DownloadAsync()
        {
            using (var db = new LiteDatabase("drawing.db"))
            {
                var posts = db.GetCollection<Post>("drawingcollection");
                var allPosts = posts.Find(Query.All());
                Console.WriteLine($"Post Count: {allPosts.Count()}");
                Directory.CreateDirectory("images");
                var webClient = new WebClient();
                DownloadDrawings(webClient, allPosts);
            }
        }

        static private void DownloadDrawings(WebClient webClient, IEnumerable<Post> allPosts)
        {
            foreach (var post in allPosts)
            {
                try
                {
                    if (!File.Exists($"images\\{post.ID}.png"))
                    {
                        Console.WriteLine($"Downloading {post.ID}.png");
                        webClient.DownloadFile(post.ImageUri, $"images\\{post.ID}.png");
                    }
                    else
                    {
                        //Console.WriteLine($"images\\{post.ID}.png exists!");
                    }
                }
                catch (Exception)
                {
                    File.AppendAllText("failed.txt", post.ImageUri.ToString() + Environment.NewLine);
                }
            }
        }

        static async Task<UserProfileFeedResponse> GetFeed(MiiverseContext ctx, string screenname, UserProfileFeedType type)
        {
            var offset = 0;
            var screennameList = new List<string>();
            while (true)
            {
                var result = await ctx.GetUserProfileFeedAsync(screenname, type, offset);
                screennameList.AddRange(result.ResultScreenNames);
                if (!result.ResultScreenNames.Any() || type == UserProfileFeedType.Friends)
                {
                    return new UserProfileFeedResponse(screenname, screennameList, type);
                }

                offset += result.ResultScreenNames.Count();
            }
        }

        static async Task MainAsync()
        {
            var testing = DateTime.Now.AddDays(30).ToUnixTime();
            var oauthClient = new MiiverseOAuthClient();
            var token = oauthClient.GetTokenAsync().GetAwaiter().GetResult();
            Console.WriteLine("client_id:\t{0}", token.ClientID);
            Console.WriteLine("response_type:\t{0}", token.ResponseType);
            Console.WriteLine("redirect_uri:\t{0}", token.RedirectUri);
            Console.WriteLine("state:\t{0}", token.State);
            Console.WriteLine("-----------");

            Console.WriteLine("Please input your NNID.");
            Console.Write("Username: ");
            var userName = Console.ReadLine();
            Console.Write("Password: ");
            var password = GetPassword();
            Console.WriteLine("");
            Console.WriteLine("-----------");

            var ctx = oauthClient.Authorize(token, new NintendoNetworkAuthenticationToken(userName, password), "en-US", ViewRegion.Japan).GetAwaiter().GetResult();

            // TODO: Figure out a way to automate archiving game/user data.
            // Hardcoding this for testing...
            Console.WriteLine("-----------");
            //Console.WriteLine("Archiving: Splatoon (Drawing)");
            //Console.WriteLine("Archiving: Game Lists");
            Console.WriteLine("Parsing Test: Users List");
            Console.WriteLine("-----------");
            var gameList = ctx.GetCommunityGameListAsync(GameSearchList.All, GamePlatformSearch.Wiiu, 300).GetAwaiter().GetResult();

            var userIds = File.ReadAllLines("miiverse_users.txt");
            using (var db = new LiteDatabase("friends.db"))
            {
                var users = db.GetCollection<UserFriend>("friends");
                var allUsers = users.Find(Query.All());
                var startingCount = 0;
                if (allUsers.Any())
                {
                    var userNames = allUsers.Select(n => n.ScreenName).ToList();
                    startingCount = userNames.IndexOf(allUsers.Last().ScreenName) + 1;
                }
                for (var i = startingCount; i <= userIds.Count(); i++)
                {
                    var user = userIds[i];
                    var friendList = await GetFeed(ctx, user, UserProfileFeedType.Friends);
                    Console.WriteLine($"{user} Friends: {friendList.ResultScreenNames.Count()}");

                    var newFriendsList = friendList.ResultScreenNames.Where(n => !allUsers.Any(o => o.ProfileFeedType == UserProfileFeedType.Friends && o.ScreenName == user && o.AcquaintanceScreenName == n));
                    foreach(var friend in newFriendsList)
                    {
                        users.Insert(new UserFriend(user, friend, UserProfileFeedType.Friends));
                    }

                    var followerList = await GetFeed(ctx, user, UserProfileFeedType.Followers);
                    var newFollowersList = friendList.ResultScreenNames.Where(n => !allUsers.Any(o => o.ProfileFeedType == UserProfileFeedType.Followers && o.ScreenName == user && o.AcquaintanceScreenName == n));

                    Console.WriteLine($"{user} Followers: {followerList.ResultScreenNames.Count()}");
                    foreach (var friend in newFollowersList)
                    {
                        users.Insert(new UserFriend(user, friend, UserProfileFeedType.Followers));
                    }

                    var FollowingList = await GetFeed(ctx, user, UserProfileFeedType.Following);
                    var newFollowingList = friendList.ResultScreenNames.Where(n => !allUsers.Any(o => o.ProfileFeedType == UserProfileFeedType.Following && o.ScreenName == user && o.AcquaintanceScreenName == n));

                    Console.WriteLine($"{user} Following: {FollowingList.ResultScreenNames.Count()}");
                    foreach (var friend in newFollowingList)
                    {
                        users.Insert(new UserFriend(user, friend, UserProfileFeedType.Following));
                    }

                }
            }


            //using (var db = new LiteDatabase("users.db"))
            //{
            //    var users = db.GetCollection<User>("users");
            //    var allUsers = users.Find(Query.All());
            //    var startingCount = 0;
            //    if (allUsers.Any())
            //    {
            //        var userNames = allUsers.Select(n => n.ScreenName).ToList();
            //        startingCount = userNames.IndexOf(allUsers.Last().ScreenName) + 1;
            //    }

            //    for (var i = startingCount; i <= userIds.Count(); i++)
            //    {
            //        var userEntity = await ctx.GetUserProfileAsync(userIds[i]);
            //        Console.WriteLine("Name: {0}", userEntity.User.Name);
            //        Console.WriteLine("ScreenName: {0}", userEntity.User.ScreenName);
            //        Console.WriteLine("Following: {0}", userEntity.User.FollowingCount);
            //        Console.WriteLine("FollowerCount: {0}", userEntity.User.FollowerCount);
            //        Console.WriteLine("FriendsCount: {0}", userEntity.User.FriendsCount);
            //        Console.WriteLine("TotalPosts: {0}", userEntity.User.TotalPosts);
            //        Console.WriteLine("EmpathyCount: {0}", userEntity.User.EmpathyCount);
            //        Console.WriteLine("Bio: {0}", userEntity.User.Bio);
            //        Console.WriteLine("IconUri: {0}", userEntity.User.IconUri);
            //        Console.WriteLine("Country: {0}", userEntity.User.Country);
            //        Console.WriteLine("Birthday: {0}", userEntity.User.Birthday);
            //        Console.WriteLine("Birthday Hidden: {0}", userEntity.User.IsBirthdayHidden);
            //        Console.WriteLine("Sidebar Image: {0}", userEntity.User.SidebarCoverUrl);
            //        if (userEntity.User.GameSystem != null)
            //        {
            //            foreach (var gameSystem in userEntity.User.GameSystem)
            //            {
            //                Console.WriteLine("GameSystem: {0}", gameSystem);
            //            }
            //        }
            //        if (userEntity.User.FavoriteGameGenre != null)
            //        {
            //            foreach (var gameGenre in userEntity.User.FavoriteGameGenre)
            //            {
            //                Console.WriteLine("GameGenre: {0}", gameGenre);
            //            }
            //        }
            //        Console.WriteLine("GameSkill: {0}", userEntity.User.GameSkill);
            //        Console.WriteLine("-----------");

            //        userEntity.User.Id = i;
            //        users.Upsert(userEntity.User);
            //    }
            //}
            //var postTest = await ctx.GetPostAsync("AYIHAAAEAABEVRTp4iPDww");
            //var repliesTest = await ctx.GetPostResponse("AYIHAAAEAABEVRTp4iPDww", MiiverseArchive.Tools.Constants.WebApiType.Replies);
            //var gameTest = new Game("community-14866558073673172583", "Splatoon", "/titles/14866558073673172576/14866558073673172583", new Uri("https://d3esbfg30x759i.cloudfront.net/cnj/zlCfzTYBRmcD4DW6Q5"), "platform-tag-wiiu.png", "Wii U Games");
        }

        #region Helpers

        private static string GetPassword()
        {
            var inputList = new List<char>();

            ConsoleKeyInfo info;
            while ((info = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (info.Key == ConsoleKey.Backspace)
                {
                    if (inputList.Count != 0)
                    {
                        inputList.RemoveAt(inputList.Count - 1);

                        var indexMinusOne = Console.CursorLeft - 1;
                        Console.SetCursorPosition(indexMinusOne, Console.CursorTop);
                        Console.Write(" ");
                        Console.SetCursorPosition(indexMinusOne, Console.CursorTop);
                    }
                    continue;
                }

                //Console.Write("*");
                inputList.Add(info.KeyChar);
            }

            return string.Concat(inputList);
        }

        #endregion
    }
}
