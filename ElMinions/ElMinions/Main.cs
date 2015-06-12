using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace ElMinions
{
    internal static class Main
    {
        public static Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        #region OnLoad

        public static void OnLoad(EventArgs args)
        {
            Notifications.AddNotification("ElMinions", 10000);
            ElMinionsMenu.Initialize();
            Drawing.OnEndScene += OnEndScene;
            Game.OnUpdate += OnUpdate;
        }

        #endregion

        #region OnGameUpdate

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var map = Utility.Map.GetMap();
            if (map.Type != Utility.Map.MapType.SummonersRift)
                return;
        }

        #endregion

        #region OnEndScene

        private static void OnEndScene(EventArgs args)
        {
            var isActive = ElMinionsMenu._Menu.Item("ElMinions.Draw.Minions").GetValue<bool>();

            if (!isActive)
                return;

            switch (Player.Team)
            {
                case GameObjectTeam.Chaos: //red side
                    DrawMinionsRed();
                    break;
                case GameObjectTeam.Order: //blue side
                    DrawMinionsBlue();
                    break;
            }
        }

        #endregion

        #region GameTime
        public static float GameTime()
        {
            return Game.Time;
        }
        #endregion

        #region DrawMinionsBlue
        private static void DrawMinionsBlue()
        {
            
        }
        #endregion

        #region DrawMinionsRed
        private static void DrawMinionsRed()
        {
            
        }
        #endregion
    }
}
