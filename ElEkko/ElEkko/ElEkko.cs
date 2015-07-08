using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace ElEkko
{
    internal enum Spells
    {
        Q,
        W,
        E,
        R
    }

    internal static class ElEkko
    {
        public static String ScriptVersion { get { return typeof(ElEkko).Assembly.GetName().Version.ToString(); } }
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static Orbwalking.Orbwalker Orbwalker;
        private static SpellSlot ignite;
        private static int lastNotification = 0;
        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 950) },
            { Spells.W, new Spell(SpellSlot.W, 1600) },
            { Spells.E, new Spell(SpellSlot.E, 425) },
            { Spells.R, new Spell(SpellSlot.R, 400) }
        };

        public static GameObject Troy { get; set; }
        private static Dictionary<float, float> incomingDamage = new Dictionary<float, float>();
        private static Dictionary<float, float> instantDamage = new Dictionary<float, float>();
        public static float IncomingDamage
        {
            get { return incomingDamage.Sum(e => e.Value) + instantDamage.Sum(e => e.Value); }
        }

        #region OnLoad
        public static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.CharData.BaseSkinName != "Ekko")
            {
                return;
            }

            Notifications.AddNotification("ElEkko by jQuery 1.0.0.1", 10000);
            ignite = Player.GetSpellSlot("summonerdot");

            spells[Spells.Q].SetSkillshot(0.25f, 60, 1650f, false, SkillshotType.SkillshotLine);
            spells[Spells.W].SetSkillshot(2.5f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            ElEkkoMenu.Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawings.OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            GameObject.OnDelete += Obj_AI_Base_OnDelete;
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
                    OnLaneClear();
                    OnJungleClear();
                    break;
            }

            FleeMode();

            var showNotifications = ElEkkoMenu._menu.Item("ElEkko.misc.Notifications").GetValue<bool>();

            if (showNotifications && Environment.TickCount - lastNotification > 5000)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(1000) && GetComboDamage(h) > h.Health))
                {
                    ShowNotification(enemy.ChampionName + ": is killable", Color.White, 4000);
                    lastNotification = Environment.TickCount;
                }
            }

            var twoStacksQ = ElEkkoMenu._menu.Item("ElEkko.Combo.Auto.Q").GetValue<bool>();
            if (twoStacksQ)
            {
                var qtarget = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
                if (qtarget == null || !qtarget.IsValid || !Orbwalking.CanMove(1))
                    return;

                if (CountPassive(qtarget) == 2 && qtarget.Distance(Player.Position) <= spells[Spells.Q].Range)
                {
                    var pred = spells[Spells.Q].GetPrediction(qtarget);
                    if (pred.Hitchance >= HitChance.High)
                        spells[Spells.Q].Cast(pred.CastPosition);
                }
            }

            

            SaveMode();
            KillSteal();
            AutoHarass();

            var rtext = ElEkkoMenu._menu.Item("ElEkko.R.text").GetValue<bool>();
            if (Troy != null && rtext)
            {
                var enemyCount =
                    HeroManager.Enemies.Count(
                        h => h.IsValidTarget() && h.Distance(Troy.Position) < spells[Spells.R].Range);
                Drawing.DrawText(
                    Drawing.Width * 0.44f,
                    Drawing.Height * 0.80f,
                    Color.White,
                    "There are {0} in R range",
                    enemyCount);
            }
        }
        #endregion

        #region Autoharass

        private static void AutoHarass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
            {
                return;
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Player.IsRecalling())
                return;

            if (ElEkkoMenu._menu.Item("ElEkko.AutoHarass.Q").GetValue<KeyBind>().Active)
            {
                var mana = ElEkkoMenu._menu.Item("ElEkko.Harass.Q.Mana").GetValue<Slider>().Value;

                if (Player.ManaPercent < mana) return;

                if (spells[Spells.Q].IsReady() && target.Distance(Player.Position) <= spells[Spells.Q].Range - 50 && !Player.IsDashing())
                {
                    spells[Spells.Q].Cast(target);
                }
            }
        }

        #endregion

        #region OnLaneClear

        private static void OnJungleClear()
        {
            var useQ = ElEkkoMenu._menu.Item("ElEkko.JungleClear.Q").GetValue<bool>();
            var useW = ElEkkoMenu._menu.Item("ElEkko.JungleClear.W").GetValue<bool>();
            var mana = ElEkkoMenu._menu.Item("ElEkko.JungleClear.mana").GetValue<Slider>().Value;
            var qMinions = ElEkkoMenu._menu.Item("ElEkko.JungleClear.Minions").GetValue<Slider>().Value;

            if (Player.ManaPercent < mana)
                return;

            var minions =MinionManager.GetMinions(spells[Spells.Q].Range, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);
            if (minions.Count <= 0)
            {
                return;
            }

            var minionsInRange = minions.Where(x => spells[Spells.Q].IsInRange(x));
            var objAiBases = minionsInRange as IList<Obj_AI_Base> ?? minionsInRange.ToList();
            if (objAiBases.Count() >= qMinions && useQ)
            {
                int qKills = 0;
                foreach (var minion in objAiBases)
                {
                    if (spells[Spells.Q].GetDamage(minion) < minion.Health)
                    {
                        qKills++;

                        if (qKills >= qMinions)
                        {
                            var bestFarmPos = spells[Spells.Q].GetLineFarmLocation(minions);
                            spells[Spells.Q].Cast(bestFarmPos.Position);
                        }
                    }
                }
            }


            if (useW && spells[Spells.W].IsReady())
            {
                var mobs = MinionManager.GetMinions(spells[Spells.W].Range, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);

                if (mobs.Count <= 0)
                {
                    return;
                }

                var bestFarmPos = spells[Spells.W].GetCircularFarmLocation(minions);
                spells[Spells.W].Cast(bestFarmPos.Position);
            }
        }

        #endregion

        #region OnLaneClear

        private static void OnLaneClear()
        {
            var useQ = ElEkkoMenu._menu.Item("ElEkko.LaneClear.Q").GetValue<bool>();
            var mana = ElEkkoMenu._menu.Item("ElEkko.LaneClear.mana").GetValue<Slider>().Value;
            var qMinions = ElEkkoMenu._menu.Item("ElEkko.LaneClear.Minions").GetValue<Slider>().Value;

            if (Player.ManaPercent < mana)
                return;

            var minions = MinionManager.GetMinions(spells[Spells.Q].Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
            if (minions.Count <= 0)
            {
                return;
            }

            var minionsInRange = minions.Where(x => spells[Spells.Q].IsInRange(x));
            var objAiBases = minionsInRange as IList<Obj_AI_Base> ?? minionsInRange.ToList();
            if (objAiBases.Count() >= qMinions && useQ)
            {
                int qKills = 0;
                foreach (var minion in objAiBases)
                {
                    if (spells[Spells.Q].GetDamage(minion) < minion.Health)
                    {
                        qKills++;

                        if (qKills >= qMinions)
                        {
                            var bestFarmPos = spells[Spells.Q].GetLineFarmLocation(minions);
                            spells[Spells.Q].Cast(bestFarmPos.Position);
                            return;
                        }
                    }
                }
            }
        }

        #endregion

        #region CountPassive

        private static int CountPassive(Obj_AI_Base target)
        {
            var ekkoPassive = target.Buffs.FirstOrDefault(x => x.Name == "EkkoStacks");
            if (ekkoPassive != null)
            {
                return ekkoPassive.Count;
            }

            return 0;
        }
        #endregion

        #region Harass

        private static void OnHarass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;

            var useQ = ElEkkoMenu._menu.Item("ElEkko.Harass.Q").GetValue<bool>();
            var useE = ElEkkoMenu._menu.Item("ElEkko.Harass.E").GetValue<bool>();
            var mana = ElEkkoMenu._menu.Item("ElEkko.Harass.Q.Mana").GetValue<Slider>().Value;

            if (Player.ManaPercent < mana) return;

            if (useQ && spells[Spells.Q].IsReady() && target.Distance(Player.Position) <= spells[Spells.Q].Range
                    && !Player.IsDashing())
            {
                spells[Spells.Q].Cast(target);
            }

            if (useE && spells[Spells.E].IsReady()
                && !spells[Spells.Q].IsReady()
                && spells[Spells.Q].IsInRange(target)
                && !ObjectManager.Player.UnderTurret(true)
                && target.HasBuff("EkkoStacks"))
            {
                var eCast = ElEkkoMenu._menu.Item("ElEkko.Combo.E.Cast").GetValue<StringList>().SelectedIndex;
                switch (eCast)
                {
                    case 0:
                        spells[Spells.E].Cast(target.Position);
                        break;

                    case 1:
                        spells[Spells.E].Cast(Game.CursorPos);
                        break;
                }
            }
        }

        #endregion

        #region Combo
        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);

            var wtarget = TargetSelector.GetTarget(spells[Spells.W].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;

            var useQ = ElEkkoMenu._menu.Item("ElEkko.Combo.Q").GetValue<bool>();
            var useW = ElEkkoMenu._menu.Item("ElEkko.Combo.W").GetValue<bool>();
            var useE = ElEkkoMenu._menu.Item("ElEkko.Combo.E").GetValue<bool>();
            var useR = ElEkkoMenu._menu.Item("ElEkko.Combo.R").GetValue<bool>();
            var useRkill = ElEkkoMenu._menu.Item("ElEkko.Combo.R.Kill").GetValue<bool>();
            var useIgnite = ElEkkoMenu._menu.Item("ElEkko.Combo.Ignite").GetValue<bool>();

            var enemies = ElEkkoMenu._menu.Item("ElEkko.Combo.W.Count").GetValue<Slider>().Value;
            var enemiesRrange = ElEkkoMenu._menu.Item("ElEkko.Combo.R.Enemies").GetValue<Slider>().Value;

            if (useQ && spells[Spells.Q].IsReady() && target.Distance(Player.Position) <= spells[Spells.Q].Range - 50
                    && !Player.IsDashing())
            {
                spells[Spells.Q].Cast(target);
            }

            if (useW && spells[Spells.W].IsReady())
            {
                if (target.Distance(Player.Position) >= spells[Spells.E].Range) return;

               if (Player.CountEnemiesInRange(spells[Spells.W].Range) >= enemies)
                {
                    var pred = spells[Spells.W].GetPrediction(wtarget);
                    if (pred.Hitchance >= HitChance.High)
                        spells[Spells.W].Cast(pred.CastPosition);
                }
                else if (wtarget.HasBuffOfType(BuffType.Slow) || wtarget.HasBuffOfType(BuffType.Taunt)
                         || wtarget.HasBuffOfType(BuffType.Stun)
                         || wtarget.HasBuffOfType(BuffType.Snare)
                         && target.Distance(Player.Position) <= spells[Spells.E].Range)
                {
                    var pred = spells[Spells.W].GetPrediction(wtarget);
                    if (pred.Hitchance >= HitChance.High)
                        spells[Spells.W].Cast(pred.CastPosition);
                }
                else
                {
                    if (target.ServerPosition.Distance(Player.Position, true) > spells[Spells.E].Range * spells[Spells.E].Range)
                    {
                        if (spells[Spells.W].GetPrediction(wtarget).Hitchance >= HitChance.VeryHigh)
                        {
                            var pred = spells[Spells.W].GetPrediction(wtarget);
                            if (pred.Hitchance >= HitChance.High)
                                spells[Spells.W].Cast(pred.CastPosition);
                        }
                    }
                }
            }

            if (useE && spells[Spells.E].IsReady() 
                && !spells[Spells.Q].IsReady() 
                && spells[Spells.Q].IsInRange(target) 
                && !ObjectManager.Player.UnderTurret(true)
                && target.HasBuff("EkkoStacks"))
            {
                var eCast = ElEkkoMenu._menu.Item("ElEkko.Combo.E.Cast").GetValue<StringList>().SelectedIndex;
                switch (eCast)
                {
                    case 0:
                        spells[Spells.E].Cast(target.Position);
                        break;

                    case 1:
                        spells[Spells.E].Cast(Game.CursorPos);
                        break;
                }
            }

            if (useR && spells[Spells.R].IsReady())
            {
                var rtarget = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);

                if (rtarget.Health < RDamage(rtarget))
                {
                    if (Troy != null)
                    {
                        if (rtarget.Distance(Troy.Position) <= spells[Spells.R].Range)
                        {
                            spells[Spells.R].Cast();
                        }
                    }
                }

                var enemyCount = HeroManager.Enemies.Count(h => h.IsValidTarget() && h.Distance(Troy.Position) < spells[Spells.R].Range);
                if (enemyCount >= enemiesRrange)
                {
                    if (Troy != null)
                    {
                        if (rtarget.Distance(Troy.Position) <= spells[Spells.R].Range)
                        {
                            spells[Spells.R].Cast();
                        }
                    }
                }
            }

            if (useIgnite && Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health)
            {
                Player.Spellbook.CastSpell(ignite, target);
            }
        }
        #endregion

        #region RMode

        private static void SaveMode()
        {
            if (Player.IsRecalling() || Player.InFountain()) return;

            var useR = ElEkkoMenu._menu.Item("ElEkko.Combo.R").GetValue<bool>();
            var playerHp = ElEkkoMenu._menu.Item("ElEkko.Combo.R.HP").GetValue<Slider>().Value;

            if (useR && spells[Spells.R].IsReady())
            {
                if (Player.HealthPercent < playerHp && Player.CountEnemiesInRange(600) > 0) //  || IncomingDamage > Player.HealthPercent
                {
                    spells[Spells.R].Cast();
                }
            }
        }

        #endregion

        #region Obj_AI_Hero_OnProcessSpellCast

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy)
            {
                if (Player != null && spells[Spells.R].IsReady())
                {
                    if ((!(sender is Obj_AI_Hero) || args.SData.IsAutoAttack()) && args.Target != null && args.Target.NetworkId == Player.NetworkId)
                    {
                        incomingDamage.Add(Player.ServerPosition.Distance(sender.ServerPosition) / args.SData.MissileSpeed + Game.Time, (float)sender.GetAutoAttackDamage(Player));
                    }
                    else if (sender is Obj_AI_Hero)
                    {
                        var attacker = (Obj_AI_Hero)sender;
                        var slot = attacker.GetSpellSlot(args.SData.Name);

                        if (slot != SpellSlot.Unknown)
                        {
                            if (slot == attacker.GetSpellSlot("SummonerDot") && args.Target != null && args.Target.NetworkId == Player.NetworkId)
                            {
                                instantDamage.Add(Game.Time + 2, (float)attacker.GetSummonerSpellDamage(Player, Damage.SummonerSpell.Ignite));
                            }
                            else if (slot.HasFlag(SpellSlot.Q | SpellSlot.W | SpellSlot.E | SpellSlot.R) &&
                                ((args.Target != null && args.Target.NetworkId == Player.NetworkId) ||
                                args.End.Distance(Player.ServerPosition) < Math.Pow(args.SData.LineWidth, 2)))
                            {
                                instantDamage.Add(Game.Time + 2, (float)attacker.GetSpellDamage(Player, slot));
                            }
                        }
                    }
                }
            }

            if (sender.IsMe)
            {
                if (args.SData.Name == "EkkoE")
                {
                    Utility.DelayAction.Add(250, Orbwalking.ResetAutoAttackTimer);
                }
            }
        }

        #endregion

        #region Killsteal

        private static void KillSteal()
        {
            var isActive = ElEkkoMenu._menu.Item("ElEkko.Killsteal.Active").GetValue<bool>();
            if (isActive)
            {
                foreach (Obj_AI_Hero hero in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            hero =>
                            ObjectManager.Player.Distance(hero.ServerPosition) <= spells[Spells.Q].Range && !hero.IsMe
                            && hero.IsValidTarget() && hero.IsEnemy && !hero.IsInvulnerable))
                {
                    var qDamage = spells[Spells.Q].GetDamage(hero);
                    var useQ = ElEkkoMenu._menu.Item("ElEkko.Killsteal.Q").GetValue<bool>();
                    var useR = ElEkkoMenu._menu.Item("ElEkko.Killsteal.R").GetValue<bool>();
                    var useIgnite = ElEkkoMenu._menu.Item("ElEkko.Killsteal.Ignite").GetValue<bool>();

                    if (useQ && hero.Health - qDamage < 0 && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(hero))
                    {
                        spells[Spells.Q].Cast(hero);
                    }

                    if (useR && spells[Spells.R].IsReady())
                    {
                        if (hero.Health < RDamage(hero))
                        {
                            if (Troy != null)
                            {
                                if (hero.Distance(Troy.Position) <= spells[Spells.R].Range)
                                {
                                    spells[Spells.R].Cast();
                                }
                            }
                        }
                    }

                    if (useIgnite && Player.Distance(hero) <= 600 && IgniteDamage(hero) >= hero.Health)
                    {
                        Player.Spellbook.CastSpell(ignite, hero);
                    }
                }
            }
        }

        #endregion

        #region IgniteDamage

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        #endregion

        #region Notifications

        private static void ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            Notifications.AddNotification(new Notification(message, duration, dispose).SetTextColor(color));
        }

        #endregion

        #region EkkoR

        private static void Obj_AI_Base_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.IsValid && obj.IsAlly)
            {
                if (obj.Name == "Ekko")
                    Troy = obj;
            }
        }

        private static void Obj_AI_Base_OnDelete(GameObject obj, EventArgs args)
        {
            if (obj.IsValid && obj.IsAlly)
            {
                if (obj.Name == "Ekko")
                    Troy = null;
            }
        }

        #endregion

        #region ComboDamage

        private static float RDamage(Obj_AI_Base enemy)
        {
            double damage = 50 + (150 * spells[Spells.R].Level) + Player.FlatMagicDamageMod * 1.3;
            return (float)Player.CalcDamage(enemy, Damage.DamageType.Magical, damage);
        }

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

            if (ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(ignite) != SpellState.Ready)
            {
                damage += (float)Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return damage + 15 + (12 * Player.Level) + Player.FlatMagicDamageMod;
        }

        #endregion

        #region Vector Helper
        // Credits to Hellsing - VectorHelper.cs
        private static bool IsLyingInCone(Vector2 position, Vector2 apexPoint, Vector2 circleCenter, double aperture)
        {
            // This is for our convenience
            double halfAperture = aperture / 2;

            // Vector pointing to X point from apex
            Vector2 apexToXVect = apexPoint - position;

            // Vector pointing from apex to circle-center point.
            Vector2 axisVect = apexPoint - circleCenter;

            // X is lying in cone only if it's lying in 
            // infinite version of its cone -- that is, 
            // not limited by "round basement".
            // We'll use dotProd() to 
            // determine angle between apexToXVect and axis.
            bool isInInfiniteCone = DotProd(apexToXVect, axisVect) / Magn(apexToXVect) / Magn(axisVect) >
            // We can safely compare cos() of angles 
            // between vectors instead of bare angles.
            Math.Cos(halfAperture);

            if (!isInInfiniteCone)
                return false;

            // X is contained in cone only if projection of apexToXVect to axis
            // is shorter than axis. 
            // We'll use dotProd() to figure projection length.
            bool isUnderRoundCap = DotProd(apexToXVect, axisVect) / Magn(axisVect) < Magn(axisVect);

            return isUnderRoundCap;
        }

        private static float DotProd(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        private static float Magn(Vector2 a)
        {
            return (float)(Math.Sqrt(a.X * a.X + a.Y * a.Y));
        }

        private static Vector2? GetFirstWallPoint(Vector3 from, Vector3 to, float step = 25)
        {
            return GetFirstWallPoint(from.To2D(), to.To2D(), step);
        }

        private static Vector2? GetFirstWallPoint(Vector2 from, Vector2 to, float step = 25)
        {
            var direction = (to - from).Normalized();

            for (float d = 0; d < from.Distance(to); d = d + step)
            {
                var testPoint = from + d * direction;
                var flags = NavMesh.GetCollisionFlags(testPoint.X, testPoint.Y);
                if (flags.HasFlag(CollisionFlags.Wall) || flags.HasFlag(CollisionFlags.Building))
                {
                    return from + (d - step) * direction;
                }
            }

            return null;
        }

        public static List<Obj_AI_Base> GetDashObjects(IEnumerable<Obj_AI_Base> predefinedObjectList = null)
        {
            var objects = predefinedObjectList != null
                ? predefinedObjectList.ToList()
                : ObjectManager.Get<Obj_AI_Base>().Where(o => o.IsValidTarget(Orbwalking.GetRealAutoAttackRange(o)));

            var apexPoint = Player.ServerPosition.To2D() +
                            (Player.ServerPosition.To2D() - Game.CursorPos.To2D()).Normalized() *
                            Orbwalking.GetRealAutoAttackRange(Player);

            return
                objects.Where(
                    o => IsLyingInCone(o.ServerPosition.To2D(), apexPoint, Player.ServerPosition.To2D(), Math.PI))
                    .OrderBy(o => o.Distance(apexPoint, true))
                    .ToList();
        }

        #endregion

        #region Flee
        // Walljumper credits to Hellsing
        private static void FleeMode()
        {
            var fleeActive = ElEkkoMenu._menu.Item("ElEkko.Flee.Key").GetValue<KeyBind>().Active;

            if (!fleeActive) return;

            var wallCheck = GetFirstWallPoint(Player.Position, Game.CursorPos);

            // Be more precise
            if (wallCheck != null)
                wallCheck = GetFirstWallPoint((Vector3)wallCheck, Game.CursorPos, 5);

            // Define more position point
            var movePosition = wallCheck != null ? (Vector3)wallCheck : Game.CursorPos;

            // Update fleeTargetPosition
            var tempGrid = NavMesh.WorldToGrid(movePosition.X, movePosition.Y);
            var fleeTargetPosition = NavMesh.GridToWorld((short)tempGrid.X, (short)tempGrid.Y);

            // Also check if we want to AA aswell
            Obj_AI_Base target = null;

            // Reset walljump indicators
            var wallJumpPossible = false;

            // Only calculate stuff when our Q is up and there is a wall inbetween
            if (spells[Spells.E].IsReady() && wallCheck != null)
            {
                // Get our wall position to calculate from
                var wallPosition = movePosition;

                // Check 300 units to the cursor position in a 160 degree cone for a valid non-wall spot
                Vector2 direction = (Game.CursorPos.To2D() - wallPosition.To2D()).Normalized();
                float maxAngle = 80;
                float step = maxAngle / 20;
                float currentAngle = 0;
                float currentStep = 0;
                bool jumpTriggered = false;
                while (true)
                {
                    // Validate the counter, break if no valid spot was found in previous loops
                    if (currentStep > maxAngle && currentAngle < 0)
                        break;

                    // Check next angle
                    if ((currentAngle == 0 || currentAngle < 0) && currentStep != 0)
                    {
                        currentAngle = (currentStep) * (float)Math.PI / 180;
                        currentStep += step;
                    }

                    else if (currentAngle > 0)
                        currentAngle = -currentAngle;

                    Vector3 checkPoint;

                    // One time only check for direct line of sight without rotating
                    if (currentStep == 0)
                    {
                        currentStep = step;
                        checkPoint = wallPosition + spells[Spells.E].Range * direction.To3D();
                    }
                    // Rotated check
                    else
                        checkPoint = wallPosition + spells[Spells.E].Range * direction.Rotated(currentAngle).To3D();

                    // Check if the point is not a wall
                    if (!checkPoint.IsWall())
                    {
                        // Check if there is a wall between the checkPoint and wallPosition
                        wallCheck = GetFirstWallPoint(checkPoint, wallPosition);
                        if (wallCheck != null)
                        {
                            // There is a wall inbetween, get the closes point to the wall, as precise as possible
                            Vector3 wallPositionOpposite =
                                (Vector3)GetFirstWallPoint((Vector3)wallCheck, wallPosition, 5);

                            // Check if it's worth to jump considering the path length
                            if (Player.GetPath(wallPositionOpposite).ToList().To2D().PathLength()
                                - Player.Distance(wallPositionOpposite) > 200)
                            {
                                // Check the distance to the opposite side of the wall
                                if (Player.Distance(wallPositionOpposite, true)
                                    < Math.Pow(spells[Spells.E].Range - Player.BoundingRadius / 2, 2))
                                {
                                    // Make the jump happen
                                    spells[Spells.E].Cast(wallPositionOpposite);

                                    // Update jumpTriggered value to not orbwalk now since we want to jump
                                    jumpTriggered = true;

                                    break;
                                }
                                else
                                {
                                    wallJumpPossible = true;
                                }
                            }
                        }
                    }
                }

                // Check if the loop triggered the jump, if not just orbwalk
                if (!jumpTriggered)
                    Orbwalking.Orbwalk(target, Game.CursorPos, 90f, 0f, false, false);
            }
            else
            {
                Orbwalking.Orbwalk(target, Game.CursorPos, 90f, 0f, false, false);
                if (spells[Spells.E].IsReady())
                    spells[Spells.E].Cast(Game.CursorPos);
            }
        }

        public static bool IsJumpPossible { get; set; }
        public static Vector3 FleePosition { get; set; }

        #endregion
    }
}
