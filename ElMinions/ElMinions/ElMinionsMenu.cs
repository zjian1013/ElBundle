using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;

namespace ElMinions
{
    public static class ElMinionsMenu
    {
        public static Menu Menu;

        public static void Initialize()
        {
            Menu = new Menu("ElMinions", "menu", true);

            var drawMenu = new Menu("Drawings", "Drawings");
            drawMenu.AddItem(new MenuItem("ElMinions.Draw.Minions", "Draw minions").SetValue(true));
            Menu.AddSubMenu(drawMenu);

            Menu.AddToMainMenu();
        }
    }
}
