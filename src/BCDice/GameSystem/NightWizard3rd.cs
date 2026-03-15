using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ナイトウィザード The 3rd Edition
    /// NightWizard基盤のNWコマンド判定を実装。
    /// クリティカル時は+10して振り足し、ファンブル時は-10。
    /// </summary>
    public sealed class NightWizard3rd : GameSystemBase
    {
        public static readonly NightWizard3rd Instance = new NightWizard3rd();

        // aNW+b@x#y$z+c 形式
        private static readonly Regex NwRegex = new Regex(
            @"^([-+]?\d+)?NW((?:[-+]\d+)+)?(?:@(\d+(?:,\d+)*))?(?:#(\d+(?:,\d+)*))?(?:\$(\d+))?((?:[-+]\d+)+)?(?:([>=]+)(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // 2R6M[p,a]C[c]F[f]>=t 形式
        private static readonly Regex R6Regex = new Regex(
            @"^2R6M\[([-+]?\d+(?:[-+]\d+)*)(?:,([-+]?\d+(?:[-+]\d+)*))?\](?:C\[(\d+(?:,\d+)*)\])?(?:F\[(\d+(?:,\d+)*)\])?(?:([>=]+)(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override string Id => "NightWizard3rd";
        public override string Name => "ナイトウィザード The 3rd Edition";
        public override string SortKey => "ないとういさあと3";

        public override string HelpMessage => @"・判定用コマンド　(aNW+b@x#y$z+c)
　　a : 基本値
　　b : 常時に準じる特技による補正
　　c : 常時以外の特技、および支援効果による補正
　　x : クリティカル値のカンマ区切り（省略時 10）
　　y : ファンブル値のカンマ区切り（省略時 5）
　　z : プラーナによる達成値補正のプラーナ消費数（ファンブル時には適用されない）
　クリティカル値、ファンブル値が無い場合は1や13などのあり得ない数値を入れてください。
　例）12NW-5@7#2$3 1NW 50nw+5@7,10#2,5 50nw-5+10@7,10#2,5+15+25
";

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var parsed = ParseNw(command) ?? Parse2R6(command);
            if (parsed == null)
            {
                return null;
            }

            var (total, interimExpr, status) = RollNw(parsed, randomizer);

            string? resultText = null;
            bool isSuccess = false;
            bool isFailure = false;

            if (parsed.CmpOp != null && parsed.TargetNumber.HasValue)
            {
                if (total >= parsed.TargetNumber.Value)
                {
                    resultText = "成功";
                    isSuccess = true;
                }
                else
                {
                    resultText = "失敗";
                    isFailure = true;
                }
            }

            var parts = new List<string>
            {
                $"({parsed})",
                interimExpr,
            };

            if (status != null)
            {
                parts.Add(status);
            }

            parts.Add(total.ToString());

            if (resultText != null)
            {
                parts.Add(resultText);
            }

            string text = string.Join(" ＞ ", parts);

            return Result.CreateBuilder(text)
                .SetSuccess(isSuccess || status == "クリティカル")
                .SetFailure(isFailure || status == "ファンブル")
                .SetCritical(status == "クリティカル")
                .SetFumble(status == "ファンブル")
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private NwParsed? ParseNw(string command)
        {
            var m = NwRegex.Match(command);
            if (!m.Success) return null;

            int baseValue = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
            int modifyNumber = m.Groups[2].Success ? EvalModifier(m.Groups[2].Value) : 0;
            int[] criticalNumbers = m.Groups[3].Success
                ? m.Groups[3].Value.Split(',').Select(int.Parse).ToArray()
                : new[] { 10 };
            int[] fumbleNumbers = m.Groups[4].Success
                ? m.Groups[4].Value.Split(',').Select(int.Parse).ToArray()
                : new[] { 5 };
            int? prana = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : (int?)null;
            int activeModifyNumber = m.Groups[6].Success ? EvalModifier(m.Groups[6].Value) : 0;
            string? cmpOp = m.Groups[7].Success ? ">=" : null;
            int? targetNumber = m.Groups[8].Success ? int.Parse(m.Groups[8].Value) : (int?)null;

            return new NwParsed()
            {
                BaseValue = baseValue,
                ModifyNumber = modifyNumber,
                CriticalNumbers = criticalNumbers,
                FumbleNumbers = fumbleNumbers,
                Prana = prana,
                ActiveModifyNumber = activeModifyNumber,
                CmpOp = cmpOp,
                TargetNumber = targetNumber,
                Is2R6 = false,
            };
        }

        private NwParsed? Parse2R6(string command)
        {
            var m = R6Regex.Match(command);
            if (!m.Success) return null;

            int passiveModifyNumber = EvalModifier(m.Groups[1].Value);
            int activeModifyNumber = m.Groups[2].Success ? EvalModifier(m.Groups[2].Value) : 0;
            int[] criticalNumbers = m.Groups[3].Success
                ? m.Groups[3].Value.Split(',').Select(int.Parse).ToArray()
                : new[] { 10 };
            int[] fumbleNumbers = m.Groups[4].Success
                ? m.Groups[4].Value.Split(',').Select(int.Parse).ToArray()
                : new[] { 5 };
            string? cmpOp = m.Groups[5].Success ? ">=" : null;
            int? targetNumber = m.Groups[6].Success ? int.Parse(m.Groups[6].Value) : (int?)null;

            return new NwParsed()
            {
                BaseValue = 0,
                ModifyNumber = 0,
                PassiveModifyNumber = passiveModifyNumber,
                CriticalNumbers = criticalNumbers,
                FumbleNumbers = fumbleNumbers,
                Prana = null,
                ActiveModifyNumber = activeModifyNumber,
                CmpOp = cmpOp,
                TargetNumber = targetNumber,
                Is2R6 = true,
            };
        }

        private (int total, string interimExpr, string? status) RollNw(NwParsed parsed, IRandomizer randomizer)
        {
            int total = 0;
            string interimExpr = "";
            string? status = null;

            // 最初のロール（ファンブル判定あり）
            var (rollStatus, rollTotal, rollExpr) = RollOnce(parsed.CriticalNumbers, parsed.FumbleNumbers, true, randomizer);
            total += rollTotal;
            interimExpr += rollExpr;
            status = rollStatus switch
            {
                RollStatus.Fumble => "ファンブル",
                RollStatus.Critical => "クリティカル",
                _ => null
            };

            // クリティカルなら振り足し
            while (rollStatus == RollStatus.Critical)
            {
                (rollStatus, rollTotal, rollExpr) = RollOnce(parsed.CriticalNumbers, parsed.FumbleNumbers, false, randomizer);
                total += rollTotal;
                interimExpr += rollExpr;
                if (rollStatus == RollStatus.Critical)
                {
                    status = "クリティカル";
                }
            }

            // プラーナ処理（ファンブルでない場合のみ）
            if (status != "ファンブル" && parsed.Prana.HasValue && parsed.Prana.Value > 0)
            {
                var diceList = randomizer.RollBarabara(parsed.Prana.Value, 6);
                int pranaBonus = diceList.Sum();
                string pranaStr = string.Join(",", diceList);
                total += pranaBonus;
                interimExpr += $"+{pranaBonus}[{pranaStr}]";
            }

            // 基礎値の計算
            int baseNum;
            if (status == "ファンブル")
            {
                // 3rd Editionのファンブル時: passive + active
                baseNum = GetPassiveModifyNumber(parsed) + parsed.ActiveModifyNumber;
            }
            else
            {
                baseNum = GetPassiveModifyNumber(parsed) + parsed.ActiveModifyNumber;
            }

            total += baseNum;
            interimExpr = baseNum.ToString() + interimExpr;

            return (total, interimExpr, status);
        }

        private static int GetPassiveModifyNumber(NwParsed parsed)
        {
            if (parsed.Is2R6)
            {
                return parsed.PassiveModifyNumber;
            }
            return parsed.BaseValue + parsed.ModifyNumber;
        }

        private enum RollStatus { Normal, Critical, Fumble }

        private (RollStatus status, int total, string expr) RollOnce(
            int[] criticalNumbers, int[] fumbleNumbers, bool fumbleable, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(2, 6);
            int diceValue = diceList.Sum();
            string diceStr = string.Join(",", diceList);

            if (fumbleable && fumbleNumbers.Contains(diceValue))
            {
                return (RollStatus.Fumble, -10, $"-10[{diceStr}]");
            }
            if (criticalNumbers.Contains(diceValue))
            {
                return (RollStatus.Critical, 10, $"+10[{diceStr}]");
            }
            return (RollStatus.Normal, diceValue, $"+{diceValue}[{diceStr}]");
        }

        private static int EvalModifier(string expr)
        {
            if (string.IsNullOrEmpty(expr)) return 0;
            int result = 0;
            var matches = Regex.Matches(expr, @"[-+]\d+");
            foreach (Match m in matches)
            {
                result += int.Parse(m.Value);
            }
            return result;
        }

        private class NwParsed
        {
            public int BaseValue { get; set; }
            public int ModifyNumber { get; set; }
            public int PassiveModifyNumber { get; set; }
            public int[] CriticalNumbers { get; set; } = new[] { 10 };
            public int[] FumbleNumbers { get; set; } = new[] { 5 };
            public int? Prana { get; set; }
            public int ActiveModifyNumber { get; set; }
            public string? CmpOp { get; set; }
            public int? TargetNumber { get; set; }
            public bool Is2R6 { get; set; }

            public override string ToString()
            {
                if (Is2R6)
                {
                    string passive = PassiveModifyNumber.ToString();
                    string active = ActiveModifyNumber != 0 ? $",{ActiveModifyNumber}" : "";
                    string crit = $"C[{string.Join(",", CriticalNumbers)}]";
                    string fumble = $"F[{string.Join(",", FumbleNumbers)}]";
                    string target = CmpOp != null && TargetNumber.HasValue ? $">={TargetNumber}" : "";
                    return $"2R6M[{passive}{active}]{crit}{fumble}{target}";
                }
                else
                {
                    string baseStr = BaseValue != 0 ? BaseValue.ToString() : "";
                    string modStr = FormatModifier(ModifyNumber);
                    string activeStr = FormatModifier(ActiveModifyNumber);
                    string dollar = Prana.HasValue ? $"${Prana}" : "";
                    string target = CmpOp != null && TargetNumber.HasValue ? $">={TargetNumber}" : "";
                    return $"{baseStr}NW{modStr}@{string.Join(",", CriticalNumbers)}#{string.Join(",", FumbleNumbers)}{dollar}{activeStr}{target}";
                }
            }

            private static string FormatModifier(int value)
            {
                if (value == 0) return "";
                return value > 0 ? $"+{value}" : value.ToString();
            }
        }
    }
}