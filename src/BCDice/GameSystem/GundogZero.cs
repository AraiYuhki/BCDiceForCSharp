using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ガンドッグゼロ
    /// </summary>
    public sealed class GundogZero : GameSystemBase
    {
        public static readonly GundogZero Instance = new GundogZero();

        public override string Id => "GundogZero";
        public override string Name => "ガンドッグゼロ";
        public override string SortKey => "かんとつくせろ";

        public override string HelpMessage => "失敗、成功、クリティカル、ファンブルとロールの達成値の自動判定を行います。\n" +
                "        nD9ロールも対応。\n" +
                "        ・ダメージペナルティ表　　(〜DPTx) (x:修正)\n" +
                "        　射撃(SDPT)、格闘(MDPT)、車両(VDPT)、汎用(GDPT)の各表を引くことが出来ます。\n" +
                "        　修正を後ろに書くことも出来ます。\n" +
                "        ・ファンブル表　　　　　　(〜FTx)  (x:修正)\n" +
                "        　射撃(SFT)、格闘(MFT)、投擲(TFT)の各表を引くことが出来ます。\n" +
                "        　修正を後ろに書くことも出来ます。";

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // TODO: Implement game-specific commands
            return null;
        }
    }
}