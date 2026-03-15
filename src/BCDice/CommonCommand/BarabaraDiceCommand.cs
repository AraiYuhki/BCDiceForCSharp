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
    /// バラバラダイスコマンド
    /// 例: 5B6>=4 - 5個のD6を振り、各ダイスの出目を表示し、成功数をカウント
    /// 例: 3B6+2B10>=5 - 複数のダイスグループを振る
    /// </summary>
    public class BarabaraDiceCommand : ICommonCommand
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly BarabaraDiceCommand Instance = new BarabaraDiceCommand();

        private static readonly Regex CommandRegex = new Regex(
            @"^S?(\d+B\d+(?:\+\d+B\d+)*)(?:([<>=!]+)(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex DiceNotationRegex = new Regex(
            @"(\d+)B(\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public string PrefixPattern => @"\d+B\d+";

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
            string? opStr = match.Groups[2].Success ? match.Groups[2].Value : null;
            int? target = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : null;

            // 比較演算子を解析
            CompareOperator? compareOp = null;
            if (!string.IsNullOrEmpty(opStr))
            {
                compareOp = Normalize.ComparisonOperator(opStr);
                if (!compareOp.HasValue)
                {
                    return null;
                }
            }

            // ゲームシステムのデフォルト設定を使用
            if (!compareOp.HasValue && gameSystem.DefaultCmpOp.HasValue)
            {
                compareOp = gameSystem.DefaultCmpOp;
                target = gameSystem.DefaultTargetNumber;
            }

            // ダイスを振る
            var diceGroups = new List<int[]>();
            var diceMatches = DiceNotationRegex.Matches(diceNotation);
            var notations = new List<string>();

            foreach (Match dm in diceMatches)
            {
                int count = int.Parse(dm.Groups[1].Value);
                int sides = int.Parse(dm.Groups[2].Value);

                if (count <= 0 || sides <= 0)
                {
                    return null;
                }

                var rolls = randomizer.RollBarabara(count, sides);

                // ソート設定に応じてソート
                if (gameSystem.SortBarabaraDice)
                {
                    Array.Sort(rolls);
                }

                diceGroups.Add(rolls);
                notations.Add($"{count}B{sides}");
            }

            // 全ての出目をフラット化
            var allDice = diceGroups.SelectMany(g => g).ToList();

            // 成功数をカウント
            int successCount = 0;
            string? successText = null;
            if (compareOp.HasValue && target.HasValue)
            {
                successCount = allDice.Count(d => Compare(d, compareOp.Value, target.Value));
                successText = $"成功数{successCount}";
            }

            // 1の出目の数（グリッチ判定用）
            int countOf1 = allDice.Count(d => d == 1);
            string? grichText = gameSystem.GetGrichText(countOf1, allDice.Count, successCount);

            // 結果テキストを構築
            var sb = new StringBuilder();
            sb.Append($"({string.Join("+", notations)}");
            if (compareOp.HasValue && target.HasValue)
            {
                sb.Append($"{CompareOpToString(compareOp.Value)}{target.Value}");
            }
            sb.Append(")");

            sb.Append($" ＞ {string.Join(",", allDice)}");

            if (!string.IsNullOrEmpty(successText))
            {
                sb.Append($" ＞ {successText}");
            }

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
