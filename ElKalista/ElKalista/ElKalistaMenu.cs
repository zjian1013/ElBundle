using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;


namespace ElKalista
{

    public class ElKalistaMenu
    {
        public static Menu _menu;

        public static void Initialize()
        {
            _menu = new Menu("ElKalista", "menu", true);

            //ElSinged.Orbwalker
            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Kalista._orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            //ElSinged.TargetSelector
            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElKalista.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElKalista.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElKalista.Combo.R", "Use R").SetValue(true));
            cMenu.AddItem(new MenuItem("ElKalista.sssssssss", ""));
            cMenu.AddItem(new MenuItem("ElKalista.hitChance", "Hitchance Q").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
            cMenu.AddItem(new MenuItem("ElKalista.SemiR", "Semi-manual R").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            cMenu.AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            _menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElKalista.Harass.Q", "Use Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElKalista.minmanaharass", "Mana need for harass ")).SetValue(new Slider(55));
            hMenu.AddItem(new MenuItem("ElKalista.hitChance", "Hitchance Q").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
            _menu.AddSubMenu(hMenu);

            var itemMenu = new Menu("Items", "Items");
            itemMenu.AddItem(new MenuItem("ElKalista.Items.Youmuu", "Use Youmuu's Ghostblade").SetValue(true));
            itemMenu.AddItem(new MenuItem("ElKalista.Items.Cutlass", "Use Cutlass").SetValue(true));
            itemMenu.AddItem(new MenuItem("ElKalista.Items.Blade", "Use Blade of the Ruined King").SetValue(true));
            itemMenu.AddItem(new MenuItem("ElKalista.Harasssfsddass.E", ""));
            itemMenu.AddItem(new MenuItem("ElKalista.Items.Blade.EnemyEHP", "Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
            itemMenu.AddItem(new MenuItem("ElKalista.Items.Blade.EnemyMHP", "My HP Percentage").SetValue(new Slider(80, 100, 0)));

            _menu.AddSubMenu(itemMenu);


            var lMenu = new Menu("Clear", "Clear");
            lMenu.AddItem(new MenuItem("useQFarm", "Use Q").SetValue(true));
            lMenu.AddItem(new MenuItem("ElKalista.Count.Minions", "Killable minions with Q >=").SetValue(new Slider(2, 1, 5)));
            lMenu.AddItem(new MenuItem("useEFarm", "Use E").SetValue(true));
            lMenu.AddItem(new MenuItem("ElKalista.Count.Minions.E", "Killable minions with E >=").SetValue(new Slider(2, 1, 5)));
            lMenu.AddItem(new MenuItem("useEFarmddsddaadsd", ""));
            lMenu.AddItem(new MenuItem("useQFarmJungle", "Use Q in jungle").SetValue(true));
            lMenu.AddItem(new MenuItem("useEFarmJungle", "Use E in jungle").SetValue(true));
            lMenu.AddItem(new MenuItem("useEFarmddssd", ""));
            lMenu.AddItem(new MenuItem("minmanaclear", "Mana needed to clear ")).SetValue(new Slider(55));

            _menu.AddSubMenu(lMenu);

            //ElSinged.Misc
            var miscMenu = new Menu("Super Secret Settings", "Misc");
            miscMenu.AddItem(new MenuItem("ElKalista.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElKalista.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElKalista.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElKalista.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("useEFarmddsddsasfsasaadsd", ""));
            miscMenu.AddItem(new MenuItem("ElKalista.misc.save", "Save ally with R").SetValue(true));
            miscMenu.AddItem(new MenuItem("ElKalista.misc.allyhp", "Ally HP Percentage").SetValue(new Slider(25, 100, 0)));
            miscMenu.AddItem(new MenuItem("useEFarmddsddsasfsasdsdsaadsd", ""));
            miscMenu.AddItem(new MenuItem("ElKalista.misc.ks", "Killsteal mode").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElKalista.misc.junglesteal", "Jungle steal mode").SetValue(true));

            var dmgAfterComboItem = new MenuItem("ElKalista.DrawComboDamage", "Draw E damage").SetValue(true);
            miscMenu.AddItem(dmgAfterComboItem);

            Utility.HpBarDamageIndicator.DamageToUnit = Kalista.GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            Utility.HpBarDamageIndicator.Color = Color.Aqua;

            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            _menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = new Menu("Credits", "jQuery");
            credits.AddItem(new MenuItem("ElKalista.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElKalista.Email", "info@zavox.nl"));
            _menu.AddSubMenu(credits);

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("422442fsaafsf", "Alpha Version: 1.0.0.2"));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            _menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}