using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class Pathfinder : GameSystemBase
    {
        public static readonly Pathfinder Instance = new Pathfinder();

        public override string Id => "Pathfinder";
        public override string Name => "Pathfinder";
        public override string SortKey => "はすふあいんたあ";
        public override string HelpMessage => "※このダイスボットは部屋のシステム名表示用となります。\n";
    }
}