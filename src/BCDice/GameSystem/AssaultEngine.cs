using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// アサルトエンジン
    /// </summary>
    public sealed class AssaultEngine : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly AssaultEngine Instance = new AssaultEngine();

        // AES (swap) command: AES followed by target number
        private static readonly Regex AesRegex = new Regex(
            @"^AES(\d+)$",
            RegexOptions.Compiled);

        // AE command without prefix: AE followed by target number
        private static readonly Regex AeRegex = new Regex(
            @"^AE(\d+)$",
            RegexOptions.Compiled);

        // AE reroll command: prefix number + AE + target number
        private static readonly Regex AeRerollRegex = new Regex(
            @"^(\d+)AE(\d+)$",
            RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "AssaultEngine";

        /// <inheritdoc/>
        public override string Name => "アサルトエンジン";

        /// <inheritdoc/>
        public override string SortKey => "あさるとえんしん";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定 AEt (t目標値)
            例: AE45 （目標値45）
        ・リロール nAEt (nロール前の値、t目標値)
            例: 76AE45 (目標値45で、76を振り直す)

        ・スワップ（t目標値） エネミーブックP11
            例: AES45 （目標値45、スワップ表示あり）
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            Match m;

            // スワップ判定
            m = AesRegex.Match(command);
            if (m.Success)
            {
                int target = Convert.ToInt32(m.Groups[1].Value);
                if (target >= 100)
                {
                    target = 99;
                }
                int total = randomizer.RollOnce(100) % 100;
                int swap = total % 10 * 10 + total / 10;
                Result r1 = Judge(target, total, randomizer);
                Result r2 = Judge(target, swap, randomizer);
                string text = $"(AES{Format00(target)}) ＞ {r1.Text} / スワップ{r2.Text}";
                return ReturnResult(r1, r2, text, randomizer);
            }

            // 初回ロール
            m = AeRegex.Match(command);
            if (m.Success)
            {
                int target = Convert.ToInt32(m.Groups[1].Value);
                if (target >= 100)
                {
                    target = 99;
                }
                int total = randomizer.RollOnce(100) % 100;
                Result r = Judge(target, total, randomizer);
                return Result.CreateBuilder($"(AE{Format00(target)}) ＞ {r.Text}")
                    .SetSuccess(r.IsSuccess)
                    .SetFailure(r.IsFailure)
                    .SetCritical(r.IsCritical)
                    .SetFumble(r.IsFumble)
                    .SetRands(randomizer.RandResults)
                    .Build();
            }

            // リロール
            m = AeRerollRegex.Match(command);
            if (m.Success)
            {
                int now = Convert.ToInt32(m.Groups[1].Value);
                int target = Convert.ToInt32(m.Groups[2].Value);
                if (target >= 100)
                {
                    target = 99;
                }
                int die = randomizer.RollOnce(10) % 10;
                Result new1 = Judge(target, now / 10 * 10 + die, randomizer);
                Result new2 = Judge(target, now % 10 + die * 10, randomizer);
                string text = $"({Format00(now)}AE{Format00(target)}) ＞ {die} ＞ {new1.Text} / {new2.Text}";
                return ReturnResult(new1, new2, text, randomizer);
            }

            return null;
        }

        private string Format00(int dice)
        {
            return dice.ToString("D2");
        }

        private Result ReturnResult(Result result1, Result result2, string text, IRandomizer randomizer)
        {
            if (result1.IsCritical || result2.IsCritical)
            {
                return Result.CreateBuilder(text)
                    .SetCritical(true)
                    .SetSuccess(true)
                    .SetRands(randomizer.RandResults)
                    .Build();
            }
            else if (result1.IsSuccess || result2.IsSuccess)
            {
                return Result.CreateBuilder(text)
                    .SetSuccess(true)
                    .SetRands(randomizer.RandResults)
                    .Build();
            }
            else if (result1.IsFumble && result2.IsFumble)
            {
                return Result.CreateBuilder(text)
                    .SetFumble(true)
                    .SetFailure(true)
                    .SetRands(randomizer.RandResults)
                    .Build();
            }
            else
            {
                return Result.CreateBuilder(text)
                    .SetFailure(true)
                    .SetRands(randomizer.RandResults)
                    .Build();
            }
        }

        private Result Judge(int target, int total, IRandomizer randomizer)
        {
            bool isDouble = total / 10 == total % 10;
            string total_text = Format00(total);
            if (total <= target)
            {
                if (isDouble)
                {
                    return Result.CreateBuilder($"({total_text}).Build()クリティカル")
                        .SetCritical(true)
                        .SetSuccess(true)
                        .SetRands(randomizer.RandResults)
                        .Build();
                }
                else
                {
                    return Result.CreateBuilder($"({total_text}).Build()成功")
                        .SetSuccess(true)
                        .SetRands(randomizer.RandResults)
                        .Build();
                }
            }
            else
            {
                if (isDouble)
                {
                    return Result.CreateBuilder($"({total_text}).Build()ファンブル")
                        .SetFumble(true)
                        .SetFailure(true)
                        .SetRands(randomizer.RandResults)
                        .Build();
                }
                else
                {
                    return Result.CreateBuilder($"({total_text}).Build()失敗")
                        .SetFailure(true)
                        .SetRands(randomizer.RandResults)
                        .Build();
                }
            }
        }

    }
}
