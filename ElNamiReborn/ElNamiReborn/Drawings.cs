using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace ElNamiReborn
{
	internal class Drawings
	{
		public static void Drawing_OnDraw(EventArgs args)
		{
			var drawOff = ElNamiMenu._menu.Item("ElNamiMenu.Draw.off").GetValue<bool>();
			var drawQ = ElNamiMenu._menu.Item("ElNamiMenu.Draw.Q").GetValue<Circle>();
			var drawW = ElNamiMenu._menu.Item("ElNamiMenu.Draw.W").GetValue<Circle>();
			var drawE = ElNamiMenu._menu.Item("ElNamiMenu.Draw.E").GetValue<Circle>();
			var drawR = ElNamiMenu._menu.Item("ElNamiMenu.Draw.R").GetValue<Circle>();
			var drawText = ElNamiMenu._menu.Item("ElNamiReborn.Draw.Text").GetValue<bool>();
			var rBool = ElNamiMenu._menu.Item("ElNamiReborn.AutoHarass.Activated").GetValue<KeyBind>().Active;

			if (drawOff)
				return;

			var playerPos = Drawing.WorldToScreen(ObjectManager.Player.Position);

			if (drawQ.Active)
				if (Nami.spells[Spells.Q].Level > 0)
					Render.Circle.DrawCircle(ObjectManager.Player.Position, Nami.spells[Spells.Q].Range, Color.White);

			if (drawE.Active)
				if (Nami.spells[Spells.E].Level > 0)
					Render.Circle.DrawCircle(ObjectManager.Player.Position, Nami.spells[Spells.E].Range, Color.White);

			if (drawW.Active)
				if (Nami.spells[Spells.W].Level > 0)
					Render.Circle.DrawCircle(ObjectManager.Player.Position, Nami.spells[Spells.W].Range, Color.White);

			if (drawR.Active)
				if (Nami.spells[Spells.R].Level > 0)
					Render.Circle.DrawCircle(ObjectManager.Player.Position, Nami.spells[Spells.R].Range, Color.White);

			if (drawText)
				Drawing.DrawText( playerPos.X - 70, playerPos.Y + 40, (rBool ? Color.Green : Color.Red), "{0}", (rBool ? "Auto harass Enabled" : "Auto harass Disabled"));
		}
	}
}