using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// グランクレストRPG
    /// nD6判定でクリティカル処理（6が2個以上で+10）。各種表あり。
    /// </summary>
    public sealed class GranCrest : GameSystemBase
    {
        public static readonly GranCrest Instance = new GranCrest();

        public override string Id => "GranCrest";
        public override string Name => "グランクレストRPG";
        public override string SortKey => "くらんくれすとRPG";
        public override D66SortType D66SortType => D66SortType.NoSort;

        public override string HelpMessage => @"・2D6の目標値判定でクリティカル処理
　例）3d6>=19 3d6+5>=24
・邂逅表（MT）
・感情表（-FT）
　ポジティブ感情表（PFT）、ネガティブ感情表（NFT）
・国特徴表（-CT）
　カテゴリー表（CT）、地形表（TCT）、産業表（ICT）、人物表（PCT）
　組織表（OCT）、拠点表（BCT）、文化表（CCT）
";

        private static readonly Dictionary<string, ITable> Tables = new Dictionary<string, ITable>(StringComparer.OrdinalIgnoreCase)
        {
            ["CT"] = new SimpleTable("国特徴・カテゴリー表", "CT", new[]
            {
                "地形（TCT）\n森林、山岳、河川など、その国に存在する地勢を表す。",
                "産業（ICT）\n農耕が盛ん、鍛冶技術に優れるなど、その国の産業を表す。",
                "人物（PCT）\n腕のよい鍛冶屋がいる、知識豊富な薬師がいるなど、その国にいる人物を表す。",
                "組織（OCT）\n狩人の組合がある、キャラバンがいるなど、その国にある組織を表す。",
                "拠点（BCT）\n砦や関所などの軍用拠点の他、市場や街道など、その国にある拠点を表す。",
                "文化（CCT）\n食のバリエーションが多い、森に造詣が深いなどのその国の文化や風習を表す。",
            }),
        };

        // コマンドエイリアス（大文字化済み）
        private static readonly Dictionary<string, string> CommandAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "POSITIVEFT", "PFT" },
            { "NEGATIVEFT", "NFT" },
            { "CATEGORYCT", "CT" },
        };

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // エイリアス変換
            string resolvedCommand = command;
            if (CommandAliases.TryGetValue(command, out var aliased))
            {
                resolvedCommand = aliased;
            }

            // テーブルコマンドのチェック
            if (Tables.TryGetValue(resolvedCommand, out var table))
            {
                return table.Roll(randomizer);
            }

            return null;
        }

        /// <summary>
        /// nD6判定の拡張（クリティカル処理: 6が2個以上で+10）
        /// </summary>
        protected override Result? EvalCommonCommand(string command, IRandomizer randomizer)
        {
            var result = base.EvalCommonCommand(command, randomizer);

            if (result != null && result.Rands != null && result.Rands.Count >= 2)
            {
                // 全てD6かチェック
                bool allD6 = result.Rands.All(r => r.Sides == 6);
                if (!allD6) return result;

                int sixCount = result.Rands.Count(r => r.Value == 6);

                if (sixCount >= 2)
                {
                    // クリティカル: +10して再計算。+10ボーナスにより成功として扱う
                    string critText = result.Text + " ＞ （クリティカル） +10";

                    return Result.CreateBuilder(critText)
                        .SetSecret(result.IsSecret)
                        .SetCritical(true)
                        .SetSuccess(true)
                        .SetFailure(false)
                        .SetRands(result.Rands)
                        .SetDetailedRands(result.DetailedRands)
                        .Build();
                }
            }

            return result;
        }
    }
}