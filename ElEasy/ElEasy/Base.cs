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

                case "Nasus":
                    Plugins.Nasus.Load();
                    break;

                case "Malphite":
                    Plugins.Malphite.Load();
                    break;

                case "Darius":
                    Plugins.Darius.Load();
                    break;

                case "Katarina":
                    Plugins.Katarina.Load();
                    break;

                case "Ryze":
                    Plugins.Ryze.Load();
                    break;

                case "Cassiopeia":
                    Plugins.Cassiopeia.Load();
                    break;
            }
        }
    }
}