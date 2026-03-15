using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 真・女神転生TRPG 覚醒篇
    /// </summary>
    public sealed class ShinMegamiTenseiKakuseihen : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly ShinMegamiTenseiKakuseihen Instance = new ShinMegamiTenseiKakuseihen();

        /// <inheritdoc/>
        public override string Id => "ShinMegamiTenseiKakuseihen";

        /// <inheritdoc/>
        public override string Name => "真・女神転生TRPG 覚醒篇";

        /// <inheritdoc/>
        public override string SortKey => "しんめかみてんせいTRPGかくせいへん";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定
        1D100<=(目標値) でスワップ・通常・逆スワップ判定を判定。
        威力ダイスは nU6[6] (nはダイス個数)でロール可能です。
        ";


        private string Check1d100(int total, int dice_total, string cmp_op, string target)
        {
            if (target == "?")
            {
                return "";
            }
            if (cmp_op != "<=")
            {
                return "";
            }
            var (dice1, dice2) = SplitTens(dice_total);
            var total1 = dice1 * 10 + dice2;
            var total2 = dice2 * 10 + dice1;
            var isRepdigit = dice1 == dice2;
            var result = " ＞ スワップ";
            result += GetCheckResultText(Convert.ToInt32(target), new[] { total1, total2 }.Min(), isRepdigit);
            result += "／通常";
            result += GetCheckResultText(Convert.ToInt32(target), total % 100, isRepdigit);
            result += "／逆スワップ";
            result += GetCheckResultText(Convert.ToInt32(target), new[] { total1, total2 }.Max(), isRepdigit);
            return result;
        }

        private (int ones, int tens) SplitTens(int value)
        {
            value %= 100;
            var ones = value / 10;
            var tens = value % 10;
            return (ones, tens);
        }

        private string GetCheckResultText(int diff, int total, bool isRepdigit)
        {
            var checkResult = GetCheckResult(diff, total, isRepdigit);
            var text = string.Format("({0:D2})", total) + checkResult;
            return text;
        }

        private string GetCheckResult(int diff, int total, bool isRepdigit)
        {
            if (diff >= total)
            {
                return GetSuccessResult(isRepdigit);
            }
            return GetFailResult(isRepdigit);
        }

        private string GetSuccessResult(bool isRepdigit)
        {
            if (isRepdigit)
            {
                return "絶対成功";
            }
            return "成功";
        }

        private string GetFailResult(bool isRepdigit)
        {
            if (isRepdigit)
            {
                return "絶対失敗";
            }
            return "失敗";
        }

    }
}
