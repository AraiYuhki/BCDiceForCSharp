using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 千幻抄
    /// </summary>
    public sealed class Sengensyou : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Sengensyou Instance = new Sengensyou();

        private static readonly Regex SgsRegex = new Regex(
            @"^SGS([+-]\d+)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "Sengensyou";

        /// <inheritdoc/>
        public override string Name => "千幻抄";

        /// <inheritdoc/>
        public override string SortKey => "せんけんしよう";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・SGS　命中判定・回避判定
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var match = SgsRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            int modifyNumber = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;

            var diceList = randomizer.RollBarabara(3, 6);
            int diceTotal = diceList.Sum();
            bool isCritical = diceTotal >= 16;
            bool isFumble = diceTotal <= 5;

            string? additionalText;
            if (isCritical)
            {
                additionalText = "クリティカル";
            }
            else if (isFumble)
            {
                additionalText = "ファンブル";
            }
            else
            {
                additionalText = null;
            }

            string modifierStr = modifyNumber > 0 ? $"+{modifyNumber}" : modifyNumber < 0 ? modifyNumber.ToString() : "";
            string? modifyText = modifyNumber != 0 ? $"{diceTotal}{modifierStr}" : null;
            int finalTotal = diceTotal + modifyNumber;

            var parts = new List<string?>
            {
                $"(3D6{modifierStr})",
                $"{diceTotal}[{string.Join(",", diceList)}]",
                modifyText,
                finalTotal.ToString(),
                additionalText,
            };

            var sequence = parts.Where(x => x != null).Select(x => x!);

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .SetRands(randomizer.RandResults)
                .Build();
        }

    }
}
