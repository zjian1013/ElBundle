using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LeagueSharp.Common.Data;


namespace ElSejuani
{
    internal enum Spells
    {
        Q,
        W,
        E,
        R
    }

    internal static class Sejuani
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
            { Spells.Q, new Spell(SpellSlot.Q, 650)},
            { Spells.W, new Spell(SpellSlot.W, 350)},
            { Spells.E, new Spell(SpellSlot.E, 1000)},
            { Spells.R, new Spell(SpellSlot.R, 1175)}
        };

        #region Gameloaded 

        #region hitchance

        private static HitChance CustomHitChance
        {
            get
            {
                return GetHitchance();
            }
        }

        private static HitChance GetHitchance()
        {
            switch (ElSejuaniMenu._menu.Item("ElSejuani.hitChance").GetValue<StringList>().SelectedIndex)
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

        #region IsFrozen

        private static bool IsFrozen(Obj_AI_Base target)
        {
            return target.HasBuff("SejuaniFrost");
        }

        #endregion

        public static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.CharData.BaseSkinName != "Sejuani")
                return;

            Console.WriteLine("Injected");
            Notifications.AddNotification("ElSejuani by jQuery v1.0.0.0", 1000);

            spells[Spells.Q].SetSkillshot(0, 70, 1600, true, SkillshotType.SkillshotLine);
            spells[Spells.R].SetSkillshot(250, 110, 1600, false, SkillshotType.SkillshotLine);

            _ignite = Player.GetSpellSlot("summonerdot");

            ElSejuaniMenu.Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawings.OnDraw;

            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        #endregion

        #region OnGameUpdate

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
  
            switch (Orbwalker.ActiveMode)
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

            //var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            //Game.PrintChat("Buffs: {0}", string.Join(" | ", target.Buffs.Select(b => b.DisplayName)));
           // Console.WriteLine("Buffs: {0}", string.Join(" | ", target.Buffs.Select(b => b.DisplayName)));

        }
        #endregion

        #region Combo

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);

            if (target == null || !target.IsValidTarget())
                return;

            var comboQ = ElSejuaniMenu._menu.Item("ElSejuani.Combo.Q").GetValue<bool>();
            var comboE = ElSejuaniMenu._menu.Item("ElSejuani.Combo.E").GetValue<bool>();
            var comboW = ElSejuaniMenu._menu.Item("ElSejuani.Combo.E").GetValue<bool>();
            var comboR = ElSejuaniMenu._menu.Item("ElSejuani.Combo.R").GetValue<bool>();
            var countEnemyR = ElSejuaniMenu._menu.Item("ElSejuani.Combo.R.Count").GetValue<Slider>().Value;
            var countEnemyE = ElSejuaniMenu._menu.Item("ElSejuani.Combo.E.Count").GetValue<Slider>().Value;

            if (comboQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                spells[Spells.Q].Cast(target);
            }

            if (comboW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
            {
                spells[Spells.W].Cast();
            }

            if (comboE && spells[Spells.E].IsReady() && IsFrozen(target))
            {
                if (IsFrozen(target)) // && spells[Spells.E].GetDamage(target) > target.Health
                {
                    spells[Spells.E].Cast(target);
                }
                    
                if (IsFrozen(target) &&
                    target.ServerPosition.Distance(Player.ServerPosition, true) >= spells[Spells.E].Range)
                {
                    spells[Spells.E].Cast(target);
                }      
            }

            if (comboR && spells[Spells.R].IsReady() && spells[Spells.R].IsInRange(target) && Player.CountEnemiesInRange(spells[Spells.R].Range) >= countEnemyR)
            {
                var prediction = spells[Spells.R].GetPrediction(target).Hitchance;
                if (prediction >= CustomHitChance)
                    spells[Spells.R].CastOnBestTarget();
            }
        }

        #endregion

        #region Laneclear

        public static BuffInstance GetFrost(Obj_AI_Base target)
        {
            return target.Buffs.FirstOrDefault(buff => buff.Name == "sejuanifrost");
        }

        private static void LaneClear()
        {
            var clearQ = ElSejuaniMenu._menu.Item("ElSejuani.Clear.Q").GetValue<bool>();
            var clearW = ElSejuaniMenu._menu.Item("ElSejuani.Clear.Q").GetValue<bool>();
            var clearE = ElSejuaniMenu._menu.Item("ElSejuani.Clear.E").GetValue<bool>();
            var minmana = ElSejuaniMenu._menu.Item("minmanaclear").GetValue<Slider>().Value;
            var minQ = ElSejuaniMenu._menu.Item("ElSejuani.Clear.Q.Count").GetValue<Slider>().Value;

            if (Player.ManaPercentage() < minmana)
                return;

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.Q].Range);
            if (minions.Count <= 0)
                return;

            foreach (var minion in minions)
            {
                if (spells[Spells.Q].IsReady() && clearQ)
                {
                    if (spells[Spells.Q].GetLineFarmLocation(minions).MinionsHit >= minQ)
                    {
                        spells[Spells.Q].Cast(spells[Spells.Q].GetLineFarmLocation(minions).Position);
                        return;
                    }
                }

                if (spells[Spells.W].IsReady() && clearW && minion.ServerPosition.Distance(Player.ServerPosition, true) >= spells[Spells.W].Range)
                {
                    spells[Spells.W].Cast();
                }

                if (spells[Spells.E].IsReady() && clearE &&
                    minions[0].Health + (minions[0].HPRegenRate / 2) <= spells[Spells.E].GetDamage(minion) && minion.HasBuff("sejuanifrost"))
                {
                    spells[Spells.E].Cast();
                }
            }
        }

        #endregion

        #region jungle

        private static void JungleClear()
        {
            var clearQ = ElSejuaniMenu._menu.Item("ElSejuani.Clear.Q").GetValue<bool>();
            var clearW = ElSejuaniMenu._menu.Item("ElSejuani.Clear.Q").GetValue<bool>();
            var clearE = ElSejuaniMenu._menu.Item("ElSejuani.Clear.E").GetValue<bool>();
            var minmana = ElSejuaniMenu._menu.Item("minmanaclear").GetValue<Slider>().Value;
            var minQ = ElSejuaniMenu._menu.Item("ElSejuani.Clear.Q.Count").GetValue<Slider>().Value;
           
            if (Player.ManaPercentage() < minmana)
                return;

            var minions = MinionManager.GetMinions(
               ObjectManager.Player.ServerPosition, spells[Spells.W].Range, MinionTypes.All, MinionTeam.Neutral,
               MinionOrderTypes.MaxHealth);

            if (minions.Count <= 0)
                return;

            foreach (var minion in minions)
            {
                if (spells[Spells.Q].IsReady() && clearQ)
                {
                    if (spells[Spells.Q].GetLineFarmLocation(minions).MinionsHit >= minQ)
                    {
                        spells[Spells.Q].Cast(spells[Spells.Q].GetLineFarmLocation(minions).Position);
                        return;
                    }
                }

                if (spells[Spells.W].IsReady() && clearW && minion.ServerPosition.Distance(Player.ServerPosition, true) >= spells[Spells.W].Range)
                {
                    spells[Spells.W].Cast();
                }

                if (spells[Spells.E].IsReady() && clearE &&
                    minions[0].Health + (minions[0].HPRegenRate / 2) <= spells[Spells.E].GetDamage(minion) && minion.HasBuff("sejuanifrost"))
                {
                    spells[Spells.E].Cast();
                }
            }
        }

        #endregion

        #region Harass

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;

            var harassQ = ElSejuaniMenu._menu.Item("ElSejuani.Harass.Q").GetValue<bool>();
            var harassW = ElSejuaniMenu._menu.Item("ElSejuani.Harass.W").GetValue<bool>();
            var harassE = ElSejuaniMenu._menu.Item("ElSejuani.Harass.E").GetValue<bool>();
            var minmana = ElSejuaniMenu._menu.Item("ElSejuani.harass.mana").GetValue<Slider>().Value;

            if (Player.ManaPercentage() < minmana)
                return;

            if (harassQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                spells[Spells.Q].Cast(target);
            }

            if (harassW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
            {
                spells[Spells.W].Cast();
            }

            if (harassE && spells[Spells.E].IsReady())
            {
                if (IsFrozen(target) && spells[Spells.E].GetDamage(target) > target.Health)
                    spells[Spells.E].Cast(target);

                if (IsFrozen(target) &&
                    target.ServerPosition.Distance(Player.ServerPosition, true) >
                    Math.Pow(spells[Spells.E].Range * 0.8, 2))
                    spells[Spells.E].Cast(target);
            }
        }
        #endregion

        #region Intterupt

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel != Interrupter2.DangerLevel.High || sender.Distance(Player) > spells[Spells.Q].Range)
                return;

            if (sender.IsValidTarget(spells[Spells.Q].Range) && args.DangerLevel == Interrupter2.DangerLevel.High && spells[Spells.Q].IsReady())
            {
                spells[Spells.Q].Cast(sender);
            }
        }

        #endregion

        #region GapCloser

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!gapcloser.Sender.IsValidTarget(spells[Spells.Q].Range))
                return;

            if (gapcloser.Sender.Distance(Player) > spells[Spells.Q].Range)
                return;

            var useQ = ElSejuaniMenu._menu.Item("ElSejuani.Interupt.Q").GetValue<bool>();
            var useR = ElSejuaniMenu._menu.Item("ElSejuani.Interupt.R").GetValue<bool>();

            if (gapcloser.Sender.IsValidTarget(spells[Spells.Q].Range))
            {
                if (useQ && spells[Spells.Q].IsReady())
                {
                    spells[Spells.Q].Cast(gapcloser.Sender);
                }

                if (useR && !spells[Spells.Q].IsReady() && spells[Spells.R].IsReady())
                {
                    spells[Spells.R].Cast(gapcloser.Sender);
                }
            }
        }
        #endregion
    }
}
