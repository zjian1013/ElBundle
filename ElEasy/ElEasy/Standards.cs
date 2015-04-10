using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace ElEasy
{
    public enum Spells
    {
        Q,
        W,
        E,
        R
    } 

    public class Standards
    {
        protected static Menu _menu;
        protected static Orbwalking.Orbwalker Orbwalker;
        protected static SpellSlot _ignite;
        protected static int lastNotification = 0;
        protected static Obj_AI_Hero Player { get { return ObjectManager.Player; } }


        #region Notifications 

        protected static void ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            Notifications.AddNotification(new Notification(message, duration, dispose).SetTextColor(color));
        }

        #endregion


    }
}
