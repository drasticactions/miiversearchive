using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiiverseArchive.Entities.Response
{
    public  class UserFeedResponse
    {
        public UserFeedResponse(IReadOnlyList<Post.Post> posts)
        {
            Posts = posts;
        }

        public IReadOnlyList<Post.Post> Posts { get; set; }
    }
}
