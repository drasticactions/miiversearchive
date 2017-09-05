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

            using (var db = new LiteDatabase("users.db"))
            {
                var users = db.GetCollection<User>("users");
                foreach (var userId in userIds)
                {
                    var userEntity = await ctx.GetUserProfileAsync(userId);
                    Console.WriteLine("Name: {0}", userEntity.User.Name);
                    Console.WriteLine("ScreenName: {0}", userEntity.User.ScreenName);
                    Console.WriteLine("IconUri: {0}", userEntity.User.IconUri);
                    Console.WriteLine("Country: {0}", userEntity.User.Country);
                    Console.WriteLine("Birthday: {0}", userEntity.User.Birthday);
                    Console.WriteLine("Birthday Hidden: {0}", userEntity.User.IsBirthdayHidden);
                    if (userEntity.User.GameSystem != null)
                    {
                        foreach (var gameSystem in userEntity.User.GameSystem)
                        {
                            Console.WriteLine("GameSystem: {0}", gameSystem);
                        }
                    }
                    if (userEntity.User.FavoriteGameGenre != null)
                    {
                        foreach (var gameGenre in userEntity.User.FavoriteGameGenre)
                        {
                            Console.WriteLine("GameGenre: {0}", gameGenre);
                        }
                    }
                    Console.WriteLine("GameSkill: {0}", userEntity.User.GameSkill);
                    Console.WriteLine("-----------");

                    users.Upsert(userEntity.User);
                }
            }
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

                Console.Write("*");
                inputList.Add(info.KeyChar);
            }

            return string.Concat(inputList);
        }

        #endregion
    }
}
