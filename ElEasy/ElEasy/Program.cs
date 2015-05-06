using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace ElEasy
{
    class Program
    {
        static void Main(string[] args)
         {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        static void OnLoad(EventArgs args)
        {
            try
            {
                Base.Load(ObjectManager.Player.ChampionName);
                Notifications.AddNotification("ElEasy - " + ObjectManager.Player.ChampionName + " 1.0.1.8", 8000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
