using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Command;
using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.CommonCommand
{
    /// <summary>
    /// 振り足しダイスコマンド
    /// 例: 2R6>=5 - 2個のD6を振り、5以上の出目が出たら振り足し
    /// 例: 2R6[>3]>=5 - 振り足し条件を明示的に指定
    /// 例: 2R6>=5@>3 - @で振り足し条件を指定
    /// </summary>
    public class RerollDiceCommand : ICommonCommand
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly RerollDiceCommand Instance = new RerollDiceCommand();

        /// <summary>
        /// 振り足しの上限回数
        /// </summary>
        private const int RerollLimit = 10000;

        // 基本パターン: 2R6>=5 または 2R6[>3]>=5 または 2R6>=5@>3
        private static readonly Regex CommandRegex = new Regex(
            @"^S?(\d+R\d+(?:\+\d+R\d+)*)(?:\[([<>=!]*)(\d+)\])?(?:([<>=!]+)(\d+))?(?:@([<>=!]*)(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex DiceNotationRegex = new Regex(
            @"(\d+)R(\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public string PrefixPattern => @"\d+R\d+";

        /// <inheritdoc/>
        public Result? Eval(string command, IGameSystemContext gameSystem, IRandomizer randomizer)
        {
            var match = CommandRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase) &&
                           !char.IsDigit(command[0]);

            string diceNotation = match.Groups[1].Value;

            // 振り足し条件（角カッコ内）
            string? bracketRerollOpStr = match.Groups[2].Success ? match.Groups[2].Value : null;
            int? bracketRerollThreshold = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : null;

            // 成功条件
            string? successOpStr = match.Groups[4].Success ? match.Groups[4].Value : null;
            int? successTarget = match.Groups[5].Success ? int.Parse(match.Groups[5].Value) : null;

            // 振り足し条件（@以降）
            string? atRerollOpStr = match.Groups[6].Success ? match.Groups[6].Value : null;
            int? atRerollThreshold = match.Groups[7].Success ? int.Parse(match.Groups[7].Value) : null;

            // 成功条件の解析
            CompareOperator? successCmpOp = null;
            if (!string.IsNullOrEmpty(successOpStr))
            {
                successCmpOp = Normalize.ComparisonOperator(successOpStr);
            }

            // ゲームシステムのデフォルト設定を使用
            if (!successCmpOp.HasValue && gameSystem.DefaultCmpOp.HasValue)
            {
                successCmpOp = gameSystem.DefaultCmpOp;
                successTarget = gameSystem.DefaultTargetNumber;
            }

            // 振り足し条件の決定
            CompareOperator rerollCmpOp;
            int? rerollThreshold;

            if (atRerollThreshold.HasValue)
            {
                // @で指定された条件を優先
                rerollCmpOp = string.IsNullOrEmpty(atRerollOpStr)
                    ? CompareOperator.GreaterThanOrEqual
                    : Normalize.ComparisonOperator(atRerollOpStr) ?? CompareOperator.GreaterThanOrEqual;
                rerollThreshold = atRerollThreshold;
            }
            else if (bracketRerollThreshold.HasValue)
            {
                // 角カッコで指定された条件
                rerollCmpOp = string.IsNullOrEmpty(bracketRerollOpStr)
                    ? (successCmpOp ?? CompareOperator.GreaterThanOrEqual)
                    : Normalize.ComparisonOperator(bracketRerollOpStr) ?? CompareOperator.GreaterThanOrEqual;
                rerollThreshold = bracketRerollThreshold;
            }
            else if (gameSystem.RerollDiceRerollThreshold.HasValue)
            {
                // ゲームシステムのデフォルト
                rerollCmpOp = successCmpOp ?? CompareOperator.GreaterThanOrEqual;
                rerollThreshold = gameSystem.RerollDiceRerollThreshold;
            }
            else
            {
                // 成功条件を流用
                rerollCmpOp = successCmpOp ?? CompareOperator.GreaterThanOrEqual;
                rerollThreshold = successTarget;
            }

            // ダイスの解析
            var diceMatches = DiceNotationRegex.Matches(diceNotation);
            var initialDice = new List<(int times, int sides)>();

            foreach (Match dm in diceMatches)
            {
                int times = int.Parse(dm.Groups[1].Value);
                int sides = int.Parse(dm.Groups[2].Value);

                if (times <= 0 || sides <= 0)
                {
                    return null;
                }

                initialDice.Add((times, sides));
            }

            // 振り足し条件のバリデーション
            if (!rerollThreshold.HasValue)
            {
                return Result.CreateBuilder($"{command} ＞ 条件が間違っています。2R6>=5 あるいは 2R6[5] のように振り足し目標値を指定してください。")
                    .SetSecret(isSecret)
                    .Build();
            }

            foreach (var (_, sides) in initialDice)
            {
                if (!IsValidRerollCondition(rerollCmpOp, rerollThreshold.Value, sides))
                {
                    return Result.CreateBuilder($"{command} ＞ 条件が間違っています。2R6>=5 あるいは 2R6[5] のように振り足し目標値を指定してください。")
                        .SetSecret(isSecret)
                        .Build();
                }
            }

            // ダイスロール実行
            var diceListList = new List<int[]>();
            var diceQueue = new Queue<(int times, int sides)>();

            foreach (var dice in initialDice)
            {
                diceQueue.Enqueue(dice);
            }

            int loopCount = 0;
            while (diceQueue.Count > 0 && loopCount < RerollLimit)
            {
                var (times, sides) = diceQueue.Dequeue();
                loopCount++;

                var rolls = randomizer.RollBarabara(times, sides);

                if (gameSystem.SortBarabaraDice)
                {
                    Array.Sort(rolls);
                }

                diceListList.Add(rolls);

                // 振り足しの個数をカウント
                int rerollCount = rolls.Count(v => Compare(v, rerollCmpOp, rerollThreshold.Value));
                if (rerollCount > 0)
                {
                    diceQueue.Enqueue((rerollCount, sides));
                }
            }

            // 結果の集計
            var allDice = diceListList.SelectMany(l => l).ToList();

            // 振り足し分は出目1の個数をカウントしない
            int oneCount = diceListList
                .Take(initialDice.Count)
                .SelectMany(l => l)
                .Count(d => d == 1);

            int successCount = 0;
            if (successCmpOp.HasValue && successTarget.HasValue)
            {
                successCount = allDice.Count(v => Compare(v, successCmpOp.Value, successTarget.Value));
            }

            // グリッチテキスト
            string? grichText = gameSystem.GetGrichText(oneCount, allDice.Count, successCount);

            // 結果テキストの構築
            var sb = new StringBuilder();

            // 式の表示
            string notationStr = string.Join("+", initialDice.Select(d => $"{d.times}R{d.sides}"));
            string rerollOpStr = rerollCmpOp == successCmpOp ? "" : CompareOpToString(rerollCmpOp);
            sb.Append($"({notationStr}[{rerollOpStr}{rerollThreshold}]");
            if (successCmpOp.HasValue && successTarget.HasValue)
            {
                sb.Append($"{CompareOpToString(successCmpOp.Value)}{successTarget.Value}");
            }
            sb.Append(")");

            // ダイス結果
            sb.Append($" ＞ {string.Join(" + ", diceListList.Select(l => string.Join(",", l)))}");

            // 成功数
            sb.Append($" ＞ 成功数{successCount}");

            // グリッチ
            if (!string.IsNullOrEmpty(grichText))
            {
                sb.Append($" ＞ {grichText}");
            }

            return Result.CreateBuilder(sb.ToString())
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static bool IsValidRerollCondition(CompareOperator cmpOp, int threshold, int sides)
        {
            return cmpOp switch
            {
                CompareOperator.LessThanOrEqual => threshold < sides,
                CompareOperator.LessThan => threshold <= sides,
                CompareOperator.GreaterThanOrEqual => threshold > 1,
                CompareOperator.GreaterThan => threshold >= 1,
                CompareOperator.NotEqual => threshold >= 1 && threshold <= sides,
                CompareOperator.Equal => true,
                _ => true
            };
        }

        private static string CompareOpToString(CompareOperator op)
        {
            return op switch
            {
                CompareOperator.Equal => "=",
                CompareOperator.NotEqual => "<>",
                CompareOperator.GreaterThan => ">",
                CompareOperator.GreaterThanOrEqual => ">=",
                CompareOperator.LessThan => "<",
                CompareOperator.LessThanOrEqual => "<=",
                _ => "?"
            };
        }

        private static bool Compare(int left, CompareOperator op, int right)
        {
            return op switch
            {
                CompareOperator.Equal => left == right,
                CompareOperator.NotEqual => left != right,
                CompareOperator.GreaterThan => left > right,
                CompareOperator.GreaterThanOrEqual => left >= right,
                CompareOperator.LessThan => left < right,
                CompareOperator.LessThanOrEqual => left <= right,
                _ => false
            };
        }
    }
}
