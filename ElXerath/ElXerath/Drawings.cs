using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace ElXerath
{
    internal class Drawings
    {
        public static void Drawing_OnDraw(EventArgs args)
        {
            var drawOff = ElXerathMenu._menu.Item("ElXerath.Draw.off").GetValue<bool>();
            var drawQ = ElXerathMenu._menu.Item("ElXerath.Draw.Q").GetValue<Circle>();
            var drawW = ElXerathMenu._menu.Item("ElXerath.Draw.W").GetValue<Circle>();
            var drawR = ElXerathMenu._menu.Item("ElXerath.Draw.R").GetValue<Circle>();
            var drawText = ElXerathMenu._menu.Item("ElXerath.Draw.Text").GetValue<bool>();
            var rBool = ElXerathMenu._menu.Item("ElXerath.AutoHarass").GetValue<KeyBind>().Active;

            if (drawOff)
                return;

            var playerPos = Drawing.WorldToScreen(ObjectManager.Player.Position);

            if (drawQ.Active)
                if (Xerath.spells[Spells.Q].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Xerath.spells[Spells.Q].Range, Xerath.spells[Spells.Q].IsReady() ? Color.Green : Color.Red);

            if (drawW.Active)
                if (Xerath.spells[Spells.W].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Xerath.spells[Spells.W].Range, Xerath.spells[Spells.W].IsReady() ? Color.Green : Color.Red);

            if (drawR.Active)
                if (Xerath.spells[Spells.R].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Xerath.spells[Spells.R].Range, Xerath.spells[Spells.R].IsReady() ? Color.Green : Color.Red);

            if (drawText)
                Drawing.DrawText(playerPos.X - 70, playerPos.Y + 40, (rBool ? Color.Green : Color.Red), "{0}", (rBool ? "Auto harass enabled" : "Auto harass disabled"));
        }
    }
}