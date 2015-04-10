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

namespace ElEasy.Plugins
{
    public class Darius : Standards
    {
        private static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 400) },
            { Spells.W, new Spell(SpellSlot.W, 145) },
            { Spells.E, new Spell(SpellSlot.E, 540) },
            { Spells.R, new Spell(SpellSlot.R, 460) }
        };

        public static void Load()
        {
            _ignite = Player.GetSpellSlot("summonerdot");
            spells[Spells.E].SetSkillshot(0.30f, 80, int.MaxValue, false, SkillshotType.SkillshotCone);

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
            }

            var showNotifications = _menu.Item("ElEasy.Darius.Notifications").GetValue<bool>();

            if (showNotifications && Environment.TickCount - lastNotification > 5000)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(1000) && GetComboDamage(h) > h.Health))
                {
                    ShowNotification(enemy.ChampionName + ": is killable", Color.LightSeaGreen, 4000);
                    lastNotification = Environment.TickCount;
                }
            }
        }

        #endregion

        #region OnHarass
        private static void OnHarass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
                return;

            var useQ = _menu.Item("ElEasy.Darius.Harass.Q").GetValue<bool>();

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                spells[Spells.Q].Cast(target);
            }
        }

        #endregion

        #region OnLaneClear
        private static void OnLaneclear()
        {
            var useQ = _menu.Item("ElEasy.Darius.LaneClear.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Darius.LaneClear.W").GetValue<bool>();
            var playerMana = _menu.Item("ElEasy.Darius.Clear.Player.Mana").GetValue<Slider>().Value;

            if (Player.Mana < playerMana)
                return;

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.Q].Range);
            if (minions.Count <= 0)
                return;

            if (useW && spells[Spells.W].IsReady())
            {
                spells[Spells.W].Cast(Player);
            }

            if (useQ && spells[Spells.Q].IsReady())
            {
                if (minions.Count > 1)
                {
                    var farmLocation = spells[Spells.Q].GetCircularFarmLocation(minions);
                    spells[Spells.Q].Cast(farmLocation.Position);
                }
            }
        }

        #endregion

        #region OnJungleClear
        private static void OnJungleclear()
        {
            var useQ = _menu.Item("ElEasy.Darius.JungleClear.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Darius.JungleClear.W").GetValue<bool>();
            var playerMana = _menu.Item("ElEasy.Darius.Clear.Player.Mana").GetValue<Slider>().Value;

            if (Player.Mana < playerMana)
                return;

            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.Q].Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (minions.Count <= 0)
                return;

            if (useW && spells[Spells.W].IsReady())
            {
                spells[Spells.W].Cast(Player);
            }

            if (useQ && spells[Spells.Q].IsReady())
            {
                if (minions.Count > 1)
                {
                    var farmLocation = spells[Spells.Q].GetCircularFarmLocation(minions);
                    spells[Spells.Q].Cast(farmLocation.Position);
                }
            }
        }

        #endregion


        #region OnCombo
        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
                return;

            var useQ = _menu.Item("ElEasy.Darius.Combo.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Darius.Combo.W").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Darius.Combo.E").GetValue<bool>();
            var useR = _menu.Item("ElEasy.Darius.Combo.R").GetValue<bool>();
            var useI = _menu.Item("ElEasy.Darius.Combo.Ignite").GetValue<bool>();
            //var ultType = _menu.Item("ElEasy.Malphite.Combo.R.Mode").GetValue<StringList>().SelectedIndex;

            /*switch (ultType)
            {
                case 0:
                    if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
                    {
                        spells[Spells.E].Cast(target);
                    }

                    if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
                    {
                        spells[Spells.Q].Cast(target);
                    }


                    break;

                case 1:
                    if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
                    {
                        spells[Spells.Q].Cast(target);
                    }

                    if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
                    {
                        spells[Spells.E].Cast(target);
                    }

                    break;
            }*/

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
            {
                spells[Spells.E].Cast(target);
            }

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                spells[Spells.Q].Cast(target);
            }

            Items(target);

            if (useW && spells[Spells.W].IsReady())
            {
                spells[Spells.W].CastOnUnit(Player);
            }


            if (useR && spells[Spells.R].IsReady() && spells[Spells.R].IsInRange(target) && spells[Spells.R].GetDamage(target) >= target.Health)
            {
                //Credits: https://github.com/TC-Crew/L-Assemblies/blob/master/Darius/ComboHandler.cs#L40
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(spells[Spells.R].Range)))
                {
                    if (Player.GetSpellDamage(target, SpellSlot.R) > hero.Health)
                    {
                        spells[Spells.R].Cast(target);
                    }

                    else if (Player.GetSpellDamage(target, SpellSlot.R) < hero.Health)
                    {
                        foreach (var buff in hero.Buffs.Where(buff => buff.Name == "dariushemo"))
                        {
                            if (Player.GetSpellDamage(target, SpellSlot.R, 1) * (1 + buff.Count / 5) - 1 > target.Health)
                            {
                                spells[Spells.R].CastOnUnit(target);
                            }
                        }
                    }
                }
            }

            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && useI)
            {
                Player.Spellbook.CastSpell(_ignite, target);
            }
        }

        #endregion

        #region Draw

        private static void OnDraw(EventArgs args)
        {
            var drawOff = _menu.Item("ElEasy.Darius.Draw.off").GetValue<bool>();
            var drawQ = _menu.Item("ElEasy.Darius.Draw.Q").GetValue<Circle>();
            var drawE = _menu.Item("ElEasy.Darius.Draw.E").GetValue<Circle>();
            var drawR = _menu.Item("ElEasy.Darius.Draw.R").GetValue<Circle>();

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
            _menu = new Menu("ElDarius", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElEasy.Darius.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Darius.Combo.W", "Use W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Darius.Combo.E", "Use E").SetValue(true));
            cMenu.SubMenu("R").AddItem(new MenuItem("ElEasy.Darius.Combo.R", "Use SlamDUNK").SetValue(true));
            //cMenu.SubMenu("R").AddItem(new MenuItem("ElEasy.Darius.Combo.R.Mode", "Mode ").SetValue(new StringList(new[] { "E->Q", "Q->E" })));
            cMenu.AddItem(new MenuItem("ElEasy.Darius.Combo.Ignite", "Use Ignite").SetValue(true));

            _menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElEasy.Darius.Harass.Q", "Use Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Darius.Harass.Player.Mana", "Minimum Mana").SetValue(new Slider(55)));

            _menu.AddSubMenu(hMenu);

            var clearMenu = new Menu("Clear", "Clear");
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Darius.LaneClear.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Darius.LaneClear.W", "Use W").SetValue(true));
            clearMenu.SubMenu("Jungleclear").AddItem(new MenuItem("ElEasy.Darius.JungleClear.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Jungleclear").AddItem(new MenuItem("ElEasy.Darius.JungleClear.W", "Use W").SetValue(true));
            clearMenu.AddItem(new MenuItem("ElEasy.Darius.Clear.Player.Mana", "Minimum Mana for clear").SetValue(new Slider(55)));

            _menu.AddSubMenu(clearMenu);
            
            var interruptMenu = new Menu("Settings", "Settings");
            interruptMenu.AddItem(new MenuItem("ElEasy.Darius.Interrupt.Activated", "Interrupt spells").SetValue(true));
            interruptMenu.AddItem(new MenuItem("ElEasy.Darius.Notifications", "Show notifications").SetValue(true));
            _menu.AddSubMenu(interruptMenu);

            var itemMenu = new Menu("Items", "Items");
            itemMenu.AddItem(new MenuItem("ElEasy.Darius.Items.Youmuu", "Use Youmuu's Ghostblade").SetValue(true));
            itemMenu.AddItem(new MenuItem("ElEasy.Darius.Items.Cutlass", "Use Cutlass").SetValue(true));
            itemMenu.AddItem(new MenuItem("ElEasy.Darius.Items.Blade", "Use Blade of the Ruined King").SetValue(true));
            itemMenu.AddItem(new MenuItem("ElEasy.Darius.Harasssfsddass.E", ""));
            itemMenu.AddItem(new MenuItem("ElEasy.Darius.Items.Blade.EnemyEHP", "Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
            itemMenu.AddItem(new MenuItem("ElEasy.Darius.Items.Blade.EnemyMHP", "My HP Percentage").SetValue(new Slider(80, 100, 0)));

            _menu.AddSubMenu(itemMenu);

            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElEasy.Darius.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElEasy.Darius.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Darius.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Darius.Draw.R", "Draw R").SetValue(new Circle()));

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

        #region itemusage

        private static void Items(Obj_AI_Base target)
        {
            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            var ghost = ItemData.Youmuus_Ghostblade.GetItem();
            var cutlass = ItemData.Bilgewater_Cutlass.GetItem();

            var useYoumuu = _menu.Item("ElEasy.Darius.Items.Youmuu").GetValue<bool>();
            var useCutlass = _menu.Item("ElEasy.Darius.Items.Cutlass").GetValue<bool>();
            var useBlade = _menu.Item("ElEasy.Darius.Items.Blade").GetValue<bool>();

            var useBladeEhp = _menu.Item("ElEasy.Darius.Items.Blade.EnemyEHP").GetValue<Slider>().Value;
            var useBladeMhp = _menu.Item("ElEasy.Darius.Items.Blade.EnemyMHP").GetValue<Slider>().Value;

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

        #region Interrupt

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            var useR = _menu.Item("ElEasy.Darius.Interrupt.Activated").GetValue<bool>();
            if (!useR)
                return;

            if (args.DangerLevel != Interrupter2.DangerLevel.High || sender.Distance(Player) > spells[Spells.E].Range)
                return;

            if (sender.IsValidTarget(spells[Spells.E].Range) && args.DangerLevel == Interrupter2.DangerLevel.High && spells[Spells.E].IsReady())
            {
                spells[Spells.E].Cast(sender);
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