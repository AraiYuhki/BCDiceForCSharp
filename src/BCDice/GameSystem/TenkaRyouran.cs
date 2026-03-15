using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class TenkaRyouran : GameSystemBase
    {
        public static readonly TenkaRyouran Instance = new TenkaRyouran();

        public override string Id => "TenkaRyouran";
        public override string Name => "天下繚乱";
        public override string SortKey => "てんかりようらん";
        public override string HelpMessage => @"・判定
　・通常判定：2D6+m@c#f>=t または 2D6+m>=t[c,f]
　　修正値m、目標値t、クリティカル値c、ファンブル値fで判定ロールを行います。
　　修正値、クリティカル値、ファンブル値は省略可能です（[]ごと省略可、@c・#fの指定は順不同）。
　　クリティカル値、ファンブル値の既定値は、それぞれ12、2です。
　　自動成功、自動失敗、成功、失敗を自動表示します。
　　例) TR+2>=10　　　　 2d6+2>=10と同じ（TRが2D6のショートカットコマンド）
・D66ダイスあり（入れ替えなし)
";
    }
}