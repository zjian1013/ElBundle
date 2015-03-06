using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;


namespace ElHecarim
{

    public class ElHecarimMenu
    {
        public static Menu _menu;

        public static void Initialize()
        {
            _menu = new Menu("ElHecarim", "menu", true);

            //ElHecarim.Orbwalker
            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Hecarim._orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            //ElHecarim.TargetSelector
            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElHecarim.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElHecarim.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElHecarim.Combo.R", "Use R").SetValue(true));
            cMenu.AddItem(new MenuItem("ElHecarim.Combo.R.Count", "R when enemies >= ")).SetValue(new Slider(1, 1, 5));
            cMenu.AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            _menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElHecarim.Harass.Q", "Use Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElHecarim.Harass.W", "Use W").SetValue(true));

            _menu.AddSubMenu(hMenu);

            var ItemMenu = new Menu("Items", "Items");
            ItemMenu.AddItem(new MenuItem("ElHecarim.Items.Youmuu", "Use Youmuu's Ghostblade").SetValue(true));
            ItemMenu.AddItem(new MenuItem("ElHecarim.Items.Cutlass", "Use Cutlass").SetValue(true));
            ItemMenu.AddItem(new MenuItem("ElHecarim.Items.Blade", "Use Blade of the Ruined King").SetValue(true));
            ItemMenu.AddItem(new MenuItem("ElHecarim.Harasssfsddass.E", ""));
            ItemMenu.AddItem(new MenuItem("ElHecarim.Items.Blade.EnemyEHP", "Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
            ItemMenu.AddItem(new MenuItem("ElHecarim.Items.Blade.EnemyMHP", "My HP Percentage").SetValue(new Slider(80, 100, 0)));

            _menu.AddSubMenu(ItemMenu);

            //ElHecarim.Misc
            var miscMenu = new Menu("Drawings", "Misc");
            miscMenu.AddItem(new MenuItem("ElHecarim.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElHecarim.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElHecarim.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElHecarim.Draw.R", "Draw R").SetValue(new Circle()));

            _menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = new Menu("Credits", "jQuery");
            credits.AddItem(new MenuItem("ElHecarim.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElHecarim.Email", "info@zavox.nl"));
            _menu.AddSubMenu(credits);

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("422442fsaafsf", "Version: 1.0.0.6"));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            _menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}