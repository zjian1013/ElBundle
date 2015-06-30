using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace ElEasy.Plugins
{
    public class Leona : Standards
    {
        private static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, Player.AttackRange + 25) },
            { Spells.W, new Spell(SpellSlot.W, 200) },
            { Spells.E, new Spell(SpellSlot.E, 700) },
            { Spells.R, new Spell(SpellSlot.R, 1200) }
        };

        public static void Load()
        {
            _ignite = Player.GetSpellSlot("summonerdot");


            Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;

            spells[Spells.E].SetSkillshot(0.25f, 120f, 2000f, false, SkillshotType.SkillshotLine);
            spells[Spells.R].SetSkillshot(1f, 300f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        #region hitchance

        private static HitChance CustomHitChance
        {
            get { return GetHitchance(); }
        }

        private static HitChance GetHitchance()
        {
            switch (_menu.Item("ElEasy.Leona.Hitchance").GetValue<StringList>().SelectedIndex)
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
            }

            AutoUlt();
        }

        #endregion

        #region AutoUlt

        private static void AutoUlt()
        {
            var rTarget = TargetSelector.GetTarget(spells[Spells.R].Range, TargetSelector.DamageType.Magical);
            if (rTarget == null || !rTarget.IsValid)
                return;

            var activated = _menu.Item("ElEasy.Leona.AutoUlt.Activated").GetValue<bool>();
            var countEnemies = _menu.Item("ElEasy.Leona.AutoUlt.Count").GetValue<Slider>().Value;

            if (activated && spells[Spells.R].IsReady() && Player.CountEnemiesInRange(spells[Spells.R].Range) >= countEnemies)
            {
                var pred = spells[Spells.R].GetPrediction(rTarget).Hitchance;
                if (pred >= CustomHitChance)
                    spells[Spells.R].CastIfWillHit(rTarget, countEnemies);
            }
        }

        #endregion

        #region OnHarass
        private static void OnHarass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.E].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
                return;

            var useQ = _menu.Item("ElEasy.Leona.Harass.Q").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Leona.Harass.E").GetValue<bool>();
            var playerMana = _menu.Item("ElEasy.Leona.Harass.Player.Mana").GetValue<Slider>().Value;

            if (Player.Mana < playerMana)
                return;

            if (useQ && spells[Spells.Q].IsReady())
            {
                spells[Spells.Q].Cast();
            }

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
            {
                spells[Spells.E].Cast(target);
            }
        }

        #endregion

        #region onCombo

        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.E].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
                return;

            var useQ = _menu.Item("ElEasy.Leona.Combo.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Leona.Combo.W").GetValue<bool>();
            var useE = _menu.Item("ElEasy.Leona.Combo.E").GetValue<bool>();
            var useR = _menu.Item("ElEasy.Leona.Combo.R").GetValue<bool>();
            var useI = _menu.Item("ElEasy.Leona.Combo.Ignite").GetValue<bool>();
            var countEnemies = _menu.Item("ElEasy.Leona.Combo.Count.Enemies").GetValue<Slider>().Value;

   
            if (useQ && spells[Spells.Q].IsReady() && 
                !target.HasBuff("BlackShield") || 
                !target.HasBuff("SivirShield") || 
                !target.HasBuff("BansheesVeil") || 
                !target.HasBuff("ShroudofDarkness"))
             {
                 spells[Spells.Q].Cast();
             }

             if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
             {
                 spells[Spells.W].Cast(Player);
             }

            if (useE && spells[Spells.E].IsReady())
            {
                var pred = spells[Spells.E].GetPrediction(target).Hitchance;
                if(pred >= CustomHitChance)
                    spells[Spells.E].Cast(target);
            }

            if (useR 
                && spells[Spells.R].IsReady() 
                && spells[Spells.R].IsInRange(target) 
                && Player.CountEnemiesInRange(spells[Spells.R].Range) >= countEnemies)
            {
                var pred = spells[Spells.R].GetPrediction(target).Hitchance;
                if(pred >= CustomHitChance)
                    spells[Spells.R].Cast(target);
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
            var drawOff = _menu.Item("ElEasy.Leona.Draw.off").GetValue<bool>();
            var drawQ = _menu.Item("ElEasy.Leona.Draw.Q").GetValue<Circle>();
            var drawE = _menu.Item("ElEasy.Leona.Draw.E").GetValue<Circle>();
            var drawW = _menu.Item("ElEasy.Leona.Draw.W").GetValue<Circle>();
            var drawR = _menu.Item("ElEasy.Leona.Draw.R").GetValue<Circle>();

            if (drawOff)
                return;

            if (drawQ.Active)
                if (spells[Spells.Q].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.Q].Range, Color.White);

            if (drawE.Active)
                if (spells[Spells.E].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.E].Range, Color.White);

            if (drawW.Active)
                if (spells[Spells.W].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.W].Range, Color.White);

            if (drawR.Active)
                if (spells[Spells.R].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.R].Range, Color.White);
        }


        #endregion

        #region Gapcloser

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var gapCloserActive = _menu.Item("ElEasy.Leona.Interrupt.Activated").GetValue<bool>();

            if (gapCloserActive && spells[Spells.Q].IsReady() &&
                gapcloser.Sender.Distance(Player) < spells[Spells.Q].Range)
            {
                spells[Spells.Q].Cast();
            }
        }

        #endregion

        #region Interrupt

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel != Interrupter2.DangerLevel.High || sender.Distance(Player) > spells[Spells.R].Range)
                return;

            if (sender.IsValidTarget(spells[Spells.R].Range) && args.DangerLevel == Interrupter2.DangerLevel.High && spells[Spells.R].IsReady())
            {
                spells[Spells.R].Cast(sender);
            }
        }

        #endregion

        #region Menu
        private static void Initialize()
        {
            _menu = new Menu("ElLeona", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElEasy.Leona.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Leona.Combo.W", "Use W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Leona.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Leona.Combo.R", "Use R").SetValue(true));
            cMenu.AddItem(
                new MenuItem("ElEasy.Leona.Combo.Count.Enemies", "Enemies in range for R").SetValue(new Slider(2, 1, 5)));
            cMenu.AddItem(new MenuItem("ElEasy.Leona.Hitchance", "Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
            cMenu.AddItem(new MenuItem("ElEasy.Leona.Combo.Ignite", "Use Ignite").SetValue(true));

            _menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElEasy.Leona.Harass.Q", "Use Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Leona.Harass.E", "Use E").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Leona.Harass.Player.Mana", "Minimum Mana").SetValue(new Slider(55)));

            _menu.AddSubMenu(hMenu);


            var settingsMenu = new Menu("Settings", "Settings");
            settingsMenu.AddItem(new MenuItem("xxx", ""));
            settingsMenu.AddItem(new MenuItem("ElEasy.Leona.Interrupt.Activated", "Interrupt spells").SetValue(true));
            settingsMenu.AddItem(new MenuItem("ElEasy.Leona.GapCloser.Activated", "Anti gapcloser").SetValue(true));
            settingsMenu.SubMenu("Automatic ult").AddItem(new MenuItem("ElEasy.Leona.AutoUlt.Activated", "Auto ult").SetValue(false));
            settingsMenu.SubMenu("Automatic ult").AddItem(new MenuItem("ElEasy.Leona.AutoUlt.Count", "Min targets for R").SetValue(new Slider(3, 1, 5)));

            _menu.AddSubMenu(settingsMenu);


            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElEasy.Leona.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElEasy.Leona.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Leona.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Leona.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Leona.Draw.R", "Draw R").SetValue(new Circle()));

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
