using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace ElEasy.Plugins
{
    public class Cassiopeia : Standards
    {
        #region Spells

        private static int _lastQ;
        private static int _lastE;

        private static readonly Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>
        {
            { Spells.Q, new Spell(SpellSlot.Q, 850) },
            { Spells.W, new Spell(SpellSlot.W, 850) },
            { Spells.E, new Spell(SpellSlot.E, 700) },
            { Spells.R, new Spell(SpellSlot.R, 825) }
        };

        #endregion

        #region Load

        public static void Load()
        {
            _ignite = Player.GetSpellSlot("summonerdot");

            spells[Spells.Q].SetSkillshot(0.6f, 75f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            spells[Spells.W].SetSkillshot(0.5f, 90f, 2500, false, SkillshotType.SkillshotCircle);
            spells[Spells.R].SetSkillshot(0.3f, (float)(80 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);
            spells[Spells.E].SetTargetted(0.25f, float.MaxValue);

            Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        #endregion

        #region Onupdate

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    OnHarass();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    OnLaneclear();
                    OnJungleclear();
                    break;

                case Orbwalking.OrbwalkingMode.LastHit:
                    OnLasthit();
                    break;
            }

            var showNotifications = _menu.Item("ElEasy.Cassio.Notifications").GetValue<bool>();

            if (showNotifications && Environment.TickCount - lastNotification > 5000)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(1000) && GetComboDamage(h) > h.Health))
                {
                    ShowNotification(enemy.ChampionName + ": is killable", Color.LightSeaGreen, 4000);
                    lastNotification = Environment.TickCount;
                }
            }

            var autoHarass = _menu.Item("ElEasy.Cassio.AutoHarass.Activated", true).GetValue<KeyBind>().Active;
            if (autoHarass)
                OnAutoHarass();

            KillSteal();
        }

        #endregion

        #region Killsteal

        private static void KillSteal()
        {
            var ks = _menu.Item("ElEasy.Cassio.Killsteal").GetValue<bool>();
            if (ks)
            {
                foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(hero => Player.Distance(hero.ServerPosition) <= spells[Spells.Q].Range && !hero.IsMe && hero.IsValidTarget() && hero.IsEnemy && !hero.IsInvulnerable))
                {
                    var qDamage = spells[Spells.Q].GetDamage(target);
                    var wDamage = spells[Spells.W].GetDamage(target);
                    //var eDamage = spells[Spells.E].GetDamage(target);

                    if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health)
                        Player.Spellbook.CastSpell(_ignite, target);

                    if (target.Health - qDamage < 0 && spells[Spells.Q].IsReady())
                    {
                        var prediction = spells[Spells.Q].GetPrediction(target);
                        if (prediction.Hitchance >= CustomHitChance && (Player.ServerPosition.Distance(spells[Spells.Q].GetPrediction(target, true).CastPosition) < spells[Spells.Q].Range))
                        {
                            spells[Spells.Q].Cast(target);
                        }
                    }

                    if (target.Health - wDamage < 0 && spells[Spells.W].IsReady())
                    {
                        if (spells[Spells.W].IsReady())
                        {
                            var prediction = spells[Spells.W].GetPrediction(target);
                            if (prediction.Hitchance >= CustomHitChance && (Player.ServerPosition.Distance(spells[Spells.W].GetPrediction(target, true).CastPosition) < spells[Spells.W].Range))
                            {
                                spells[Spells.W].Cast(target);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region AutoHarass

        private static void OnAutoHarass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                return;

            var useQ = _menu.Item("ElEasy.Cassio.AutoHarass.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Cassio.AutoHarass.W").GetValue<bool>();
            var mana = _menu.Item("ElEasy.Cassio.AutoHarass.Mana").GetValue<Slider>().Value;

            if (Player.Mana < mana)
                return;

            if (useQ && spells[Spells.Q].IsReady())
            {
                var prediction = spells[Spells.Q].GetPrediction(target);
                if (prediction.Hitchance >= CustomHitChance && (Player.ServerPosition.Distance(spells[Spells.Q].GetPrediction(target, true).CastPosition) < spells[Spells.Q].Range))
                {
                    spells[Spells.Q].Cast(target);
                }
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
            {
                if (target.HasBuffOfType(BuffType.Poison))
                    return;

                var prediction = spells[Spells.W].GetPrediction(target);
                if (prediction.Hitchance >= CustomHitChance && (Player.ServerPosition.Distance(spells[Spells.W].GetPrediction(target, true).CastPosition) < spells[Spells.W].Range))
                {
                    spells[Spells.W].Cast(target);
                }
            }
        }

        #endregion

        #region OnLasthit

        private static void OnLasthit()
        {
            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.Q].Range);
            if (minions.Count <= 0)
            {
                return;
            }

            var useE = _menu.Item("ElEasy.Cassio.LastHit.E").GetValue<bool>();

            if (useE && spells[Spells.E].IsReady())
            {
                var etarget =
                    minions.Where(x => x.Distance(Player) < spells[Spells.E].Range && x.Health <= ObjectManager.Player.GetSpellDamage(x, SpellSlot.E)
                    && x.HasBuffOfType(BuffType.Poison))
                        .OrderByDescending(x => x.Health)
                        .FirstOrDefault(y => y.HPRegenRate + y.Health <= spells[Spells.E].GetDamage(y) && HealthPrediction.GetHealthPrediction(y, (int)spells[Spells.E].Delay, (int)spells[Spells.E].Speed) <= spells[Spells.E].GetDamage(y));

                spells[Spells.E].Cast(etarget);
            }
        }

        #endregion

        #region OnJungleClear

        private static void OnJungleclear()
        {
            var mana = _menu.Item("ElEasy.Cassio.LaneClear.Mana").GetValue<Slider>().Value;
            if (Player.Mana < mana)
            {
                return;
            }

            var minions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, spells[Spells.E].Range, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);
            if (minions.Count <= 0)
            {
                return;
            }

            var useQ = _menu.Item("ElEasy.Cassio.JungleClear.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Cassio.JungleClear.E").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Cassio.JungleClear.W").GetValue<bool>();

            if (useQ && spells[Spells.Q].IsReady())
            {
                var farmLocation = spells[Spells.Q].GetCircularFarmLocation(minions);
                spells[Spells.Q].Cast(farmLocation.Position);
            }

            if (useE && spells[Spells.E].IsReady())
            {
                var etarget =
                    minions.Where(x => x.Distance(Player) < spells[Spells.E].Range && x.HasBuffOfType(BuffType.Poison))
                        .OrderByDescending(x => x.Health)
                        .FirstOrDefault();

                spells[Spells.E].Cast(etarget);
            }

            if (useW && spells[Spells.W].IsReady())
            {
                var farmLocation = spells[Spells.W].GetCircularFarmLocation(minions);
                spells[Spells.W].Cast(farmLocation.Position);
            }
        }

        #endregion

        #region OnLaneClear

        private static void OnLaneclear()
        {
            var mana = _menu.Item("ElEasy.Cassio.LaneClear.Mana").GetValue<Slider>().Value;
            if (Player.Mana < mana)
            {
                return;
            }

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.Q].Range);
            if (minions.Count <= 0)
            {
                return;
            }

            var useQ = _menu.Item("ElEasy.Cassio.LaneClear.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Cassio.LaneClear.E").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Cassio.LaneClear.W").GetValue<bool>();

            if (useQ && spells[Spells.Q].IsReady())
            {
                if (minions.Count <= 1)
                {
                    return;
                }

                var farmLocation = spells[Spells.Q].GetCircularFarmLocation(minions);
                spells[Spells.Q].Cast(farmLocation.Position);
            }

            if (useE && spells[Spells.E].IsReady())
            {
                var etarget =
                    minions.Where(x => x.Distance(Player) < spells[Spells.E].Range && x.Health <= ObjectManager.Player.GetSpellDamage(x, SpellSlot.E)
                    && x.HasBuffOfType(BuffType.Poison))
                        .OrderByDescending(x => x.Health)
                        .FirstOrDefault(y => y.HPRegenRate + y.Health <= spells[Spells.E].GetDamage(y) && HealthPrediction.GetHealthPrediction(y, (int)spells[Spells.E].Delay, (int)spells[Spells.E].Speed) <= spells[Spells.E].GetDamage(y));

                spells[Spells.E].Cast(etarget);
            }

            if (useW && spells[Spells.W].IsReady())
            {
                var farmLocation = spells[Spells.W].GetCircularFarmLocation(minions);
                if (farmLocation.MinionsHit >= 1)
                {
                    spells[Spells.W].Cast(farmLocation.Position);
                }
            }
        }

        #endregion

        #region OnCombo

        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            var rtarget = TargetSelector.GetTarget(spells[Spells.R].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid || rtarget == null || !rtarget.IsValid)
            {
                return;
            }

            var useQ = _menu.Item("ElEasy.Cassio.Combo.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Cassio.Combo.W").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Cassio.Combo.E").GetValue<bool>();
            var useR = _menu.Item("ElEasy.Cassio.Combo.R").GetValue<bool>();
            var useI = _menu.Item("ElEasy.Cassio.Combo.Ignite").GetValue<bool>();
            var countEnemies = _menu.Item("ElEasy.Cassio.Combo.R.Count").GetValue<Slider>().Value;

            if (useQ && spells[Spells.Q].IsReady())
            {
                var prediction = spells[Spells.Q].GetPrediction(target);
                if ((Player.ServerPosition.Distance(prediction.CastPosition) < spells[Spells.Q].Range) && target.IsVisible && !target.IsDead)
                {
                    spells[Spells.Q].CastIfHitchanceEquals(target, CustomHitChance);
                    _lastQ = Environment.TickCount;
                }
            }

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
            {
                if (!target.HasBuffOfType(BuffType.Poison))
                    return;

                var playLegit = _menu.Item("ElEasy.Cassio.E.Legit").GetValue<bool>();
                var legitCastDelay = _menu.Item("ElEasy.Cassio.E.Delay").GetValue<Slider>().Value;

                if (playLegit)
                {
                    if (Environment.TickCount > _lastE + legitCastDelay)
                    {
                        spells[Spells.E].CastOnUnit(target);
                        _lastE = Environment.TickCount;
                    }
                }
                else
                {
                    spells[Spells.E].Cast(target);
                    _lastE = Environment.TickCount;
                }
            }

            if (useW && spells[Spells.W].IsReady() && Environment.TickCount > _lastQ + spells[Spells.Q].Delay * 1000)
            {
                var prediction = spells[Spells.W].GetPrediction(target);
                if ((Player.ServerPosition.Distance(prediction.CastPosition) <
                     spells[Spells.W].Range))
                {
                    spells[Spells.W].CastIfHitchanceEquals(target, CustomHitChance);

                }
            }

            if (useR && spells[Spells.R].IsReady() && spells[Spells.R].IsInRange(rtarget))
            {
                var prediction = spells[Spells.R].GetPrediction(rtarget);
                if (prediction.Hitchance >= CustomHitChance &&
                    Player.CountEnemiesInRange(spells[Spells.R].Range) >= countEnemies)
                {
                    spells[Spells.R].Cast(rtarget);
                }
            }

            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && useI)
            {
                Player.Spellbook.CastSpell(_ignite, target);
            }
        }

        #endregion

        #region OnHarass

        private static void OnHarass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            var rtarget = TargetSelector.GetTarget(spells[Spells.R].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid || rtarget == null || !rtarget.IsValid)
            {
                return;
            }

            var useQ = _menu.Item("ElEasy.Cassio.Harass.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Cassio.Harass.W").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Cassio.Harass.E").GetValue<bool>();
            var playerMana = _menu.Item("ElEasy.Cassio.Harass.Mana").GetValue<Slider>().Value;

            if (Player.ManaPercent < playerMana)
            {
                return;
            }

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                if ((Player.ServerPosition.Distance(spells[Spells.Q].GetPrediction(target, true).CastPosition) < spells[Spells.Q].Range))
                {
                    spells[Spells.Q].CastIfHitchanceEquals(target, CustomHitChance);
                }
            }

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
            {
                if (!target.HasBuffOfType(BuffType.Poison))
                {
                    return;
                }

                spells[Spells.E].Cast(target);
            }

            if (useW && spells[Spells.W].IsReady())
            {
                if ((Player.ServerPosition.Distance(spells[Spells.W].GetPrediction(target, true).CastPosition) < spells[Spells.W].Range))
                {
                    spells[Spells.W].CastIfHitchanceEquals(target, CustomHitChance);
                }
            }
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

        #region Draw

        private static void OnDraw(EventArgs args)
        {
            var drawOff = _menu.Item("ElEasy.Cassio.Draw.off").GetValue<bool>();
            var drawQ = _menu.Item("ElEasy.Cassio.Draw.Q").GetValue<Circle>();
            var drawW = _menu.Item("ElEasy.Cassio.Draw.W").GetValue<Circle>();
            var drawE = _menu.Item("ElEasy.Cassio.Draw.E").GetValue<Circle>();
            var drawR = _menu.Item("ElEasy.Cassio.Draw.R").GetValue<Circle>();

            if (drawOff)
            {
                return;
            }

            if (drawQ.Active)
            {
                if (spells[Spells.Q].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.Q].Range, Color.White);
                }
            }

            if (drawW.Active)
            {
                if (spells[Spells.W].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.W].Range, Color.White);
                }
            }

            if (drawE.Active)
            {
                if (spells[Spells.E].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.E].Range, Color.White);
                }
            }

            if (drawR.Active)
            {
                if (spells[Spells.R].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.R].Range, Color.White);
                }
            }
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

            if (spells[Spells.W].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);
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

        #region Gapcloser

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var gapCloserActive = _menu.Item("ElEasy.Cassio.GapCloser.Activated").GetValue<bool>();

            if (gapCloserActive && spells[Spells.R].IsReady() &&
                gapcloser.Sender.Distance(Player) < spells[Spells.R].Range)
            {
                spells[Spells.W].Cast(gapcloser.End);
            }
        }

        #endregion

        #region BeforeAttack
        private static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            var target = args.Target;
            if (!target.IsValidTarget() || !(args.Target is Obj_AI_Base) || Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return;

            var t = (Obj_AI_Base)target;
            if (spells[Spells.E].IsReady() && t.HasBuffOfType(BuffType.Poison) && target.IsValidTarget(spells[Spells.E].Range))
                args.Process = false;
        }

        #endregion

        #region Interrupter2

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            var gapCloserActive = _menu.Item("ElEasy.Cassio.Interrupt.Activated").GetValue<bool>();
            if (!gapCloserActive)
            {
                return;
            }

            if (args.DangerLevel != Interrupter2.DangerLevel.High || sender.Distance(Player) > spells[Spells.R].Range)
            {
                return;
            }

            if (sender.IsValidTarget(spells[Spells.R].Range) && args.DangerLevel == Interrupter2.DangerLevel.High &&
                spells[Spells.R].IsReady())
            {
                spells[Spells.R].Cast(sender);
            }
        }

        #endregion

        #region Menu

        private static void Initialize()
        {
            _menu = new Menu("ElCassiopeia", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElEasy.Cassio.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Cassio.Combo.W", "Use W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Cassio.Combo.E", "Use E").SetValue(true));
            cMenu.SubMenu("R").AddItem(new MenuItem("ElEasy.Cassio.Combo.R", "Use R").SetValue(true));
            cMenu.SubMenu("R").AddItem(new MenuItem("ElEasy.Cassio.Combo.R.Count", "Enemies for R").SetValue(new Slider(2, 1, 5)));
            cMenu.AddItem(new MenuItem("ElEasy.Cassio.Combo.Ignite", "Use Ignite").SetValue(true));
            cMenu.SubMenu("E").AddItem(new MenuItem("ElEasy.Cassio.E.Legit", "Legit E").SetValue(false));
            cMenu.SubMenu("E").AddItem(new MenuItem("ElEasy.Cassio.E.Delay", "E Delay").SetValue(new Slider(1000, 0, 2000)));

            _menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "harass");
            hMenu.AddItem(new MenuItem("ElEasy.Cassio.Harass.Q", "Use Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Cassio.Harass.W", "Use W").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Cassio.Harass.E", "Use E").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Cassio.Harass.Mana", "Minimum Mana").SetValue(new Slider(55)));

            hMenu.SubMenu("Harass").SubMenu("AutoHarass settings").AddItem(new MenuItem("ElEasy.Cassio.AutoHarass.Activated", "Auto harass", true).SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));
            hMenu.SubMenu("Harass").SubMenu("AutoHarass settings").AddItem(new MenuItem("ElEasy.Cassio.AutoHarass.Q", "Use Q").SetValue(true));
            hMenu.SubMenu("Harass").SubMenu("AutoHarass settings").AddItem(new MenuItem("ElEasy.Cassio.AutoHarass.W", "Use W").SetValue(true));
            hMenu.SubMenu("Harass").SubMenu("AutoHarass settings").AddItem(new MenuItem("ElEasy.Cassio.AutoHarass.Mana", "Minimum mana").SetValue(new Slider(55)));

            _menu.AddSubMenu(hMenu);

            var clearMenu = new Menu("Clear", "Clear");
            clearMenu.SubMenu("Lasthit").AddItem(new MenuItem("ElEasy.Cassio.LastHit.E", "Use E").SetValue(true));

            clearMenu.SubMenu("Lane clear").AddItem(new MenuItem("ElEasy.Cassio.LaneClear.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Lane clear").AddItem(new MenuItem("ElEasy.Cassio.LaneClear.W", "Use W").SetValue(true));
            clearMenu.SubMenu("Lane clear").AddItem(new MenuItem("ElEasy.Cassio.LaneClear.E", "Use E").SetValue(true));
            clearMenu.SubMenu("Lane clear").AddItem(new MenuItem("ElEasy.Cassio.LaneClear.MinionsHit", "W minions hit").SetValue(new Slider(2, 1, 5)));

            clearMenu.SubMenu("Jungle clear").AddItem(new MenuItem("ElEasy.Cassio.JungleClear.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Jungle clear").AddItem(new MenuItem("ElEasy.Cassio.JungleClear.W", "Use W").SetValue(true));
            clearMenu.SubMenu("Jungle clear").AddItem(new MenuItem("ElEasy.Cassio.JungleClear.E", "Use E").SetValue(true));
            clearMenu.AddItem(new MenuItem("ElEasy.Cassio.LaneClear.Mana", "Minimum mana").SetValue(new Slider(55)));

            _menu.AddSubMenu(clearMenu);

            var hitchanceMenu = new Menu("Settings", "Settings");
            hitchanceMenu.AddItem(new MenuItem("ElEasy.Cassio.Killsteal", "Killsteal").SetValue(true));
            hitchanceMenu.AddItem(new MenuItem("ElEasy.Cassio.Hitchance", "Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
            _menu.AddSubMenu(hitchanceMenu);

            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElEasy.Cassio.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElEasy.Cassio.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Cassio.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Cassio.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Cassio.Draw.R", "Draw R").SetValue(new Circle()));

            var dmgAfterE = new MenuItem("ElEasy.Cassio.DrawComboDamage", "Draw combo damage").SetValue(true);
            var drawFill =
                new MenuItem("ElEasy.Cassio.DrawColour", "Fill colour", true).SetValue(
                    new Circle(true, Color.FromArgb(0xcc, 0xcc, 0x0, 0x0)));
            miscMenu.AddItem(drawFill);
            miscMenu.AddItem(dmgAfterE);

            DrawDamage.DamageToUnit = GetComboDamage;
            DrawDamage.Enabled = dmgAfterE.GetValue<bool>();
            DrawDamage.Fill = drawFill.GetValue<Circle>().Active;
            DrawDamage.FillColor = drawFill.GetValue<Circle>().Color;

            dmgAfterE.ValueChanged +=
                delegate (object sender, OnValueChangeEventArgs eventArgs)
                {
                    DrawDamage.Enabled = eventArgs.GetNewValue<bool>();
                };

            drawFill.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                DrawDamage.Fill = eventArgs.GetNewValue<Circle>().Active;
                DrawDamage.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };

            miscMenu.AddItem(new MenuItem("ElEasy.Cassio.GapCloser.Activated", "Anti gapcloser").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElEasy.Cassio.Interrupt.Activated", "Interupt spells").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElEasy.Cassio.Notifications", "Show notifications").SetValue(true));

            _menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = _menu.AddSubMenu(new Menu("Credits", "jQuery"));
            credits.AddItem(new MenuItem("ElEasy.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElEasy.Email", "info@zavox.nl"));

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            _menu.AddToMainMenu();
        }

        #endregion

        #region hitchance

        private static HitChance CustomHitChance
        {
            get { return GetHitchance(); }
        }

        private static HitChance GetHitchance()
        {
            switch (_menu.Item("ElEasy.Cassio.Hitchance").GetValue<StringList>().SelectedIndex)
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
    }
}
