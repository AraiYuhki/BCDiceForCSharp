using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class ShadowRun5 : GameSystemBase
    {
        public static readonly ShadowRun5 Instance = new ShadowRun5();

        public override string Id => "ShadowRun5";
        public override string Name => "シャドウラン 5th Edition";
        public override string SortKey => "しやとうらん5";
        public override string HelpMessage => @"個数振り足しロール(xRn)の境界値を6にセット、バラバラロール(xBn)の目標値を5以上にセットします。
バラバラロール(xBn)のみ、リミットをセットできます。リミットの指定は(xBn@l)のように指定します。(省略可)
BコマンドとRコマンド時に、グリッチの表示を行います。
";

        public override bool SortBarabaraDice => true;
        public override int? RerollDiceRerollThreshold => 6;
        public override CompareOperator? DefaultCmpOp => CompareOperator.GreaterThanOrEqual;
        public override int? DefaultTargetNumber => 5;
    }
}