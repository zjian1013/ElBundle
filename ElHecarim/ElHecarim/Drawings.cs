using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace ElHecarim
{
    internal class Drawings
    {
        public static void Drawing_OnDraw(EventArgs args)
        {
            var drawOff = ElHecarimMenu._menu.Item("ElHecarim.Draw.off").GetValue<bool>();
            var drawQ = ElHecarimMenu._menu.Item("ElHecarim.Draw.Q").GetValue<Circle>();
            var drawW = ElHecarimMenu._menu.Item("ElHecarim.Draw.W").GetValue<Circle>();
            var drawR = ElHecarimMenu._menu.Item("ElHecarim.Draw.R").GetValue<Circle>();

            if (drawOff)
                return;

            if (drawQ.Active)
                if (Hecarim.spells[Spells.Q].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Hecarim.spells[Spells.Q].Range, Hecarim.spells[Spells.Q].IsReady() ? Color.Green : Color.Red);

            if (drawW.Active)
                if (Hecarim.spells[Spells.W].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Hecarim.spells[Spells.W].Range, Hecarim.spells[Spells.W].IsReady() ? Color.Green : Color.Red);

            if (drawR.Active)
                if (Hecarim.spells[Spells.R].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Hecarim.spells[Spells.R].Range, Hecarim.spells[Spells.R].IsReady() ? Color.Green : Color.Red);
        }
    }
}