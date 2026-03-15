using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ルーンクエスト
    /// </summary>
    public sealed class RuneQuest : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly RuneQuest Instance = new RuneQuest();

        /// <inheritdoc/>
        public override string Id => "RuneQuest";

        /// <inheritdoc/>
        public override string Name => "ルーンクエスト";

        /// <inheritdoc/>
        public override string SortKey => "るうんくえすと";

        /// <inheritdoc/>
        public override string HelpMessage => @"
クリティカル、エフェクティブ(効果的成功)、ファンブルの自動判定を行います。
";

        /// <summary>
        /// 1d100の判定結果を返す
        /// </summary>
        private Result? Result1d100(int total, int _diceTotal, string cmpOp, int target)
        {
            if (target.ToString() == "?")
            {
                return Result.CreateBuilder("").Build();
            }

            if (cmpOp != "<=")
            {
                return null;
            }

            // RuneQuest QUICK-START RULESを元に修正
            var criticalValue = (int)Math.Round((double)target / 20);

            if (total <= 1 || total <= criticalValue)
            {
                // 1は常に決定的成功
                return Result.CreateBuilder("決定的成功").SetSuccess(true).SetCritical(true).Build();
            }
            else if (total >= 100)
            {
                // 100は常に致命的失敗
                return Result.CreateBuilder("致命的失敗").SetFumble(true).SetFailure(true).Build();
            }
            else if (total <= (int)Math.Round((double)target / 5))
            {
                return Result.CreateBuilder("効果的成功").SetSuccess(true).Build();
            }
            else if (total <= target)
            {
                return Result.CreateBuilder("成功").SetSuccess(true).Build();
            }
            else if (total >= 95 + criticalValue)
            {
                return Result.CreateBuilder("致命的失敗").SetFumble(true).SetFailure(true).Build();
            }
            else
            {
                return Result.CreateBuilder("失敗").SetFailure(true).Build();
            }
        }
    }
}
