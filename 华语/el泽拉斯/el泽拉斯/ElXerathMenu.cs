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

            var cMenu = new Menu("连招 设置", "Combo");
            cMenu.AddItem(new MenuItem("ElXerath.Combo.Q", "使用 Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElXerath.Combo.W", "使用 W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElXerath.Combo.E", "使用 E").SetValue(true));
            cMenu.AddItem(new MenuItem("ComboActive", "连招!").SetValue(new KeyBind(32, KeyBindType.Press)));

            _menu.AddSubMenu(cMenu);

            var rMenu = new Menu("大招 设置", "Ult");
            rMenu.AddItem(new MenuItem("ElXerath.R.AutoUseR", "自动 R释放").SetValue(true));
            rMenu.AddItem(new MenuItem("ElXerath.R.Mode", "模 式").SetValue(new StringList(new[] { "Normal", "Custom delays", "OnTap", "Custom hitchance", "Near mouse" })));
            rMenu.AddItem(new MenuItem("ElXerath.R.OnTap", "释放 按键").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            rMenu.AddItem(new MenuItem("ElXerath.R.Block", "右键停止R(按多次继续)").SetValue(true));

            rMenu.SubMenu("CustomDelay").AddItem(new MenuItem("ElXerath.R.Delay", "自定义 延迟").SetValue(true));
            for (var i = 1; i <= 3; i++)
                rMenu.SubMenu("CustomDelay").SubMenu("Custom delay").AddItem(new MenuItem("Delay" + i, "延迟" + i).SetValue(new Slider(0, 1500, 0)));

            rMenu.AddItem(new MenuItem("ElXerath.R.Radius", "R 半径").SetValue(new Slider(700, 1500, 300)));

            _menu.AddSubMenu(rMenu);   

            var hMenu = new Menu("骚 扰", "Harass");
            hMenu.AddItem(new MenuItem("ElXerath.Harass.Q", "使用 Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElXerath.Harass.W", "使用 W").SetValue(true));
            hMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElXerath.AutoHarass", "骚扰 (自动)!", false).SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Toggle)));
            hMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElXerath.UseQAutoHarass", "使用 Q").SetValue(true));
            hMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElXerath.UseWAutoHarass", "使用 W").SetValue(true));
            hMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElXerath.harass.mana", "骚扰 最低魔")).SetValue(new Slider(55));

            _menu.AddSubMenu(hMenu);

            var lMenu = new Menu("清线", "LaneClear");
            lMenu.AddItem(new MenuItem("ElXerath.clear.Q", "使用 Q").SetValue(true));
            lMenu.AddItem(new MenuItem("ElXerath.clear.W", "使用 W").SetValue(true));
            lMenu.AddItem(new MenuItem("fasfsafsafsasfasfa", ""));
            lMenu.AddItem(new MenuItem("ElXerath.jclear.Q", "清野 使用 Q").SetValue(true));
            lMenu.AddItem(new MenuItem("ElXerath.jclear.W", "清野 使用 W").SetValue(true));
            lMenu.AddItem(new MenuItem("ElXerath.jclear.E", "清野 使用 E").SetValue(true));
            lMenu.AddItem(new MenuItem("fasfsafsafsadsasasfasfa", ""));
            lMenu.AddItem(new MenuItem("minmanaclear", "清线 最低魔")).SetValue(new Slider(55));

            _menu.AddSubMenu(lMenu);

            //ElXerath.Misc
            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.off", "关 范围").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.Q", "Q 范围").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.W", "W 范围").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.E", "E 范围").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.R", "R 范围").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.Text", "通知").SetValue(true));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.RON", "R target 范围").SetValue(true));
            miscMenu.AddItem(new MenuItem("useEFafsdsgdrmddsddsasfsasdsdsaadsd", ""));
            miscMenu.AddItem(new MenuItem("ElXerath.Ignite", "使用 引燃").SetValue(true));
            miscMenu.AddItem(new MenuItem("ElXerath.misc.ks", "抢人头 mode").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElXerath.misc.Antigapcloser", "使用 E 防止突进").SetValue(true));
            miscMenu.AddItem(new MenuItem("ElXerath.misc.Notifications", "显示 通知").SetValue(true));
            miscMenu.AddItem(new MenuItem("useEdaadaDFafsdsgdrmddsddsasfsasdsdsaadsd", ""));
            miscMenu.AddItem(new MenuItem("ElXerath.Misc.E", "E 按键").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Press)));
            miscMenu.AddItem(new MenuItem("useEdaadaDFafsddssdsgdrmddsddsasfsasdsdsaadsd", ""));
            miscMenu.AddItem(new MenuItem("ElXerath.hitChance", "Q 命中率").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));

            _menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = new Menu("Credits", "jQuery");
            credits.AddItem(new MenuItem("ElXerath.Paypal", "如果你想如果你想捐献 via paypal:"));
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
