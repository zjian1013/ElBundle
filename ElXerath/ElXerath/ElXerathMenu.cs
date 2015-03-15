using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;


namespace ElXerath
{

    public class ElXerathMenu
    {
        public static Menu _menu;

        public static void Initialize()
        {
            _menu = new Menu("ElXerath", "menu", true);

            //ElXerath.Orbwalker
            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Xerath.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            //ElXerath.TargetSelector
            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElXerath.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElXerath.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            _menu.AddSubMenu(cMenu);

            var rMenu = new Menu("Ult", "Ult");
            rMenu.AddItem(new MenuItem("ElXerath.R.AutoUseR", "Auto use charges").SetValue(true));
            rMenu.AddItem(new MenuItem("ElXerath.R.Mode", "Mode ").SetValue(new StringList(new[] { "Normal", "Custom delays", "OnTap", "Custom hitchance" })));
            rMenu.AddItem(new MenuItem("ElXerath.R.OnTap", "Ult on tap").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            rMenu.AddItem(new MenuItem("ElXerath.R.Block", "Block movement").SetValue(true));

            rMenu.SubMenu("CustomDelay").AddItem(new MenuItem("ElXerath.R.Delay", "Custom delays"));
            for (var i = 1; i <= 3; i++)
                rMenu.SubMenu("CustomDelay").SubMenu("Custom delay").AddItem(new MenuItem("Delay" + i, "Delay" + i).SetValue(new Slider(0, 1500, 0)));

            _menu.AddSubMenu(rMenu);   

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElXerath.Harass.Q", "Use Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElXerath.Harass.W", "Use W").SetValue(true));
            hMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElXerath.AutoHarass", "[Toggle] Auto harass", false).SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Toggle)));
            hMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElXerath.UseQAutoHarass", "Use Q").SetValue(true));
            hMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElXerath.UseWAutoHarass", "Use W").SetValue(true));
            hMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElXerath.harass.mana", "Auto harass mana")).SetValue(new Slider(55));

            _menu.AddSubMenu(hMenu);

            var lMenu = new Menu("Lane clear", "LaneClear");
            lMenu.AddItem(new MenuItem("ElXerath.clear.Q", "Use Q").SetValue(true));
            lMenu.AddItem(new MenuItem("ElXerath.clear.W", "Use W").SetValue(true));
            lMenu.AddItem(new MenuItem("minmanaclear", "Auto harass mana")).SetValue(new Slider(55));

            _menu.AddSubMenu(lMenu);

            //ElXerath.Misc
            var miscMenu = new Menu("Drawings", "Misc");
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.R", "Draw R").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.Text", "Draw Text").SetValue(true));
            miscMenu.AddItem(new MenuItem("useEFafsdsgdrmddsddsasfsasdsdsaadsd", ""));
            miscMenu.AddItem(new MenuItem("ElXerath.Ignite", "Use ignite").SetValue(true));
            miscMenu.AddItem(new MenuItem("ElXerath.misc.ks", "Killsteal mode").SetValue(false));
            miscMenu.AddItem(new MenuItem("useEdaadaDFafsdsgdrmddsddsasfsasdsdsaadsd", ""));
            miscMenu.AddItem(new MenuItem("ElXerath.hitChance", "Hitchance Q").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));

            _menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = new Menu("Credits", "jQuery");
            credits.AddItem(new MenuItem("ElXerath.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElXerath.Email", "info@zavox.nl"));
            _menu.AddSubMenu(credits);

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("422442fsaafsf", "Version: 1.0.0.6"));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            _menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}