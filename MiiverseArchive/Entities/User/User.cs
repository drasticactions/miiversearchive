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
            IEnumerable<string> favoriteGameGenre)
        {
            Name = name;
            ScreenName = screenName;
            IconUri = iconUri;
            Country = country;
            Birthday = birthday.Contains("Private") ? DateTime.MinValue : DateTime.Parse(birthday);
            IsBirthdayHidden = Birthday == DateTime.MinValue;
            GameSkill = GameSkillHelper.DetectGameSkillFromClassName(gameSkill);
            GameSystem = new List<GameSystem>();
            foreach (var gameSystem in gameSystems)
            {
                GameSystem.Add(GameSystemHelper.DetectGameSystemFromClassName(gameSystem));
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string ScreenName { get; set; }

        public Uri IconUri { get; set; }

        public string Country { get; set; }

        public DateTime Birthday { get; set; }

        public GameSkill GameSkill { get; set; }

        public List<GameSystem> GameSystem { get; set; }

        public List<string> FavoriteGameGenre { get; set; }

        public bool IsHidden { get; set; }

        public bool IsBirthdayHidden { get; set; }

        public bool IsError { get; set; }
    }
}
