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
        #region OnLoad

        public static void OnLoad(EventArgs args)
        {
            Notifications.AddNotification("ElMinions", 10000);
            ElMinionsMenu.Initialize();
            Drawing.OnEndScene += OnEndScene;
        }

        #endregion

        #region OnEndScene

        private static void OnEndScene(EventArgs args)
        {
            var isActive = ElMinionsMenu.Menu.Item("ElMinions.Draw.Minions").GetValue<bool>();

            if (isActive)
            {
                // blabla
            }
        }

        #endregion
    }
}
