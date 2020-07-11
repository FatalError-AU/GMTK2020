using System;
using System.Collections.Generic;
using System.Linq;

namespace GMTK2020.Data.Settings
{
    [SettingsProvider]
    public class SoundSettings
    {
        public Dictionary<Channel, int> volume = Enum.GetValues(typeof(Channel)).Cast<Channel>().ToDictionary(x => x, x => 100);

        public enum Channel : uint
        {
            Music,
            Sfx,
            Master = uint.MaxValue
        }
    }
}