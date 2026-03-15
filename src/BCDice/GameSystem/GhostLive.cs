using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 実況ゴーストライヴ
    /// </summary>
    public sealed class GhostLive : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly GhostLive Instance = new GhostLive();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "GhostLive";

        /// <inheritdoc/>
        public override string Name => "実況ゴーストライヴ";

        /// <inheritdoc/>
        public override string SortKey => "しつきようこおすとらいふ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■追加目標表（p11）
        ATT, AdditionalTargetTable

        ■種別：地縛霊（p26）
        □Ａ.霊障リスト
        JHA, JibakuHauntA
        □Ｂ.霊障効果リスト
        JHB, JibakuHauntB

        ■種別：シャイな幽霊（p27）
        □Ａ.霊障リスト
        SHA, ShyHauntA
        □Ｂ.霊障効果リスト
        SHB, ShyHauntB

        ■種別：ぐちゃぐちゃ（p28）
        □Ａ.霊障リスト
        GHA, GuchaHauntA
        □Ｂ.霊障効果リスト
        GHB, GuchaHauntB
        ";


        private static readonly Dictionary<string, string> ALIAS = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "ATT", "ADDITIONALTARGETTABLE" },
            { "JHA", "JIBAKUHAUNTA" },
            { "JHB", "JIBAKUHAUNTB" },
            { "SHA", "SHYHAUNTA" },
            { "SHB", "SHYHAUNTB" },
            { "GHA", "GUCHAHAUNTA" },
            { "GHB", "GUCHAHAUNTB" },
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var resolvedCommand = ALIAS.TryGetValue(command, out var aliasValue) ? aliasValue : command;
            return RollTables(resolvedCommand, TABLES);
        }

    }
}