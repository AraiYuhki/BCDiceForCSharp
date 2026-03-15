using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ダークソウルTRPG
    /// </summary>
    public sealed class DarkSouls : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly DarkSouls Instance = new DarkSouls();

        private static readonly Regex CommandRegex = new Regex(
            @"^(\d+)?(A)?DS([-+\d]*)(@(\d+))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "DarkSouls";

        /// <inheritdoc/>
        public override string Name => "ダークソウルTRPG";

        /// <inheritdoc/>
        public override string SortKey => "たあくそうるTRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・行為判定：[n]DS[a±b][@t]　　[]内のコマンドは省略可
        ・能動判定：[n]ADS[a±b][@t]　　FP消費を判定
        　n：ダイス数。省略時は「2」
        　a±b：修正値。「1+2-1」のように、複数定可
        　@t：目標値。省略時は達成値を、指定時は判定の正否を表示
        例）DS → 2D6の達成値を表示
        　　1DS → 1D6の達成値を表示
        　　ADS+2-2 → 2D6+2の達成値を表示（能動判定）
        　　DS+2@10 → 2D6+2で目標値10の判定
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = CommandRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var diceCount = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 2;
            var isActive = m.Groups[2].Success;
            var modify = GetValue(m.Groups[3].Value);
            var target = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : 0;

            var output = CheckRoll(diceCount, isActive, modify, target, randomizer);
            return Result.CreateBuilder(output).SetSuccess(true).Build();
        }

        private string CheckRoll(int diceCount, bool isActive, int modify, int target, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(diceCount, 6);
            var dice = diceList.Sum();
            var diceText = string.Join(",", diceList);
            var successValue = dice + modify;
            var modifyText = GetValueText(modify);
            var targetText = (target == 0) ? "" : $">={target}";
            var focusText = "";

            if (isActive)
            {
                var diceArray = diceList;
                var focusDamage = diceArray.Count(i => i == 1);
                if (focusDamage > 0)
                {
                    focusText = new string('■', focusDamage);
                    focusText = $"（FP{focusText}消費）";
                }
            }

            var result = $"({diceCount}D6{modifyText}{targetText})";
            result += $" ＞ {dice}({diceText}){modifyText}";
            result += $" ＞ {successValue}{targetText}";

            if (target > 0)
            {
                if (successValue >= target)
                {
                    result += " ＞ 【成功】";
                }
                else
                {
                    result += " ＞ 【失敗】";
                }
            }

            result += focusText;
            return result;
        }

        private int GetValue(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }
            return ArithmeticEvaluator.Eval(text, RoundType) ?? 0;
        }

        private string GetValueText(int value)
        {
            if (value == 0)
            {
                return "";
            }
            if (value < 0)
            {
                return value.ToString();
            }
            return $"+{value}";
        }

    }
}
