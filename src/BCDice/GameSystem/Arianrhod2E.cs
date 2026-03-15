using System;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// アリアンロッド2E
    /// </summary>
    public sealed class Arianrhod2E : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Arianrhod2E Instance = new Arianrhod2E();

        private static readonly Regex JudgeRegex = new Regex(
            @"^S?2D6?(?:\+(\d+))?(?:\-(\d+))?(?:([<>=]+)(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "Arianrhod2E";

        /// <inheritdoc/>
        public override string Name => "アリアンロッド2E";

        /// <inheritdoc/>
        public override string SortKey => "ありあんろつと2E";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
【アリアンロッド2E】

・判定コマンド
　2D6>=10    目標値10で判定
　2D6+5>=10  修正値+5で判定
　           12（ゾロ目6）：クリティカル（自動成功）
　           2（ゾロ目1）：ファンブル（自動失敗）

・D66ダイス（昇順ソート）
";

        /// <inheritdoc/>
        protected override Result? EvalCommonCommand(string command, IRandomizer randomizer)
        {
            var result = base.EvalCommonCommand(command, randomizer);

            // 2D6判定でクリティカル/ファンブル判定を追加
            if (result != null && result.Rands != null && result.Rands.Count >= 2)
            {
                if (result.Rands[0].Sides == 6 && result.Rands[1].Sides == 6)
                {
                    int die1 = result.Rands[0].Value;
                    int die2 = result.Rands[1].Value;
                    int total = die1 + die2;

                    if (total == 12)
                    {
                        // クリティカル
                        return Result.CreateBuilder(result.Text + "【クリティカル】")
                            .SetSecret(result.IsSecret)
                            .SetCritical(true)
                            .SetSuccess(true)
                            .SetRands(result.Rands)
                            .SetDetailedRands(result.DetailedRands)
                            .Build();
                    }
                    else if (total == 2)
                    {
                        // ファンブル
                        return Result.CreateBuilder(result.Text + "【ファンブル】")
                            .SetSecret(result.IsSecret)
                            .SetFumble(true)
                            .SetFailure(true)
                            .SetRands(result.Rands)
                            .SetDetailedRands(result.DetailedRands)
                            .Build();
                    }
                }
            }

            return result;
        }
    }
}
