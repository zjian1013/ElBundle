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

            Console.WriteLine("Injected");

            Notifications.AddNotification("ElVarus by jQuery v1.0.0.2", 10000);

            spells[Spells.Q].SetSkillshot(0.25f, 70, 1900, false, SkillshotType.SkillshotLine);
            spells[Spells.E].SetSkillshot(0.1f, 235, 1500, false, SkillshotType.SkillshotCircle);
            spells[Spells.R].SetSkillshot(0.25f, 120, 1950, true, SkillshotType.SkillshotCircle);
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
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Physical);

            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo(target);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    JungleClear();
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

        #region Laneclear

        private static void LaneClear()
        {
            var useQ = ElVarusMenu._menu.Item("useQFarm").GetValue<bool>();
            var useE = ElVarusMenu._menu.Item("useQFarm").GetValue<bool>();
            var minmana = ElVarusMenu._menu.Item("minmanaclear").GetValue<Slider>().Value;
            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, spells[Spells.E].Range, MinionTypes.All);

            if(Player.ManaPercentage() >= minmana)
            {                    
                foreach (var minion in minions)
                {
                    if (spells[Spells.Q].IsReady() && useQ)
                    {
                        if (spells[Spells.Q].IsCharging)
                        {
                            spells[Spells.Q].Cast(minion, true);
                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        }
                        if (!spells[Spells.Q].IsCharging)
                        {
                            spells[Spells.Q].StartCharging();
                            return;
                        }
                    }
                    if (spells[Spells.E].IsReady() && useE)
                    {
                        spells[Spells.E].Cast(minion);
                    }
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

            if (Player.ManaPercentage() >= minmana)
            {
                foreach (var minion in minions)
                {
                    if (spells[Spells.Q].IsReady() && useQ)
                    {
                        if (spells[Spells.Q].IsCharging)
                        {
                            spells[Spells.Q].Cast(minion, true);
                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        }
                        if (!spells[Spells.Q].IsCharging)
                        {
                            spells[Spells.Q].StartCharging();
                            return;
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

        private static void Harass(Obj_AI_Base target)
        {
            if (target == null || !target.IsValidTarget())
                return;

            var harassQ = ElVarusMenu._menu.Item("ElVarus.Harass.Q").GetValue<bool>();
            var harassE = ElVarusMenu._menu.Item("ElVarus.Harass.E").GetValue<bool>();
            var minmana = ElVarusMenu._menu.Item("minmanaharass").GetValue<Slider>().Value;

            if (Player.ManaPercentage() >= minmana)
            {
                if (harassE && spells[Spells.E].IsReady())
                {
                    spells[Spells.E].CastOnBestTarget();
                }

                if (harassQ && spells[Spells.Q].IsReady())
                {
                    CastQ(target);
                }
            }
        }

        #endregion

        #region itemusage

        private static void items(Obj_AI_Base target)
        {
            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            var Ghost = ItemData.Youmuus_Ghostblade.GetItem();
            var cutlass = ItemData.Bilgewater_Cutlass.GetItem();

            var useYoumuu = ElVarusMenu._menu.Item("ElVarus.Items.Youmuu").GetValue<bool>();
            var useCutlass = ElVarusMenu._menu.Item("ElVarus.Items.Cutlass").GetValue<bool>();
            var useBlade = ElVarusMenu._menu.Item("ElVarus.Items.Blade").GetValue<bool>();

            var useBladeEhp = ElVarusMenu._menu.Item("ElVarus.Items.Blade.EnemyEHP").GetValue<Slider>().Value;
            var useBladeMhp = ElVarusMenu._menu.Item("ElVarus.Items.Blade.EnemyMHP").GetValue<Slider>().Value;

            if (botrk.IsReady() && botrk.IsOwned(Player) && botrk.IsInRange(target)
            && target.HealthPercentage() <= useBladeEhp
            && useBlade)

                botrk.Cast(target);

            if (botrk.IsReady() && botrk.IsOwned(Player) && botrk.IsInRange(target)
                && Player.HealthPercentage() <= useBladeMhp
                && useBlade)

                botrk.Cast(target);

            if (cutlass.IsReady() && cutlass.IsOwned(Player) && cutlass.IsInRange(target) &&
                target.HealthPercentage() <= useBladeEhp
                && useCutlass)
                cutlass.Cast(target);

            if (Ghost.IsReady() && Ghost.IsOwned(Player) && target.IsValidTarget(spells[Spells.Q].Range)
                && useYoumuu)
                Ghost.Cast();
        }

        #endregion

        #region GetComboDamage   

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (spells[Spells.Q].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            return (float)damage;
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

            items(target);

            if (comboE && spells[Spells.E].IsReady())
            {
                spells[Spells.E].Cast(target);
            }

            if (comboR && Player.CountEnemiesInRange(spells[Spells.R].Range) >= rCount && spells[Spells.R].IsReady())
            {
                spells[Spells.R].CastOnBestTarget();
            }

           
            var comboDamage = GetComboDamage(target);

            if (spells[Spells.Q].IsCharging)
            {
                if (spells[Spells.Q].IsInRange(target))
                {                                
                    if (comboQ && spells[Spells.Q].IsReady())
                    {
                        if (spells[Spells.Q].Range == spells[Spells.Q].ChargedMaxRange)
                        {
                            CastQ(target);
                        }
                    }
                }
            }
            else if (comboDamage > target.Health || Player.AttackRange < Player.Distance(target) || GetStacksOn(target) >= stackCount)
            {
                spells[Spells.Q].StartCharging();
            }
        }
        #endregion
    }
}