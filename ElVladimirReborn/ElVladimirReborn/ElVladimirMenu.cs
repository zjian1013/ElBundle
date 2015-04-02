using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;


namespace ElVladimirReborn
{

    public class ElVladimirMenu
    {
        public static Menu _menu;

        public static void Initialize()
        {
            _menu = new Menu("ElVladimir:Reborn", "menu", true);

            //ElVladimir.Orbwalker
            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Vladimir.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            _menu.AddSubMenu(orbwalkerMenu);

            //ElVladimir.TargetSelector
            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);

            _menu.AddSubMenu(targetSelector);

            //ElVladimir.Combo
            var comboMenu = new Menu("Combo", "Combo");
            comboMenu.AddItem(new MenuItem("ElVladimir.Combo.Q", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElVladimir.Combo.W", "Use W").SetValue(false));
            comboMenu.AddItem(new MenuItem("ElVladimir.Combo.E", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElVladimir.Combo.R", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElVladimir.Combo.SmartUlt", "Use Smartult").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElVladimir.Combo.Count.R", "Minimum targets for R")).SetValue(new Slider(1, 1, 5));
            comboMenu.AddItem(new MenuItem("separator", ""));
            comboMenu.AddItem(new MenuItem("ElVladimir.Combo.R.Killable", "Use R only when killable").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElVladimir.Combo.Ignite", "Use ignite").SetValue(true));

            _menu.AddSubMenu(comboMenu);

            //ElVladimir.Harass
            var harassMenu = new Menu("Harass", "Harass");
            harassMenu.AddItem(new MenuItem("ElVladimir.Harass.Q", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("ElVladimir.Harass.E", "Use E").SetValue(true));

            //ElVladimir.Auto.Harass
            harassMenu.SubMenu("AutoHarass settings").AddItem(new MenuItem("ElVladimir.AutoHarass.Health.E", "Minimum Health for E").SetValue(new Slider(20)));
            harassMenu.SubMenu("AutoHarass settings").AddItem(new MenuItem("ElVladimir.AutoHarass.Activated", "Auto harass", true).SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));
            harassMenu.SubMenu("AutoHarass settings").AddItem(new MenuItem("spacespacespace", ""));
            harassMenu.SubMenu("AutoHarass settings").AddItem(new MenuItem("ElVladimir.AutoHarass.Q", "Use Q").SetValue(true));
            harassMenu.SubMenu("AutoHarass settings").AddItem(new MenuItem("ElVladimir.AutoHarass.E", "Use E").SetValue(true));

            _menu.AddSubMenu(harassMenu);

            var clearMenu = new Menu("Waveclear", "Waveclear");
            clearMenu.AddItem(new MenuItem("ElVladimir.WaveClear.Q", "Use Q").SetValue(true));
            clearMenu.AddItem(new MenuItem("ElVladimir.WaveClear.E", "Use E").SetValue(true));
            clearMenu.AddItem(new MenuItem("ElVladimir.JungleClear.Q", "Use Q in jungle").SetValue(true));
            clearMenu.AddItem(new MenuItem("ElVladimir.JungleClear.E", "Use E in jungle").SetValue(true));
            clearMenu.AddItem(new MenuItem("ElVladimir.WaveClear.Health.E", "Minimum Health for E").SetValue(new Slider(20)));

            _menu.AddSubMenu(clearMenu);

            var settingsMenu = new Menu("Settings", "Settings");
            settingsMenu.AddItem(new MenuItem("ElVladimir.Settings.Stack.E", "Automatic stack E", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
            settingsMenu.AddItem(new MenuItem("ElVladimir.Settings.Stack.HP", "Minimum automatic stack HP")).SetValue(new Slider(20));
            settingsMenu.AddItem(new MenuItem("ElVladimir.Settings.AntiGapCloser.Active", "Anti gapcloser")).SetValue(true);

            _menu.AddSubMenu(settingsMenu);

            //ElVladimir.Misc
            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElVladimir.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElVladimir.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElVladimir.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElVladimir.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElVladimir.Draw.R", "Draw R").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElVladimir.Draw.Text", "Draw Text").SetValue(true));
            miscMenu.AddItem(new MenuItem("separator1", ""));
            miscMenu.AddItem(new MenuItem("ElVladimir.misc.Notifications", "Notifications").SetValue(true));

            _menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = new Menu("Credits", "jQuery");
            credits.AddItem(new MenuItem("ElVladimir.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElVladimir.Email", "info@zavox.nl"));
            _menu.AddSubMenu(credits);

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("422442fsaafsf", "Version: 1.0.0.0"));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            _menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}