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
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static Orbwalking.Orbwalker Orbwalker;
        private static SpellSlot _ignite;
        private static int lastNotification = 0;

        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 1600) },
            { Spells.W, new Spell(SpellSlot.W, 1000) },
            { Spells.E, new Spell(SpellSlot.E, 1150) },
            { Spells.R, new Spell(SpellSlot.R, 5600) }
        };

        private static class RCombo
        {
            public static int CastSpell;
            public static int _index;
            public static Vector3 _position;
            public static bool _tapKey;
        }

        #region casting R

        public static bool CastingR
        {
            get
            {
                return ObjectManager.Player.HasBuff("XerathLocusOfPower2", true) ||
                       (ObjectManager.Player.LastCastedSpellName() == "XerathLocusOfPower2" &&
                        Environment.TickCount - ObjectManager.Player.LastCastedSpellT() < 500);
            }
        }

        #endregion

        #region Notifications 

        private static void ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            Notifications.AddNotification(new Notification(message, duration, dispose).SetTextColor(color));
        }

        #endregion

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

        #region Gameloaded 

        public static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Xerath")
            {
                return;
            }

            Console.WriteLine("Injected");

            Notifications.AddNotification("ElXerath by jQuery v1.0.0.6", 1000);

            spells[Spells.Q].SetSkillshot(0.6f, 100f, float.MaxValue, false, SkillshotType.SkillshotLine);
            spells[Spells.W].SetSkillshot(0.7f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            spells[Spells.E].SetSkillshot(0.25f, 60f, 1400f, true, SkillshotType.SkillshotLine);
            spells[Spells.R].SetSkillshot(0.7f, 120f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            spells[Spells.Q].SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 750, 1550, 1.5f);
            _ignite = Player.GetSpellSlot("summonerdot");

            ElXerathMenu.Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawings.Drawing_OnDraw;
            Obj_AI_Hero.OnIssueOrder += Obj_AI_Hero_OnIssueOrder;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Game.OnWndProc += Game_OnWndProc;
        }

        #endregion

        #region OnGameUpdate

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            var utarget = TargetSelector.GetTarget(spells[Spells.R].Range, TargetSelector.DamageType.Magical);
            spells[Spells.R].Range = 2000 + spells[Spells.R].Level * 1200;

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

            var showNotifications = ElXerathMenu._menu.Item("ElXerath.misc.Notifications").GetValue<bool>();

            if (spells[Spells.R].IsReady() &&  showNotifications &&Environment.TickCount - lastNotification > 5000)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget() && (float) Player.GetSpellDamage(h, SpellSlot.R) * 3 > h.Health))
                {
                    ShowNotification(enemy.ChampionName + ": 杀得了", Color.White, 4000);
                    lastNotification = Environment.TickCount;
                }
            }

            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);

            AutoHarassMode();
            KsMode();

            if (CastingR)
            {
                CastR(utarget);
            }

            if (spells[Spells.E].IsReady())
            {
                var useE = ElXerathMenu._menu.Item("ElXerath.Misc.E").GetValue<KeyBind>().Active;
                var eTarget = TargetSelector.GetTarget(spells[Spells.E].Range, TargetSelector.DamageType.Magical);

                if (useE)
                    spells[Spells.E].Cast(eTarget);
            }
        }

        #endregion

        #region Obj_AI_Hero_OnIssueOrder

        private static void Obj_AI_Hero_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {   
            var blockMovement = ElXerathMenu._menu.Item("ElXerath.R.Block").GetValue<bool>();
            if (CastingR && blockMovement)
            {
                args.Process = false;
            }
        }

        #endregion

        #region KSMode

        private static void KsMode()
        {
            var useKs = ElXerathMenu._menu.Item("ElXerath.misc.ks").GetValue<bool>();
            if (!useKs)
            {
                return;
            }

            var target =
                HeroManager.Enemies.FirstOrDefault(
                    x =>
                        !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield) &&
                        spells[Spells.Q].CanCast(x) && (x.Health + (x.HPRegenRate / 2)) <= spells[Spells.Q].GetDamage(x));

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
                }
            }
        }

        #endregion

        #region Autoharass

        private static void AutoHarassMode()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].ChargedMaxRange, TargetSelector.DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(spells[Spells.W].Range + spells[Spells.W].Width * 0.5f, TargetSelector.DamageType.Magical);

            if (target == null || !target.IsValidTarget())
            {
                return;
            }

            if (ElXerathMenu._menu.Item("ElXerath.AutoHarass").GetValue<KeyBind>().Active)
            {
                var q = ElXerathMenu._menu.Item("ElXerath.UseQAutoHarass").GetValue<bool>();
                var w = ElXerathMenu._menu.Item("ElXerath.UseWAutoHarass").GetValue<bool>();
                var mana = ElXerathMenu._menu.Item("ElXerath.harass.mana").GetValue<Slider>().Value;

                if (Player.ManaPercent < mana)
                    return;

                if (q && spells[Spells.Q].IsReady() && target.IsValidTarget(spells[Spells.Q].ChargedMaxRange))
                {
                    if (!spells[Spells.Q].IsCharging)
                    {
                        spells[Spells.Q].StartCharging();
                        return;
                    }
                    else if (spells[Spells.Q].IsCharging)
                    {
                        spells[Spells.Q].CastIfHitchanceEquals(target, CustomHitChance);
                    }
                }
                if (wTarget != null && w && spells[Spells.W].IsReady())
                {
                    spells[Spells.W].CastIfHitchanceEquals(wTarget, CustomHitChance);
                }
            }
        }

        #endregion

        #region Laneclear

        private static void LaneClear()
        {
            var clearQ = ElXerathMenu._menu.Item("ElXerath.clear.Q").GetValue<bool>();
            var clearW = ElXerathMenu._menu.Item("ElXerath.clear.W").GetValue<bool>();
            var minmana = ElXerathMenu._menu.Item("minmanaclear").GetValue<Slider>().Value;

            if (Player.ManaPercent < minmana)
                return;

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.Q].ChargedMaxRange);
            if (minions.Count <= 0)
            {
                return;
            }

            /*if (spells[Spells.Q].IsCharging)
                 {
                     if (minions.Max(x => x.Distance(Player, true)) < spells[Spells.Q].RangeSqr)
                     {
                         if (minions.Max(x => x.Distance(Player, true)) < spells[Spells.Q].RangeSqr)
                         {
                             spells[Spells.Q].Cast(spells[Spells.Q].GetLineFarmLocation(minions).Position);
                         }
                     }
                 }

                if (spells[Spells.Q].IsCharging)
                 {
                     return;
                 }

                 if (spells[Spells.Q].IsReady() && clearQ)
                 {
                     if (spells[Spells.Q].GetLineFarmLocation(minions).MinionsHit >= 0)
                     {
                         spells[Spells.Q].StartCharging();
                         return;
                     }
                 }
                 */

            if (clearQ && spells[Spells.Q].IsReady())
            {
                if (spells[Spells.Q].IsCharging)
                {
                    var bestFarmPos = spells[Spells.Q].GetLineFarmLocation(minions);
                    if (minions.Count == minions.Count(x => Player.Distance(x) < spells[Spells.Q].Range) 
                        && bestFarmPos.Position.IsValid()
                        && bestFarmPos.MinionsHit > 0 )
                        spells[Spells.Q].Cast(bestFarmPos.Position);
                }
                else if (minions.Count > 0)
                    spells[Spells.Q].StartCharging();
            }

  
            if (spells[Spells.W].IsReady() && clearW)
            {
                var farmLocation = spells[Spells.W].GetCircularFarmLocation(minions);
                spells[Spells.W].Cast(farmLocation.Position);
            }
        }

        #endregion

        #region JungleClear

        private static void JungleClear()
        {
            var clearQ = ElXerathMenu._menu.Item("ElXerath.jclear.Q").GetValue<bool>();
            var clearW = ElXerathMenu._menu.Item("ElXerath.jclear.W").GetValue<bool>();
            var clearE = ElXerathMenu._menu.Item("ElXerath.jclear.E").GetValue<bool>();
            var minmana = ElXerathMenu._menu.Item("minmanaclear").GetValue<Slider>().Value;

            if (Player.ManaPercent < minmana)
            {
                return;
            }

            var minions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, spells[Spells.W].Range, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);

            if (minions.Count <= 0)
                return;


            if (spells[Spells.Q].IsCharging)
            {
                if (minions.Max(x => x.Distance(Player, true)) < spells[Spells.Q].RangeSqr)
                {
                    if (minions.Max(x => x.Distance(Player, true)) < spells[Spells.Q].RangeSqr)
                    {
                        spells[Spells.Q].Cast(spells[Spells.Q].GetLineFarmLocation(minions).Position);
                    }
                }
            }

            if (spells[Spells.Q].IsCharging)
            {
                return;
            }

            if (spells[Spells.Q].IsReady() && clearQ)
            {
                if (spells[Spells.Q].GetLineFarmLocation(minions).MinionsHit >= 1)
                {
                    spells[Spells.Q].StartCharging();
                    return;
                }
            }

            if (spells[Spells.W].IsReady() && clearW)
            {
                var farmLocation = spells[Spells.W].GetCircularFarmLocation(minions);
                spells[Spells.W].Cast(farmLocation.Position);
            }

            if (spells[Spells.E].IsReady() && clearE)
            {
                spells[Spells.E].Cast();
            }
        }

        #endregion

        #region Harass

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].ChargedMaxRange, TargetSelector.DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(spells[Spells.W].Range + spells[Spells.W].Width * 0.5f, TargetSelector.DamageType.Magical);

            if (target == null || !target.IsValidTarget())
                return;

            var harassQ = ElXerathMenu._menu.Item("ElXerath.Harass.Q").GetValue<bool>();
            var harassW = ElXerathMenu._menu.Item("ElXerath.Harass.W").GetValue<bool>();

            if (wTarget != null && harassW && spells[Spells.W].IsReady())
            {
                spells[Spells.W].CastIfHitchanceEquals(wTarget, CustomHitChance);
            }

            if (harassQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target) && target.IsValidTarget(spells[Spells.Q].ChargedMaxRange))
            {
                if (!spells[Spells.Q].IsCharging)
                {
                    spells[Spells.Q].StartCharging();
                    return;
                }

                if (spells[Spells.Q].IsCharging)
                {
                    spells[Spells.Q].CastIfHitchanceEquals(target, CustomHitChance);
                }
            }
        }

        #endregion

        #region Combo

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].ChargedMaxRange, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(spells[Spells.E].Range, TargetSelector.DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(spells[Spells.W].Range + spells[Spells.W].Width * 0.5f, TargetSelector.DamageType.Magical);

            if (target == null || !target.IsValidTarget())
                return;

            var comboQ = ElXerathMenu._menu.Item("ElXerath.Combo.Q").GetValue<bool>();
            var comboW = ElXerathMenu._menu.Item("ElXerath.Combo.W").GetValue<bool>();
            var comboE = ElXerathMenu._menu.Item("ElXerath.Combo.E").GetValue<bool>();

            if (wTarget != null && comboW && spells[Spells.W].IsReady())
            {
                spells[Spells.W].CastIfHitchanceEquals(wTarget, CustomHitChance);
            }

            if (eTarget != null && comboE && spells[Spells.E].IsReady() && Player.Distance(target) < spells[Spells.E].Range)
            {
                spells[Spells.E].Cast(eTarget);
            }

            if (comboQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                if (!spells[Spells.Q].IsCharging)
                {
                    spells[Spells.Q].StartCharging();
                    return;
                }

                if (spells[Spells.Q].IsCharging)
                {
                    spells[Spells.Q].CastIfHitchanceEquals(target, CustomHitChance);
                }
            }

            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health &&
                ElXerathMenu._menu.Item("ElXerath.Ignite").GetValue<bool>())
            {
                Player.Spellbook.CastSpell(_ignite, target);
            }
        }

        #endregion

        #region XerathR

        private static void CastR(Obj_AI_Base target)
        {
            var useR = ElXerathMenu._menu.Item("ElXerath.R.AutoUseR").GetValue<bool>();
            var tapkey = ElXerathMenu._menu.Item("ElXerath.R.OnTap").GetValue<KeyBind>().Active;
            var ultRadius = ElXerathMenu._menu.Item("ElXerath.R.Radius").GetValue<Slider>().Value;
            var drawROn = ElXerathMenu._menu.Item("ElXerath.Draw.RON").GetValue<bool>();

            if (!useR)
                return;

            if (target == null || !target.IsValidTarget())
                return;

            var ultType = ElXerathMenu._menu.Item("ElXerath.R.Mode").GetValue<StringList>().SelectedIndex;

            if (target.Health - spells[Spells.R].GetDamage(target) < 0)
            {
                if (Utils.TickCount - RCombo.CastSpell <= 700)
                {
                    return;
                }
            }

            if ((RCombo._index != 0 && target.Distance(RCombo._position) > 1000))
            {
                if (Utils.TickCount - RCombo.CastSpell <= Math.Min(2500, target.Distance(RCombo._position) - 1000))
                {
                    return;
                }
            }


            /*var orb = ItemData.Scrying_Orb_Trinket.GetItem();
            if ((orb.IsOwned() && orb.IsReady()))
            {
                if (orb.IsOwned() && orb.IsReady() && (Player.Level >= 9 ? 3500f : orb.Range) >= Player.Distance(target))
                {
                    orb.Cast(target.Position);
                    Console.WriteLine("Cast ORB");
                }
            }*/

            switch (ultType)
            {
                case 0:
                    spells[Spells.R].Cast(target);
                    break;

                case 1:
                    var d = ElXerathMenu._menu.Item("Delay" + (RCombo._index + 1)).GetValue<Slider>().Value;
                    if (Utils.TickCount - RCombo.CastSpell > d)
                    {
                        spells[Spells.R].Cast(target, true);
                    }
                    break;

                case 2:
                    //if (tapkey)
                        if (RCombo._tapKey)
                            spells[Spells.R].Cast(target, true);
                    break;

                case 3:
                    if (spells[Spells.R].GetPrediction(target).Hitchance >= CustomHitChance)
                    {
                        spells[Spells.R].Cast(target, true);
                    }

                    break;

                case 4:
                   
                    if (Game.CursorPos.Distance(target.ServerPosition) < ultRadius
                        && ObjectManager.Player.Distance(target.ServerPosition) < spells[Spells.R].Range)
                    {
                        spells[Spells.R].Cast(target, true);
                    }

                    if (drawROn)
                    {
                        Render.Circle.DrawCircle(Game.CursorPos, ultRadius, Color.White);
                    }

                    break;
            }
        }

        #endregion


        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!gapcloser.Sender.IsValidTarget(spells[Spells.E].Range) ||
                gapcloser.Sender.Distance(ObjectManager.Player) > spells[Spells.E].Range)
            {
                return;
            }

            if (gapcloser.Sender.IsValidTarget(spells[Spells.E].Range) &&
                (ElXerathMenu._menu.Item("ElXerath.misc.Antigapcloser").GetValue<bool>() && spells[Spells.E].IsReady()))
            {
                spells[Spells.E].Cast(gapcloser.Sender);
            }
        }

        #region GetComboDamage   

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (spells[Spells.R].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
            }

            return (float) damage;
        }

        #endregion

        //Thanks to Esk0r for the R
        static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == (uint)WindowsMessages.WM_KEYUP)
                RCombo._tapKey = true;
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "XerathLocusOfPower2")
                {
                    RCombo.CastSpell = 0;
                    RCombo._index = 0;
                    RCombo._position = new Vector3();
                    RCombo._tapKey = false;
                }
                else if (args.SData.Name == "xerathlocuspulse")
                {
                    RCombo.CastSpell = Utils.TickCount;
                    RCombo._index++;
                    RCombo._position = args.End;
                    RCombo._tapKey = false;
                }
            }
        }
        //End Thanks to Esk0r for the R

        #region Ignite Damage

        private static float IgniteDamage(Obj_AI_Base target)
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
