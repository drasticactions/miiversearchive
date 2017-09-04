
namespace MiiverseArchive.Entities.Response
{
    public  class PostResponse
    {
        public PostResponse(Post.Post post)
        {
            this.Post = post;
        }

        public Post.Post Post { get; set; }
    }
}
