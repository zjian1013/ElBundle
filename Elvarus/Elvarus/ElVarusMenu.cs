using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;


namespace Elvarus
{

    public class ElVarusMenu
    {
        public static Menu _menu;

        public static void Initialize()
        {
            _menu = new Menu("ElVarus", "menu", true);

            //ElSinged.Orbwalker
            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Varus._orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            //ElSinged.TargetSelector
            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElVarus.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElVarus.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElVarus.Combo.R", "Use R").SetValue(true));
            cMenu.AddItem(new MenuItem("ElVarus.Combo.R.Count", "R when enemies >= ")).SetValue(new Slider(1, 1, 5));
            cMenu.AddItem(new MenuItem("ElVarus.Combo.Stack.Count", "Q when stacks >= ")).SetValue(new Slider(3, 1, 3));
            cMenu.AddItem(new MenuItem("ElVarus.hitChance", "Hitchance Q").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
            cMenu.AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            _menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElVarus.Harass.Q", "Use Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElVarus.Harass.E", "Use E").SetValue(true));
            hMenu.AddItem(new MenuItem("ElVarus.hitChance", "Hitchance Q").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
            _menu.AddSubMenu(hMenu);

            var clearlaneMenu = new Menu("Clear", "Clear");
            clearlaneMenu.AddItem(new MenuItem("ElVarusQ", "Use Q").SetValue(true));
            clearlaneMenu.AddItem(new MenuItem("ElVarusE", "Use E").SetValue(true));
            _menu.AddSubMenu(hMenu);

            //ElSinged.Misc
            var miscMenu = new Menu("Drawings", "Misc");
            miscMenu.AddItem(new MenuItem("ElVarus.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElVarus.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElVarus.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElVarus.Draw.E", "Draw E").SetValue(new Circle()));
            _menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = new Menu("Credits", "jQuery");
            credits.AddItem(new MenuItem("ElSinged.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElSinged.Email", "info@zavox.nl"));
            _menu.AddSubMenu(credits);

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("422442fsaafsf", "Version: 1.0.0.0"));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            _menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}