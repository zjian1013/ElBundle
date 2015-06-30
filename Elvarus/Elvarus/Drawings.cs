using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Elvarus
{
    internal class Drawings
    {
        public static void Drawing_OnDraw(EventArgs args)
        {
            var drawOff = ElVarusMenu._menu.Item("ElVarus.Draw.off").GetValue<bool>();
            var drawQ = ElVarusMenu._menu.Item("ElVarus.Draw.Q").GetValue<Circle>();
            var drawW = ElVarusMenu._menu.Item("ElVarus.Draw.W").GetValue<Circle>();
            var drawE = ElVarusMenu._menu.Item("ElVarus.Draw.E").GetValue<Circle>();
            var drawR = ElVarusMenu._menu.Item("ElVarus.Draw.E").GetValue<Circle>();


            if (drawOff)
                return;

            if (drawQ.Active)
                if (Varus.spells[Spells.Q].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Varus.spells[Spells.Q].Range, Varus.spells[Spells.Q].IsReady() ? Color.Green : Color.Red);

            if (drawW.Active)
                if (Varus.spells[Spells.W].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Varus.spells[Spells.W].Range, Varus.spells[Spells.W].IsReady() ? Color.Green : Color.Red);

            if (drawE.Active)
                if (Varus.spells[Spells.E].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Varus.spells[Spells.E].Range, Varus.spells[Spells.E].IsReady() ? Color.Green : Color.Red);

            if (drawR.Active)
                if (Varus.spells[Spells.R].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Varus.spells[Spells.R].Range, Varus.spells[Spells.R].IsReady() ? Color.Green : Color.Red);
        }
    }
}
