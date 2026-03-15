using System;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// エネカデット
    /// </summary>
    public sealed class Ainecadette : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Ainecadette Instance = new Ainecadette();

        /// <inheritdoc/>
        public override string Id => "Ainecadette";

        /// <inheritdoc/>
        public override string Name => "エネカデット";

        /// <inheritdoc/>
        public override string SortKey => "えねかてつと";

        /// <inheritdoc/>
        public override string HelpMessage => @"
■ 判定
- 先輩 (AI) 10面ダイスを2つ振って判定します。『有利』なら【3AI】、『不利』なら【1AI】を使います。
- 後輩 (CA) 6面ダイスを2つ振って判定します。『有利』なら【3CA】、『不利』なら【1CA】を使います。
";

        private static readonly Regex RollCommand = new Regex(
            @"^(\d+)?(AI|CA)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private const int SuccessThreshold = 4;
        private const int SpecialDice = 6;

        private Ainecadette() { }

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var match = RollCommand.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSenpai = match.Groups[2].Value.Equals("AI", StringComparison.OrdinalIgnoreCase);
            int times = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 2;
            int sides = isSenpai ? 10 : 6;

            if (times <= 0)
            {
                return null;
            }

            var diceList = randomizer.RollBarabara(times, sides);
            int max = diceList.Max();

            string resultText;
            bool isSuccess = false;
            bool isFailure = false;
            bool isCritical = false;
            bool isFumble = false;

            if (max <= 1)
            {
                resultText = "ファンブル（もやもやカウンターを2個獲得）";
                isFumble = true;
                isFailure = true;
            }
            else if (diceList.Contains(SpecialDice))
            {
                string me = isSenpai ? "先輩" : "後輩";
                string target = isSenpai ? "後輩" : "先輩";
                resultText = $"スペシャル（絆カウンターを1個獲得し、{target}は{me}への感情を1つ獲得）";
                isCritical = true;
                isSuccess = true;
            }
            else if (max >= SuccessThreshold)
            {
                resultText = "成功";
                isSuccess = true;
            }
            else
            {
                resultText = "失敗";
                isFailure = true;
            }

            string diceListText = string.Join(",", diceList);
            string text = $"({command.ToUpper()}) ＞ [{diceListText}] ＞ {resultText}";

            return Result.CreateBuilder(text)
                .SetSuccess(isSuccess)
                .SetFailure(isFailure)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }
    }
}
