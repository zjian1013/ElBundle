using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LeagueSharp.Common.Data;
using Collision = LeagueSharp.Common.Collision;


namespace ElXerath
{
    internal enum Spells
    {
        Q,
        W,
        E,
        R
    }

    internal class Xerath
    {

        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Orbwalking.Orbwalker _orbwalker;
        private static SpellSlot _ignite;


        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 1550)},
            { Spells.W, new Spell(SpellSlot.W, 1000)},
            { Spells.E, new Spell(SpellSlot.E, 1150)},
            { Spells.R, new Spell(SpellSlot.R, 675)}
        };

        #region Gameloaded 

        #region hitchance

        private static HitChance CustomHitChance
        {
            get { return GetHitchance(); }
        }

        private static HitChance GetHitchance()
        {
            switch (ElXerathMenu._menu.Item("ElXerath.hitChance").GetValue<StringList>().SelectedIndex)
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
            if (ObjectManager.Player.BaseSkinName != "Xerath")
                return;

            Console.WriteLine("Injected");

            Notifications.AddNotification("ElXerath by jQuery v1.0.0.0", 1000);

            spells[Spells.Q].SetSkillshot(0.6f, 100f, float.MaxValue, false, SkillshotType.SkillshotLine);
            spells[Spells.W].SetSkillshot(0.7f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            spells[Spells.E].SetSkillshot(0.25f, 60f, 1400f, true, SkillshotType.SkillshotLine);
            spells[Spells.R].SetSkillshot(0.7f, 120f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            spells[Spells.Q].SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 750, 1550, 1.5f);
            _ignite = Player.GetSpellSlot("summonerdot");


            ElXerathMenu.Initialize();
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

            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo(target);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    //JungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass(target);
                    break;
            }

            AutoHarassMode(target);
            KsMode();

            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health &&
               ElXerathMenu._menu.Item("ElXerath.Ignite").GetValue<bool>())
            {
                Player.Spellbook.CastSpell(_ignite, target);
            }
        }

        #endregion

        #region KSMode

        private static void KsMode()
        {
            var useKs = ElXerathMenu._menu.Item("ElXerath.misc.ks").GetValue<bool>();
            if (!useKs)
                return;

            var target = HeroManager.Enemies.FirstOrDefault(x => !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield) && spells[Spells.Q].CanCast(x) && (x.Health + (x.HPRegenRate / 2)) <= spells[Spells.Q].GetDamage(x));

            if (spells[Spells.Q].CanCast(target) && spells[Spells.Q].IsReady())
            {
                if (!spells[Spells.Q].IsCharging)
                {
                    spells[Spells.Q].StartCharging();
                    return;
                }
                else if (spells[Spells.Q].IsCharging)
                {
                    spells[Spells.Q].Cast(target);
                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                }
            }
        }

        #endregion

        #region Autoharass

        private static void AutoHarassMode(Obj_AI_Base target)
        {
            if (target == null || !target.IsValidTarget())
                return;

            if (ElXerathMenu._menu.Item("ElXerath.AutoHarass").GetValue<KeyBind>().Active)
            {
                var q = ElXerathMenu._menu.Item("ElXerath.UseQAutoHarass").GetValue<bool>();
                var w = ElXerathMenu._menu.Item("ElXerath.UseWAutoHarass").GetValue<bool>();
                var mana = ElXerathMenu._menu.Item("ElXerath.harass.mana").GetValue<Slider>().Value;

                if (Player.ManaPercentage() < mana)
                    return;

                if (w && spells[Spells.W].IsReady())
                {
                    spells[Spells.W].CastIfHitchanceEquals(target, CustomHitChance);
                }

                if (q && spells[Spells.Q].IsReady())
                {
                    if (!spells[Spells.Q].IsCharging)
                    {
                        spells[Spells.Q].StartCharging();
                        return;
                    }
                    else if (spells[Spells.Q].IsCharging)
                    {
                        spells[Spells.Q].CastIfHitchanceEquals(target, CustomHitChance);
                        Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                    }
                }
            }
        }

        #endregion

        #region Laneclear

        private static
            void LaneClear()
        {
            var clearQ = ElXerathMenu._menu.Item("ElXerath.clear.Q").GetValue<bool>();
            var clearW = ElXerathMenu._menu.Item("ElXerath.clear.Q").GetValue<bool>();
            var minmana = ElXerathMenu._menu.Item("minmanaclear").GetValue<Slider>().Value;

            if (Player.ManaPercentage() < minmana)
                return;

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.Q].ChargedMaxRange);
            if (minions.Count <= 0)
                return;

            if (spells[Spells.Q].IsReady() && clearQ)
            {
                
            }
        }

        #endregion

        #region Harass

        private static void Harass(Obj_AI_Base target)
        {
            if (target == null || !target.IsValidTarget())
                return;

            var harassQ = ElXerathMenu._menu.Item("ElXerath.Harass.Q").GetValue<bool>();
            var harassW = ElXerathMenu._menu.Item("ElXerath.Harass.W").GetValue<bool>();

            if (harassW && spells[Spells.W].IsReady())
            {
                spells[Spells.W].CastIfHitchanceEquals(target, CustomHitChance);
            }

            if (harassQ && spells[Spells.Q].IsReady())
            {
                if (!spells[Spells.Q].IsCharging)
                {
                    spells[Spells.Q].StartCharging();
                    return;
                }
                else if (spells[Spells.Q].IsCharging)
                {
                    spells[Spells.Q].CastIfHitchanceEquals(target, CustomHitChance);
                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                }
            }
        }

            
        #endregion


        #region Combo

        private static void Combo(Obj_AI_Base target)
        {
            if (target == null || !target.IsValidTarget())
                return;

            var comboQ = ElXerathMenu._menu.Item("ElXerath.Combo.Q").GetValue<bool>();
            var comboW = ElXerathMenu._menu.Item("ElXerath.Combo.Q").GetValue<bool>();
            var comboE = ElXerathMenu._menu.Item("ElXerath.Combo.E").GetValue<bool>();
            var comboR = ElXerathMenu._menu.Item("ElXerath.Combo.R").GetValue<bool>();


            if (comboW && spells[Spells.W].IsReady())
            {
                spells[Spells.W].CastIfHitchanceEquals(target, CustomHitChance);
            }

            if (comboE && spells[Spells.E].IsReady())
            {
                spells[Spells.E].CastIfHitchanceEquals(target, CustomHitChance);
            }

            if (comboQ && spells[Spells.Q].IsReady())
            {
                if (!spells[Spells.Q].IsCharging)
                {
                    spells[Spells.Q].StartCharging();
                    return;
                }
                else if (spells[Spells.Q].IsCharging)
                {
                    spells[Spells.Q].CastIfHitchanceEquals(target, CustomHitChance);
                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                }
            }
        }

        #endregion

        #region GetComboDamage   

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (spells[Spells.R].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
            }

            return (float)damage;
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