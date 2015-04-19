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

namespace ElVi
{
    internal enum Spells
    {
        Q,
        W,
        E,
        R
    }

    internal static class Vi
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static Orbwalking.Orbwalker Orbwalker;
        private static SpellSlot _ignite;
        private static SpellSlot _flash;
        private static int _lastNotification = 0;
        private static Obj_AI_Hero _qTargetLock = null;

        public static readonly Dictionary<Spells, Spell> Spells = new Dictionary<Spells, Spell>()
        {
            { ElVi.Spells.Q, new Spell(SpellSlot.Q, 800) },
            { ElVi.Spells.W, new Spell(SpellSlot.W) },
            { ElVi.Spells.E, new Spell(SpellSlot.E, 600) },
            { ElVi.Spells.R, new Spell(SpellSlot.R, 800) }
        };

        public static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Vi")
            {
                return;
            }

            Notifications.AddNotification("ElVi by jQuery v1.0.0.0", 5000);
            _ignite = Player.GetSpellSlot("summonerdot");
            _flash = Player.GetSpellSlot("SummonerFlash");


            Spells[ElVi.Spells.Q].SetSkillshot(
                Spells[ElVi.Spells.Q].Instance.SData.SpellCastTime, Spells[ElVi.Spells.Q].Instance.SData.LineWidth,
                Spells[ElVi.Spells.Q].Instance.SData.MissileSpeed, false, SkillshotType.SkillshotLine);
            Spells[ElVi.Spells.Q].SetCharged("ViQ", "ViQ", 100, 860, 1f);
            Spells[ElVi.Spells.E].SetSkillshot(
                Spells[ElVi.Spells.E].Instance.SData.SpellCastTime, Spells[ElVi.Spells.E].Instance.SData.LineWidth,
                Spells[ElVi.Spells.E].Instance.SData.MissileSpeed, false, SkillshotType.SkillshotLine);
            Spells[ElVi.Spells.R].SetTargetted(0.15f, 1500f);

            ElViMenu.Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawings.Drawing_OnDraw;
            Orbwalking.AfterAttack += OrbwalkingAfterAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        private static void OrbwalkingAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
            {
                return;
            }

            var useE = ElViMenu._menu.Item("ElVi.Combo.E").GetValue<bool>();

            if (unit.IsMe && useE)
            {
                Spells[ElVi.Spells.E].Cast();
            }

