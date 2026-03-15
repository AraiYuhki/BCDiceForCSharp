using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Arithmetic;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// トーグ
    /// </summary>
    public sealed class Torg : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Torg Instance = new Torg();


        /// <inheritdoc/>
        public override string Id => "Torg";

        /// <inheritdoc/>
        public override string Name => "トーグ";

        /// <inheritdoc/>
        public override string SortKey => "とおく";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定　(TGm)
        　TORG専用の判定コマンドです。
        　""TG(技能基本値)""でロールします。Rコマンドに読替されます。
        　振り足しを自動で行い、20の出目が出たときには技能無し値も並記します。
        ・各種表　""(表コマンド)(数値)""で振ります。
        　・一般結果表 成功度出力「RTx or RESULTx」
        　・威圧/威嚇 対人行為結果表「ITx or INTIMIDATEx or TESTx」
        　・挑発/トリック 対人行為結果表「TTx or TAUNTx or TRICKx or CTx」
        　・間合い 対人行為結果表「MTx or MANEUVERx」
        　・オーズ（一般人）ダメージ　「ODTx or ORDSx or ODAMAGEx」
        　・ポシビリティー能力者ダメージ「DTx or DAMAGEx」
        　・ボーナス表「BTx+y or BONUSx+y or TOTALx+y」 xは数値, yは技能基本値
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var str = command.ToUpperInvariant();
            str = ReplaceText(str);

            var torgResult = TorgCheck(str, randomizer);
            if (torgResult != null)
            {
                return Result.CreateBuilder(torgResult).Build();
            }

            string output = "1";
            string ttype = "";
            int value = 0;

            var m = Regex.Match(str, @"([RITMDB]T)(\d+([+-]\d+)*)");
            if (!m.Success)
            {
                return null;
            }

            var type = m.Groups[1].Value;
            var num = m.Groups[2].Value;

            switch (type)
            {
                case "RT":
                    value = ArithmeticEvaluator.Eval(num, RoundType) ?? 0;
                    output = GetTorgSuccessLevel(value);
                    ttype = "一般結果";
                    break;
                case "IT":
                    value = ArithmeticEvaluator.Eval(num, RoundType) ?? 0;
                    output = GetTorgInteractionResultIntimidateTest(value);
                    ttype = "威圧/威嚇";
                    break;
                case "TT":
                    value = ArithmeticEvaluator.Eval(num, RoundType) ?? 0;
                    output = GetTorgInteractionResultTauntTrick(value);
                    ttype = "挑発/トリック";
                    break;
                case "MT":
                    value = ArithmeticEvaluator.Eval(num, RoundType) ?? 0;
                    output = GetTorgInteractionResultManeuver(value);
                    ttype = "間合い";
                    break;
                case "DT":
                    value = ArithmeticEvaluator.Eval(num, RoundType) ?? 0;
                    if (Regex.IsMatch(str, @"ODT", RegexOptions.IgnoreCase))
                    {
                        output = GetTorgDamageOrds(value);
                        ttype = "オーズダメージ";
                    }
                    else
                    {
                        output = GetTorgDamagePosibility(value);
                        ttype = "ポシビリティ能力者ダメージ";
                    }
                    break;
                case "BT":
                    (output, value) = GetTorgBonusText(num);
                    ttype = "ボーナス";
                    break;
            }

            if (ttype != "")
            {
                output = $"{ttype}表[{value}] ＞ {output}";
            }

            return Result.CreateBuilder(output).Build();
        }

        private string ReplaceText(string str)
        {
            str = Regex.Replace(str, @"Result", "RT", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"(Intimidate|Test)", "IT", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"(Taunt|Trick|CT)", "TT", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"Maneuver", "MT", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"(ords|odamage)", "ODT", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"damage", "DT", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"(bonus|total)", "BT", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"TG(\d+)", m => $"1R20+{m.Groups[1].Value}", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"TG", "1R20", RegexOptions.IgnoreCase);
            return str;
        }

        private string? TorgCheck(string str, IRandomizer randomizer)
        {
            var m = Regex.Match(str, @"(?:^|\s)S?(1R20([+-]\d+)*)(?:\s|$)", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            str = m.Groups[1].Value;
            var modStr = m.Groups[2].Value;

            int mod = 0;
            if (!string.IsNullOrEmpty(modStr))
            {
                mod = ArithmeticEvaluator.Eval(modStr, RoundType) ?? 0;
            }

            var (skilled, unskilled, dice_str) = TorgDice(randomizer);
            var sk_bonus = GetTorgBonus(skilled);

            string output;
            if (mod != 0)
            {
                if (mod > 0)
                {
                    output = $"{sk_bonus}[{dice_str}]+{mod}";
                }
                else
                {
                    output = $"{sk_bonus}[{dice_str}]{mod}";
                }
            }
            else
            {
                output = $"{sk_bonus}[{dice_str}]";
            }

            output += " ＞ " + (sk_bonus + mod).ToString();

            if (skilled != unskilled)
            {
                output += "(技能無" + (GetTorgBonus(unskilled) + mod).ToString() + ")";
            }

            output = $"({str}) ＞ {output}";

            return output;
        }

        private (int skilled, int unskilled, string dice_str) TorgDice(IRandomizer randomizer)
        {
            var isSkilledCritical = true;
            var isCritical = true;
            var skilled = 0;
            var unskilled = 0;
            var dice_str = "";

            while (isSkilledCritical)
            {
                var dice_n = randomizer.RollOnce(20);
                skilled += dice_n;
                if (isCritical)
                {
                    unskilled += dice_n;
                }

                if (dice_str.Length > 0)
                {
                    dice_str += ",";
                }
                dice_str += dice_n.ToString();

                if (dice_n == 20)
                {
                    isCritical = false;
                }
                else if (dice_n != 10)
                {
                    isSkilledCritical = false;
                    isCritical = false;
                }
            }

            return (skilled, unskilled, dice_str);
        }

        private string GetTorgSuccessLevel(int value)
        {
            var success_table = new (int threshold, string text)[] {
                (0, "ぎりぎり"), (1, "ふつう"), (3, "まあよい"), (7, "かなりよい"), (12, "すごい")
            };
            return GetTorgTableResultStr(value, success_table);
        }

        private string GetTorgInteractionResultIntimidateTest(int value)
        {
            var table = new (int threshold, string text)[] {
                (0, "技能なし"), (5, "萎縮"), (10, "逆転負け"), (15, "モラル崩壊"), (17, "プレイヤーズコール")
            };
            return GetTorgTableResultStr(value, table);
        }

        private string GetTorgInteractionResultTauntTrick(int value)
        {
            var table = new (int threshold, string text)[] {
                (0, "技能なし"), (5, "萎縮"), (10, "逆転負け"), (15, "高揚／逆転負け"), (17, "プレイヤーズコール")
            };
            return GetTorgTableResultStr(value, table);
        }

        private string GetTorgInteractionResultManeuver(int value)
        {
            var table = new (int threshold, string text)[] {
                (0, "技能なし"), (5, "疲労"), (10, "萎縮／疲労"), (15, "逆転負け／疲労"), (17, "プレイヤーズコール")
            };
            return GetTorgTableResultStr(value, table);
        }

        private string GetTorgTableResultStr(int value, (int threshold, string text)[] table)
        {
            string output = "1";
            foreach (var item in table)
            {
                if (item.threshold > value)
                {
                    break;
                }
                output = item.text;
            }
            return output;
        }

        private int GetTorgTableResultInt(int value, (int threshold, int bonus)[] table)
        {
            int output = 0;
            foreach (var item in table)
            {
                if (item.threshold > value)
                {
                    break;
                }
                output = item.bonus;
            }
            return output;
        }

        private string GetTorgDamageOrds(int value)
        {
            var damage_table_ords = new (int threshold, string text)[] {
                (0, "1"), (1, "O1"), (2, "K1"), (3, "O2"), (4, "O3"), (5, "K3"),
                (6, "転倒 K／O4"), (7, "転倒 K／O5"),
                (8, "1レベル負傷  K／O7"), (9, "1レベル負傷  K／O9"),
                (10, "1レベル負傷  K／O10"), (11, "2レベル負傷  K／O11"),
                (12, "2レベル負傷  KO12"), (13, "3レベル負傷  KO13"),
                (14, "3レベル負傷  KO14"), (15, "4レベル負傷  KO15")
            };
            return GetTorgDamage(value, 4, "レベル負傷  KO15", damage_table_ords);
        }

        private string GetTorgDamagePosibility(int value)
        {
            var damage_table_posibility = new (int threshold, string text)[] {
                (0, "1"), (1, "1"), (2, "O1"), (3, "K2"), (4, "2"), (5, "O2"),
                (6, "転倒 O2"), (7, "転倒 K2"), (8, "転倒 K2"),
                (9, "1レベル負傷  K3"), (10, "1レベル負傷  K4"),
                (11, "1レベル負傷  O4"), (12, "1レベル負傷  K5"),
                (13, "2レベル負傷  O4"), (14, "2レベル負傷  KO5"),
                (15, "3レベル負傷  KO5")
            };
            return GetTorgDamage(value, 3, "レベル負傷  KO5", damage_table_posibility);
        }

        private string GetTorgDamage(int value, int maxDamage, string maxDamageString, (int threshold, string text)[] damage_table)
        {
            if (value < 0)
            {
                return "1";
            }

            var table_max_value = damage_table.Length - 1;
            if (value <= table_max_value)
            {
                return GetTorgTableResultStr(value, damage_table);
            }

            var over_kill_damage = (value - table_max_value) / 2;
            return "" + (over_kill_damage + maxDamage).ToString() + maxDamageString;
        }

        private (string output, int value) GetTorgBonusText(string num)
        {
            var val_arr = num.Split('+').ToList();
            var value = int.Parse(val_arr[0]);
            val_arr.RemoveAt(0);

            var mod = 0;
            if (val_arr.Count > 0)
            {
                mod = ArithmeticEvaluator.Eval(string.Join("+", val_arr), RoundType) ?? 0;
            }
            var resultValue = GetTorgBonus(value);

            string output;
            if (mod == 0)
            {
                output = resultValue.ToString();
            }
            else
            {
                output = GetTorgBonusOutputTextWhenModDefined(value, resultValue, mod);
                value = int.Parse($"{value}");
            }

            return (output, value);
        }

        private string GetTorgBonusOutputTextWhenModDefined(int value, int resultValue, int mod)
        {
            if (mod > 0)
            {
                return $"{resultValue}[{value}]+{mod} ＞ {resultValue + mod}";
            }
            else
            {
                return $"{resultValue}[{value}]{mod} ＞ {resultValue + mod}";
            }
        }

        private int GetTorgBonus(int value)
        {
            var bonus_table = new (int threshold, int bonus)[] {
                (1, -12), (2, -10), (3, -8), (5, -5), (7, -2), (9, -1), (11, 0),
                (13, 1), (15, 2), (16, 3), (17, 4), (18, 5), (19, 6), (20, 7)
            };

            var bonus = GetTorgTableResultInt(value, bonus_table);

            if (value > 20)
            {
                var over_value_bonus = (value - 21) / 5 + 1;
                bonus += over_value_bonus;
            }

            return bonus;
        }

    }
}
