using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class BadLife : GameSystemBase
    {
        public static readonly BadLife Instance = new BadLife();

        public override string Id => "BadLife";
        public override string Name => "バッドライフ";
        public override string SortKey => "はつとらいふ";

        private static readonly Regex JudgeReg = new Regex(
            @"^(\d+)?(BAD|BL|GL)([-+\d]*)((C|F)([-+\d]*)?)?((C|F)([-+\d]*))?(@([-+\d]*))?(!([A-Za-z]*))?" ,
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return JudgeDice(command, randomizer);
        }

        private Result? JudgeDice(string command, IRandomizer randomizer)
        {
            var m = JudgeReg.Match(command);
            if (!m.Success) return null;

            int diceCount = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 1;
            int critical = 20;
            int fumble = 1;
            bool isStormy = m.Groups[2].Value.ToUpper() == "GL";
            if (isStormy) { critical -= 3; fumble += 1; }

            int modify = GetValue(m.Groups[3].Value);
            (critical, fumble) = GetCriticalFumble(critical, fumble, m.Groups[5].Value, m.Groups[6].Value);
            (critical, fumble) = GetCriticalFumble(critical, fumble, m.Groups[8].Value, m.Groups[9].Value);

            int target = GetValue(m.Groups[11].Value);
            string optText = m.Groups[13].Success ? m.Groups[13].Value : "";

            return CheckRoll(diceCount, modify, critical, fumble, target, isStormy, optText, randomizer);
        }

        private static (int, int) GetCriticalFumble(int c, int f, string mk, string txt)
        {
            string m = mk?.ToUpper() ?? "";
            if (m == "C") c += GetValue(txt);
            else if (m == "F") f += GetValue(txt);
            return (c, f);
        }


        private static int GetValue(string text) {
            if (string.IsNullOrEmpty(text)) return 0;
            int total = 0;
            foreach (var p in System.Text.RegularExpressions.Regex.Split(text, @"(?=[+-])"))
                if (!string.IsNullOrEmpty(p)) total += int.Parse(p);
            return total;
        }
        private Result CheckRoll(int diceCount, int modify, int critical, int fumble,
            int target, bool isStormy, string optText, IRandomizer randomizer)
        {
            bool isAnticip = optText.IndexOf("A", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isHeavy = optText.IndexOf("H", StringComparison.OrdinalIgnoreCase) >= 0;

            var diceList = randomizer.RollBarabara(diceCount, 20);
            string diceText = string.Join(",", diceList);
            int diceMax = diceList.Max();
            if (isHeavy && diceMax <= 5) diceMax = 5;
            bool isCrit = (diceMax >= critical);
            bool isFumb = (diceMax <= fumble);
            if (isCrit) diceMax = 20;
            int total = diceMax + modify;
            if (isAnticip && diceMax <= 7) total += 5;
            if (isFumb) total = 0;

            var sb = new StringBuilder();
            sb.Append(diceCount + "D20(C:" + critical + ",F:" + fumble + ") ＞ ");
            sb.Append(diceMax + "[" + diceText + "]");
            if (modify > 0) sb.Append("+" + modify);
            else if (modify != 0) sb.Append(modify.ToString());
            if (isAnticip && diceMax <= 7) sb.Append("+5");
            sb.Append(" ＞ 達成値：" + total);
            bool isSuccess = false;
            if (target > 0)
            {
                int success = total - target;
                sb.Append(">=" + target + " 成功度：" + success + " ＞ ");
                if (isCrit) { sb.Append("成功（クリティカル）"); isSuccess = true; }
                else if (total >= target) { sb.Append("成功"); isSuccess = true; }
                else { sb.Append("失敗"); if (isFumb) sb.Append("（ファンブル）"); }
            }
            else
            {
                isSuccess = true;
                if (isCrit) sb.Append(" クリティカル");
                if (isFumb) { sb.Append(" ファンブル"); isSuccess = false; }
            }

            string skillText = "";
            if (isStormy) skillText += "《波乱万丈》";
            if (isAnticip) skillText += "《先見の明》";
            if (isHeavy) skillText += "［重撃］";
            if (skillText != "") sb.Append(" " + skillText);

            return isSuccess ? Result.Success(sb.ToString()) : Result.Failure(sb.ToString());
        }
    }
}
