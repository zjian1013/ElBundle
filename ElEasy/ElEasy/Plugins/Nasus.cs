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
    public class Nasus : Standards
    {
        private static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, Player.AttackRange + 50) },
            { Spells.W, new Spell(SpellSlot.W, 600) },
            { Spells.E, new Spell(SpellSlot.E, 650) },
            { Spells.R, new Spell(SpellSlot.R) }
        };

        public static void Load()
        {
            _ignite = Player.GetSpellSlot("summonerdot");
            spells[Spells.E].SetSkillshot(spells[Spells.E].Instance.SData.SpellCastTime, spells[Spells.E].Instance.SData.LineWidth, spells[Spells.E].Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCircle);

            Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
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
                    Laneclear();
                    Jungleclear();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    OnHarass();
                    break;

                case Orbwalking.OrbwalkingMode.LastHit:
                    OnLastHit();
                    break;
            }

            var active = _menu.Item("ElEasy.Nasus.Lasthit.Activated").GetValue<KeyBind>().Active;
            if (active)
                AutoLastHit();            
        }

        #endregion


        #region Farm

        private static void Laneclear()
        {
            var useQ = _menu.Item("ElEasy.Nasus.LaneClear.Q").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Nasus.LaneClear.E").GetValue<bool>();

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.E].Range);
            if (minions.Count <= 0)
                return;

          /*  if (useQ && spells[Spells.Q].IsReady())
            {
                if (minions.Find(x => x.Health >= spells[Spells.Q].GetDamage(x) && x.IsValidTarget()) != null)
                {
                    spells[Spells.Q].Cast();
                }
            }*/

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

            if (useE && spells[Spells.E].IsReady())
            {
                if (minions.Count > 1)
                {
                    var farmLocation = spells[Spells.E].GetCircularFarmLocation(minions);
                    spells[Spells.E].Cast(farmLocation.Position);
                }
            }
        }

        private static void Jungleclear()
        {

            var useQ = _menu.Item("ElEasy.Nasus.JungleClear.Q").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Nasus.JungleClear.E").GetValue<bool>();

            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.E].Range, MinionTypes.All, MinionTeam.Neutral,
           MinionOrderTypes.MaxHealth);

            if (minions.Count <= 0)
                return;

            if (useQ && spells[Spells.Q].IsReady())
            {
                if (minions.Find(x => x.Health >= spells[Spells.Q].GetDamage(x) && x.IsValidTarget()) != null)
                {
                    spells[Spells.Q].Cast();
                }
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

        #region OnHarass
        private static void OnHarass()
        {
            var eTarget = TargetSelector.GetTarget(spells[Spells.E].Range + spells[Spells.E].Width, TargetSelector.DamageType.Magical);
            if (eTarget == null || !eTarget.IsValid)
                return;

            var useE = _menu.Item("ElEasy.Nasus.Harass.E").GetValue<bool>();

            if (useE && spells[Spells.E].IsReady() && eTarget.IsValidTarget() && spells[Spells.E].IsInRange(eTarget))
            {
                var pred = spells[Spells.E].GetPrediction(eTarget).Hitchance;
                if (pred >= HitChance.High)
                    spells[Spells.E].Cast(eTarget);
            }

        }

        #endregion

        #region LastHit

        private static void OnLastHit()
        {
            var minions = MinionManager.GetMinions(Player.Position, spells[Spells.E].Range, MinionTypes.All, MinionTeam.Enemy,
               MinionOrderTypes.MaxHealth);

            foreach (var minion in minions)
            {
                if (spells[Spells.Q].GetDamage(minion) > minion.Health && spells[Spells.Q].IsReady())
                {
                    spells[Spells.Q].Cast();
                }
            }
        }

        #endregion

        #region AutoLasthit
        private static void AutoLastHit()
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed || Player.IsRecalling())
                return;

            var minions = MinionManager.GetMinions(Player.Position, spells[Spells.E].Range, MinionTypes.All, MinionTeam.Enemy,
               MinionOrderTypes.MaxHealth);

            foreach (var minion in minions)
            {
                if (spells[Spells.Q].GetDamage(minion) > minion.Health &&
                   Vector3.Distance(ObjectManager.Player.ServerPosition, minion.Position) < Player.AttackRange + 50 && spells[Spells.Q].IsReady())
                {
                    Orbwalker.SetAttack(false);
                    spells[Spells.Q].Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                    Orbwalker.SetAttack(true);
                    break;
                }
            }
        }
        #endregion

        #region OnCombo

        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.W].Range, TargetSelector.DamageType.Physical);
            var eTarget = TargetSelector.GetTarget(spells[Spells.E].Range + spells[Spells.E].Width, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
                return;

            var useQ = _menu.Item("ElEasy.Nasus.Combo.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Nasus.Combo.W").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Nasus.Combo.E").GetValue<bool>();
            var useR = _menu.Item("ElEasy.Nasus.Combo.R").GetValue<bool>();
            var useI = _menu.Item("ElEasy.Nasus.Combo.Ignite").GetValue<bool>();
            var countEnemies = _menu.Item("ElEasy.Nasus.Combo.Count.R").GetValue<Slider>().Value;
            var playerHp = _menu.Item("ElEasy.Nasus.Combo.HP").GetValue<Slider>().Value;

            if (useQ && spells[Spells.Q].IsReady())
            {
                spells[Spells.Q].Cast();
            }

            if (useW && spells[Spells.W].IsReady())
            {
                spells[Spells.W].Cast(target);
            }

            if (useE && spells[Spells.E].IsReady() && eTarget.IsValidTarget() && spells[Spells.E].IsInRange(eTarget))
            {
                var pred = spells[Spells.E].GetPrediction(eTarget).Hitchance;
                if (pred >= HitChance.High)
                    spells[Spells.E].Cast(eTarget);
            }

            if (useR && spells[Spells.R].IsReady() && Player.CountEnemiesInRange(spells[Spells.W].Range) >= countEnemies 
                || (Player.Health / Player.MaxHealth) * 100 <= playerHp)
            {
                spells[Spells.R].CastOnUnit(Player);
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
            var drawOff = _menu.Item("ElEasy.Nasus.Draw.off").GetValue<bool>();
            var drawW = _menu.Item("ElEasy.Nasus.Draw.W").GetValue<Circle>();
            var drawE = _menu.Item("ElEasy.Nasus.Draw.E").GetValue<Circle>();
            var drawText = _menu.Item("ElEasy.Nasus.Draw.Text").GetValue<bool>();
            var rBool = _menu.Item("ElEasy.Nasus.Lasthit.Activated").GetValue<KeyBind>().Active;
            var helper = _menu.Item("ElEasy.Nasus.Draw.MinionHelper").GetValue<bool>();

            var playerPos = Drawing.WorldToScreen(ObjectManager.Player.Position);

            if (drawOff)
                return;

            if (drawE.Active)
                if (spells[Spells.E].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.E].Range, Color.White);

            if (drawW.Active)
                if (spells[Spells.W].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.W].Range, Color.White);

            if (drawText)
                Drawing.DrawText(
                    playerPos.X - 70, playerPos.Y + 40, (rBool ? Color.Green : Color.Red), "{0}",
                    (rBool ? "Auto lasthit enabled" : "Auto lasthit disabled"));

            if (helper)
            {
                var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.E].Range, MinionTypes.All, MinionTeam.NotAlly);
                foreach (var minion in minions)
                {
                    if (minion != null)
                    {
                        var qDamage = ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q);
                        if ((qDamage > minion.Health))
                            Render.Circle.DrawCircle(minion.ServerPosition, minion.BoundingRadius, Color.Black);
                    }
                }
            }
        }

        #endregion


        #region ComboDamage

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (spells[Spells.Q].IsReady())
            {
                damage += (float)ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            return damage;
        }

        #endregion
    

        #region Menu
        private static void Initialize()
        {
            _menu = new Menu("ElNasus", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElEasy.Nasus.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Nasus.Combo.W", "Use W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Nasus.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Nasus.Combo.R", "Use R").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Nasus.Combo.Count.R", "Minimum champions in range for R").SetValue(new Slider(2, 1, 5)));
            cMenu.AddItem(new MenuItem("ElEasy.Nasus.Combo.HP", "Minimum HP for R").SetValue(new Slider(55)));
            cMenu.AddItem(new MenuItem("ElEasy.Nasus.Combo.Ignite", "Use Ignite").SetValue(true));

            _menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElEasy.Nasus.Harass.E", "Use E").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Nasus.Harass.Player.Mana", "Minimum Mana").SetValue(new Slider(55)));

            _menu.AddSubMenu(hMenu);

            var clearMenu = new Menu("Clear", "Clear");
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Nasus.LaneClear.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Nasus.LaneClear.E", "Use E").SetValue(true));
            clearMenu.SubMenu("Jungleclear").AddItem(new MenuItem("ElEasy.Nasus.JungleClear.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Jungleclear").AddItem(new MenuItem("ElEasy.Nasus.JungleClear.E", "Use E").SetValue(true));

            _menu.AddSubMenu(clearMenu);


            var settingsMenu = new Menu("Lasthit", "Lasthit");
            settingsMenu.AddItem(new MenuItem("ElEasy.Nasus.Lasthit.Activated", "Auto Lasthit").SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));
            _menu.AddSubMenu(settingsMenu);

            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElEasy.Nasus.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElEasy.Nasus.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Nasus.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Nasus.Draw.Text", "Draw text").SetValue(true));
            miscMenu.AddItem(new MenuItem("ElEasy.Nasus.Draw.MinionHelper", "Draw killable minions").SetValue(true));


            /*var dmgAfterE = new MenuItem("ElDiana.DrawComboDamage", "Draw Q damage").SetValue(true);
            var drawFill = new MenuItem("ElDiana.DrawColour", "Fill colour", true).SetValue(new Circle(true, Color.FromArgb(204, 204, 0, 0)));
            miscMenu.AddItem(drawFill);
            miscMenu.AddItem(dmgAfterE);

            DrawDamage.DamageToUnit = GetComboDamage;
            DrawDamage.Enabled = dmgAfterE.GetValue<bool>();
            DrawDamage.Fill = drawFill.GetValue<Circle>().Active;
            DrawDamage.FillColor = drawFill.GetValue<Circle>().Color;

            dmgAfterE.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                DrawDamage.Enabled = eventArgs.GetNewValue<bool>();
            };

            drawFill.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                DrawDamage.Fill = eventArgs.GetNewValue<Circle>().Active;
                DrawDamage.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };*/


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
    }
}