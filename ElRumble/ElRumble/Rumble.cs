using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


namespace ElRumble
{
    /// <summary>
    /// ElRumble by jQuery - BETA
    /// Version 1.0.0.1
    /// 
    /// Combo
    /// Q, W, E, R
    /// Auto ignite when target is killable
    /// 
    /// Harass
    /// Q, E
    /// 
    /// Autoheat
    /// Q, W to control heat (Rumble passive)
    /// 
    /// Clear settings
    /// 
    /// - Lane clear
    /// Q,  E
    /// 
    /// - Jungleclear
    /// Q, E
    /// 
    /// - Lasthit
    /// Q
    /// 
    /// Drawings (Misc)
    /// Draws combo damage
    /// Q, W, E, R ranges
    /// 
    /// Extra
    /// Custom hitchanes in combo menu, default is set to high.
    /// Notifications when target is killable
    /// 
    /// Credits to xSalice for a part of his Rumble ult
    /// 
    /// Updated and tested 4/9/2015
    /// </summary>
    internal enum Spells
    {
        Q,
        W,
        E,
        R,
        R1
    }

    internal static class Rumble
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static Orbwalking.Orbwalker Orbwalker;
        private static SpellSlot _ignite;
        private static int _lastNotification = 0;

        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 600) },
            { Spells.W, new Spell(SpellSlot.W, 0) },
            { Spells.E, new Spell(SpellSlot.E, 850) },
            { Spells.R, new Spell(SpellSlot.R, 1700) },
            { Spells.R1, new Spell(SpellSlot.R, 800) }
        };


        #region Gameloaded 

        public static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Rumble")
                return;

            Notifications.AddNotification("ElRumble by jQuery", 5000);
            _ignite = Player.GetSpellSlot("summonerdot");

            spells[Spells.R].SetSkillshot(1700, 120, 1400, false, SkillshotType.SkillshotLine);
            spells[Spells.R1].SetSkillshot(0.25f, 110, 2600, false, SkillshotType.SkillshotLine);
            spells[Spells.E].SetSkillshot(0.5f, 90, 1200, true, SkillshotType.SkillshotLine);

            ElRumbleMenu.Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawings.OnDraw;
        }

        #endregion

        #region hitchance

        private static HitChance CustomHitChance
        {
            get { return GetHitchance(); }
        }

        private static HitChance GetHitchance()
        {
            switch (ElRumbleMenu._menu.Item("ElRumble.hitChance").GetValue<StringList>().SelectedIndex)
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

        #region OnUpdate

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    OnHarass();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    OnClear();
                    OnJungleClear();
                    break;

                case Orbwalking.OrbwalkingMode.LastHit:
                    OnLastHit();
                    break;
            }

            var keepHeat = ElRumbleMenu._menu.Item("ElRumble.KeepHeat.Activated", true).GetValue<KeyBind>().Active;
            if (keepHeat)
                KeepHeat();

            if (ElRumbleMenu._menu.Item("ElRumble.Misc.R").GetValue<KeyBind>().Active && spells[Spells.R].IsReady())
            {
                CastR();
            }

            var showNotifications = ElRumbleMenu._menu.Item("ElRumble.misc.Notifications").GetValue<bool>();

            if (showNotifications && Environment.TickCount - _lastNotification > 5000)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(1000) && GetComboDamage(h) > h.Health))
                {
                    ShowNotification(enemy.ChampionName + ": is killable", Color.White, 4000);
                    _lastNotification = Environment.TickCount;
                }
            }
        }

        #endregion

        #region Combo

        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            var rTarget = TargetSelector.GetTarget(spells[Spells.R].Range, TargetSelector.DamageType.Magical);

            if (target == null || !target.IsValid)
                return;

            var useQ = ElRumbleMenu._menu.Item("ElRumble.Combo.Q").GetValue<bool>();
            var useW = ElRumbleMenu._menu.Item("ElRumble.Combo.W").GetValue<bool>();
            var useE = ElRumbleMenu._menu.Item("ElRumble.Combo.E").GetValue<bool>();
            var useR = ElRumbleMenu._menu.Item("ElRumble.Combo.R").GetValue<bool>();
            var useI = ElRumbleMenu._menu.Item("ElRumble.Combo.Ignite").GetValue<bool>();
            var countEnemies = ElRumbleMenu._menu.Item("ElRumble.Combo.Count.Enemies").GetValue<Slider>().Value;

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                spells[Spells.Q].Cast(target);
            }

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
            {
                var pred = spells[Spells.E].GetPrediction(target);
                if (pred.Hitchance >= CustomHitChance)
                    spells[Spells.E].Cast(target);
            }


            if (useW && spells[Spells.W].IsReady())
            {
                spells[Spells.W].Cast();
            }

            if (useR && spells[Spells.R].IsReady() && Player.CountEnemiesInRange(spells[Spells.R].Range) >= countEnemies)
            {
                CastR();
            }

            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && useI)
            {
                Player.Spellbook.CastSpell(_ignite, target);
            }
        }

        #endregion

        #region Harass 

        private static void OnHarass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
                return;

            var useQ = ElRumbleMenu._menu.Item("ElRumble.Harass.Q").GetValue<bool>();
            var useE = ElRumbleMenu._menu.Item("ElRumble.Harass.E").GetValue<bool>();

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                spells[Spells.Q].Cast(target);
            }

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
            {
                var pred = spells[Spells.E].GetPrediction(target);
                if (pred.Hitchance >= CustomHitChance)
                    spells[Spells.E].Cast(target);
            }
        }

        #endregion

        #region OnLastHit

        private static void OnLastHit()
        {
            var useE = ElRumbleMenu._menu.Item("ElRumble.LastHit.E").GetValue<bool>();
            if (useE && spells[Spells.E].IsReady())
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.E].Range);
                {
                    foreach (var minion in
                        allMinions.Where(
                            minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.E)))
                    {
                        if (minion.IsValidTarget())
                        {
                            spells[Spells.E].Cast(minion);
                            return;
                        }
                    }
                }
            }
        }

        #endregion

        #region OnClear

        private static void OnClear()
        {
            var useQ = ElRumbleMenu._menu.Item("ElRumble.LaneClear.Q").GetValue<bool>();
            var useE = ElRumbleMenu._menu.Item("ElRumble.LaneClear.E").GetValue<bool>();

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.Q].Range);
            if (minions.Count <= 0)
                return;

            if (useQ && spells[Spells.Q].IsReady())
            {
                if (minions.Count > 1)
                {
                    var farmLocation = spells[Spells.Q].GetCircularFarmLocation(minions);
                    spells[Spells.Q].Cast(farmLocation.Position);
                }
            }

            if (useE && spells[Spells.E].IsReady())
            {
                spells[Spells.E].Cast(minions.FirstOrDefault());
            }
        }

        #endregion

        #region Jungleclear 

        private static void OnJungleClear()
        {
            var useQ = ElRumbleMenu._menu.Item("ElRumble.JungleClear.Q").GetValue<bool>();
            var useE = ElRumbleMenu._menu.Item("ElRumble.JungleClear.E").GetValue<bool>();

            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.Q].Range, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);

            if (minions.Count <= 0)
                return;

            if (useQ && spells[Spells.Q].IsReady())
            {
                if (minions.Count > 1)
                {
                    var farmLocation = spells[Spells.Q].GetCircularFarmLocation(minions);
                    spells[Spells.Q].Cast(farmLocation.Position);
                }
            }

            if (useE && spells[Spells.E].IsReady())
            {
                spells[Spells.E].Cast(minions.FirstOrDefault());
            }
        }

        #endregion

        #region KeepHeat

        private static void KeepHeat()
        {
            var useQ = ElRumbleMenu._menu.Item("ElRumble.Heat.Q").GetValue<bool>();
            var useW = ElRumbleMenu._menu.Item("ElRumble.Heat.W").GetValue<bool>();

            if (Player.Mana < 50)
            {
                if (useQ && spells[Spells.Q].IsReady())
                {
                    spells[Spells.Q].Cast(Game.CursorPos);
                }

                if (useW && spells[Spells.W].IsReady())
                {
                    spells[Spells.W].Cast();
                }
            }
        }

        #endregion

        #region Cast R

        //CREDITS TO XSALICE - Made a few changes to it
        private static void CastR()
        {
            var target = TargetSelector.GetTarget(spells[Spells.R].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;

            var vector1 = target.ServerPosition - Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * 300;

            spells[Spells.R1].UpdateSourcePosition(vector1, vector1);

            var pred = spells[Spells.R1].GetPrediction(target, true);

            if (Player.Distance(target.Position) < 400)
            {
                var midpoint = (Player.ServerPosition + pred.UnitPosition) / 2;

                vector1 = midpoint + Vector3.Normalize(pred.UnitPosition - Player.ServerPosition) * 800;
                var vector2 = midpoint - Vector3.Normalize(pred.UnitPosition - Player.ServerPosition) * 300;

                if (!IsPassWall(pred.UnitPosition, vector1) && !IsPassWall(pred.UnitPosition, vector2))
                    CastR2(vector1, vector2);
            }
            else if (!IsPassWall(pred.UnitPosition, vector1) && !IsPassWall(pred.UnitPosition, pred.CastPosition))
            {
                if (pred.Hitchance >= CustomHitChance)
                    CastR2(vector1, pred.CastPosition);
            }
        }

        private static void CastR2(Vector3 start, Vector3 end)
        {
            if (!spells[Spells.R].IsReady())
                return;

            spells[Spells.R].Cast(start, end);
        }

        #endregion

        #region Ignite
        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (_ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(_ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        #endregion

        #region ComboDamage

        public static float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (spells[Spells.Q].IsReady())
            {
                damage += spells[Spells.Q].GetDamage(enemy);
            }

            if (spells[Spells.W].IsReady())
            {
                damage += spells[Spells.W].GetDamage(enemy);
            }

            if (spells[Spells.E].IsReady())
            {
                damage += spells[Spells.E].GetDamage(enemy);
            }

            if (spells[Spells.R].IsReady())
            {
                damage += spells[Spells.R].GetDamage(enemy);
            }

            if (_ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(_ignite) != SpellState.Ready)
            {
                damage += (float)Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return damage;
        }

        #endregion

        #region Notifications 

        private static void ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            Notifications.AddNotification(new Notification(message, duration, dispose).SetTextColor(color));
        }

        #endregion


        #region Wall

        //CREDITS TO XSALICE
        private static bool IsWall(Vector2 pos)
        {
            return (NavMesh.GetCollisionFlags(pos.X, pos.Y) == CollisionFlags.Wall ||
                    NavMesh.GetCollisionFlags(pos.X, pos.Y) == CollisionFlags.Building);
        }

        private static bool IsPassWall(Vector3 start, Vector3 end)
        {
            double count = Vector3.Distance(start, end);
            for (uint i = 0; i <= count; i += 25)
            {
                Vector2 pos = start.To2D().Extend(Player.ServerPosition.To2D(), -i);
                if (IsWall(pos))
                    return true;
            }
            return false;
        }

        #endregion
    }
}
