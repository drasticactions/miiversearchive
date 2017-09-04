using System;
using System.Collections.Generic;
using System.Text;

namespace MiiverseArchive.Entities.Response
{
    public class WebApiResponse
    {
        public WebApiResponse(double currentTimestamp, double nextPageUrl, IReadOnlyList<Post.Post> posts)
        {
            CurrentTimestamp = currentTimestamp;
            NextPageTimestamp = nextPageUrl;
            Posts = posts;
        }

        public double CurrentTimestamp { get; set; }

        public double NextPageTimestamp { get; set; }

        public IReadOnlyList<Post.Post> Posts { get; set; }
    }
}
