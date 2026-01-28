using System;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// りゅうたま
    /// </summary>
    public sealed class Ryutama : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Ryutama Instance = new Ryutama();

        /// <inheritdoc/>
        public override string Id => "Ryutama";

        /// <inheritdoc/>
        public override string Name => "りゅうたま";

        /// <inheritdoc/>
        public override string SortKey => "りゆうたま";

        /// <inheritdoc/>
        public override string HelpMessage => @"
・判定
  Rx,y>=t（x,y：使用する能力値、t：目標値）
  1ゾロ、クリティカルも含めて判定結果を表示します
  能力値１つでの判定は Rx>=t で行えます
例）R8,6>=13
";

        private static readonly int[] ValidDiceTypes = { 20, 12, 10, 8, 6, 4, 2 };

        private static readonly Regex RollCommand = new Regex(
            @"^R(\d+)(,(\d+))?([+\-]\d+)?(>=(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private Ryutama() { }

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var match = RollCommand.Match(command);
            if (!match.Success)
            {
                return null;
            }

            int dice1Input = int.Parse(match.Groups[1].Value);
            int dice2Input = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
            int modifier = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 0;
            int? difficulty = match.Groups[6].Success ? int.Parse(match.Groups[6].Value) : (int?)null;

            var (dice1, dice2) = GetDiceType(dice1Input, dice2Input);
            if (dice1 == 0)
            {
                return null;
            }

            int value1 = dice1 > 0 ? randomizer.RollOnce(dice1) : 0;
            int value2 = dice2 > 0 ? randomizer.RollOnce(dice2) : 0;
            int total = value1 + value2 + modifier;

            // 結果判定
            bool isFumble = IsFumble(value1, value2);
            bool isCritical = IsCritical(value1, value2, dice1, dice2);
            string resultText = GetResultText(value1, value2, dice1, dice2, difficulty, total);

            // 出力構築
            string value1Text = $"{value1}({dice1})";
            string value2Text = dice2 > 0 ? $"+{value2}({dice2})" : "";
            string modifyText = GetModifyString(modifier);
            string baseText = GetBaseText(dice1, dice2, modifier, difficulty);

            string result = string.IsNullOrEmpty(resultText) ? "" : $" ＞ {resultText}";
            string text = $"({baseText}) ＞ {value1Text}{value2Text}{modifyText} ＞ {total}{result}";

            bool isSuccess = difficulty.HasValue && total >= difficulty.Value && !isFumble;
            bool isFailure = (difficulty.HasValue && total < difficulty.Value) || isFumble;

            return Result.CreateBuilder(text)
                .SetSuccess(isSuccess || isCritical)
                .SetFailure(isFailure)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static (int, int) GetDiceType(int dice1, int dice2)
        {
            if (dice2 != 0)
            {
                if (IsValidDice(dice1) && IsValidDice(dice2))
                {
                    return (dice1, dice2);
                }
                return (0, 0);
            }

            // 単一の有効なダイスタイプ
            if (IsValidDice(dice1))
            {
                return (dice1, 0);
            }

            // 2桁の組み合わせを試す (例: 86 -> 8, 6)
            int d1 = dice1 / 10;
            int d2 = dice1 % 10;
            if (IsValidDice(d1) && IsValidDice(d2))
            {
                return (d1, d2);
            }

            // 3桁の組み合わせを試す (例: 128 -> 12, 8)
            d1 = dice1 / 100;
            d2 = dice1 % 100;
            if (IsValidDice(d1) && IsValidDice(d2))
            {
                return (d1, d2);
            }

            return (0, 0);
        }

        private static bool IsValidDice(int dice)
        {
            return Array.IndexOf(ValidDiceTypes, dice) >= 0;
        }

        private static bool IsFumble(int value1, int value2)
        {
            return value1 == 1 && value2 == 1;
        }

        private static bool IsCritical(int value1, int value2, int dice1, int dice2)
        {
            if (dice2 == 0) return false;

            // 両方6
            if (value1 == 6 && value2 == 6)
            {
                return true;
            }

            // 両方最大値
            if (value1 == dice1 && value2 == dice2)
            {
                return true;
            }

            return false;
        }

        private static string GetResultText(int value1, int value2, int dice1, int dice2, int? difficulty, int total)
        {
            if (IsFumble(value1, value2))
            {
                return "１ゾロ【１ゾロポイント＋１】";
            }

            if (IsCritical(value1, value2, dice1, dice2))
            {
                return "クリティカル成功";
            }

            if (!difficulty.HasValue)
            {
                return "";
            }

            if (total >= difficulty.Value)
            {
                return "成功";
            }

            return "失敗";
        }

        private static string GetBaseText(int dice1, int dice2, int modifier, int? difficulty)
        {
            string baseText = $"R{dice1}";

            if (dice2 != 0)
            {
                baseText += $",{dice2}";
            }

            baseText += GetModifyString(modifier);

            if (difficulty.HasValue)
            {
                baseText += $">={difficulty.Value}";
            }

            return baseText;
        }

        private static string GetModifyString(int modifier)
        {
            if (modifier > 0)
            {
                return $"+{modifier}";
            }
            else if (modifier < 0)
            {
                return modifier.ToString();
            }
            return "";
        }
    }
}
