using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace ElRumble
{
    internal class Drawings
    {
        public static void OnDraw(EventArgs args)
        {
            var drawOff = ElRumbleMenu._menu.Item("ElRumble.Draw.off").GetValue<bool>();
            var drawQ = ElRumbleMenu._menu.Item("ElRumble.Draw.Q").GetValue<Circle>();
            var drawE = ElRumbleMenu._menu.Item("ElRumble.Draw.E").GetValue<Circle>();
            var drawR = ElRumbleMenu._menu.Item("ElRumble.Draw.R").GetValue<Circle>();

            if (drawOff)
                return;

            if (drawQ.Active)
                if (Rumble.spells[Spells.Q].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Rumble.spells[Spells.Q].Range, Color.White);

            if (drawE.Active)
                if (Rumble.spells[Spells.E].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Rumble.spells[Spells.E].Range, Color.White);

            if (drawR.Active)
                if (Rumble.spells[Spells.R].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Rumble.spells[Spells.R].Range, Color.White);
        }
    }
}