﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LeagueSharp.Common.Data;
using ItemData = LeagueSharp.Common.Data.ItemData;


namespace ElCorki
{
    internal enum Spells
    {
        Q,
        W,
        E,
        R1,
        R2
    }

    internal static class Corki
    {
        public static String ScriptVersion { get { return typeof(Corki).Assembly.GetName().Version.ToString(); } }
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Orbwalking.Orbwalker Orbwalker;
        private static SpellSlot _ignite;
        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 825)},
            { Spells.W, new Spell(SpellSlot.W, 800)},
            { Spells.E, new Spell(SpellSlot.E, 600)},
            { Spells.R1, new Spell(SpellSlot.R, 1300)},
            { Spells.R2, new Spell(SpellSlot.R, 1500)}
        };

        #region Gameloaded 


        #region hitchance

        private static HitChance CustomHitChance
        {
            get { return GetHitchance(); }
        }

        private static HitChance GetHitchance()
        {
            switch (ElCorkiMenu._menu.Item("ElCorki.hitChance").GetValue<StringList>().SelectedIndex)
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

        public static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.CharData.BaseSkinName != "Corki")
                return;

            Console.WriteLine("Injected");
            Notifications.AddNotification(String.Format("ElCorki by jQuery v{0}", ScriptVersion), 8000);

            spells[Spells.Q].SetSkillshot(0.35f, 250f, 1000f, false, SkillshotType.SkillshotCircle);
            spells[Spells.E].SetSkillshot(0f, (float)(45 * Math.PI / 180), 1500, false, SkillshotType.SkillshotCone);
            spells[Spells.R1].SetSkillshot(0.2f, 40f, 2000f, true, SkillshotType.SkillshotLine);
            spells[Spells.R2].SetSkillshot(0.2f, 40f, 2000f, true, SkillshotType.SkillshotLine);
            _ignite = Player.GetSpellSlot("summonerdot");

            ElCorkiMenu.Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawings.Drawing_OnDraw;
        }

        #endregion

        #region OnGameUpdate

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Physical);

            switch (Orbwalker.ActiveMode)
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

            KsMode();
            AutoHarassMode();
            JungleStealMode();

            spells[Spells.R1].Range = ObjectManager.Player.HasBuff("corkimissilebarragecounterbig") ? spells[Spells.R2].Range : spells[Spells.R1].Range;
        }
        #endregion

        #region itemusage

        private static void Items(Obj_AI_Base target)
        {
            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            var ghost = ItemData.Youmuus_Ghostblade.GetItem();
            var cutlass = ItemData.Bilgewater_Cutlass.GetItem();

            var useYoumuu = ElCorkiMenu._menu.Item("ElCorki.Items.Youmuu").GetValue<bool>();
            var useCutlass = ElCorkiMenu._menu.Item("ElCorki.Items.Cutlass").GetValue<bool>();
            var useBlade = ElCorkiMenu._menu.Item("ElCorki.Items.Blade").GetValue<bool>();

            var useBladeEhp = ElCorkiMenu._menu.Item("ElCorki.Items.Blade.EnemyEHP").GetValue<Slider>().Value;
            var useBladeMhp = ElCorkiMenu._menu.Item("ElCorki.Items.Blade.EnemyMHP").GetValue<Slider>().Value;

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

        #region Combo

        private static void Combo(Obj_AI_Base target)
        {
            if (target == null || !target.IsValidTarget())
                return;

            var comboQ = ElCorkiMenu._menu.Item("ElCorki.Combo.Q").GetValue<bool>();
            var comboE = ElCorkiMenu._menu.Item("ElCorki.Combo.E").GetValue<bool>();
            var comboR = ElCorkiMenu._menu.Item("ElCorki.Combo.R").GetValue<bool>();
            var useIgnite = ElCorkiMenu._menu.Item("ElCorki.Combo.Ignite").GetValue<bool>();
            var rStacks = ElCorkiMenu._menu.Item("ElCorki.Combo.RStacks").GetValue<Slider>().Value;
            var rTarget = TargetSelector.GetTarget(spells[Spells.R1].Range, TargetSelector.DamageType.Magical);

            Items(target);

            if (comboQ && spells[Spells.Q].IsReady())
            {
                var pred = spells[Spells.Q].GetPrediction(target);
                if (pred.Hitchance >= HitChance.VeryHigh)
                {
                    spells[Spells.Q].Cast(target);
                }    
            }

            if (comboE && spells[Spells.E].IsReady())
            {
                spells[Spells.E].Cast(target);
            }

            if (comboR && spells[Spells.R1].IsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Ammo > rStacks && spells[Spells.R1].IsInRange(rTarget))
            {
                var bigR = ObjectManager.Player.HasBuff("corkimissilebarragecounterbig");

                var _target = TargetSelector.GetTarget(bigR ? spells[Spells.R2].Range : spells[Spells.R1].Range, TargetSelector.DamageType.Magical);
                if (_target != null)
                if (bigR)
                {
                    var pred = spells[Spells.R2].GetPrediction(target);
                    if (pred.Hitchance >= CustomHitChance)
                    {
                        spells[Spells.R2].Cast(target);
                    }
                }
                else
                {
                    var pred = spells[Spells.R1].GetPrediction(target);
                    if (pred.Hitchance >= CustomHitChance)
                    {
                        spells[Spells.R1].Cast(target);
                    }
                }
            }

            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health &&
               useIgnite)
            {
                Player.Spellbook.CastSpell(_ignite, target);
            }
        }

        #endregion

        #region Laneclear

        private static void LaneClear()
        {
            var useQ = ElCorkiMenu._menu.Item("useQFarm").GetValue<bool>();
            var useE = ElCorkiMenu._menu.Item("useEFarm").GetValue<bool>();
            var useR = ElCorkiMenu._menu.Item("useRFarm").GetValue<bool>();
            var countMinions = ElCorkiMenu._menu.Item("ElCorki.Count.Minions").GetValue<Slider>().Value;
            var countMinionsE = ElCorkiMenu._menu.Item("ElCorki.Count.Minions.E").GetValue<Slider>().Value;
            var countMinionsR = ElCorkiMenu._menu.Item("ElCorki.Count.Minions.R").GetValue<Slider>().Value;
            var minmana = ElCorkiMenu._menu.Item("minmanaclear").GetValue<Slider>().Value;

            if (Player.ManaPercent < minmana)
                return;

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.Q].Range);

            if (minions.Count <= 0)
                return;

            if (spells[Spells.Q].IsReady() && useQ)
            {
                foreach (var minion in minions.Where(x => x.Health <= spells[Spells.Q].GetDamage(x)))
                {
                    var killcount = 0;

                    foreach (var cminion in minions)
                    {
                        if (cminion.Health <= spells[Spells.Q].GetDamage(cminion))
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
                        spells[Spells.Q].Cast(minion);
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
                    spells[Spells.E].Cast();
                }
            }

            if (!useR || !spells[Spells.R1].IsReady())
                return;

            var rMinionkillcount =
                minions.Count(x => spells[Spells.R1].CanCast(x) && x.Health <= spells[Spells.R1].GetDamage(x));

            if (rMinionkillcount >= countMinionsR)
            {
                foreach (var minion in minions.Where(x => x.Health <= spells[Spells.R1].GetDamage(x)))
                {
                    spells[Spells.R1].Cast(minion);
                }
            }
        }
        #endregion

        #region jungle

        private static void JungleClear()
        {
            var useQ = ElCorkiMenu._menu.Item("useQFarmJungle").GetValue<bool>();
            var useE = ElCorkiMenu._menu.Item("useEFarmJungle").GetValue<bool>();
            var useR = ElCorkiMenu._menu.Item("useRFarmJungle").GetValue<bool>();
            var minmana = ElCorkiMenu._menu.Item("minmanaclear").GetValue<Slider>().Value;
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 700, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Player.ManaPercent < minmana)
                return;

            foreach (var minion in minions)
            {
                if (spells[Spells.Q].IsReady() && useQ)
                {
                    spells[Spells.Q].Cast(minion);
                }

                if (spells[Spells.E].IsReady() && useE)
                {
                    spells[Spells.E].Cast(minion);
                }

                if (spells[Spells.R1].IsReady() && useR)
                {
                    spells[Spells.R1].Cast(minion);
                }
            }
        }
        #endregion

        #region Harass

        private static void Harass(Obj_AI_Base target)
        {
            if (target == null || !target.IsValidTarget())
                 return;
        
            var harassQ = ElCorkiMenu._menu.Item("ElCorki.Harass.Q").GetValue<bool>();
            var harassE = ElCorkiMenu._menu.Item("ElCorki.Harass.E").GetValue<bool>();
            var harassR = ElCorkiMenu._menu.Item("ElCorki.Harass.R").GetValue<bool>();
            var minmana = ElCorkiMenu._menu.Item("ElCorki.harass.mana").GetValue<Slider>().Value;
            var rStacks = ElCorkiMenu._menu.Item("ElCorki.Harass.RStacks").GetValue<Slider>().Value;

            if (Player.ManaPercent < minmana)
                 return;

            if (harassQ && spells[Spells.Q].IsReady())
             {
                 spells[Spells.Q].Cast(target);
             }

            if (harassE && spells[Spells.E].IsReady())
             {
                 spells[Spells.E].CastOnBestTarget();
             }

             if (harassR && spells[Spells.R1].IsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Ammo > rStacks)
             {
                 spells[Spells.R1].CastIfHitchanceEquals(target, CustomHitChance, true);
             }
        }
        #endregion

        #region Autoharass

        private static void AutoHarassMode()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Physical);
            var rTarget = TargetSelector.GetTarget(spells[Spells.R1].Range, TargetSelector.DamageType.Magical);

            if (target == null || !target.IsValidTarget() || rTarget == null || !rTarget.IsValidTarget())
                return;

            if (ElCorkiMenu._menu.Item("ElCorki.AutoHarass").GetValue<KeyBind>().Active)
            {
                var q = ElCorkiMenu._menu.Item("ElCorki.UseQAutoHarass").GetValue<bool>();
                var r = ElCorkiMenu._menu.Item("ElCorki.UseQAutoHarass").GetValue<bool>();
                var mana = ElCorkiMenu._menu.Item("ElCorki.harass.mana").GetValue<Slider>().Value;

                if (Player.ManaPercent < mana)
                    return;

                if (r && spells[Spells.R1].IsReady() && spells[Spells.R1].IsInRange(rTarget) || spells[Spells.R2].IsInRange(rTarget))
                {
                    var bigR = ObjectManager.Player.HasBuff("corkimissilebarragecounterbig");

                    var _target = TargetSelector.GetTarget(bigR ? spells[Spells.R2].Range : spells[Spells.R1].Range, TargetSelector.DamageType.Magical);
                    if (_target != null)
                    if (bigR)
                    {
                        spells[Spells.R2].Cast(_target);
                    }
                    else
                    {
                        spells[Spells.R1].Cast(_target);
                    }
                }

                if (q && spells[Spells.Q].IsReady() && target.IsValidTarget(spells[Spells.Q].Range))
                {
                    spells[Spells.Q].Cast(target);
                }
            }
        }

        #endregion

        #region KSMode

        private static void KsMode()
        {
            var useKs = ElCorkiMenu._menu.Item("ElCorki.misc.ks").GetValue<bool>();
            if (!useKs)
                return;

            var target = HeroManager.Enemies.FirstOrDefault(x => !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield) && spells[Spells.R1].CanCast(x) && (x.Health + (x.HPRegenRate / 2)) <= spells[Spells.R1].GetDamage(x));

            if (spells[Spells.R1].IsReady() && spells[Spells.R1].CanCast(target))
            {
                spells[Spells.R1].Cast(target);
            }
        }

        #endregion

        #region JungleSteal

        static void JungleStealMode()
        {
            var useJsm = ElCorkiMenu._menu.Item("ElCorki.misc.junglesteal").GetValue<bool>();

            if (!useJsm)
                return;

            var jMob = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.R1].Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.Health + (x.HPRegenRate / 2) <= spells[Spells.R1].GetDamage(x));

            if (spells[Spells.R1].CanCast(jMob))
                spells[Spells.R1].Cast(jMob);

            var minion = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.R1].Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.Health <= spells[Spells.E].GetDamage(x) && (x.SkinName.ToLower().Contains("siege") || x.SkinName.ToLower().Contains("super")));

            if (spells[Spells.R1].IsReady() && spells[Spells.R1].CanCast(minion))
            {
                spells[Spells.R1].Cast(minion);
            }
        }

        #endregion

        #region Ignite Damage

        private static float IgniteDamage(Obj_AI_Base target)
        {
            if (_ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(_ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        #endregion
    }
}
