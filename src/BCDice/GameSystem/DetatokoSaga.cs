using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class DetatokoSaga : GameSystemBase
    {
        public static readonly DetatokoSaga Instance = new DetatokoSaga();
        public override string Id => "DetatokoSaga";
        public override string Name => "でたとこサーガ";
        public override string SortKey => "てたとこさあか";

        private static readonly Regex DsReg = new Regex(
            @"^(\d+)DS(\d+)?(([+-/])(\d+))?(?:>=(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex JdReg = new Regex(
            @"^(\d+)JD(\d+)?(([+-/])(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var r1 = CheckRoll(command, randomizer);
            if (r1 != null) return r1;
            var r2 = CheckJudgeValue(command, randomizer);
            if (r2 != null) return r2;
            return null;
        }

        private Result? CheckRoll(string s, IRandomizer randomizer)
        {
            var m = DsReg.Match(s);
            if (!m.Success) return null;
            int skill = int.Parse(m.Groups[1].Value);
            int flag = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
            string? op = m.Groups[4].Success ? m.Groups[4].Value : null;
            int value = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : 0;
            int target = m.Groups[6].Success ? int.Parse(m.Groups[6].Value) : 8;
            var sb = new System.Text.StringBuilder();
            sb.Append($"(DS ＞ スキル:{skill} フラグ:{flag} 目標:{target})");
            string modText = GetModifyText(op, value);
            if (!string.IsNullOrEmpty(modText)) sb.Append($"(修正{modText})");
            var (total, rollText) = GetRollResult(skill, randomizer);
            sb.Append($" ＞ {total}[{rollText}]{modText}");
            sb.Append($" ＞ {GetTotalResultValue(total, value, op)}");
            if (!string.IsNullOrEmpty(modText))
            {
                if (op == "+") total += value;
                else if (op == "-") total -= value;
            }
            string successStr = total >= target ? "成功" : "失敗";
            sb.Append($" ＞ {successStr}");
            string flagResult = GetCheckFlagResult(total, flag, randomizer);
            if (!string.IsNullOrEmpty(flagResult)) sb.Append(flagResult);
            return total >= target ? Result.Success(sb.ToString()) : Result.Failure(sb.ToString());
        }

        private Result? CheckJudgeValue(string s, IRandomizer randomizer)
        {
            var m = JdReg.Match(s);
            if (!m.Success) return null;
            int skill = int.Parse(m.Groups[1].Value);
            int flag = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
            string? op = m.Groups[4].Success ? m.Groups[4].Value : null;
            int value = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : 0;
            var sb = new System.Text.StringBuilder();
            sb.Append($"(JD ＞ スキル:{skill} フラグ:{flag})");
            string modText = GetModifyText(op, value);
            if (!string.IsNullOrEmpty(modText)) sb.Append($"(修正{modText})");
            var (total, rollText) = GetRollResult(skill, randomizer);
            sb.Append($" ＞ {total}[{rollText}]{modText}");
            sb.Append($" ＞ {GetTotalResultValue(total, value, op)}");
            string flagResult = GetCheckFlagResult(total, flag, randomizer);
            if (!string.IsNullOrEmpty(flagResult)) sb.Append(flagResult);
            return Result.Success(sb.ToString());
        }

        private (int total, string rollText) GetRollResult(int skill, IRandomizer randomizer)
        {
            int dc = skill == 0 ? 3 : skill + 1;
            var dice = randomizer.RollBarabara(dc, 6);
            string diceText = string.Join(",", dice);
            var sorted = skill == 0
                ? dice.OrderBy(x => x).ToList()
                : dice.OrderByDescending(x => x).ToList();
            return (sorted[0] + sorted[1], diceText);
        }

        private string GetCheckFlagResult(int total, int flag, IRandomizer randomizer)
        {
            if (total > flag) return "";
            string will = flag >= 10 ? "6" : $"1D6->{randomizer.RollOnce(6)}";
            return $" (フラグ山下げ: 気力ランク低下 {will})";
        }

        private static string GetModifyText(string? op, int value)
        {
            if (value == 0) return "";
            string opText = op switch
            {
                "+" => "＋",
                "-" => "－",
                "/" => "÷",
                _   => "",
            };
            return string.IsNullOrEmpty(opText) ? "" : $"{opText}{value}";
        }

        private string GetTotalResultValue(int total, int value, string? op)
        {
            if (value == 0 || op == null) return $"判定値: {total}";
            if (op == "+") return $"{total}+{value} ＞ 判定値: {total + value}";
            if (op == "-") return $"{total}-{value} ＞ 判定値: {total - value}";
            if (op == "/") {
                if (value == 0) return "ゼロ除算エラー";
                int q = (int)Math.Ceiling((double)total / value);
                return $"{total}÷{value} ＞ 判定値: {q}";
            }
            return $"判定値: {total}";
        }
    }
}
