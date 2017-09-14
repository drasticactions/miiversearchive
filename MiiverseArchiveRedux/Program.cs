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
using MiiverseArchive.Tools.Constants;

namespace MiiverseArchiveRedux
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
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
                    var filepath = $"{post.ImageUri.Segments[1]}{Path.GetFileName(post.ImageUri.ToString())}.png";
                    if (!File.Exists(filepath))
                    {
                        Console.WriteLine($"Downloading {post.ID}.png");
                        Directory.CreateDirectory($"{post.ImageUri.Segments[1]}");
                        webClient.DownloadFile(post.ImageUri, filepath);
                    }
                }
                catch (Exception ex)
                {
                    // If we fail, append to failed.txt
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
                Console.WriteLine($"Offset - {offset}");
                var result = await ctx.GetUserProfileFeedAsync(screenname, type, offset);
                screennameList.AddRange(result.ResultScreenNames);
                if (!result.ResultScreenNames.Any() || type == UserProfileFeedType.Friends)
                {
                    return new UserProfileFeedResponse(screenname, screennameList, type);
                }

                offset += result.ResultScreenNames.Count();
            }
        }

        static async Task MainAsync(string[] args)
        {            
            //using (var db2 = new LiteDatabase("fulldb.db"))
            //{
            //    var posts2 = db2.GetCollection<Post>("posts");
            //    var allPosts = posts2.Find(Query.All()).Where(node => node.Text != null && node.Text.ToLower().Contains("trump")).ToList();
            //    //Parallel.For(0, allPosts.Count(), index =>
            //    //{
            //    //    var game = allPosts[index];
            //    //    Console.WriteLine($"{game.ID}");
            //    //    using (var webClient = new WebClient())
            //    //    {
            //    //        Directory.CreateDirectory($"{game.ImageUri.Segments[1]}");
            //    //        webClient.DownloadFile(game.ImageUri, $"{game.ImageUri.Segments[1]}" + Path.GetFileName(game.ImageUri.ToString()) + ".png");
            //    //    }
            //    //});
            //}

            var filename = args.Length > 0 ? args[0] : "";
            var gameId = args.Length > 2 ? args[2] : "";
            var webType = args.Length > 1 ? (WebApiType)Convert.ToInt32(args[1]) : WebApiType.Diary;
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
            var viewRegion = ViewRegion.America;

            var ctx = oauthClient.Authorize(token, new NintendoNetworkAuthenticationToken(userName, password), "en-US", viewRegion).GetAwaiter().GetResult();

            // TODO: Figure out a way to automate archiving game/user data.
            // Hardcoding this for testing...
            Console.WriteLine("-----------");

            //var testGame = ctx.GetCommunityGameListAsync(GameSearchList.All, GamePlatformSearch.Wiiu, 300).GetAwaiter().GetResult();

            //using (var db = new LiteDatabase("gamelist.db"))
            //using (var db2 = new LiteDatabase("gamelistrelated.db"))
            //{
            //    var games = db.GetCollection<Game>("gamelist");
            //    var gamesRelated = db2.GetCollection<CommunityItem>("gamelistrelated");
            //    var allGames = games.Find(Query.All()).ToList();
            //    foreach (var game in allGames)
            //    {
            //        var testingbesting = game.TitleUrl.Split('/');
            //        var result = await ctx.GetRelatedCommunityGameListAsync(testingbesting[2]);
            //        if (result.Games == null) continue;
            //        foreach (var test in result.Games)
            //        {
            //            test.ViewRegion = game.ViewRegion;
            //            test.Type = game.Type;
            //            Console.WriteLine($"{test.Title}");
            //            gamesRelated.Insert(test);
            //        }
            //    }

            //    var allGamesRelated = gamesRelated.Find(Query.All()).ToList();
            //    File.WriteAllText("gamelistrelated.json", JsonConvert.SerializeObject(allGamesRelated, Formatting.Indented));
            //    File.WriteAllText("gamelistrelated-notmain.json", JsonConvert.SerializeObject(allGamesRelated.Where(node => node.CommunityBadge != "Main Community"), Formatting.Indented));
            //}

            //var gameList = JsonConvert.DeserializeObject<List<Game>>(File.ReadAllText($"{filename}.json"));
            //var communityItemList = JsonConvert.DeserializeObject<List<CommunityItem>>(File.ReadAllText($"{filename}.json"));

            //var startIndex = 0;

            //if (gameId != "")
            //{
            //    var game = gameList.FirstOrDefault(node => node.Id == gameId);
            //    if (game != null)
            //        startIndex = gameList.IndexOf(game);
            //}

            //var filenametest = "";

            //switch (webType)
            //{
            //    case WebApiType.Diary:
            //        filenametest = "diary";
            //        break;
            //    case WebApiType.Discussion:
            //        filenametest = "discussion";
            //        break;
            //    case WebApiType.Drawing:
            //        filenametest = "drawing";
            //        break;
            //    case WebApiType.InGame:
            //        filenametest = "ingame";
            //        break;
            //    case WebApiType.OldGame:
            //        filenametest = "oldgame";
            //        break;
            //    case WebApiType.Posts:
            //        filenametest = "posts";
            //        break;
            //    case WebApiType.Replies:
            //        filenametest = "replies";
            //        break;
            //    case WebApiType.Special:
            //        filenametest = "special";
            //        break;
            //}

            //for (var i = startIndex; i < gameList.Count; i++)
            //{
            //    var game = gameList[i];
            //    Console.WriteLine($"Game - {game.Title} - {game.Id} - {game.TitleUrl}");
            //    using (var db = new LiteDatabase($"{game.Id}-{filenametest}.db"))
            //    {
            //        var posts = db.GetCollection<Post>("posts");
            //        var allPosts = posts.Find(Query.All());

            //        double nextPost = 0;
            //        double nextPostMinutes = 0;
            //        DateTime time = DateTime.UtcNow;
            //        var nextPageUrl = "";
            //        if (allPosts.Any())
            //        {
            //            if (File.Exists($"{game.Id}-nextUrl.txt")) {
            //                nextPageUrl = File.ReadAllLines($"{game.Id}-nextUrl.txt").Last();
            //            }
            //        }

            //        var countInserted = 0;

            //        while (true)
            //        {
            //            var response = await ctx.GetWebApiResponse(game, webType, nextPageUrl);

            //            if (response.Posts == null || !response.Posts.Any())
            //            {
            //                // We're done! Time to wrap it up.
            //                Console.WriteLine(Environment.NewLine);
            //                break;
            //            }

            //            foreach (var post in response.Posts)
            //            {
            //                // Upsert either "inserts" a new post, or "Updates" an existing post
            //                // I use this so, in case the same post shows up again,
            //                // we can continue without the program throwing an error.
            //                post.GameCommunity = communityItemList[i];
            //                posts.Upsert(post);
            //            }


            //            Console.Write("\rNext Post Time: {0} Total Inserted: {1}", nextPost, posts.Count());

            //            if (response.NextPageUrl == nextPageUrl || response.NextPageUrl == "")
            //            {
            //                // We're done! Time to wrap it up.
            //                Console.WriteLine(Environment.NewLine);
            //                break;
            //            }

            //            File.AppendAllText($"{game.Id}-nextUrl.txt", response.NextPageUrl + Environment.NewLine);
            //            countInserted = posts.Count();
            //            nextPageUrl = response.NextPageUrl;
            //        }
            //    }
            //}

            //var userIds = File.ReadAllLines(filename).ToList();
            //using (var db = new LiteDatabase($"{filename}.db"))
            //{
            //    var users = db.GetCollection<UserFriend>("friends");
            //    var allUsers = users.Find(Query.All()).ToList();
            //    var startingCount = 0;
            //    if (allUsers.Any())
            //    {
            //        var lastUser = allUsers.Last().ScreenName;
            //        startingCount = userIds.IndexOf(lastUser);
            //    }
            //    for (var i = startingCount; i <= userIds.Count(); i++)
            //    {
            //        var user = userIds[i];
            //        Console.WriteLine($"Getting Friends for {user}");
            //        var friendList = await GetFeed(ctx, user, UserProfileFeedType.Friends);
            //        Console.WriteLine($"{user} Friends: {friendList.ResultScreenNames.Count()}");

            //        var newFriendsList = friendList.ResultScreenNames.Where(n => !allUsers.Any(o => o.ProfileFeedType == UserProfileFeedType.Friends && o.ScreenName == user && o.AcquaintanceScreenName == n)).ToList();
            //        foreach (var friend in newFriendsList)
            //        {
            //            users.Insert(new UserFriend(user, friend, UserProfileFeedType.Friends));
            //        }

            //        Console.WriteLine($"Getting Followers for {user}");
            //        var followerList = await GetFeed(ctx, user, UserProfileFeedType.Followers);
            //        var newFollowersList = followerList.ResultScreenNames.Where(n => !allUsers.Any(o => o.ProfileFeedType == UserProfileFeedType.Followers && o.ScreenName == user && o.AcquaintanceScreenName == n)).ToList();

            //        Console.WriteLine($"{user} Followers: {followerList.ResultScreenNames.Count()}");
            //        foreach (var friend in newFollowersList)
            //        {
            //            users.Insert(new UserFriend(user, friend, UserProfileFeedType.Followers));
            //        }

            //        Console.WriteLine($"Getting Following for {user}");
            //        var FollowingList = await GetFeed(ctx, user, UserProfileFeedType.Following);
            //        var newFollowingList = FollowingList.ResultScreenNames.Where(n => !allUsers.Any(o => o.ProfileFeedType == UserProfileFeedType.Following && o.ScreenName == user && o.AcquaintanceScreenName == n)).ToList();

            //        Console.WriteLine($"{user} Following: {FollowingList.ResultScreenNames.Count()}");
            //        foreach (var friend in newFollowingList)
            //        {
            //            users.Insert(new UserFriend(user, friend, UserProfileFeedType.Following));
            //        }

            //    }
            //}

            //return;

            // GET SPLATOON POSTS

            //using (var db = new LiteDatabase("gamelist.db"))
            //using (var db2 = new LiteDatabase("postlist.db"))
            //{
            //    var games = db.GetCollection<Game>("gamelist");
            //    var posts = db2.GetCollection<Post>("postList");
            //    var allGames = games.Find(Query.All()).ToList();
            //    var allPosts = posts.Find(Query.All()).ToList();

            //    double nextPost = 0;
            //    double nextPostMinutes = 0;

            //    DateTime time = DateTime.UtcNow;
            //    if (allPosts.Any())
            //    {
            //        var post = allPosts.OrderBy(n => n.PostedDate).First();
            //        var secondsSinceEpoch = post.PostedDate.ToUnixTime();
            //        nextPost = -(secondsSinceEpoch);
            //        time = post.PostedDate;
            //    }

            //    // Get Japanese version of Splatoon
            //    var splatoon = allGames.First(n => n.Title.Contains("Splatoon") && n.ViewRegion == ViewRegion.Japan);

            //    var countInserted = 0;
            //    while (true)
            //    {
            //        var splatoonGameDrawingResponse = await ctx.GetWebApiResponse(splatoon, MiiverseArchive.Tools.Constants.WebApiType.Drawing, nextPost);
            //        if (splatoonGameDrawingResponse.Posts == null)
            //        {
            //            // We're done! Time to wrap it up.
            //            return;
            //        }

            //        foreach (var post in splatoonGameDrawingResponse.Posts)
            //        {
            //            // Upsert either "inserts" a new post, or "Updates" an existing post
            //            // I use this so, in case the same post shows up again,
            //            // we can continue without the program throwing an error.
            //            posts.Upsert(post);
            //        }

            //        // We can't get exact times for posts, only relative times like "About an hour".
            //        // Because of that, we can't rely on using the last post to set where we start from.
            //        // Because we could end up just getting the same last hour of posts. So instead.
            //        // Keep substracting 15 minutes from the current time. That should result in getting newer posts.

            //        TimeSpan epoch;
            //        time = splatoonGameDrawingResponse.Posts.Last().PostedDate;
            //        if (countInserted != posts.Count())
            //        {
            //            epoch = time - new DateTime(1970, 1, 1);
            //        }
            //        else
            //        {
            //            nextPostMinutes = nextPostMinutes + 100;
            //            epoch = time.AddMinutes(-1 * nextPostMinutes) - new DateTime(1970, 1, 1);
            //        }
            //        double secondsSinceEpoch = epoch.TotalSeconds;
            //        nextPost = -(secondsSinceEpoch);
            //        Console.WriteLine("Next Post Time: {0} Total Inserted: {1}", nextPost, posts.Count());
            //        countInserted = posts.Count();
            //    }
            //}

            //using (var db = new LiteDatabase("gamelist.db"))
            //{
            //    var posts = db.GetCollection<Game>("gamelist");
            //    var allPosts = posts.Find(Query.All()).ToList();
            //    var allPostsTip = allPosts.Where(node => node.IconUri.ToString().Contains("/tip/")).Count();
            //    var postIndex = allPosts.IndexOf(allPosts.First(n => n.Title == "Excave III : Tower of Destiny"));

            //    // New, Parallel, l337 way!

            //    Parallel.For(2000, allPosts.Count(), index =>
            //    {
            //        var game = allPosts[index];
            //        Console.WriteLine($"{game.Title}");
            //        using (var webClient = new WebClient())
            //        {
            //            if (game.CommunityListIcon != null)
            //            {
            //                Directory.CreateDirectory($"{game.CommunityListIcon.Segments[1]}");
            //                webClient.DownloadFile(game.CommunityListIcon, $"{game.CommunityListIcon.Segments[1]}" + Path.GetFileName(game.CommunityListIcon.ToString()) + ".png");
            //            }

            //            Directory.CreateDirectory($"{game.IconUri.Segments[1]}");
            //            webClient.DownloadFile(game.IconUri, $"{game.IconUri.Segments[1]}" + Path.GetFileName(game.IconUri.ToString()) + ".png");
            //        }
            //    });

            //    ////Old, terrible way
            //    //foreach (var game in allPosts)
            //    //{
            //    //    Console.WriteLine($"{game.Title}");
            //    //    if (game.CommunityListIcon != null)
            //    //    {
            //    //        Directory.CreateDirectory($"{game.CommunityListIcon.Segments[1]}");
            //    //        webClient.DownloadFile(game.CommunityListIcon, $"{game.CommunityListIcon.Segments[1]}" + Path.GetFileName(game.CommunityListIcon.ToString()) + ".png");
            //    //    }

            //    //    Directory.CreateDirectory($"{game.IconUri.Segments[1]}");
            //    //    webClient.DownloadFile(game.IconUri, $"{game.IconUri.Segments[1]}" + Path.GetFileName(game.IconUri.ToString()) + ".png");
            //    //}
            //}

            // GET GAME LIST

            //using (var db = new LiteDatabase("gamelist-test.db"))
            //{
            //    var posts = db.GetCollection<Game>("gamelist");
            //    var allPosts = posts.Find(Query.All());
            //    var offset = 0;

            //    Console.WriteLine("Getting Nintendo3DS Game List");
            //    while (true)
            //    {
            //        var communityList = await ctx.GetCommunityGameListAsync(GameSearchList.All, GamePlatformSearch.Nintendo3ds, offset);
            //        if (communityList.Games == null)
            //        {
            //            // We're done! Time to wrap it up.
            //            break;
            //        }

            //        foreach (var game in communityList.Games)
            //        {
            //            var test = posts.FindById(game.Id);
            //            if (test == null)
            //            {
            //                game.ViewRegion = viewRegion;
            //                posts.Insert(game);
            //            }
            //            else
            //            {
            //                // Game exists in database, so say that it's a world release.
            //                game.ViewRegion = ViewRegion.World;
            //                posts.Upsert(game);
            //            }
            //        }
            //        Console.WriteLine($"{posts.Count()}");
            //        offset = offset + 30;
            //    }

            //    offset = 0;

            //    Console.WriteLine("Getting WiiU Game List");
            //    while (true)
            //    {
            //        var communityList = await ctx.GetCommunityGameListAsync(GameSearchList.All, GamePlatformSearch.Wiiu, offset);
            //        if (communityList.Games == null)
            //        {
            //            // We're done! Time to wrap it up.
            //            break;
            //        }

            //        foreach (var game in communityList.Games)
            //        {
            //            var test = posts.FindById(game.Id);
            //            if (test == null)
            //            {
            //                game.ViewRegion = viewRegion;
            //                posts.Insert(game);
            //            }
            //            else
            //            {
            //                // Game exists in database, so say that it's a world release.
            //                game.ViewRegion = ViewRegion.World;
            //                posts.Upsert(game);
            //            }
            //        }
            //        Console.WriteLine($"{posts.Count()}");
            //        offset = offset + 30;
            //    }
            //}
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

    public static class ListExtensions
    {
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
