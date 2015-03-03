using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;


namespace Elvarus
{
    internal enum Spells
    {
        Q,
        W,
        E,
        R
    }

    internal class Varus
    {
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Orbwalking.Orbwalker _orbwalker;


        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 925)},
            { Spells.W, new Spell(SpellSlot.W, 0)},
            { Spells.E, new Spell(SpellSlot.E, 925)},
            { Spells.R, new Spell(SpellSlot.R, 1100)}
        };

        #region hitchance

        private static HitChance CustomHitChance
        {
            get { return GetHitchance(); }
        }

        private static HitChance GetHitchance()
        {
            switch (ElVarusMenu._menu.Item("ElVarus.hitChance").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        #endregion

        #region Gameloaded 

        public static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Varus")
                return;

            Console.WriteLine("Injected");

            Notifications.AddNotification("ElVarus by jQuery v1.0.0.0", 10000);

            spells[Spells.Q].SetSkillshot(0.25f, 70, 1900, false, SkillshotType.SkillshotLine);
            spells[Spells.E].SetSkillshot(0.1f, 235, 1500, false, SkillshotType.SkillshotCircle);
            spells[Spells.R].SetSkillshot(0.25f, 120, 1950, true, SkillshotType.SkillshotCircle);
            spells[Spells.Q].SetCharged("VarusQ", "VarusQ", 250, 1600, 1.2f);

            ElVarusMenu.Initialize();
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawings.Drawing_OnDraw;
        }

        #endregion

        #region OnGameUpdate

        private static void OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Physical);

            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo(target);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass(target);
                    break;
            }
        }
        #endregion

        private static int GetStacksOn(Obj_AI_Base target)
        {
            var buff = target.Buffs.Find(b => b.Caster.IsMe && b.DisplayName == "VarusWDebuff");
            return buff != null ? buff.Count : 0;
        }

        private static void CastQ(Obj_AI_Base target)
        {
            if (target == null)
                return;

            if (!spells[Spells.Q].IsReady())
                return;

            if (spells[Spells.Q].IsCharging)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                if (spells[Spells.Q].Range >= 100)
                {
                    spells[Spells.Q].CastIfHitchanceEquals(target, CustomHitChance, true);
                }
            }
            else
            {
                spells[Spells.Q].StartCharging();
            }
        }

        #region Harass

        private static void LaneClear()
        {   
            var minions = MinionManager.GetMinions(spells[Spells.Q].Range);
            if (minions.Count >= 3)
            {
                var prediction = spells[Spells.E].GetCircularFarmLocation(minions);
                spells[Spells.E].Cast(prediction.Position);
                spells[Spells.Q].Cast(prediction.Position);
            }
        }
        
        #endregion

        #region Harass

        private static void Harass(Obj_AI_Base target)
        {
            if (target == null || !target.IsValidTarget())
                return;

            var harassQ = ElVarusMenu._menu.Item("ElVarus.Harass.Q").GetValue<bool>();
            var harassE = ElVarusMenu._menu.Item("ElVarus.Harass.E").GetValue<bool>();

            if (harassE && spells[Spells.E].IsReady())
            {
                spells[Spells.E].CastOnBestTarget();
            }

            if (harassQ && spells[Spells.Q].IsReady())
            {
                CastQ(target);
            }
        }

        #endregion

        #region Combo

        private static void Combo(Obj_AI_Base target)
        {
            if (target == null || !target.IsValidTarget())
                return;

            var stackCount = ElVarusMenu._menu.Item("ElVarus.Combo.Stack.Count").GetValue<Slider>().Value;
            var rCount = ElVarusMenu._menu.Item("ElVarus.Combo.R.Count").GetValue<Slider>().Value;
            var comboQ = ElVarusMenu._menu.Item("ElVarus.Combo.Q").GetValue<bool>();
            var comboE = ElVarusMenu._menu.Item("ElVarus.Combo.E").GetValue<bool>();
            var comboR = ElVarusMenu._menu.Item("ElVarus.Combo.R").GetValue<bool>();

            if (comboE && spells[Spells.E].IsReady())
            {
                spells[Spells.E].Cast(target);
            }

            if (comboR && Player.CountEnemiesInRange(spells[Spells.R].Range) >= rCount && spells[Spells.R].IsReady())
            {
                spells[Spells.R].Cast(target);
            }

            if (comboQ && GetStacksOn(target) >= stackCount && spells[Spells.Q].IsReady())
            {
                CastQ(target);
            }
        }
        #endregion
    }
}