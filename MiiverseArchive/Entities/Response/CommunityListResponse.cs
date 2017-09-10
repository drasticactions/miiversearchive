using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiiverseArchive.Entities.Community;

namespace MiiverseArchive.Entities.Response
{
    public  class CommunityListResponse
    {
        public CommunityListResponse(List<Game> game)
        {
            this.Games = game;
        }
        public List<Game> Games { get; set; } 
    }

    public class RelatedCommunityListResponse
    {
        public RelatedCommunityListResponse(List<CommunityItem> game)
        {
            this.Games = game;
        }
        public List<CommunityItem> Games { get; set; }
    }
}
