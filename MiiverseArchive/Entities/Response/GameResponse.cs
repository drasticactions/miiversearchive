using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiiverseArchive.Entities.Response
{
    public  class GameResponse
    {
        public GameResponse(bool canPost, bool isFavorite, string nextPageUrl, bool before, IReadOnlyList<Post.Post> posts)
        {
            NextPageUrl = nextPageUrl;
            CanPost = canPost;
            Posts = posts;
            IsFavorite = isFavorite;
            BeforeRenewal = before;
        }

        public string NextPageUrl { get; set; }

        public bool CanPost { get; set; }

        public bool IsFavorite { get; set; }

        public bool BeforeRenewal { get; set; }

        public IReadOnlyList<Post.Post> Posts { get; set; }
    }

    public  class OldGameResponse
    {
        public OldGameResponse(double nextPageUrl, IReadOnlyList<Post.Post> posts)
        {
            NextPageTimestamp = nextPageUrl;
            Posts = posts;
        }

        public double NextPageTimestamp { get; set; }

        public IReadOnlyList<Post.Post> Posts { get; set; }
    }
}
