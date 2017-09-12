using System;
using System.Collections.Generic;
using System.Text;

namespace MiiverseArchive.Entities.Response
{
    public class WebApiResponse
    {
        public WebApiResponse(string nextPageUrl, IReadOnlyList<Post.Post> posts)
        {
            NextPageUrl = nextPageUrl;
            Posts = posts;
        }

        public string NextPageUrl { get; set; }

        public IReadOnlyList<Post.Post> Posts { get; set; }
    }
}
