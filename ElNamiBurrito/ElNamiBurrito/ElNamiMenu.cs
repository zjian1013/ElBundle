using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;


namespace ElNamiBurrito
{

    public class ElNamiMenu
    {
        public static Menu _menu;

        public static void Initialize()
        {
            _menu = new Menu("ElNamiReborn", "menu", true);

            //ElNamiReborn.Orbwalker
            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Nami.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            _menu.AddSubMenu(orbwalkerMenu);

            //ElNamiReborn.TargetSelector
            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            //ElNamiReborn.Combo
            var comboMenu = new Menu("Combo", "Combo");
            comboMenu.AddItem(new MenuItem("ElNamiReborn.Combo.Q", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElNamiReborn.Combo.W", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElNamiReborn.Combo.E", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElNamiReborn.Combo.R", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElNamiReborn.Combo.R.Count", "Minimum targets R")).SetValue(new Slider(3, 1, 5));
            comboMenu.AddItem(new MenuItem("ElNamiReborn.Combo.Ignite", "Use ignite").SetValue(true));

            _menu.AddSubMenu(comboMenu);

            //ElNamiReborn.Harass
            var harassMenu = new Menu("Harass", "Harass");
            harassMenu.AddItem(new MenuItem("ElNamiReborn.Harass.Q", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("ElNamiReborn.Harass.W", "Use W").SetValue(true));
            harassMenu.AddItem(new MenuItem("ElNamiReborn.Harass.E", "Use E").SetValue(true));
            harassMenu.AddItem(new MenuItem("ElNamiReborn.Harass.Mana", "Minimum mana for harass")).SetValue(new Slider(55));

           /* harassMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElNamiReborn.AutoHarass.Activated", "[Toggle] Auto harass", false).SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Toggle)));
            harassMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElNamiReborn.AutoHarass.Q", "Use Q").SetValue(true));
            harassMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElNamiReborn.AutoHarass.W", "Use W").SetValue(true));
            harassMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElNamiReborn.AutoHarass.Mana", "Minimum mana")).SetValue(new Slider(55));
            */
            _menu.AddSubMenu(harassMenu);

            //ElNamiReborn.E
            var castEMenu = _menu.AddSubMenu(new Menu("E settings", "ESettings"));
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(champ => champ.IsAlly))
            {
                castEMenu.AddItem(new MenuItem("ElNamiReborn.Settings.E1" + ally.BaseSkinName, string.Format("Cast E: {0}", ally.BaseSkinName)).SetValue(true));
            }

            //ElNamiReborn.Heal
            var healMenu = new Menu("Heal settings", "HealSettings");
            healMenu.AddItem(new MenuItem("ElNamiReborn.Heal.Activate", "Use heal").SetValue(true));
            healMenu.AddItem(new MenuItem("ElNamiReborn.Heal.Player.HP", "HP percentage").SetValue(new Slider(25, 1, 100)));
            healMenu.AddItem(new MenuItem("ElNamiReborn.Heal.Ally.HP", "Use heal on ally's").SetValue(true));
            healMenu.AddItem(new MenuItem("ElNamiReborn.Heal.Ally.HP.Percentage", "HP percentage ally's").SetValue(new Slider(25, 1, 100)));
            healMenu.AddItem(new MenuItem("ElNamiReborn.Heal.Mana", "Mininum mana needed")).SetValue(new Slider(55));

            _menu.AddSubMenu(healMenu);

            //ElNamiReborn.Interupt
            var interuptMenu = new Menu("Interupt settings", "interuptsettings");
            interuptMenu.AddItem(new MenuItem("ElNamiReborn.Interupt.Q", "Use Q").SetValue(true));
            interuptMenu.AddItem(new MenuItem("ElNamiReborn.Interupt.R", "Use R").SetValue(false));

            _menu.AddSubMenu(interuptMenu);

            //ElNamiReborn.Misc
            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElNamiReborn.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElNamiReborn.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElNamiReborn.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElNamiReborn.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElNamiReborn.Draw.R", "Draw R").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElNamiReborn.Draw.Text", "Draw Text").SetValue(true));

            miscMenu.AddItem(new MenuItem("sep1", ""));
            miscMenu.AddItem(new MenuItem("ElNamiReborn.misc.ks", "Killsteal mode").SetValue(false));
            miscMenu.AddItem(new MenuItem("sep5", ""));
            miscMenu.AddItem(new MenuItem("ElNamiReborn.hitChance", "Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));

            _menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = new Menu("Credits", "jQuery");
            credits.AddItem(new MenuItem("ElNamiReborn.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElNamiReborn.Email", "info@zavox.nl"));
            _menu.AddSubMenu(credits);

            _menu.AddItem(new MenuItem("sep2", ""));
            _menu.AddItem(new MenuItem("sep3", "Version: 1.0.0.2"));
            _menu.AddItem(new MenuItem("sep4", "Made By jQuery"));

            _menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}
