using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class BarnaKronika : GameSystemBase
    {
        public static readonly BarnaKronika Instance = new BarnaKronika();

        public override string Id => "BarnaKronika";
        public override string Name => "バルナ・クロニカ";
        public override string SortKey => "はるなくろにか";
        public override bool SortBarabaraDice => true;

        private static readonly Regex R6Reg = new Regex(
            @"(^|\s)S?((?:\d+)[rR]6(\[([,\d]+)\])?)(\s|$)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly string[] HitLocations = new[]
        {
            "頭部", "右腕", "左腕", "右脂", "左脂", "胴体"
        };

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            string replaced = ReplaceText(command);
            var m = R6Reg.Match(replaced);
            if (!m.Success) return null;

            string matched = m.Groups[2].Value;
            string? option = m.Groups[4].Success ? m.Groups[4].Value : null;
            var numMatch = Regex.Match(matched, @"\d+");
            int diceN = numMatch.Success ? int.Parse(numMatch.Value) : 1;

            bool isBattleMode = false;
            int criticalCallDice = 0;
            if (option != null)
            {
                var parts = option.Split(',');
                if (parts.Length >= 1 && int.TryParse(parts[0], out int bm)) isBattleMode = (bm == 1);
                if (parts.Length >= 2 && int.TryParse(parts[1], out int cd)) criticalCallDice = cd;
            }

            var (diceStr, suc, set, atStr) = RollBarnaKronika(diceN, criticalCallDice, isBattleMode, randomizer);

            var sb = new StringBuilder();
            sb.Append($"({matched}) ＞ [{diceStr}] ＞ ");

            if (isBattleMode)
            {
                sb.Append(atStr);
            }
            else
            {
                if (suc > 1)
                    sb.Append($"成功数{suc}");
                else
                    sb.Append("失敗");
                if (set > 0)
                    sb.Append($",セット{set}");
            }

            return Result.Success(sb.ToString());
        }

        private static string ReplaceText(string s)
        {
            s = Regex.Replace(s, @"(\d+)BKC(\d)", m => $"{m.Groups[1].Value}R6[0,{m.Groups[2].Value}]");
            s = Regex.Replace(s, @"(\d+)BAC(\d)", m => $"{m.Groups[1].Value}R6[1,{m.Groups[2].Value}]");
            s = Regex.Replace(s, @"(\d+)BK", m => $"{m.Groups[1].Value}R6[0,0]");
            s = Regex.Replace(s, @"(\d+)BA", m => $"{m.Groups[1].Value}R6[1,0]");
            return s;
        }

        private (string diceStr, int suc, int set, string atStr) RollBarnaKronika(
            int diceN, int criticalCallDice, bool isBattleMode, IRandomizer randomizer)
        {
            var diceCountList = new int[6];
            int suc = 0;

            for (int i = 0; i < diceN; i++)
            {
                int index = randomizer.RollIndex(6);
                diceCountList[index]++;
                if (diceCountList[index] > suc)
                    suc = diceCountList[index];
            }

            var outputSb = new StringBuilder();
            var atSb = new StringBuilder();
            int set = 0;

            for (int i = 0; i < 6; i++)
            {
                int diceCount = diceCountList[i];
                if (diceCount == 0) continue;

                for (int j = 0; j < diceCount; j++)
                    outputSb.Append($"{i + 1},");

                if (IsCriticalCall(i, criticalCallDice, isBattleMode))
                    atSb.Append(GetAttackStringWhenCriticalCall(i, diceCount));
                else if (IsNormalAttack(criticalCallDice, diceCount, isBattleMode))
                    atSb.Append(GetAttackStringWhenNormal(i, diceCount));

                if (diceCount > 1) set++;
            }

            if (criticalCallDice != 0)
            {
                int cCnt = diceCountList[criticalCallDice - 1];
                suc = cCnt * 2;
                set = (cCnt != 0) ? 1 : 0;
            }

            string atStr;
            if (isBattleMode && suc < 2)
                atStr = "失敗";
            else
            {
                string raw = atSb.ToString();
                atStr = raw.Length > 0 ? raw.TrimEnd(',') : "";
            }

            string diceStr = outputSb.ToString().TrimEnd(',');
            return (diceStr, suc, set, atStr);
        }

        private static bool IsCriticalCall(int index, int criticalCallDice, bool isBattleMode)
        {
            if (!isBattleMode) return false;
            if (criticalCallDice == 0) return false;
            return criticalCallDice == (index + 1);
        }

        private static bool IsNormalAttack(int criticalCallDice, int diceCount, bool isBattleMode)
        {
            if (!isBattleMode) return false;
            if (criticalCallDice != 0) return false;
            return diceCount > 1;
        }

        private static string GetAttackStringWhenCriticalCall(int index, int diceCount)
        {
            int attackValue = diceCount * 2;
            return $"{HitLocations[index]}:攻撃値{attackValue},";
        }

        private static string GetAttackStringWhenNormal(int index, int diceCount)
        {
            return $"{HitLocations[index]}:攻撃値{diceCount},";
        }
    }
}
