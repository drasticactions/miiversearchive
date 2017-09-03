﻿using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using Mntone.MiiverseClient.Entities.Community;
using Mntone.MiiverseClient.Entities.Token;
using Mntone.MiiverseClient.Managers;
using System.Threading.Tasks;
using System.Net;
using Mntone.MiiverseClient.Entities.Post;
using System.IO;
namespace MiiverseArchive
{
    class Program
    {
        static void Main()
        {
            //MainAsync().GetAwaiter().GetResult();
            DownloadAsync().GetAwaiter().GetResult();
        }

        static async Task DownloadAsync()
        {
            using (var db = new LiteDatabase("drawing.db"))
            {
                var posts = db.GetCollection<Post>("drawingcollection");
                var allPosts = posts.Find(Query.All());
                Console.WriteLine($"Post Count: {allPosts.Count()}");
                var webClient = new WebClient();
                Directory.CreateDirectory("images");
                foreach(var post in allPosts)
                {
                    Console.WriteLine($"Downloading {post.ID}.jpg");
                    webClient.DownloadFile(post.ImageUri, $"images\\{post.ID}.jpg");
                }
            }
        }

        static async Task MainAsync()
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

            var ctx = oauthClient.Authorize(token, new NintendoNetworkAuthenticationToken(userName, password)).GetAwaiter().GetResult();

            // TODO: Figure out a way to automate archiving game/user data.
            // Hardcoding this for testing...
            Console.WriteLine("-----------");
            Console.WriteLine("Archiving: Splatoon (Drawing)");
            Console.WriteLine("-----------");
            var gameList = ctx.GetCommunityGameListAsync(GameSearchList.All, GamePlatformSearch.Wiiu, 300).GetAwaiter().GetResult();
            var gameTest = new Game("community-14866558073673172583", "Splatoon", "/titles/14866558073673172576/14866558073673172583", new Uri("https://d3esbfg30x759i.cloudfront.net/cnj/zlCfzTYBRmcD4DW6Q5"), "platform-tag-wiiu.png", "Wii U Games");

            using (var db = new LiteDatabase("drawing.db"))
            {
                var posts = db.GetCollection<Post>("drawingcollection");
                var allPosts = posts.Find(Query.All());
                double nextPost = 0;
                double nextPostMinutes = 0;
                while (true)
                {
                    var indieGameDrawing = await ctx.GetDrawingAsync(gameTest, nextPost);
                    if (!indieGameDrawing.Posts.Any())
                    {
                        // We're done! Time to wrap it up.
                        return;
                    }
                    foreach(var post in indieGameDrawing.Posts)
                    {
                        posts.Upsert(post);
                    }
                    // We can't get exact times for posts, only relative times like "About an hour".
                    // Because of that, we can't rely on using the last post to set where we start from.
                    // Because we could end up just getting the same last hour of posts. So instead.
                    // Keep substracting 15 minutes from the current time. That should result in getting newer posts.
                    nextPostMinutes = nextPostMinutes + 15;
                    var epoch = DateTime.UtcNow.AddMinutes(-1 * nextPostMinutes) - new DateTime(1970, 1, 1);
                    int secondsSinceEpoch = (int)epoch.TotalSeconds;
                    nextPost = -(secondsSinceEpoch);
                    Console.WriteLine("Next Post Time: {0} Total Inserted: {1}", nextPost, posts.Count());
                }
            }
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