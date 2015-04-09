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
    public class Taric : Standards
    {
        private static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 750) },
            { Spells.W, new Spell(SpellSlot.W, 200) },
            { Spells.E, new Spell(SpellSlot.E, 625) },
            { Spells.R, new Spell(SpellSlot.R, 200) }
        };

        public static void Load()
        {
            _ignite = Player.GetSpellSlot("summonerdot");

            Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
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
     
                case Orbwalking.OrbwalkingMode.Mixed:
                   OnHarass();
                    break;
            }

            HealManager();
        }
        #endregion

        #region HealManager

        private static void HealManager()
        {

            var useHeal = _menu.Item("ElEasy.Taric.Heal.Activated").GetValue<bool>();
            var playerMana = _menu.Item("ElEasy.Taric.Heal.Player.Mana").GetValue<Slider>().Value;
            var playerHp = _menu.Item("ElEasy.Taric.Heal.Player.HP").GetValue<Slider>().Value;
            var allyHp = _menu.Item("ElEasy.Taric.Heal.Ally.HP").GetValue<Slider>().Value;

            if (Player.HasBuff("Recall") || Player.InFountain() || !useHeal|| Player.ManaPercent < playerMana || !spells[Spells.Q].IsReady())
                return;

            //self heal
            if ((Player.Health / Player.MaxHealth) * 100 <= playerHp)
            {
                spells[Spells.Q].CastOnUnit(Player);
            }

            //ally
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && !h.IsMe))
            {
                if ((hero.Health / hero.MaxHealth) * 100 <= allyHp && spells[Spells.Q].IsInRange(hero))
                {
                    spells[Spells.Q].Cast(hero);
                }
            }
        }

        #endregion

        #region Draw

        private static void OnDraw(EventArgs args)
        {
            var drawOff = _menu.Item("ElEasy.Taric.Draw.off").GetValue<bool>();
            var drawQ = _menu.Item("ElEasy.Taric.Draw.Q").GetValue<Circle>();
            var drawW = _menu.Item("ElEasy.Taric.Draw.W").GetValue<Circle>();
            var drawE = _menu.Item("ElEasy.Taric.Draw.E").GetValue<Circle>();
            var drawR = _menu.Item("ElEasy.Taric.Draw.R").GetValue<Circle>();


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
                if (spells[Spells.W].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.R].Range, Color.White);
        }


    #endregion

        #region OnCombo
    private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.E].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
                return;

            var useE = _menu.Item("ElEasy.Taric.Combo.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Taric.Combo.W").GetValue<bool>();
            var useR = _menu.Item("ElEasy.Taric.Combo.R").GetValue<bool>();
            var useI = _menu.Item("ElEasy.Taric.Combo.Ignite").GetValue<bool>();
            var countEnemies = _menu.Item("ElEasy.Taric.Combo.Count.Enemies").GetValue<Slider>().Value;

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
            {
                spells[Spells.E].Cast(target);
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
            {
                spells[Spells.W].CastOnUnit(Player);
            }

            if (useR && spells[Spells.R].IsReady() && spells[Spells.R].IsInRange(target) && Player.CountEnemiesInRange(spells[Spells.R].Range) >= countEnemies)
            {
                spells[Spells.R].CastOnUnit(Player);
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
            var target = TargetSelector.GetTarget(spells[Spells.E].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
                return;

            var useE = _menu.Item("ElEasy.Taric.Harass.Q").GetValue<bool>();
            var useW = _menu.Item("ElEasy.Taric.Harass.W").GetValue<bool>();
            var playerMana = _menu.Item("ElEasy.Taric.Harass.Player.Mana").GetValue<Slider>().Value;

            if (Player.Mana < playerMana)
                return;

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
            {
                spells[Spells.E].Cast(target);
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
            {
                spells[Spells.W].CastOnUnit(Player);
            }
        }
        #endregion

        #region Gapcloser

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var gapCloserActive = _menu.Item("ElEasy.Taric.GapCloser.Activated").GetValue<bool>();

            if (gapCloserActive && spells[Spells.E].IsReady() &&
                gapcloser.Sender.Distance(Player) < spells[Spells.E].Range)
            {
                spells[Spells.E].Cast(gapcloser.Sender);
            }
        }

        #endregion

        #region Interrupt

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel != Interrupter2.DangerLevel.High || sender.Distance(Player) > spells[Spells.E].Range)
                return;

            if (sender.IsValidTarget(spells[Spells.E].Range) && args.DangerLevel == Interrupter2.DangerLevel.High && spells[Spells.E].IsReady())
            {
                spells[Spells.E].Cast(sender);
            }
        }

        #endregion

        #region Menu
        private static void Initialize()
        {
            _menu = new Menu("ElTaric", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElEasy.Taric.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Taric.Combo.W", "Use W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Taric.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Taric.Combo.R", "Use R").SetValue(true));
            cMenu.AddItem(
                new MenuItem("ElEasy.Taric.Combo.Count.Enemies", "Enemies in range for R").SetValue(new Slider(2, 1, 5)));
            cMenu.AddItem(new MenuItem("ElEasy.Taric.Combo.Ignite", "Use Ignite").SetValue(true));

            _menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElEasy.Taric.Harass.W", "Use W").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Taric.Harass.E", "Use E").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Taric.Harass.Player.Mana", "Minimum Mana").SetValue(new Slider(55)));

            _menu.AddSubMenu(hMenu);

            var healMenu = new Menu("Heal", "Heal");
            healMenu.AddItem(new MenuItem("ElEasy.Taric.Heal.Activated", "Heal").SetValue(true));
            healMenu.AddItem(new MenuItem("ElEasy.Taric.Heal.Player.HP", "Player HP").SetValue(new Slider(55)));
            healMenu.AddItem(new MenuItem("ElEasy.Taric.Heal.Ally.HP", "Ally HP").SetValue(new Slider(55)));
            healMenu.AddItem(new MenuItem("ElEasy.Taric.Heal.Player.Mana", "Minimum Mana").SetValue(new Slider(55)));

            _menu.AddSubMenu(healMenu);

            var interruptMenu = new Menu("Settings", "Settings");
            interruptMenu.AddItem(new MenuItem("ElEasy.Taric.Interrupt.Activated", "Interrupt spells").SetValue(true));
            interruptMenu.AddItem(new MenuItem("ElEasy.Taric.GapCloser.Activated", "Anti gapcloser").SetValue(true));

            _menu.AddSubMenu(interruptMenu);

            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElEasy.Taric.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElEasy.Taric.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Taric.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Taric.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Taric.Draw.R", "Draw R").SetValue(new Circle()));

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