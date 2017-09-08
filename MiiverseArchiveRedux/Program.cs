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

            

            using (var db = new LiteDatabase("gamelist.db"))
            {
                var posts = db.GetCollection<Game>("gamelist");
                var allPosts = posts.Find(Query.All()).ToList();
                var allPostsTip = allPosts.Where(node => node.IconUri.ToString().Contains("/tip/")).Count();
                var postIndex = allPosts.IndexOf(allPosts.First(n => n.Title == "Excave III : Tower of Destiny"));

                // New, Parallel, l337 way!

                Parallel.For(2000, allPosts.Count(), index => {
                    var game = allPosts[index];
                    Console.WriteLine($"{game.Title}");
                    using (var webClient = new WebClient())
                    {
                        if (game.CommunityListIcon != null)
                        {
                            Directory.CreateDirectory($"{game.CommunityListIcon.Segments[1]}");
                            webClient.DownloadFile(game.CommunityListIcon, $"{game.CommunityListIcon.Segments[1]}" + Path.GetFileName(game.CommunityListIcon.ToString()) + ".png");
                        }

                        Directory.CreateDirectory($"{game.IconUri.Segments[1]}");
                        webClient.DownloadFile(game.IconUri, $"{game.IconUri.Segments[1]}" + Path.GetFileName(game.IconUri.ToString()) + ".png");
                    }
                }); 

                // Old, terrible way
                //foreach (var game in allPosts)
                //{
                //    Console.WriteLine($"{game.Title}");
                //    if (game.CommunityListIcon != null)
                //    {
                //        Directory.CreateDirectory($"{game.CommunityListIcon.Segments[1]}");
                //        webClient.DownloadFile(game.CommunityListIcon, $"{game.CommunityListIcon.Segments[1]}" + Path.GetFileName(game.CommunityListIcon.ToString()) + ".png");
                //    }

                //    Directory.CreateDirectory($"{game.IconUri.Segments[1]}");
                //    webClient.DownloadFile(game.IconUri, $"{game.IconUri.Segments[1]}" + Path.GetFileName(game.IconUri.ToString()) + ".png");
                //}
            }

            //using (var db = new LiteDatabase("gamelist.db"))
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
}
