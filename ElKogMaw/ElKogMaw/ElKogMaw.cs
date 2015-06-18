using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ElKogMaw
{
    internal enum Spells
    {
        Q,
        W,
        E,
        R
    }

    internal static class KogMaw
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static String ScriptVersion
        {
            get
            {
                return typeof(KogMaw).Assembly.GetName().Version.ToString();
            }
        }
        public static Orbwalking.Orbwalker Orbwalker;
        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 675f) },
            { Spells.W, new Spell(SpellSlot.W, 1000f) },
            { Spells.E, new Spell(SpellSlot.E, 425f) },
            { Spells.R, new Spell(SpellSlot.R, 1400f) }
        };


        #region Gameloaded 

        public static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "KogMaw")
            {
                return;
            }

            Notifications.AddNotification(String.Format("ElKogMaw by jQuery v{0}", ScriptVersion), 10000);
            ElKogMawMenu.Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawings.OnDraw;
        }

        #endregion

        #region OnGameUpdate

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
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    break;
            }
        }
        #endregion

        #region OnGameUpdate

        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValidTarget())
                return;
        }

        #endregion
    }
}