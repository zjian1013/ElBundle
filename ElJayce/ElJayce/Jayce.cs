using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LeagueSharp.Common.Data;

namespace ElJayce
{
    internal enum Spells
    {
        Q,
        Q1,
        W,
        W1,
        E,
        E1,
        R
    }

    internal static class Jayce
    {
        private static Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        public static Orbwalking.Orbwalker Orbwalker;
        private static SpellSlot _ignite;
        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 1050) },
            { Spells.Q1, new Spell(SpellSlot.Q, 1500) },
            { Spells.W, new Spell(SpellSlot.W, 750) },
            { Spells.W1, new Spell(SpellSlot.W, 0) },
            { Spells.E, new Spell(SpellSlot.E, 650) },
            { Spells.E1, new Spell(SpellSlot.E, 240) },
            { Spells.R, new Spell(SpellSlot.R, 0) }
        };

        #region IsHammer

        private static bool IsHammer()
        {
            return Player.Spellbook.GetSpell(SpellSlot.Q).SData.Name.Contains("jayceshockblast");
        }

        #endregion

        public static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Jayce")
                return;

            Notifications.AddNotification("ElJayce by jQuery v1.0.0.0", 1000);
            _ignite = Player.GetSpellSlot("summonerdot");

            spells[Spells.Q].SetSkillshot(0.15f, 70, 1200, true, SkillshotType.SkillshotLine);
            spells[Spells.Q1].SetSkillshot(0.15f, 70, 1680, true, SkillshotType.SkillshotLine);
            spells[Spells.E].SetSkillshot(0.1f, 120, float.MaxValue, false, SkillshotType.SkillshotCircle);

            ElJayceMenu.Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawings.Drawing_OnDraw;
        }

        #region OnGameUpdate

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Physical);

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                   // LaneClear();
                    //JungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    //Harass(target);
                    break;
            }
        }
        #endregion
           
        #region Combo

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q1].Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValid)
                return;

            var useQ = ElJayceMenu._menu.Item("ElJayce.Combo.Q").GetValue<bool>();
            var useW = ElJayceMenu._menu.Item("ElJayce.Combo.W").GetValue<bool>();
            var useE = ElJayceMenu._menu.Item("ElJayce.Combo.E").GetValue<bool>();

            Vector3 ePosition = Player.ServerPosition + Vector3.Normalize(Player.ServerPosition) * 50;

            if (useQ && useE && spells[Spells.Q].IsReady() && spells[Spells.E].IsReady())
            {
                var pred = spells[Spells.Q1].GetPrediction(target);

                if (pred.Hitchance >= HitChance.High)
                {
                    spells[Spells.Q1].Cast(pred.CastPosition);
                    spells[Spells.E].Cast(ePosition, true);
                }
            } 
        }
        #endregion
    }
}