using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;


namespace ElSinged
{
    internal enum Spells
    {
        Q,
        W,
        E,
        R
    }

    internal class Singed
    {
        private static String hero = "Singed";
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Orbwalking.Orbwalker _orbwalker;

        private static SpellSlot Ignite;
        private static bool useQAgain;


        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 0)},
            { Spells.W, new Spell(SpellSlot.W, 1000)},
            { Spells.E, new Spell(SpellSlot.E, 125)},
            { Spells.R, new Spell(SpellSlot.R, 0)}
        };

        #region hitchance

        private static HitChance CustomHitChance
        {
            get { return GetHitchance(); }
        }

        private static HitChance GetHitchance()
        {
            switch (ElSingedMenu._menu.Item("ElSinged.hitChance").GetValue<StringList>().SelectedIndex)
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

        #region IgniteDamage

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        #endregion

        #region Gameloaded 

        public static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Singed")
             return;
          
            Console.WriteLine("Injected");
               
            Ignite = Player.GetSpellSlot("summonerdot");

            Notifications.AddNotification("ElSinged by jQuery v1.0.0.3", 10000);
            spells[Spells.W].SetSkillshot(0.5f, 350, 700, false, SkillshotType.SkillshotCircle);


            useQAgain = true;

            ElSingedMenu.Initialize();
            Game.OnUpdate += OnGameUpdate;
            Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;
            Drawing.OnDraw += Drawings.Drawing_OnDraw;
            checkTime = Environment.TickCount;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        #endregion

        private static int checkTime = 0;

       private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (spells[Spells.Q].Instance.Name == args.SData.Name)
            {
                checkTime = Environment.TickCount + 1000;
            }
        }

        
        #region OnGameUpdate

        private static void OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(spells[Spells.W].Range, TargetSelector.DamageType.Magical);

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

        #region Laneclear

        private static void LaneClear()
        {
            var clearQ = ElSingedMenu._menu.Item("ElSinged.Laneclear.Q").GetValue<bool>();
            var clearE = ElSingedMenu._menu.Item("ElSinged.Laneclear.E").GetValue<bool>();

            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, 400, MinionTypes.All);
            if (minions.Count > 1)
            {
                foreach (var minion in minions)
                {
                    var minionHP = HealthPrediction.GetHealthPrediction(minion, (int)spells[Spells.E].Delay);
                    if (spells[Spells.E].GetDamage(minion) > minion.Health && minionHP > 0 && minion.IsValidTarget(spells[Spells.E].Range) && clearE)
                    {
                        spells[Spells.E].CastOnUnit(minion, true);
                    }

                    if(spells[Spells.Q].IsReady() && clearQ)
                    {
                        if (!PosionActivation && !PosionActive())
                        {
                            spells[Spells.Q].Cast();
                            PosionActivation = true;
                            Utility.DelayAction.Add(1000, () => PosionActivation = false);
                        }
                    }
                }
            }
        }

        #endregion

        #region Harass

        private static void Harass(Obj_AI_Hero target)
        {
            if (target == null || !target.IsValidTarget())
                return;

            var harassQ = ElSingedMenu._menu.Item("ElSinged.Harass.Q").GetValue<bool>();
            var harassW = ElSingedMenu._menu.Item("ElSinged.Harass.W").GetValue<bool>();
            var harassE = ElSingedMenu._menu.Item("ElSinged.Harass.E").GetValue<bool>();

            if (harassQ && spells[Spells.Q].IsReady() && !PosionActivation && !PosionActive())
            {
                spells[Spells.Q].Cast(Player);
                PosionActivation = true;
                Utility.DelayAction.Add(1000, () => PosionActivation = false);
            }

            if (PosionActive())
            {
                if (target.HasBuff("Fling") && harassW && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) == false && spells[Spells.W].IsReady())
                {
                    spells[Spells.W].CastIfHitchanceEquals(target, CustomHitChance);
                }

                if (Player.Distance(target) >= 500)
                {
                    spells[Spells.W].CastIfHitchanceEquals(target, CustomHitChance);
                }

                if (harassE && spells[Spells.E].IsReady())
                {
                    spells[Spells.E].CastOnUnit(target);
                }
            }
        }

        #endregion

        private static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (spells[Spells.E].IsReady())
                {
                    spells[Spells.E].Cast();
                }
                //args.Process = false;
            }
        }

        #region Combo 

        private static bool PosionActive()
        {
            return ObjectManager.Player.Buffs.Any(a => a.DisplayName == "Poison Trail");
        }

        private static bool PosionActivation = false;

        private static void Combo(Obj_AI_Hero target)
        {

            if (target == null || !target.IsValidTarget())
                return;

            var comboQ = ElSingedMenu._menu.Item("ElSinged.Combo.Q").GetValue<bool>();
            var comboW = ElSingedMenu._menu.Item("ElSinged.Combo.W").GetValue<bool>();
            var comboE = ElSingedMenu._menu.Item("ElSinged.Combo.E").GetValue<bool>();
            var comboR = ElSingedMenu._menu.Item("ElSinged.Combo.R").GetValue<bool>();
            var comboCount = ElSingedMenu._menu.Item("ElSinged.Combo.R.Count").GetValue<Slider>().Value;
            var useIgnite = ElSingedMenu._menu.Item("ElSinged.Combo.Ignite").GetValue<bool>();
            var exploit = ElSingedMenu._menu.Item("exploit").GetValue<bool>();
            var delay = ElSingedMenu._menu.Item("delayms").GetValue<Slider>().Value;


            if (comboQ && spells[Spells.Q].IsReady())
            {
                if (!exploit && checkTime <= Environment.TickCount && !PosionActivation && !PosionActive())
                {
                    spells[Spells.Q].Cast(Player);
                    PosionActivation = true;
                    Utility.DelayAction.Add(1000, () => PosionActivation = false);
                }
            }

            if (exploit)
            {
                if (PosionActive())
                {
                    spells[Spells.Q].CastOnUnit(Player);
                }
                if (PosionActive() == false && useQAgain)
                {
                    spells[Spells.Q].CastOnUnit(Player);
                    useQAgain = false;
                    Utility.DelayAction.Add(delay, () => useQAgain = true);
                }
            }

            if (PosionActive())
            {      
                if (target.HasBuff("Fling") && comboW && target.IsValidTarget() && spells[Spells.W].IsReady())
                {
                    spells[Spells.W].CastIfHitchanceEquals(target, CustomHitChance);
                }
         
                if (target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) == false && comboW)
                {
                    spells[Spells.W].CastIfHitchanceEquals(target, CustomHitChance);
                }

                if (comboE && spells[Spells.E].IsReady())
                {
                    spells[Spells.E].CastOnUnit(target);
                }

                if (Player.CountEnemiesInRange(spells[Spells.W].Range) >= comboCount && comboR)
                {
                    spells[Spells.R].Cast(Player);
                }
            }

            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && useIgnite)
            {
                Player.Spellbook.CastSpell(Ignite, target);
            }
        }
        #endregion
    }
}