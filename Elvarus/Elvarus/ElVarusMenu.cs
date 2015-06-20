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

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Varus._orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");

            cMenu.AddItem(new MenuItem("ElVarus.Combo.Q", "Use Q").SetValue(true));
            //cMenu.AddItem(new MenuItem("ElVarus.combo.always.Q", "always Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElVarus.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElVarus.Combo.R", "Use R").SetValue(true));
            cMenu.AddItem(new MenuItem("ElVarus.sssss", ""));
            cMenu.AddItem(new MenuItem("ElVarus.Q.Min.Charge", "Min. Q Charge").SetValue(new Slider(1000, Varus.spells[Spells.Q].ChargedMinRange, Varus.spells[Spells.Q].ChargedMaxRange)));
            cMenu.AddItem(new MenuItem("ElVarus.Combo.R.Count", "R when enemies >= ")).SetValue(new Slider(1, 1, 5));
            cMenu.AddItem(new MenuItem("ElVarus.Combo.Stack.Count", "Q when stacks >= ")).SetValue(new Slider(3, 1, 3));
            cMenu.AddItem(new MenuItem("ElVarus.sssssssss", ""));
            cMenu.AddItem(new MenuItem("ElVarus.SemiR", "Semi-manual cast R key").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            cMenu.AddItem(new MenuItem("ElVarus.Always.Q", "Cast instant Q").SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Press)));

            //cMenu.AddItem(new MenuItem("ElVarus.SemiR", "Semi-manual cast R key").SetValue(new KeyBind('t', KeyBindType.Press))); 
            cMenu.AddItem(new MenuItem("ElVarus.ssssssssssss", ""));
            cMenu.AddItem(new MenuItem("ElVarus.hitChance", "Hitchance Q").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
            cMenu.AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            _menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElVarus.Harass.Q", "Use Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElVarus.Harass.E", "Use E").SetValue(true));
            hMenu.AddItem(new MenuItem("ElVarus.Harasssfsass.E", ""));
            hMenu.AddItem(new MenuItem("minmanaharass", "Mana needed to clear ")).SetValue(new Slider(55));
            hMenu.AddItem(new MenuItem("ElVarus.hitChance", "Hitchance Q").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
            _menu.AddSubMenu(hMenu);

            var itemMenu = new Menu("Items", "Items");
            itemMenu.AddItem(new MenuItem("ElVarus.Items.Youmuu", "Use Youmuu's Ghostblade").SetValue(true));
            itemMenu.AddItem(new MenuItem("ElVarus.Items.Cutlass", "Use Cutlass").SetValue(true));
            itemMenu.AddItem(new MenuItem("ElVarus.Items.Blade", "Use Blade of the Ruined King").SetValue(true));
            itemMenu.AddItem(new MenuItem("ElVarus.Harasssfsddass.E", ""));
            itemMenu.AddItem(new MenuItem("ElVarus.Items.Blade.EnemyEHP", "Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
            itemMenu.AddItem(new MenuItem("ElVarus.Items.Blade.EnemyMHP", "My HP Percentage").SetValue(new Slider(80, 100, 0)));

            _menu.AddSubMenu(itemMenu);

            var lMenu = new Menu("Clear", "Clear");
            lMenu.AddItem(new MenuItem("useQFarm", "Use Q").SetValue(true));
            lMenu.AddItem(new MenuItem("ElVarus.Count.Minions", "Killable minions with Q >=").SetValue(new Slider(2, 1, 5)));
            lMenu.AddItem(new MenuItem("useEFarm", "Use E").SetValue(true));
            lMenu.AddItem(new MenuItem("ElVarus.Count.Minions.E", "Killable minions with E >=").SetValue(new Slider(2, 1, 5)));
            lMenu.AddItem(new MenuItem("useEFarmddsddaadsd", ""));
            lMenu.AddItem(new MenuItem("useQFarmJungle", "Use Q in jungle").SetValue(true));
            lMenu.AddItem(new MenuItem("useEFarmJungle", "Use E in jungle").SetValue(true));
            lMenu.AddItem(new MenuItem("useEFarmddssd", ""));
            lMenu.AddItem(new MenuItem("minmanaclear", "Mana needed to clear ")).SetValue(new Slider(55));

            _menu.AddSubMenu(lMenu);

            //ElVarus.Misc
            var miscMenu = new Menu("Drawings", "Misc");
            miscMenu.AddItem(new MenuItem("ElVarus.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElVarus.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElVarus.Draw.Q.Charge", "Draw Q charge range").SetValue(new Circle()));

            miscMenu.AddItem(new MenuItem("ElVarus.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElVarus.Draw.E", "Draw E").SetValue(new Circle()));

            var dmgAfterE = new MenuItem("ElDiana.DrawComboDamage", "Draw combo damage").SetValue(true);
            var drawFill = new MenuItem("ElDiana.DrawColour", "Fill colour", true).SetValue(new Circle(true, Color.FromArgb(204, 204, 0, 0)));
            miscMenu.AddItem(drawFill);
            miscMenu.AddItem(dmgAfterE);

            DrawDamage.DamageToUnit = Varus.GetComboDamage;
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
            credits.AddItem(new MenuItem("ElVarus.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElVarus.Email", "info@zavox.nl"));
            _menu.AddSubMenu(credits);

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
<<<<<<< HEAD
            _menu.AddItem(new MenuItem("422442fsaafsf", "Version: 1.0.1.9"));
=======
            _menu.AddItem(new MenuItem("422442fsaafsf", "Version: 1.0.1.7"));
>>>>>>> parent of b5813af... Revert "varus fixes"
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            _menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}
