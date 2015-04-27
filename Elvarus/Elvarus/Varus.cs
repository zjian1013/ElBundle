using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LeagueSharp.Common.Data;

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

        private static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            args.Process = !spells[Spells.Q].IsCharging;
        }

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

            Notifications.AddNotification("ElVarus by jQuery v1.0.0.8", 10000);

            spells[Spells.Q].SetSkillshot(0.25f, 70, 1900, false, SkillshotType.SkillshotLine);
            spells[Spells.E].SetSkillshot(0.1f, 235, 1500, false, SkillshotType.SkillshotCircle);
            spells[Spells.R].SetSkillshot(0.25f, 120, 1950, true, SkillshotType.SkillshotLine);
            spells[Spells.Q].SetCharged("VarusQ", "VarusQ", 250, 1600, 1.2f);

            ElVarusMenu.Initialize();
            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawings.Drawing_OnDraw;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
        }

        #endregion

        #region OnGameUpdate

        private static void OnGameUpdate(EventArgs args)
        {
            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }

            var target = TargetSelector.GetTarget(spells[Spells.R].Range, TargetSelector.DamageType.Physical);

            if (spells[Spells.R].IsReady() && target.IsValidTarget() && ElVarusMenu._menu.Item("ElVarus.SemiR").GetValue<KeyBind>().Active)
            {
                spells[Spells.R].CastOnUnit(target);
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

            if (spells[Spells.Q].IsReady())
            {
                if (!spells[Spells.Q].IsCharging)
                {
                   spells[Spells.Q].StartCharging();
                }
                else
                {
                    if (spells[Spells.Q].IsInRange(target))
                    {
                        spells[Spells.Q].Cast(target);
                    }
                }
            }
        }

        #region Laneclear

        private static void LaneClear()
        {
            var useQ = ElVarusMenu._menu.Item("useQFarm").GetValue<bool>();
            var useE = ElVarusMenu._menu.Item("useQFarm").GetValue<bool>();
            var countMinions = ElVarusMenu._menu.Item("ElVarus.Count.Minions").GetValue<Slider>().Value;
            var countMinionsE = ElVarusMenu._menu.Item("ElVarus.Count.Minions.E").GetValue<Slider>().Value;
            var minmana = ElVarusMenu._menu.Item("minmanaclear").GetValue<Slider>().Value;

            if (Player.ManaPercent < minmana)
                return;

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.Q].Range);
            if (minions.Count <= 0)
                return;

            if (spells[Spells.Q].IsReady() && useQ)
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.Q].Range);
                {
                    foreach (var minion in
                        allMinions.Where(
                            minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q)))
                    {

                        var killcount = 0;

                        foreach (var colminion in minions)
                        {
                            if (colminion.Health <= spells[Spells.Q].GetDamage(colminion))
                            {
                                killcount++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (killcount >= countMinions)
                        {
                            if (minion.IsValidTarget())
                            {
                                spells[Spells.Q].Cast(minion);
                                return;
                            }
                        }
                    }
                }
            }

            if (!useE || !spells[Spells.E].IsReady())
                return;

            var minionkillcount =
                minions.Count(x => spells[Spells.E].CanCast(x) && x.Health <= spells[Spells.E].GetDamage(x));

            if (minionkillcount >= countMinionsE)
            {
                foreach (var minion in minions.Where(x => x.Health <= spells[Spells.E].GetDamage(x)))
                {
                    spells[Spells.E].Cast(minion);
                }
            }
        }

        #endregion

        #region jungle

        private static void JungleClear()
        {
            var useQ = ElVarusMenu._menu.Item("useQFarmJungle").GetValue<bool>();
            var useE = ElVarusMenu._menu.Item("useEFarmJungle").GetValue<bool>();
            var minmana = ElVarusMenu._menu.Item("minmanaclear").GetValue<Slider>().Value;
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 700, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Player.ManaPercent >= minmana)
            {
                foreach (var minion in minions)
                {
                    if (spells[Spells.Q].IsReady() && useQ)
                    {
                        if (!spells[Spells.Q].IsCharging)
                        {
                            spells[Spells.Q].StartCharging();
                        }
                        else
                        {
                            spells[Spells.Q].CastOnUnit(minion);
                        }
                    }

                    if (spells[Spells.E].IsReady() && useE)
                    {
                        spells[Spells.E].CastOnUnit(minion);
                    }
                }    
            }
        }

        #endregion

        #region Harass

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].ChargedMaxRange, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValidTarget())
                return;

            var harassQ = ElVarusMenu._menu.Item("ElVarus.Harass.Q").GetValue<bool>();
            var harassE = ElVarusMenu._menu.Item("ElVarus.Harass.E").GetValue<bool>();
            var minmana = ElVarusMenu._menu.Item("minmanaharass").GetValue<Slider>().Value;

            if (Player.ManaPercent < minmana)
                return;

            if (harassE && spells[Spells.E].IsReady())
            {
                spells[Spells.E].CastOnBestTarget();
            }

            if (spells[Spells.Q].IsReady() && harassQ)
            {
                //CastQ(target);

                var prediction = spells[Spells.Q].GetPrediction(target);
                var distance = Player.ServerPosition.Distance(prediction.UnitPosition + 200 * (prediction.UnitPosition - Player.ServerPosition).Normalized(), true);
                if (distance < spells[Spells.Q].RangeSqr)
                {
                    if (spells[Spells.Q].Cast(prediction.CastPosition))
                        return;
                }
            }
        }

        #endregion

        #region itemusage

        private static void Items(Obj_AI_Base target)
        {
            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            var ghost = ItemData.Youmuus_Ghostblade.GetItem();
            var cutlass = ItemData.Bilgewater_Cutlass.GetItem();

            var useYoumuu = ElVarusMenu._menu.Item("ElVarus.Items.Youmuu").GetValue<bool>();
            var useCutlass = ElVarusMenu._menu.Item("ElVarus.Items.Cutlass").GetValue<bool>();
            var useBlade = ElVarusMenu._menu.Item("ElVarus.Items.Blade").GetValue<bool>();

            var useBladeEhp = ElVarusMenu._menu.Item("ElVarus.Items.Blade.EnemyEHP").GetValue<Slider>().Value;
            var useBladeMhp = ElVarusMenu._menu.Item("ElVarus.Items.Blade.EnemyMHP").GetValue<Slider>().Value;

            if (botrk.IsReady() && botrk.IsOwned(Player) && botrk.IsInRange(target)
            && target.HealthPercent <= useBladeEhp
            && useBlade)

                botrk.Cast(target);

            if (botrk.IsReady() && botrk.IsOwned(Player) && botrk.IsInRange(target)
                && Player.HealthPercent <= useBladeMhp
                && useBlade)

                botrk.Cast(target);

            if (cutlass.IsReady() && cutlass.IsOwned(Player) && cutlass.IsInRange(target) &&
                target.HealthPercent <= useBladeEhp
                && useCutlass)
                cutlass.Cast(target);

            if (ghost.IsReady() && ghost.IsOwned(Player) && target.IsValidTarget(spells[Spells.Q].Range)
                && useYoumuu)
                ghost.Cast();
        }

        #endregion

        #region GetComboDamage   

        public static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (spells[Spells.Q].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (spells[Spells.W].IsReady())
            {
                damage += enemy.Buffs.Where(buff => buff.Name == "VarusWDebuff").Sum(buff => Player.GetSpellDamage(enemy, SpellSlot.W, 1) * (1 + buff.Count / 3) - 1);
            }

            if (spells[Spells.E].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);
            }

            if (spells[Spells.R].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
            }

            return (float)damage;
        }

        #endregion

        #region Combo

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].ChargedMaxRange, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValidTarget())
                return;

            var stackCount = ElVarusMenu._menu.Item("ElVarus.Combo.Stack.Count").GetValue<Slider>().Value;
            var rCount = ElVarusMenu._menu.Item("ElVarus.Combo.R.Count").GetValue<Slider>().Value;
            var comboQ = ElVarusMenu._menu.Item("ElVarus.Combo.Q").GetValue<bool>();
            var comboE = ElVarusMenu._menu.Item("ElVarus.Combo.E").GetValue<bool>();
            var comboR = ElVarusMenu._menu.Item("ElVarus.Combo.R").GetValue<bool>();

            Items(target);

            if (comboE && spells[Spells.E].IsReady())
            {
                spells[Spells.E].Cast(target);
            }

            if (spells[Spells.Q].IsReady() && comboQ)
            {
                if (spells[Spells.Q].GetDamage(target) > target.Health || GetStacksOn(target) >= stackCount) //|| spells[Spells.W].Level == 0
                {
                    var prediction = spells[Spells.Q].GetPrediction(target);
                    var distance = Player.ServerPosition.Distance(prediction.UnitPosition + 200 * (prediction.UnitPosition - Player.ServerPosition).Normalized(), true);
                    if (distance < spells[Spells.Q].RangeSqr)
                    {
                        if (spells[Spells.Q].Cast(prediction.CastPosition))
                            return;
                    }
                    //CastQ(target);
                }
            }

            if (comboR && Player.CountEnemiesInRange(spells[Spells.R].Range) >= rCount && spells[Spells.R].IsReady())
            {
                spells[Spells.R].CastOnBestTarget();
            }
        }
        #endregion
    }
}
