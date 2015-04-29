using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace ElEasy.Plugins
{
    public class Malphite : Standards
    {
        private static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 625) },
            { Spells.W, new Spell(SpellSlot.W, 125) },
            { Spells.E, new Spell(SpellSlot.E, 375) },
            { Spells.R, new Spell(SpellSlot.R, 1000) }
        };

        public static void Load()
        {
            _ignite = Player.GetSpellSlot("summonerdot");
             spells[Spells.R].SetSkillshot(0.00f, 270, 700, false, SkillshotType.SkillshotCircle);

            Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        #region Onupdate

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    OnLaneclear();
                    OnJungleclear();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    OnHarass();
                    break;

                case Orbwalking.OrbwalkingMode.LastHit:
                    OnLastHit();
                    break;
            }

            var autoHarass = _menu.Item("ElEasy.Malphite.AutoHarass.Activate", true).GetValue<KeyBind>().Active;
            if (autoHarass)
                OnAutoHarass();

            var showNotifications = _menu.Item("ElEasy.Malphite.Notifications").GetValue<bool>();

            if (showNotifications && Environment.TickCount - lastNotification > 5000)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(spells[Spells.R].Range) && GetComboDamage(h) > h.Health))
                {
                    ShowNotification(enemy.ChampionName + ": is killable", Color.LightSeaGreen, 4000);
                    lastNotification = Environment.TickCount;
                }
            }
        }

        #endregion

        #region OnCombo

        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            var rTarget = TargetSelector.GetTarget(spells[Spells.R].Range, TargetSelector.DamageType.Magical);

            if (target == null || !target.IsValid)
                return;

            var useQ = _menu.Item("ElEasy.Malphite.Combo.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Malphite.Combo.W").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Malphite.Combo.E").GetValue<bool>();
            var useR = _menu.Item("ElEasy.Malphite.Combo.R").GetValue<bool>();
            var useI = _menu.Item("ElEasy.Malphite.Combo.Ignite").GetValue<bool>();
            var ultType = _menu.Item("ElEasy.Malphite.Combo.R.Mode").GetValue<StringList>().SelectedIndex;

            var countEnemies = _menu.Item("ElEasy.Malphite.Combo.Count.R").GetValue<Slider>().Value;

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                spells[Spells.Q].Cast(target);
            }

            if (useW && spells[Spells.W].IsReady())
            {
                spells[Spells.W].Cast();
            }

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
            {
                spells[Spells.E].Cast();
            }

            switch (ultType)
            {
                case 0:
                    if (useR && spells[Spells.R].IsReady() && rTarget != null)
                    {
                        var pred = spells[Spells.R].GetPrediction(rTarget).Hitchance;
                        if (pred >= HitChance.High)
                            spells[Spells.R].Cast(rTarget);
                    }
                    break;

                case 1:
                    if (useR && spells[Spells.R].IsReady() && rTarget != null)
                    {
                        var pred = spells[Spells.R].GetPrediction(rTarget).Hitchance;
                        if (pred >= HitChance.High)
                            spells[Spells.R].CastIfWillHit(rTarget, countEnemies);
                    }
                    break;
            }
            
            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && useI)
            {
                Player.Spellbook.CastSpell(_ignite, target);
            }
        }

        #endregion

        #region OnJungleClear

        private static void OnJungleclear()
        {
            var useQ = _menu.Item("ElEasy.Malphite.JungleClear.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Malphite.JungleClear.W").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Malphite.JungleClear.E").GetValue<bool>();
            var playerMana = _menu.Item("ElEasy.Malphite.Clear.Player.Mana").GetValue<Slider>().Value;

            if (Player.Mana < playerMana)
                return;

            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.E].Range, MinionTypes.All, MinionTeam.Neutral,
           MinionOrderTypes.MaxHealth);
            if (minions.Count <= 0)
                return;

            if (spells[Spells.Q].IsReady() && useQ)
            {
                foreach (var minion in
                    minions.Where(minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    if (minion.IsValidTarget())
                    {
                        spells[Spells.Q].CastOnUnit(minion);
                        return;
                    }
                }
            }

            if (useW && spells[Spells.W].IsReady())
            {
                spells[Spells.W].Cast(Player);
            }

            if (useE && spells[Spells.E].IsReady())
            {
                if (minions.Count > 1)
                {
                    var farmLocation = spells[Spells.E].GetCircularFarmLocation(minions);
                    spells[Spells.E].Cast(farmLocation.Position);
                }
            }
        }

        #endregion

        #region OnLaneClear
        private static void OnLaneclear()
        {
            var useQ = _menu.Item("ElEasy.Malphite.LaneClear.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Malphite.LaneClear.W").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Malphite.LaneClear.E").GetValue<bool>();
            var playerMana = _menu.Item("ElEasy.Malphite.Clear.Player.Mana").GetValue<Slider>().Value;

            if (Player.Mana < playerMana)
                return;

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.E].Range);
            if (minions.Count <= 0)
                return;

            if (spells[Spells.Q].IsReady() && useQ)
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.E].Range);
                {
                    foreach (
                        var minion in
                            allMinions.Where(
                                minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q)))
                    {
                        if (minion.IsValidTarget())
                        {
                            spells[Spells.Q].CastOnUnit(minion);
                            return;
                        }
                    }
                }
            }

            if (useW && spells[Spells.W].IsReady())
            {
                spells[Spells.W].Cast(Player);
            }

            if (useE && spells[Spells.E].IsReady())
            {
                if (minions.Count > 1)
                {
                    var farmLocation = spells[Spells.E].GetCircularFarmLocation(minions);
                    spells[Spells.E].Cast(farmLocation.Position);
                }
            }
        }

        #endregion

        #region OnLastHit

        private static void OnLastHit()
        {
            var useQ = _menu.Item("ElEasy.Malphite.Lasthit.Q").GetValue<bool>();
            var playerMana = _menu.Item("ElEasy.Malphite.Clear.Player.Mana").GetValue<Slider>().Value;

            if (Player.Mana < playerMana || !useQ)
                return;

            var minions = MinionManager.GetMinions(Player.Position, spells[Spells.E].Range, MinionTypes.All, MinionTeam.Enemy,
              MinionOrderTypes.MaxHealth);

            foreach (var minion in minions)
            {
                if (spells[Spells.Q].GetDamage(minion) > minion.Health && spells[Spells.Q].IsReady())
                {
                    spells[Spells.Q].CastOnUnit(minion);
                }
            }
        }


        #endregion

        #region OnHarass

        private static void OnHarass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(spells[Spells.E].Range, TargetSelector.DamageType.Magical);

            if (target == null || !target.IsValid)
                return;

            var useQ = _menu.Item("ElEasy.Malphite.Harass.Q").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Malphite.Harass.E").GetValue<bool>();
            var playerMana = _menu.Item("ElEasy.Malphite.Harass.Player.Mana").GetValue<Slider>().Value;

            if (Player.Mana < playerMana)
                return;

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                spells[Spells.Q].Cast(target);
            }

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target) && eTarget != null )
            {
                spells[Spells.E].Cast(eTarget);
            }
        }

        #endregion

        #region OnAutoHarass

        private static void OnAutoHarass()
        {
            var qTarget = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(spells[Spells.E].Range, TargetSelector.DamageType.Magical);

            if (qTarget == null || !qTarget.IsValid)
                return;

            var useQ = _menu.Item("ElEasy.Malphite.AutoHarass.Q").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Malphite.AutoHarass.E").GetValue<bool>();
            var playerMana = _menu.Item("ElEasy.Malphite.AutoHarass.PlayerMana").GetValue<Slider>().Value;

            if (Player.Mana < playerMana)
                return;

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(qTarget))
            {
                spells[Spells.Q].Cast(qTarget);
            }

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(eTarget) && eTarget != null)
            {
                spells[Spells.E].Cast();
            }
        }

        #endregion

        #region Draw

        private static void OnDraw(EventArgs args)
        {
            var drawOff = _menu.Item("ElEasy.Malphite.Draw.off").GetValue<bool>();
            var drawQ = _menu.Item("ElEasy.Malphite.Draw.Q").GetValue<Circle>();
            var drawE = _menu.Item("ElEasy.Malphite.Draw.E").GetValue<Circle>();
            var drawR = _menu.Item("ElEasy.Malphite.Draw.R").GetValue<Circle>();

            if (drawOff)
                return;

            if (drawQ.Active)
                if (spells[Spells.Q].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.Q].Range, Color.White);

            if (drawE.Active)
                if (spells[Spells.E].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.E].Range, Color.White);

             if (drawR.Active)
                 if (spells[Spells.R].Level > 0)
                     Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.R].Range, Color.White);
        }

        #endregion

        #region Menu
        private static void Initialize()
        {
            _menu = new Menu("ElMalphite", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElEasy.Malphite.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Malphite.Combo.W", "Use W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Malphite.Combo.E", "Use E").SetValue(true));
            cMenu.SubMenu("R").AddItem(new MenuItem("ElEasy.Malphite.Combo.R", "Use R").SetValue(true));
            cMenu.SubMenu("R").AddItem(new MenuItem("ElEasy.Malphite.Combo.R.Mode", "Mode ").SetValue(new StringList(new[] { "Normal", "Champions hit" })));
            cMenu.SubMenu("R").AddItem(new MenuItem("ElEasy.Malphite.Combo.Count.R", "Minimum champions hit by R").SetValue(new Slider(2, 1, 5)));

            cMenu.AddItem(new MenuItem("ElEasy.Malphite.Combo.Ignite", "Use Ignite").SetValue(true));

            _menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElEasy.Malphite.Harass.Q", "Use Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Malphite.Harass.E", "Use E").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Malphite.Harass.Player.Mana", "Minimum Mana").SetValue(new Slider(55)));
            hMenu.SubMenu("AutoHarass settings").AddItem(new MenuItem("ElEasy.Malphite.AutoHarass.Activate", "Auto harass", true).SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));
            hMenu.SubMenu("AutoHarass settings").AddItem(new MenuItem("ElEasy.Malphite.AutoHarass.Q", "Use Q").SetValue(true));
            hMenu.SubMenu("AutoHarass settings").AddItem(new MenuItem("ElEasy.Malphite.AutoHarass.E", "Use E").SetValue(true));
            hMenu.SubMenu("AutoHarass settings").AddItem(new MenuItem("ElEasy.Malphite.AutoHarass.PlayerMana", "Minimum mana").SetValue(new Slider(55)));

            _menu.AddSubMenu(hMenu);

            var clearMenu = new Menu("Clear", "Clear");
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Malphite.LaneClear.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Malphite.LaneClear.W", "Use W").SetValue(true));
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Malphite.LaneClear.E", "Use E").SetValue(true));
            clearMenu.SubMenu("Jungleclear").AddItem(new MenuItem("ElEasy.Malphite.JungleClear.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Jungleclear").AddItem(new MenuItem("ElEasy.Malphite.JungleClear.W", "Use W").SetValue(true));
            clearMenu.SubMenu("Jungleclear").AddItem(new MenuItem("ElEasy.Malphite.JungleClear.E", "Use E").SetValue(true));
            clearMenu.SubMenu("Lasthit").AddItem(new MenuItem("ElEasy.Malphite.Lasthit.Q", "Use Q").SetValue(true));
            clearMenu.AddItem(new MenuItem("ElEasy.Malphite.Clear.Player.Mana", "Minimum Mana for clear").SetValue(new Slider(55)));


            _menu.AddSubMenu(clearMenu);

            var interruptMenu = new Menu("Settings", "Settings");
            interruptMenu.AddItem(new MenuItem("ElEasy.Malphite.Interrupt.Activated", "Interrupt spells").SetValue(true));
            interruptMenu.AddItem(new MenuItem("ElEasy.Malphite.Notifications", "Show notifications").SetValue(true));
            _menu.AddSubMenu(interruptMenu);

            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElEasy.Malphite.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElEasy.Malphite.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Malphite.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Malphite.Draw.R", "Draw R").SetValue(new Circle()));

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

        #region Interrupt

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            var useR = _menu.Item("ElEasy.Malphite.Interrupt.Activated").GetValue<bool>();
            if (!useR)
                return;

            if (args.DangerLevel != Interrupter2.DangerLevel.High || sender.Distance(Player) > spells[Spells.R].Range)
                return;

            if (sender.IsValidTarget(spells[Spells.R].Range) && args.DangerLevel == Interrupter2.DangerLevel.High && spells[Spells.R].IsReady())
            {
                spells[Spells.R].Cast(sender);
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
    }
}