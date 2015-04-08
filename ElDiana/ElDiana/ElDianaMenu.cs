using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;


namespace ElDiana
{

    public class ElDianaMenu
    {
        public static Menu _menu;

        public static void Initialize()
        {
            _menu = new Menu("ElDiana", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Diana.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            _menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);

            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElDiana.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElDiana.Combo.W", "Use W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElDiana.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElDiana.Combo.R", "Use R").SetValue(true));
            //cMenu.AddItem(new MenuItem("ElDiana.Combo.Misaya", "Use Misaya combo when out of range").SetValue(true));
            cMenu.AddItem(new MenuItem("ElDiana.Combo.Secure", "Use R to secure kill").SetValue(true));
            cMenu.AddItem(new MenuItem("ElDiana.Combo.Ignite", "Use Ignite").SetValue(true));
            cMenu.AddItem(new MenuItem("ElDiana.ssssssssssss", ""));
            cMenu.AddItem(new MenuItem("ElDiana.hitChance", "Hitchance Q").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
            //cMenu.AddItem(new MenuItem("ElDiana.Combo.Leapcombo", "Leap Combo").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            cMenu.AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            _menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElDiana.Harass.Q", "Use Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElDiana.Harass.W", "Use W").SetValue(true));
            hMenu.AddItem(new MenuItem("ElDiana.Harass.E", "Use E").SetValue(true));
            hMenu.AddItem(new MenuItem("ElDiana.Harass.Mana", "Minimum mana for harass")).SetValue(new Slider(55));

            _menu.AddSubMenu(hMenu);

            var lMenu = new Menu("Laneclear", "Laneclear");
            lMenu.AddItem(new MenuItem("ElDiana.LaneClear.Q", "Use Q").SetValue(true));
            lMenu.AddItem(new MenuItem("ElDiana.LaneClear.W", "Use W").SetValue(true));
            lMenu.AddItem(new MenuItem("ElDiana.LaneClear.E", "Use E").SetValue(true));
            lMenu.AddItem(new MenuItem("ElDiana.LaneClear.R", "Use R").SetValue(false));
            lMenu.AddItem(new MenuItem("xxx", ""));

            lMenu.AddItem(new MenuItem("ElDiana.LaneClear.Count.Minions.Q", "Minions in range for Q").SetValue(new Slider(2, 1, 5)));
            lMenu.AddItem(new MenuItem("ElDiana.LaneClear.Count.Minions.W", "Minions in range for W").SetValue(new Slider(2, 1, 5)));
            lMenu.AddItem(new MenuItem("ElDiana.LaneClear.Count.Minions.E", "Minions in range for E").SetValue(new Slider(2, 1, 5)));

            _menu.AddSubMenu(lMenu);

            var jMenu = new Menu("Jungleclear", "Jungleclear");
            jMenu.AddItem(new MenuItem("ElDiana.JungleClear.Q", "Use Q").SetValue(true));
            jMenu.AddItem(new MenuItem("ElDiana.JungleClear.W", "Use W").SetValue(true));
            jMenu.AddItem(new MenuItem("ElDiana.JungleClear.E", "Use E").SetValue(true));
            jMenu.AddItem(new MenuItem("ElDiana.JungleClear.R", "Use R").SetValue(false));

            _menu.AddSubMenu(jMenu);

            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElDiana.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElDiana.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElDiana.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElDiana.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElDiana.Draw.R", "Draw R").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElDiana.Draw.Text", "Draw Text").SetValue(true));
            miscMenu.AddItem(new MenuItem("ElDiana.misc.Notifications", "Use Notifications").SetValue(true));

            var dmgAfterE = new MenuItem("ElDiana.DrawComboDamage", "Draw combo damage").SetValue(true);
            var drawFill = new MenuItem("ElDiana.DrawColour", "Fill colour", true).SetValue(new Circle(true, Color.FromArgb(204, 204, 0, 0)));
            miscMenu.AddItem(drawFill);
            miscMenu.AddItem(dmgAfterE);

            DrawDamage.DamageToUnit = Diana.GetComboDamage;
            DrawDamage.Enabled = dmgAfterE.GetValue<bool>();
            DrawDamage.Fill = drawFill.GetValue<Circle>().Active;
            DrawDamage.FillColor = drawFill.GetValue<Circle>().Color;

            dmgAfterE.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                DrawDamage.Enabled = eventArgs.GetNewValue<bool>();
            };

            drawFill.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                DrawDamage.Fill = eventArgs.GetNewValue<Circle>().Active;
                DrawDamage.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };

            _menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = new Menu("Credits", "jQuery");
            credits.AddItem(new MenuItem("ElDiana.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElDiana.Email", "info@zavox.nl"));
            _menu.AddSubMenu(credits);

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("422442fsaafsf", "Version: 1.0.0.1"));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            _menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}