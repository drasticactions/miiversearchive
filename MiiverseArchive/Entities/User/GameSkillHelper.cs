using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiiverseArchive.Entities.User
{
    public static class GameSkillHelper
    {
        private const string IntermediateLevel = "intermediate";
        private const string ExpertLevel = "expert";
        private const string CasualLevel = "casual";

        public static GameSkill DetectGameSkillFromClassName(string className)
        {
            switch (className)
            {
                case IntermediateLevel:
                    return GameSkill.Intermediate;
                case ExpertLevel:
                    return GameSkill.Expert;
                case CasualLevel:
                    return GameSkill.Casual;
                default:
                    throw new Exception("Class name is invalid");
            }
        }
    }
}
