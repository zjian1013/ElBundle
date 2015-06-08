using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace ElLucian
{
    internal class Drawings
    {

        public static void OnDraw(EventArgs args)
        {
 
            var drawOff = ElLucianMenu._menu.Item("ElLucian.Draw.off").GetValue<bool>();
            var drawQ = ElLucianMenu._menu.Item("ElLucian.Draw.Q").GetValue<Circle>();
            var drawW = ElLucianMenu._menu.Item("ElLucian.Draw.W").GetValue<Circle>();
            var drawE = ElLucianMenu._menu.Item("ElLucian.Draw.E").GetValue<Circle>();
            var drawR = ElLucianMenu._menu.Item("ElLucian.Draw.R").GetValue<Circle>();

            if (drawOff)
                return;

            if (drawQ.Active)
                if (Lucian.spells[Spells.Q].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Lucian.spells[Spells.Q].Range, Color.White);

            if (drawW.Active)
                if (Lucian.spells[Spells.W].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Lucian.spells[Spells.W].Range, Color.White);

            if (drawE.Active)
                if (Lucian.spells[Spells.E].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Lucian.spells[Spells.E].Range, Color.White);

            if (drawR.Active)
                if (Lucian.spells[Spells.R].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Lucian.spells[Spells.R].Range, Color.White);

        }
    }
}