using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class WorldsEndFrontline : GameSystemBase
    {
        public static readonly WorldsEndFrontline Instance = new WorldsEndFrontline();

        public override string Id => "WorldsEndFrontline";
        public override string Name => "ワールドエンドフロントライン";
        public override string SortKey => "わあるとえんとふろんとらいん";
        public override string HelpMessage => @"・ダイスチェック xDC+y
　【ダイスチェック】を行う。《トライアンフ》を結果に自動反映する。
　x: ダイス数
　y: 結果への修正値 （省略可）
";
    }
}