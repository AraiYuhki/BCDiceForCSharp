using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ヴィジョンコネクト
    /// </summary>
    public sealed class VisionConnect : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly VisionConnect Instance = new VisionConnect();

        /// <inheritdoc/>
        public override string Id => "VisionConnect";

        /// <inheritdoc/>
        public override string Name => "ヴィジョンコネクト";

        /// <inheritdoc/>
        public override string SortKey => "ういしよんこねくと";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定(VC+x@c#f>=y)
          !：コマンドの最初に付けると致命的失敗が全てアクシデントになる。
          x：修正値。能力値、戦闘値、その他修正値など。省略可。
          y：目標値。省略時は決定的成功/致命的失敗のみ表示。
          c：クリティカル値。@ごと省略可。省略時は12。
          f：ファンブル値。#ごと省略可。省略時は3。
          (例)VC+3>=8
              VC+7@11>=12
              !VC+6-1#4>=10

        ・各種表
          アクシデント表 AT
          トラブル表 TT
        ";

        private static readonly Regex VcRegex = new Regex(
            @"^(!?VC)([+\-]\d+(?:[+\-]\d+)*)?(?:@(\d+))?(?:#(\d+))?(?:>=(\d+))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return CheckAction(command, randomizer);
        }

        private Result? CheckAction(string command, IRandomizer randomizer)
        {
            var m = VcRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var vcCmd = m.Groups[1].Value;
            var staminaZero = vcCmd.StartsWith("!");
            var modifyStr = m.Groups[2].Value;
            int modifyNumber = string.IsNullOrEmpty(modifyStr) ? 0 : (ArithmeticEvaluator.Eval(modifyStr, RoundType.Floor) ?? 0);
            var criticalTarget = m.Groups[3].Success && !string.IsNullOrEmpty(m.Groups[3].Value) ? int.Parse(m.Groups[3].Value) : 12;
            var fumbleTarget = m.Groups[4].Success && !string.IsNullOrEmpty(m.Groups[4].Value) ? int.Parse(m.Groups[4].Value) : 3;
            int? targetNumber = m.Groups[5].Success && !string.IsNullOrEmpty(m.Groups[5].Value) ? int.Parse(m.Groups[5].Value) : (int?)null;
            var accidentTarget = staminaZero ? fumbleTarget : 2;

            var diceArr = randomizer.RollBarabara(2, 6);
            var diceSum = diceArr.Sum();
            var resultSum = diceSum + modifyNumber;
            var isCritical = diceSum >= criticalTarget;
            var isFumble = diceSum <= fumbleTarget;
            var isAccident = diceSum <= accidentTarget;
            var isTrouble = isFumble && !isAccident;

            bool? isSuccess = null;
            string resultStr = null;

            if (isCritical)
            {
                isSuccess = true;
                resultStr = "決定的成功";
            }
            else if (isAccident)
            {
                isSuccess = false;
                resultStr = "致命的失敗(アクシデント)";
            }
            else if (isTrouble)
            {
                isSuccess = false;
                resultStr = "致命的失敗(トラブル)";
            }
            else if (targetNumber == null)
            {
                isSuccess = null;
            }
            else if (resultSum >= targetNumber.Value)
            {
                isSuccess = true;
                resultStr = "成功";
            }
            else
            {
                isSuccess = false;
                resultStr = "失敗";
            }

            // Build command display string
            var cmdDisplay = vcCmd;
            var modDisplay = GetModifyText(modifyNumber);
            var critDisplay = criticalTarget != 12 ? $"@{criticalTarget}" : "";
            var fumbleDisplay = fumbleTarget != 3 ? $"#{fumbleTarget}" : "";
            var targetDisplay = targetNumber != null ? $">={targetNumber}" : "";
            var parsedDisplay = $"({cmdDisplay}{modDisplay}{critDisplay}{fumbleDisplay}{targetDisplay})";

            var diceDisplay = $"{diceSum}[{string.Join(",", diceArr)}]{GetModifyText(modifyNumber)}";

            var sequence = new List<string> { parsedDisplay, diceDisplay, resultSum.ToString() };
            if (resultStr != null)
            {
                sequence.Add(resultStr);
            }

            var text = string.Join(" ＞ ", sequence);

            return Result.CreateBuilder(text)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .SetSuccess(isSuccess == true)
                .SetFailure(isSuccess == null ? false : !isSuccess.Value)
                .Build();
        }

        private string GetModifyText(int modify)
        {
            if (modify == 0) return "";
            if (modify > 0) return $"+{modify}";
            return modify.ToString();
        }
    }
}
