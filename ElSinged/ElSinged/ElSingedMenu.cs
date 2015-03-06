using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;


namespace ElSinged
{

    public class ElSingedMenu
    {
        public static Menu _menu;

        public static void Initialize()
        {
            _menu = new Menu("ElSinged", "menu", true);

            //ElSinged.Orbwalker
            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Singed._orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            //ElSinged.TargetSelector
            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElSinged.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElSinged.Combo.W", "Use W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElSinged.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElSinged.Combo.R", "Use R").SetValue(true));
            cMenu.AddItem(new MenuItem("exploit", "Exploit Enabled [RISKY]").SetValue(false));
            cMenu.AddItem(new MenuItem("delayms", "Delay (MS)").SetValue(new Slider(150, 0, 1000)));
            cMenu.AddItem(new MenuItem("ElSinged.Coffasfsafsambo.R", ""));
            cMenu.AddItem(new MenuItem("ElSinged.Combo.R.Count", "Use R enemies >= ")).SetValue(new Slider(2, 1, 5));
            cMenu.AddItem(new MenuItem("ElSinged.Combo.Ignite", "Use Ignite").SetValue(true));
            cMenu.AddItem(new MenuItem("ElSinged.hitChance", "Hitchance W").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
            cMenu.AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            _menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElSinged.Harass.Q", "Use Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElSinged.Harass.W", "Use W").SetValue(true));
            hMenu.AddItem(new MenuItem("ElSinged.Harass.E", "Use E").SetValue(true));
            hMenu.AddItem(new MenuItem("ElSinged.hitChance", "Hitchance W").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
            _menu.AddSubMenu(hMenu);

            var lcMenu = new Menu("Laneclear", "Laneclear");
            lcMenu.AddItem(new MenuItem("ElSinged.Laneclear.Q", "Use Q").SetValue(true));
            lcMenu.AddItem(new MenuItem("ElSinged.Laneclear.E", "Use E").SetValue(true));
            _menu.AddSubMenu(lcMenu);

            //ElSinged.Misc
            var miscMenu = new Menu("Drawings", "Misc");
            miscMenu.AddItem(new MenuItem("ElSinged.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElSinged.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElSinged.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElSinged.Draw.E", "Draw E").SetValue(new Circle()));
            _menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = new Menu("Credits", "jQuery");
            credits.AddItem(new MenuItem("ElSinged.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElSinged.Email", "info@zavox.nl"));
            _menu.AddSubMenu(credits);

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("422442fsaafsf", "Version: 1.0.0.3"));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            _menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}