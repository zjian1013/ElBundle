using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;


namespace ElEasy.Plugins
{
    public class _Syndra : Standards
    {
        #region Spells

        private static readonly Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>
        {
            { Spells.Q, new Spell(SpellSlot.Q, 0) },
            { Spells.W, new Spell(SpellSlot.W, 0) },
            { Spells.E, new Spell(SpellSlot.E, 0) },
            { Spells.R, new Spell(SpellSlot.R, 0) }
        };

        #endregion

        #region Load
        public static void Load()
        {
            _ignite = Player.GetSpellSlot("summonerdot");

            Initialize();
            Game.OnUpdate += OnUpdate;
        }
        #endregion

        #region Onupdate
        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    //OnCombo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    //OnHarass();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                   // OnLaneclear();
                    //OnJungleclear();
                    break;

                case Orbwalking.OrbwalkingMode.LastHit:
                    //OnLasthit();
                    break;
            }
        }

        #endregion

        #region Menu

        private static void Initialize()
        {
            _menu = new Menu("ElSyndra || Beta 1.0", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElEasy.Syndra.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Syndra.Combo.W", "Use W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Syndra.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Syndra.Combo.R", "Use R").SetValue(true));

            _menu.AddSubMenu(cMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = _menu.AddSubMenu(new Menu("Credits", "jQuery"));
            credits.AddItem(new MenuItem("ElEasy.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElEasy.Email", "info@zavox.nl"));

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            _menu.AddToMainMenu();
        }

        #endregion
    }
}
