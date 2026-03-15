using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// フルメタル・パニック！RPG
    /// </summary>
    public sealed class FullMetalPanic : GameSystemBase
    {
        public static readonly FullMetalPanic Instance = new FullMetalPanic();

        public override string Id => "FullMetalPanic";
        public override string Name => "フルメタル・パニック！RPG";
        public override string SortKey => "ふるめたるはにつくRPG";

        public override string HelpMessage => "・判定\n" +
                "　・通常判定：2D6+m@c#f>=t または 2D6+m>=t[c,f]\n" +
                "　　修正値m、目標値t、クリティカル値c、ファンブル値fで判定ロールを行います。\n" +
                "　　修正値、クリティカル値、ファンブル値は省略可能です（[]ごと省略可、@c・#fの指定は順不同）。\n" +
                "　　クリティカル値、ファンブル値の既定値は、それぞれ12、2です。\n" +
                "　　自動成功、自動失敗、成功、失敗を自動表示します。\n" +
                "　　例) MG+2>=10　　　　 2d6+2>=10と同じ（MGが2D6のショートカットコマンド）\n" +
                "　　例) FP+2>=10　　　　 2d6+2>=10と同じ（FPが2D6のショートカットコマンド）\n" +
                "・D66ダイスあり（入れ替えなし)";

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // TODO: Implement game-specific commands
            return null;
        }
    }
}