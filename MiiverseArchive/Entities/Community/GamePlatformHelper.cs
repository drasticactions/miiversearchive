using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiiverseArchive.Entities.Community
{
    public class GamePlatformHelper
    {
        public static Platform DetectPlatformFromImageName(string className)
        {
            switch (className)
            {
                case "platform-tag-wiiu.png":
                    return Platform.NintendoWiiU;
                case "platform-tag-wiiu-3ds.png":
                    return Platform.Both;
                default:
                    return Platform.Nintendo3DS;
            }
        }
    }
}
