using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ジキルとハイドとグリトグラ
    /// </summary>
    public sealed class JekyllAndHyde : GameSystemBase
    {
        public static readonly JekyllAndHyde Instance = new JekyllAndHyde();

        public override string Id => "JekyllAndHyde";
        public override string Name => "ジキルとハイドとグリトグラ";
        public override string SortKey => "しきるとはいととくりとくら";

        public override string HelpMessage => "・難易度算出コマンド　DDC\n" +
                "        ・判定コマンド　RCx　or　RCx+y　or　RCx-y（x＝難易度、y=修正値（省略可能））\n" +
                "        ・目標決定表　GOALT";

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // TODO: Implement game-specific commands
            return null;
        }
    }
}