            Orbwalking.ResetAutoAttackTimer();
        }

        #region OnGameUpdate

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Obj_AI_Hero target = TargetSelector.GetTarget(
                        Spells[ElVi.Spells.Q].Range, TargetSelector.DamageType.Physical);

                    if (_qTargetLock == null)
                    {
                        _qTargetLock = target;
                        OnCombo(_qTargetLock);
                    }
                    else
                    {
                        _qTargetLock = null;
                    }

                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    OnLaneClear();
                    OnJungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    OnHarass();
                    break;
            }

            if (ElViMenu._menu.Item("ElVi.Combo.Flash").GetValue<KeyBind>().Active)
            {
                FlashQ();
            }

            var showNotifications = ElViMenu._menu.Item("ElVi.misc.Notifications").GetValue<bool>();

            if (showNotifications && Environment.TickCount - _lastNotification > 5000)
            {
                foreach (
                    var enemy in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(h => h.IsValidTarget(1000) && GetComboDamage(h) > h.Health))
                {
                    ShowNotification(enemy.ChampionName + ": is killable", Color.White, 4000);
                    _lastNotification = Environment.TickCount;
                }
            }
        }

        #endregion

        #region Interrupters

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!ElViMenu._menu.Item("ElVi.misc.AntiGapCloser").GetValue<bool>())
            {
                return;
            }

            if (Spells[ElVi.Spells.Q].IsReady())
            {
                if (!Spells[ElVi.Spells.Q].IsCharging)
                {
                    Spells[ElVi.Spells.Q].StartCharging();
                }
                else
                {
                    Spells[ElVi.Spells.Q].Cast(gapcloser.Sender);
                }
            }
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!ElViMenu._menu.Item("ElVi.misc.Interrupter").GetValue<bool>())
            {
                return;
            }

            if (Spells[ElVi.Spells.Q].IsReady())
            {
                if (!Spells[ElVi.Spells.Q].IsCharging)
                {
                    Spells[ElVi.Spells.Q].StartCharging();
                }
                else
                {
                    Spells[ElVi.Spells.Q].Cast(sender);
                }
            }

            if (Spells[ElVi.Spells.R].CanCast(sender) && args.DangerLevel >= Interrupter2.DangerLevel.High)
            {
                Spells[ElVi.Spells.R].Cast(sender);
            }
        }

        #endregion

        #region OnJungleClear

        private static void OnJungleClear()
        {
            var useQ = ElViMenu._menu.Item("ElVi.JungleClear.Q").GetValue<bool>();
            var useE = ElViMenu._menu.Item("ElVi.JungleClear.E").GetValue<bool>();
            var playerMana = ElViMenu._menu.Item("ElVi.Clear.Player.Mana").GetValue<Slider>().Value;

            if (Player.ManaPercentage() < playerMana)
                return;

            var minions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, Spells[ElVi.Spells.E].Range, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);

            if (minions.Count <= 0)
            {
                return;
            }

            if (useQ && Spells[ElVi.Spells.Q].IsReady())
            {
                if (!Spells[ElVi.Spells.Q].IsCharging)
                {
                    Spells[ElVi.Spells.Q].StartCharging();
                }
                else
                {
                    if (minions.Count == minions.Count(x => Player.Distance(x) < Spells[ElVi.Spells.Q].Range))
                    {
                        Spells[ElVi.Spells.Q].Cast(minions[0]);
                    }
                }
            }

            if (useE && Spells[ElVi.Spells.E].IsReady())
            {
                var bestFarmPos = Spells[ElVi.Spells.E].GetLineFarmLocation(minions);
                if (minions.Count == minions.Count(x => Player.Distance(x) < Spells[ElVi.Spells.E].Range) &&
                    bestFarmPos.Position.IsValid() && bestFarmPos.MinionsHit > 1)
                {
                    Spells[ElVi.Spells.E].Cast();
                }
            }
        }

        #endregion

        #region OnLaneClear

        private static void OnLaneClear()
        {
            var useQ = ElViMenu._menu.Item("ElVi.LaneClear.Q").GetValue<bool>();
            var useE = ElViMenu._menu.Item("ElVi.LaneClear.E").GetValue<bool>();
            var playerMana = ElViMenu._menu.Item("ElVi.Clear.Player.Mana").GetValue<Slider>().Value;

            if (Player.ManaPercentage() < playerMana)
                return;

            var minions = MinionManager.GetMinions(Player.ServerPosition, Spells[ElVi.Spells.Q].Range);
            if (minions.Count <= 1)
            {
                return;
            }

            if (useQ && Spells[ElVi.Spells.Q].IsReady())
            {
                if (!Spells[ElVi.Spells.Q].IsCharging)
                {
                    Spells[ElVi.Spells.Q].StartCharging();
                }
                else
                {
                    var bestFarmPos = Spells[ElVi.Spells.Q].GetLineFarmLocation(minions);
                    if (minions.Count == minions.Count(x => Player.Distance(x) < Spells[ElVi.Spells.Q].Range) &&
                        bestFarmPos.Position.IsValid() && bestFarmPos.MinionsHit > 2)
                    {
                        Spells[ElVi.Spells.Q].Cast(bestFarmPos.Position);
                    }
                }
            }

            if (useE && Spells[ElVi.Spells.E].IsReady())
            {
                var bestFarmPos = Spells[ElVi.Spells.E].GetLineFarmLocation(minions);
                if (minions.Count == minions.Count(x => Player.Distance(x) < Spells[ElVi.Spells.E].Range) &&
                    bestFarmPos.Position.IsValid() && bestFarmPos.MinionsHit > 1)
                {
                    Spells[ElVi.Spells.E].Cast();
                }
            }
        }

        #endregion

        #region Harass

        private static void OnHarass()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(
                Spells[ElVi.Spells.Q].Range, TargetSelector.DamageType.Physical);

            if (_qTargetLock == null)
            {
                _qTargetLock = target;
            }
            else
            {
                _qTargetLock = null;
            }

            if (target == null || !target.IsValid)
            {
                return;
            }

            var useQ = ElViMenu._menu.Item("ElVi.Harass.Q").GetValue<bool>();

            if (useQ && Spells[ElVi.Spells.Q].IsReady())
            {
                if (Spells[ElVi.Spells.Q].IsCharging)
                {
                    Spells[ElVi.Spells.Q].Cast(target);
                    return;
                }

                if (!Spells[ElVi.Spells.Q].IsCharging)
                {
                    Spells[ElVi.Spells.Q].StartCharging();
                    return;
                }
            }
        }

        #endregion

        #region Combo

        private static void OnCombo(Obj_AI_Hero target)
        {
            /* var target = TargetSelector.GetTarget(Spells[ElVi.Spells.Q].Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValid)
                return;*/

            if (_qTargetLock != null)
            {
                target = _qTargetLock;
            }
            else
            {
                _qTargetLock = target;
            }

            if (target == null || !target.IsValid)
            {
                return;
            }

            var useQ = ElViMenu._menu.Item("ElVi.Combo.Q").GetValue<bool>();
            var useR = ElViMenu._menu.Item("ElVi.Combo.R").GetValue<bool>();
            var useI = ElViMenu._menu.Item("ElVi.Combo.I").GetValue<bool>();

            if (useQ && Spells[ElVi.Spells.Q].IsReady())
            {
                if (Spells[ElVi.Spells.Q].IsCharging)
                {
                    Spells[ElVi.Spells.Q].Cast(target);
                    return;
                }

                if (!Spells[ElVi.Spells.Q].IsCharging)
                {
                    Spells[ElVi.Spells.Q].StartCharging();
                    return;
                }
            }

            UseItems(target);

            if (useR && Spells[ElVi.Spells.R].IsReady() && Spells[ElVi.Spells.R].IsInRange(target))
            {
                var selectedEnemy =
                    HeroManager.Enemies.Where(
                        hero =>
                            hero.IsEnemy && !hero.HasBuff("BlackShield") || !hero.HasBuff("SivirShield") ||
                            !hero.HasBuff("BansheesVeil") ||
                            !hero.HasBuff("ShroudofDarkness") &&
                            ElViMenu._menu.Item("ElVi.Settings.R" + hero.BaseSkinName).GetValue<bool>())
                        .OrderByDescending(x => x.MaxHealth)
                        .FirstOrDefault();

                if (selectedEnemy == null || !selectedEnemy.IsValid)
                {
                    return;
                }

                var rTarget = TargetSelector.GetTarget(Spells[ElVi.Spells.R].Range, TargetSelector.DamageType.Physical);

                if (Spells[ElVi.Spells.R].CanCast(rTarget) &&
                    rTarget.Health <= (Spells[ElVi.Spells.Q].GetDamage(rTarget) * 2) + GetComboDamage(rTarget))
                {
                    Spells[ElVi.Spells.R].CastOnUnit(rTarget);
                }

                // Console.WriteLine(selectedEnemy);
                //Console.WriteLine("R Damage 1: {0}", rDamage);
                //Console.WriteLine("R Damage: {0}", Spells[ElVi.Spells.R].GetDamage(selectedEnemy));
            }


            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && useI)
            {
                Player.Spellbook.CastSpell(_ignite, target);
            }
        }

        #endregion

        #region FlashQ

        private static void FlashQ()
        {
            var target = TargetSelector.GetTarget(Spells[ElVi.Spells.Q].ChargedMaxRange, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValid)
                return;

            var position = Spells[ElVi.Spells.Q].GetPrediction(target, true).CastPosition;

            if (Spells[ElVi.Spells.Q].IsReady())
            {
                if (!Spells[ElVi.Spells.Q].IsCharging)
                {
                    Spells[ElVi.Spells.Q].StartCharging();
                }
                else
                {
                    ObjectManager.Player.Spellbook.CastSpell(_flash, position);
                    Spells[ElVi.Spells.Q].Cast(target.ServerPosition);
                }
            }
        }

        #endregion

        #region itemusage

        private static void UseItems(Obj_AI_Base target)
        {
            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            var ghost = ItemData.Youmuus_Ghostblade.GetItem();
            var cutlass = ItemData.Bilgewater_Cutlass.GetItem();
            var tiamat = ItemData.Tiamat_Melee_Only.GetItem();
            var hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();

            var useYoumuu = ElViMenu._menu.Item("ElVi.Items.Youmuu").GetValue<bool>();
            var useCutlass = ElViMenu._menu.Item("ElVi.Items.Cutlass").GetValue<bool>();
            var useBlade = ElViMenu._menu.Item("ElVi.Items.Blade").GetValue<bool>();

            var useBladeEhp = ElViMenu._menu.Item("ElVi.Items.Blade.EnemyEHP").GetValue<Slider>().Value;
            var useBladeMhp = ElViMenu._menu.Item("ElVi.Items.Blade.EnemyMHP").GetValue<Slider>().Value;


            if (tiamat.IsReady() && tiamat.IsOwned(Player) && tiamat.IsInRange(target))
            {
                tiamat.Cast();
            }

            if (hydra.IsReady() && hydra.IsOwned(Player) && hydra.IsInRange(target))
            {
                hydra.Cast();
            }
            // && (Player.Health / Player.MaxHealth) * 100 <= useBladeMhp

            //Console.WriteLine("Player healt {0}", Player.HealthPercentage());
            if (botrk.IsReady() && botrk.IsOwned(Player) && botrk.IsInRange(target) && useBlade &&
                Player.HealthPercentage() < useBladeMhp)
            {
                botrk.Cast(target);
            }
            //&& (target.Health / target.MaxHealth) * 100 <= useBladeEhp

            //Console.WriteLine("Target healt {0}", target.HealthPercentage());
            if (cutlass.IsReady() && cutlass.IsOwned(Player) && cutlass.IsInRange(target) && useCutlass &&
                target.HealthPercentage() < useBladeEhp)
            {
                cutlass.Cast(target);
            }

            if (ghost.IsReady() && ghost.IsOwned(Player) && target.IsValidTarget(Spells[ElVi.Spells.Q].Range) &&
                useYoumuu)
            {
                ghost.Cast();
            }
        }

        #endregion

        #region GetComboDamage   

        private static float GetRDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Spells[ElVi.Spells.R].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
            }

            return (float) damage;
        }

        public static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Spells[ElVi.Spells.Q].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }
            if (Spells[ElVi.Spells.E].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.E) * Spells[ElVi.Spells.E].Instance.Ammo +
                          (float) Player.GetAutoAttackDamage(enemy);
            }

            if (Spells[ElVi.Spells.R].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
            }

            return (float) damage;
        }

        #endregion

        #region Notifications 

        private static void ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            Notifications.AddNotification(new Notification(message, duration, dispose).SetTextColor(color));
        }

        #endregion

        #region IgniteDamage

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (_ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(_ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float) Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        #endregion
    }
}