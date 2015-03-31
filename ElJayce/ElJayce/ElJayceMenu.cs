using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;


namespace ElJayce
{

    public class ElJayceMenu
    {
        public static Menu _menu;

        public static void Initialize()
        {
            _menu = new Menu("ElJayce", "menu", true);

            //ElJayce.Orbwalker
            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Jayce.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            _menu.AddSubMenu(orbwalkerMenu);

            //ElJayce.TargetSelector
            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);

            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElJayce.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElJayce.Combo.W", "Use W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElJayce.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElJayce.Combo.Ignite", "Use Ignite").SetValue(true));
            cMenu.AddItem(new MenuItem("ElJayce.ssssssssssss", ""));
            cMenu.AddItem(new MenuItem("ElJayce.hitChance", "Hitchance Q").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
            cMenu.AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            _menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElJayce.Harass.Q", "Use Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElJayce.Harass.W", "Use W").SetValue(true));
            hMenu.AddItem(new MenuItem("ElJayce.Harass.E", "Use E").SetValue(true));

            _menu.AddSubMenu(hMenu);


            //ElJayce.Misc
            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElJayce.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElJayce.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElJayce.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElJayce.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElJayce.Draw.Text", "Draw Text").SetValue(true));

            miscMenu.AddItem(new MenuItem("useEFafsdsgdrmddsddsasfsasdsdsaadsd", ""));
            miscMenu.AddItem(new MenuItem("ElJayce.misc.ks", "Killsteal mode").SetValue(false));

            _menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = new Menu("Credits", "jQuery");
            credits.AddItem(new MenuItem("ElJayce.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElJayce.Email", "info@zavox.nl"));
            _menu.AddSubMenu(credits);

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("422442fsaafsf", "Version: 1.0.0.0"));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            _menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}