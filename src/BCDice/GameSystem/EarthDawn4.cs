using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// アースドーン4版
    /// </summary>
    public sealed class EarthDawn4 : GameSystemBase
    {
        public static readonly EarthDawn4 Instance = new EarthDawn4();

        public override string Id => "EarthDawn4";
        public override string Name => "アースドーン4版";
        public override string SortKey => "ああすとおん4";

        public override string HelpMessage => "ステップダイス　(xEnK)\n" +
                "        ステップx、目標値n(省略可能）でステップダイスをロール。\n" +
                "        カルマダイス使用時は末尾にKを追加（省略可能）\n" +
                "        例）ステップ10：10E\n" +
                "        　　ステップ10、目標値8：10E8\n" +
                "        　　ステップ10、目標値8、カルマダイス：10E8K";

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // TODO: Implement game-specific commands
            return null;
        }
    }
}