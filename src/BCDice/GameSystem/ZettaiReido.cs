using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 絶対隷奴
    /// </summary>
    public sealed class ZettaiReido : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly ZettaiReido Instance = new ZettaiReido();

        /// <inheritdoc/>
        public override string Id => "ZettaiReido";

        /// <inheritdoc/>
        public override string Name => "絶対隷奴";

        /// <inheritdoc/>
        public override string SortKey => "せつたいれいと";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定
        m-2DR+n>=x
        m(基本能力),n(修正値),x(目標値)
        DPの取得の有無も表示されます。
        ";

        private static readonly Regex CmdRegex = new Regex(
            @"^(\d+)-2DR([+\-\d]*)(>=(\d+))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = CmdRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var baseAbility = int.Parse(m.Groups[1].Value);
            var modText = m.Groups[2].Value;
            string diffValue = m.Groups[4].Success && !string.IsNullOrEmpty(m.Groups[4].Value) ? m.Groups[4].Value : null;

            return Roll2DR(baseAbility, modText, diffValue, randomizer);
        }

        private Result Roll2DR(int baseAbility, string modText, string diffValue, IRandomizer randomizer)
        {
            var (diceTotal, diceText, darkPoint) = Roll2DarkDice(randomizer);
            var (mod, modDisplayText) = GetModInfo(modText);
            var (diff, diffText) = GetDiffInfo(diffValue);

            var baseCommandText = $"({baseAbility}-2DR{modDisplayText}{diffText})";
            var diceCommandText = $"{baseAbility}-{diceTotal}[{diceText}]{modDisplayText}";
            var total = baseAbility - diceTotal + mod;

            var result = GetResult(diceTotal, total, diff);

            string darkPointText = null;
            if (darkPoint > 0)
            {
                darkPointText = $"{darkPoint}DP";
            }

            var sequence = new List<string> { baseCommandText, diceCommandText, total.ToString() };
            sequence.Add(result.Text);
            if (darkPointText != null)
            {
                sequence.Add(darkPointText);
            }

            var text = string.Join(" ＞ ", sequence);

            return Result.CreateBuilder(text)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .Build();
        }

        private (int diceTotal, string diceText, int darkPoint) Roll2DarkDice(IRandomizer randomizer)
        {
            var dice1 = randomizer.RollOnce(6);
            var dice2 = randomizer.RollOnce(6);

            var (darkDice1, darkPoint1) = ChangeDiceToDarkDice(dice1);
            var (darkDice2, darkPoint2) = ChangeDiceToDarkDice(dice2);

            var darkPoint = darkPoint1 + darkPoint2;
            if (darkPoint == 2)
            {
                darkPoint = 4;
            }

            var darkTotal = darkDice1 + darkDice2;
            var darkDiceText = $"{darkDice1},{darkDice2}";

            return (darkTotal, darkDiceText, darkPoint);
        }

        private (int darkDice, int darkPoint) ChangeDiceToDarkDice(int dice)
        {
            var darkPoint = 0;
            var darkDice = dice;
            if (dice == 6)
            {
                darkDice = 0;
                darkPoint = 1;
            }
            return (darkDice, darkPoint);
        }

        private (int value, string text) GetModInfo(string modText)
        {
            var value = string.IsNullOrEmpty(modText) ? 0 : (ArithmeticEvaluator.Eval(modText, RoundType.Floor) ?? 0);

            var text = "";
            if (value < 0)
            {
                text = value.ToString();
            }
            else if (value > 0)
            {
                text = "+" + value.ToString();
            }

            return (value, text);
        }

        private (int? diff, string diffText) GetDiffInfo(string diffValue)
        {
            var diffText = "";

            if (diffValue == null)
            {
                return (null, diffText);
            }

            var diff = int.Parse(diffValue);
            diffText = $">={diff}";

            return (diff, diffText);
        }

        private Result GetResult(int diceTotal, int total, int? diff)
        {
            if (diceTotal == 0)
            {
                return Result.CreateBuilder("クリティカル").SetSuccess(true).SetCritical(true).Build();
            }

            if (diceTotal == 10)
            {
                return Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
            }

            var diffVal = diff ?? 0;

            var successLevel = total - diffVal;
            if (successLevel >= 0)
            {
                return Result.CreateBuilder($"{successLevel} 成功").SetSuccess(true).Build();
            }

            return Result.CreateBuilder("失敗").SetFailure(true).Build();
        }
    }
}
