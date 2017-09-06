using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiiverseArchive.Entities.User
{
    public class User
    {
        public User() { }

        public User(string name,
            string screenName,
            Uri iconUri,
            string country,
            string birthday,
            string gameSkill,
            IEnumerable<string> gameSystems,
            IEnumerable<string> favoriteGameGenre,
            string bio,
            int followerCount,
            int followingCount,
            int friendCount,
            int postCount,
            int empathyCount,
            string bannerImage)
        {
            Name = name;
            ScreenName = screenName;
            IconUri = iconUri;
            Country = country;
            Birthday = birthday;
            IsBirthdayHidden = birthday.Contains("Private");
            GameSkill = GameSkillHelper.DetectGameSkillFromClassName(gameSkill);
            GameSystem = new List<GameSystem>();
            foreach (var gameSystem in gameSystems)
            {
                GameSystem.Add(GameSystemHelper.DetectGameSystemFromClassName(gameSystem));
            }
            Bio = bio;
            FollowerCount = followerCount;
            FollowingCount = followingCount;
            FriendsCount = friendCount;
            TotalPosts = postCount;
            EmpathyCount = empathyCount;
            SidebarCoverUrl = bannerImage;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string ScreenName { get; set; }

        public Uri IconUri { get; set; }

        public string Country { get; set; }

        public string Birthday { get; set; }

        public GameSkill GameSkill { get; set; }

        public List<GameSystem> GameSystem { get; set; }

        public List<string> FavoriteGameGenre { get; set; }

        public bool IsHidden { get; set; }

        public bool IsBirthdayHidden { get; set; }

        public bool IsError { get; set; }

        public string Bio { get; set; }

        public int EmpathyCount { get; set; }

        public int TotalPosts { get; set; }

        public int FriendsCount { get; set; }

        public int FollowingCount { get; set; }

        public int FollowerCount { get; set; }

        public string SidebarCoverUrl { get; set; }
    }
}
