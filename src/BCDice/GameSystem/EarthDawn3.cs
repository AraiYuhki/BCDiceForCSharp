using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// アースドーン3版
    /// </summary>
    public sealed class EarthDawn3 : GameSystemBase
    {
        public static readonly EarthDawn3 Instance = new EarthDawn3();

        public override string Id => "EarthDawn3";
        public override string Name => "アースドーン3版";
        public override string SortKey => "ああすとおん3";

        public override string HelpMessage => "ステップダイス　(xEn+k)\n" +
                "        ステップx、目標値n(省略可能）、カルマダイスk(D2～D20)でステップダイスをロールします。\n" +
                "        振り足しも自動。\n" +
                "        例）ステップ10：10E\n" +
                "        　　ステップ10、目標値8：10E8\n" +
                "        　　ステップ12、目標値8、カルマダイスD12：10E8+1D6";

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // TODO: Implement game-specific commands
            return null;
        }
    }
}