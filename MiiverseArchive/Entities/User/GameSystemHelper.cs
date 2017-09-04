using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiiverseArchive.Entities.User
{
    public static class GameSystemHelper
    {
        private const string Device3DS = "device-3ds";
        private const string DeviceWiiu = "device-wiiu";

        public static GameSystem DetectGameSystemFromClassName(string className)
        {
            switch (className)
            {
                case Device3DS:
                    return GameSystem.Nintendo3DS;
                case DeviceWiiu:
                    return GameSystem.NintendoWiiU;
                default:
                    throw new Exception("Class name is invalid");
            }
        }
    }
}
