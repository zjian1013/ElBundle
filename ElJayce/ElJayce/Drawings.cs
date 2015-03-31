using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace ElJayce
{
    internal class Drawings
    {
        public static void Drawing_OnDraw(EventArgs args)
        {
            var drawOff = ElJayceMenu._menu.Item("ElJayce.Draw.off").GetValue<bool>();
            var drawQ = ElJayceMenu._menu.Item("ElJayce.Draw.Q").GetValue<Circle>();
            var drawW = ElJayceMenu._menu.Item("ElJayce.Draw.W").GetValue<Circle>();
            var drawE = ElJayceMenu._menu.Item("ElJayce.Draw.E").GetValue<Circle>();

            if (drawOff)
                return;

            if (drawQ.Active)
                if (Jayce.spells[Spells.Q].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Jayce.spells[Spells.Q].Range, Color.White);

            if (drawE.Active)
                if (Jayce.spells[Spells.E].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Jayce.spells[Spells.E].Range, Color.White);

            if (drawW.Active)
                if (Jayce.spells[Spells.W].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Jayce.spells[Spells.W].Range, Color.White);
        }
    }
}