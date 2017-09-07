using MiiverseArchive.Entities.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiiverseArchive.Entities.User
{
    public class UserFriend
    {
        public UserFriend()
        {

        }

        public UserFriend(string screenName, string acq, UserProfileFeedType type)
        {
            ScreenName = screenName;
            AcquaintanceScreenName = acq;
            ProfileFeedType = type;
        }

        public int Id { get; set; }

        public string ScreenName { get; set; }

        public string AcquaintanceScreenName { get; set; }

        public UserProfileFeedType ProfileFeedType { get; set; }
    }
}
