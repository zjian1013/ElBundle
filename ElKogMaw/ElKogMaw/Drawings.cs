using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace ElKogMaw
{
    internal class Drawings
    {

        public static void OnDraw(EventArgs args)
        {

            var drawOff = ElKogMawMenu._menu.Item("ElKogMaw.Draw.off").GetValue<bool>();
            var drawQ = ElKogMawMenu._menu.Item("ElKogMaw.Draw.Q").GetValue<Circle>();
            var drawW = ElKogMawMenu._menu.Item("ElKogMaw.Draw.W").GetValue<Circle>();
            var drawE = ElKogMawMenu._menu.Item("ElKogMaw.Draw.E").GetValue<Circle>();
            var drawR = ElKogMawMenu._menu.Item("ElKogMaw.Draw.R").GetValue<Circle>();

            if (drawOff)
                return;

            if (drawQ.Active)
                if (KogMaw.spells[Spells.Q].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, KogMaw.spells[Spells.Q].Range, Color.White);

            if (drawW.Active)
                if (KogMaw.spells[Spells.W].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, KogMaw.spells[Spells.W].Range, Color.White);

            if (drawE.Active)
                if (KogMaw.spells[Spells.E].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, KogMaw.spells[Spells.E].Range, Color.White);

            if (drawR.Active)
                if (KogMaw.spells[Spells.R].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, KogMaw.spells[Spells.R].Range, Color.White);

        }
    }
}