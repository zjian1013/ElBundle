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


namespace ElHecarim
{
    internal enum Spells
    {
        Q,
        W,
        E,
        R
    }

    internal class Hecarim
    {

        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Orbwalking.Orbwalker _orbwalker;
        private static Obj_AI_Hero ConnectedAlly { get; set; }


        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 350)},
            { Spells.W, new Spell(SpellSlot.W, 525)},
            { Spells.E, new Spell(SpellSlot.E, 0)},
            { Spells.R, new Spell(SpellSlot.R, 1000)}
        };

        #region Gameloaded 

        public static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Hecarim")
                return;

            Console.WriteLine("Injected");

            Notifications.AddNotification("ElHecarim by jQuery v1.0.0.0", 1000);

            spells[Spells.Q].SetSkillshot(0.5f, 200f, 1200f, false, SkillshotType.SkillshotLine);

            ElHecarimMenu.Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawings.Drawing_OnDraw;
        }

        #endregion

        #region OnGameUpdate

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Physical);

            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo(target);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    //LaneClear();
                    //JungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    //Harass(target);
                    break;
            }
        }
        #endregion

        #region Combo

        private static void Combo(Obj_AI_Base target)
        {
            if (target == null || !target.IsValidTarget())
                return;

            //combo soon tm 
        }

        #endregion
    }
}