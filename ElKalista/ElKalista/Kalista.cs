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


namespace ElKalista
{
    internal enum Spells
    {
        Q,
        W,
        E,
        R
    }

    internal class Kalista
    {
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Orbwalking.Orbwalker _orbwalker;
        private static Obj_AI_Hero ConnectedAlly { get; set; }


        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 1180)},
            { Spells.W, new Spell(SpellSlot.W, 5200)},
            { Spells.E, new Spell(SpellSlot.E, 1000)},
            { Spells.R, new Spell(SpellSlot.R, 1400)}
        };
        #region hitchance

        private static HitChance CustomHitChance
        {
            get { return GetHitchance(); }
        }

        private static HitChance GetHitchance()
        {
            switch (ElKalistaMenu._menu.Item("ElKalista.hitChance").GetValue<StringList>().SelectedIndex)
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
            if (ObjectManager.Player.BaseSkinName != "Kalista")
                return;

            Console.WriteLine("Injected");

            Notifications.AddNotification("ElKalista by jQuery v1.0.0.0", 10000);

            spells[Spells.Q].SetSkillshot(0.25f, 30f, 1700f, true, SkillshotType.SkillshotLine);

            ElKalistaMenu.Initialize();
            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawings.Drawing_OnDraw;
        }

        #endregion

        #region OnGameUpdate

        private static void OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Physical);

            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo(target);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                   LaneClear();
                   JungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass(target);
                    break;
            }

            var save = ElKalistaMenu._menu.Item("ElKalista.misc.save").GetValue<bool>();
            var allyHp = ElKalistaMenu._menu.Item("ElKalista.misc.allyhp").GetValue<Slider>().Value;

            if (save)
            {
                ConnectedAlly = HeroManager.Allies.Find(h => h.Buffs.Any(b => b.Caster.IsMe && b.Name.Contains("kalistacoopstrikeally")));
                if (ConnectedAlly.HealthPercentage() < allyHp && ConnectedAlly.CountEnemiesInRange(spells[Spells.R].Range) > 0)
                    spells[Spells.R].Cast();
            }
        }
        #endregion

        #region itemusage

        private static void Items(Obj_AI_Base target)
        {
            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            var Ghost = ItemData.Youmuus_Ghostblade.GetItem();
            var cutlass = ItemData.Bilgewater_Cutlass.GetItem();

            var useYoumuu = ElKalistaMenu._menu.Item("ElKalista.Items.Youmuu").GetValue<bool>();
            var useCutlass = ElKalistaMenu._menu.Item("ElKalista.Items.Cutlass").GetValue<bool>();
            var useBlade = ElKalistaMenu._menu.Item("ElKalista.Items.Blade").GetValue<bool>();

            var useBladeEhp = ElKalistaMenu._menu.Item("ElKalista.Items.Blade.EnemyEHP").GetValue<Slider>().Value;
            var useBladeMhp = ElKalistaMenu._menu.Item("ElKalista.Items.Blade.EnemyMHP").GetValue<Slider>().Value;

            if (botrk.IsReady() && botrk.IsOwned(Player) && botrk.IsInRange(target)
            && target.HealthPercentage() <= useBladeEhp
            && useBlade)

                botrk.Cast(target);

            if (botrk.IsReady() && botrk.IsOwned(Player) && botrk.IsInRange(target)
                && Player.HealthPercentage() <= useBladeMhp
                && useBlade)

                botrk.Cast(target);

            if (cutlass.IsReady() && cutlass.IsOwned(Player) && cutlass.IsInRange(target) &&
                target.HealthPercentage() <= useBladeEhp
                && useCutlass)
                cutlass.Cast(target);

            if (Ghost.IsReady() && Ghost.IsOwned(Player) && target.IsValidTarget(spells[Spells.Q].Range)
                && useYoumuu)
                Ghost.Cast();
        }

        #endregion

        #region Harass

        private static void Harass(Obj_AI_Base target)
        {
            if (target == null || !target.IsValidTarget())
                return;

            var harassQ = ElKalistaMenu._menu.Item("ElKalista.Harass.Q").GetValue<bool>();
            var harassE = ElKalistaMenu._menu.Item("ElKalista.Harass.E").GetValue<bool>();
            var minmana = ElKalistaMenu._menu.Item("ElKalista.minmanaharass").GetValue<Slider>().Value;

            if (Player.ManaPercentage() < minmana)
                return;

            if (harassQ && spells[Spells.Q].IsReady())
            {
                if (spells[Spells.Q].GetPrediction(target).Hitchance >= CustomHitChance)
                    spells[Spells.Q].Cast(target);
            }

            if (harassE && spells[Spells.E].IsReady())
            {
                var minion = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.E].Range).Where(x => x.Health <= spells[Spells.E].GetDamage(x)).OrderBy(x => x.Health).FirstOrDefault();
                var Target = HeroManager.Enemies.Where(x => spells[Spells.E].CanCast(x) && spells[Spells.E].GetDamage(x) >= 1 && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield)).OrderByDescending(x => spells[Spells.E].GetDamage(x)).FirstOrDefault();

                if (Target != null && (Target.Health <= spells[Spells.E].GetDamage(Target) || (spells[Spells.E].CanCast(minion) && spells[Spells.E].CanCast(Target))))
                    spells[Spells.E].Cast();

                if (spells[Spells.E].CanCast(target) && spells[Spells.E].GetPrediction(target).Hitchance >= CustomHitChance && !Player.IsWindingUp && !Player.IsDashing())
                    spells[Spells.E].Cast(target);
            }
        }

        #endregion

        #region Combo

        private static void Combo(Obj_AI_Base target)
        {
            if (target == null || !target.IsValidTarget())
                return;

            Items(target);

            var comboQ = ElKalistaMenu._menu.Item("ElKalista.Combo.Q").GetValue<bool>();
            var comboE = ElKalistaMenu._menu.Item("ElKalista.Combo.E").GetValue<bool>();

            if (comboQ && spells[Spells.Q].IsReady())
            {
                if (spells[Spells.Q].GetPrediction(target).Hitchance >= CustomHitChance)
                    spells[Spells.Q].Cast(target);
            }

            if (comboE && spells[Spells.E].IsReady())
            {
                var minion = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.E].Range).Where(x => x.Health <= spells[Spells.E].GetDamage(x)).OrderBy(x => x.Health).FirstOrDefault();
                var Target = HeroManager.Enemies.Where(x => spells[Spells.E].CanCast(x) && spells[Spells.E].GetDamage(x) >= 1 && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield)).OrderByDescending(x => spells[Spells.E].GetDamage(x)).FirstOrDefault();

                if (Target != null && (Target.Health <= spells[Spells.E].GetDamage(Target) || (spells[Spells.E].CanCast(minion) && spells[Spells.E].CanCast(Target))))
                    spells[Spells.E].Cast();

                if (spells[Spells.E].CanCast(target) && spells[Spells.E].GetPrediction(target).Hitchance >= CustomHitChance && !Player.IsWindingUp && !Player.IsDashing())
                    spells[Spells.E].Cast(target);
            }
        }
        #endregion

        #region Laneclear

        private static void JungleClear()
        {

            var useQ = ElKalistaMenu._menu.Item("useQFarmJungle").GetValue<bool>();
            var useE = ElKalistaMenu._menu.Item("useEFarmJungle").GetValue<bool>();
            var minmana = ElKalistaMenu._menu.Item("minmanaclear").GetValue<Slider>().Value;
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 700, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Player.ManaPercentage() < minmana)
                return;
           
            foreach (var minion in minions)
            {
                if (spells[Spells.Q].IsReady() && useQ)
                {
                    spells[Spells.Q].Cast(minion);
                }

                if (spells[Spells.E].IsReady() && useE && minions[0].Health + (minions[0].HPRegenRate / 2) <= spells[Spells.E].GetDamage(minion))
                {
                    spells[Spells.E].Cast();
                }
            }
        }

        private static void LaneClear()
        {
            var useQ = ElKalistaMenu._menu.Item("useQFarm").GetValue<bool>();
            var useE = ElKalistaMenu._menu.Item("useQFarm").GetValue<bool>();
            var countMinions = ElKalistaMenu._menu.Item("ElKalista.Count.Minions").GetValue<Slider>().Value;

            var minmana = ElKalistaMenu._menu.Item("minmanaclear").GetValue<Slider>().Value;
            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, spells[Spells.Q].Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);

            //var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.Q].Range, MinionTypes.All, MinionTeam.Enemy);

            if (minions.Count == 0)
                return;

            if (Player.ManaPercentage() < minmana)
                return;

            if (spells[Spells.Q].IsReady() && useQ)
            {
                foreach (var minion in minions.Where(x => x.Health <= spells[Spells.Q].GetDamage(x)))
                {
                    spells[Spells.Q].Cast(minion);
                }
            }
        }

        #endregion
    }
}