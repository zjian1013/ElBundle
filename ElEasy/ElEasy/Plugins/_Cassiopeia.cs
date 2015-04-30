using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;
using Color = System.Drawing.Color;

namespace ElEasy.Plugins
{
    public class _Cassiopeia : Standards
    {
        private static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 850) },
            { Spells.W, new Spell(SpellSlot.W, 850) },
            { Spells.E, new Spell(SpellSlot.E, 700) },
            { Spells.R, new Spell(SpellSlot.R, 825) }
        }; 

        public static void Load()
        {
            _ignite = Player.GetSpellSlot("summonerdot");

            //U must be pretty salty - WIP still need to find correct data :D
            spells[Spells.Q].SetSkillshot(
                spells[Spells.Q].Instance.SData.SpellCastTime, spells[Spells.Q].Instance.SData.LineWidth,
                spells[Spells.Q].Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCircle);
            spells[Spells.W].SetSkillshot(
                spells[Spells.W].Instance.SData.SpellCastTime, spells[Spells.W].Instance.SData.LineWidth,
                spells[Spells.W].Instance.SData.MissileSpeed, false, SkillshotType.SkillshotLine);
            spells[Spells.R].SetSkillshot(
                spells[Spells.R].Instance.SData.SpellCastTime, spells[Spells.R].Instance.SData.LineWidth,
                spells[Spells.R].Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCone);

            spells[Spells.E].SetTargetted(0.25f, float.MaxValue);

            Initialize();
            Game.OnUpdate += OnUpdate;
        }

        #region Onupdate
        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    //OnHarass();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    //OnLaneclear();
                    //OnJungleclear();
                    break;

                case Orbwalking.OrbwalkingMode.LastHit:
                    //OnLasthit();
                    break;
            }
        }
        #endregion

        #region OnCombo

        private static void OnCombo()
        {
            
        }

        #endregion

        #region Menu

        private static void Initialize()
        {
            _menu = new Menu("ElCassiopeia WIP", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElEasy.Cassio.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Cassio.Combo.W", "Use W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Cassio.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Cassio.Combo.R", "Use R").SetValue(true));

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
