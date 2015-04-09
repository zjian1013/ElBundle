using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace ElEasy
{
    class Base
    {
        internal static void Load(string champName)
        {
            switch (champName)
            {
                case "Taric":
                    Plugins.Taric.Load();
                    break;

                case "Leona":
                    Plugins.Leona.Load();
                    break;

                case "Sona":
                    Plugins.Sona.Load();
                    break;
            }
        }
    }
}