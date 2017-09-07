using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiiverseArchive.Entities.Response
{
    public class UserProfileResponse
    {
        public UserProfileResponse(User.User user)
        {
            User = user;
        }

        public User.User User { get; set; }
    }

    public class UserProfileFeedResponse
    {
        public UserProfileFeedResponse(string username, List<string> responseUsernames, UserProfileFeedType type)
        {
            ScreenName = username;
            ResultScreenNames = responseUsernames;
            ResultType = type;
        }

        public string ScreenName { get; set; }

        public List<string> ResultScreenNames { get; set; }

        public UserProfileFeedType ResultType { get; set; }
    }

    public enum UserProfileFeedType
    {
        Followers,
        Following,
        Friends
    }
}
