using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;


namespace ElJayce
{
    internal enum Spells
    {
        Q,
        W,
        E,
        R
    }

    internal static class Jayce
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }
    }
}
