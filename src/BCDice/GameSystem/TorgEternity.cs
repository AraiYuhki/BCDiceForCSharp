using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Arithmetic;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// トーグ エタニティ
    /// </summary>
    public sealed class TorgEternity : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly TorgEternity Instance = new TorgEternity();

        /// <inheritdoc/>
        public override string Id => "TorgEternity";

        /// <inheritdoc/>
        public override string Name => "トーグ エタニティ";

        /// <inheritdoc/>
        public override string SortKey => "とおくえたにてい";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定
        　・TG
        　　""TG[m]""で1d20をロールします。[]内は省略可能。
        　　mは技能基本値を入れて下さい。Rコマンドに読替されます。
        　　振り足しを自動で行い、20の出目が出たときには技能無し値も並記します。
        　　(TORGダイスボットと同じ挙動をするコマンドです。ロールボーナスの読み替えのみ、Eternity版となります)
        　・TE
        　　""TE""で1d20をロールします。
        　　振り足しを自動で行い、20の出目が出たときには技能無し値も並記します。
        　　出目1の時には「Mishap!　自動失敗！」と出力されます。
        　・UP
        　　""UP[m]""で高揚状態のロール(通常の1d20に加え、1d20を追加で振り足し)を行います。
        　　[]内は省略可能。mは技能基本値を入れて下さい。
        　　各ロールでの振り足しを自動で行い、20の出目が出たときには技能無し値も並記します。
        　　一投目で出目1の時には「Mishap!　自動失敗！」と出力され、二投目は行われません。
        　・POS
        　　""POSm""で、ポシビリティ使用による1d20のロールを行います。
        　　mはポシビリティを使用する前のロール結果を入れて下さい。
        　　出目が10未満の場合は、10への読み替えが行われます。
        　　また、振り足しを自動で行い、20の出目が出たときには技能無し値も並記します。
        　・CPOS
        　　""CPOSm""で、コズムポシビリティ使用による1d20のロールを行います。
        　　mはポシビリティを使用する前のロール結果を入れて下さい。
        　　出目が10未満の場合でも、10への読み替えが行われません。
        　　振り足しは自動で行い、20の出目が出たときには技能無し値も並記します。
        ・ボーナスダメージロール
        　""xBD[+y]""でロールします。[]内は省略可能。
        　xはダメージダイス数。yはダメージ基本値 or 式を入れて下さい。
        　xは1以上が必要です。0だとエラーが出力されます。マイナス値はコマンドとして認識されません。
        　振り足し処理は自動で行われます。(振り足し発生時の目は、「5∞」と出力されます)
        ・各種表
        　""(表コマンド)(数値)""で振ります。
        　・成功レベル表「RTx or RESULTx」
        　・ダメージ結果表「DTx or DAMAGEx」
        　・ロールボーナス表「BTx+y or BONUSx+y or TOTALx+y」 xは数値, yは技能基本値
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return TorgCheck(command, randomizer) ??
                GetRolld20DiceCommandResult(command, randomizer) ??
                GetUpRollDiceCommandResult(command, randomizer) ??
                GetPossibilityRollDiceCommandResult(command, randomizer) ??
                GetCosmPossibilityRollDiceCommandResult(command, randomizer) ??
                GetBonusDamageDiceCommandResult(command, randomizer) ??
                GetSuccessLevelDiceCommandResult(command) ??
                GetDamageResultDiceCommandResult(command) ??
                GetRollBonusDiceCommandResult(command);
        }

        private string ReplaceText(string str)
        {
            str = Regex.Replace(str, @"TG(\d+)", m => $"1R20+{m.Groups[1].Value}", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"TG", "1R20", RegexOptions.IgnoreCase);
            return str;
        }

        private Result? TorgCheck(string command, IRandomizer randomizer)
        {
            var str = ReplaceText(command);
            var m = Regex.Match(str, @"^1R20(([+-]\d+)*)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var modStr = m.Groups[1].Value;
            int mod = !string.IsNullOrEmpty(modStr)
                ? ArithmeticEvaluator.Eval(modStr, RoundType) ?? 0
                : 0;

            var (skilled, unskilled, dice_str, _) = TorgEternityDice(false, false, randomizer);
            var sk_bonus = GetTorgEternityBonus(skilled);

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
                output += "(技能無" + (GetTorgEternityBonus(unskilled) + mod).ToString() + ")";
            }

            output = $"({str}) ＞ {output}";

            return Result.CreateBuilder(output).Build();
        }

        private Result? GetRolld20DiceCommandResult(string command, IRandomizer randomizer)
        {
            if (command != "TE")
            {
                return null;
            }

            var (skilled, unskilled, dice_str, mishap) = TorgEternityDice(false, true, randomizer);
            string output;
            if (mishap == 1)
            {
                output = $"d20ロール（通常） ＞ 1d20[{dice_str}] ＞ Mishap!　絶対失敗！";
            }
            else
            {
                var value_skilled = string.Format("{0:+#;-#;+0}", GetTorgEternityBonus(skilled));
                if (skilled != unskilled)
                {
                    var value_unskilled = string.Format("{0:+#;-#;+0}", GetTorgEternityBonus(unskilled));
                    output = $"d20ロール（通常） ＞ 1d20[{dice_str}] ＞ {value_skilled}[{skilled}]（技能有） / {value_unskilled}[{unskilled}]（技能無）";
                }
                else
                {
                    output = $"d20ロール（通常） ＞ 1d20[{dice_str}] ＞ {value_skilled}[{skilled}]";
                }
            }

            return Result.CreateBuilder(output).Build();
        }

        private Result? GetUpRollDiceCommandResult(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^UP(\d*)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var modStr = m.Groups[1].Value;
            int mod = string.IsNullOrEmpty(modStr) ? 0 : int.Parse(modStr);

            var (skilled1, unskilled1, dice_str1, mishap) = TorgEternityDice(false, true, randomizer);
            var sequence = new List<string>();

            if (mishap == 1)
            {
                sequence.Add("d20ロール（高揚）");
                sequence.Add($"1d20[{dice_str1}]");
                sequence.Add("Mishap!　絶対失敗！");
            }
            else
            {
                var (skilled2, unskilled2, dice_str2, _) = TorgEternityDice(false, false, randomizer);
                var subtotal_skilled = skilled1 + skilled2;
                var subtotal_unskilled = unskilled1 + unskilled2;
                var value_skilled = string.Format("{0:+#;-#;+0}", GetTorgEternityBonus(subtotal_skilled));
                var value_unskilled = string.Format("{0:+#;-#;+0}", GetTorgEternityBonus(subtotal_unskilled));

                if (mod <= 0)
                {
                    if (subtotal_skilled != subtotal_unskilled)
                    {
                        sequence.Add("d20ロール（高揚）");
                        sequence.Add($"1d20[{dice_str1}] + 1d20[{dice_str2}]");
                        sequence.Add($"{value_skilled}[{subtotal_skilled}]（技能有） / {value_unskilled}[{subtotal_unskilled}]（技能無）");
                    }
                    else
                    {
                        sequence.Add("d20ロール（高揚）");
                        sequence.Add($"1d20[{dice_str1}] + 1d20[{dice_str2}]");
                        sequence.Add($"{value_skilled}[{subtotal_skilled}]");
                    }
                }
                else
                {
                    if (subtotal_skilled != subtotal_unskilled)
                    {
                        sequence.Add("d20ロール（高揚）");
                        sequence.Add($"1d20[{dice_str1}] + 1d20[{dice_str2}] + {mod}");
                        sequence.Add($"{value_skilled}[{subtotal_skilled}]+{mod}（技能有） / {value_unskilled}[{subtotal_unskilled}]+{mod}（技能無）");
                        sequence.Add(string.Format("{0:+#;-#;+0}", int.Parse(value_skilled) + mod) + "（技能有） / " + string.Format("{0:+#;-#;+0}", int.Parse(value_unskilled) + mod) + "（技能無）");
                    }
                    else
                    {
                        sequence.Add("d20ロール（高揚）");
                        sequence.Add($"1d20[{dice_str1}] + 1d20[{dice_str2}] + {mod}");
                        sequence.Add($"{value_skilled}[{subtotal_skilled}]+{mod}");
                        sequence.Add(string.Format("{0:+#;-#;+0}", int.Parse(value_skilled) + mod));
                    }
                }
            }

            var output = string.Join(" ＞ ", sequence);
            return Result.CreateBuilder(output).Build();
        }

        private Result? GetPossibilityRollDiceCommandResult(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^POS((\d+)(\+\d+)?)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var output_modifier = !string.IsNullOrEmpty(m.Groups[1].Value)
                ? ArithmeticEvaluator.Eval(m.Groups[1].Value, RoundType) ?? 0
                : 0;
            var (skilled, unskilled, dice_str, _) = TorgEternityDice(true, false, randomizer);
            var subtotal_skilled = skilled + output_modifier;
            var subtotal_unskilled = unskilled + output_modifier;
            var value_skilled = string.Format("{0:+#;-#;+0}", GetTorgEternityBonus(subtotal_skilled));

            string output;
            if (subtotal_skilled != subtotal_unskilled)
            {
                var value_unskilled = string.Format("{0:+#;-#;+0}", GetTorgEternityBonus(subtotal_unskilled));
                output = $"d20ロール（ポシビリティ） ＞ {output_modifier}+1d20[{dice_str}] ＞ {value_skilled}[{subtotal_skilled}]（技能有） / {value_unskilled}[{subtotal_unskilled}]（技能無）";
            }
            else
            {
                output = $"d20ロール（ポシビリティ） ＞ {output_modifier}+1d20[{dice_str}] ＞ {value_skilled}[{subtotal_skilled}]";
            }

            return Result.CreateBuilder(output).Build();
        }

        private Result? GetCosmPossibilityRollDiceCommandResult(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^CPOS((\d+)(\+\d+)?)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var output_modifier = !string.IsNullOrEmpty(m.Groups[1].Value)
                ? ArithmeticEvaluator.Eval(m.Groups[1].Value, RoundType) ?? 0
                : 0;
            var (skilled, unskilled, dice_str, _) = TorgEternityDice(false, false, randomizer);
            var subtotal_skilled = skilled + output_modifier;
            var subtotal_unskilled = unskilled + output_modifier;
            var value_skilled = string.Format("{0:+#;-#;+0}", GetTorgEternityBonus(subtotal_skilled));

            string output;
            if (subtotal_skilled != subtotal_unskilled)
            {
                var value_unskilled = string.Format("{0:+#;-#;+0}", GetTorgEternityBonus(subtotal_unskilled));
                output = $"d20ロール（ポシビリティ） ＞ {output_modifier}+1d20[{dice_str}] ＞ {value_skilled}[{subtotal_skilled}]（技能有） / {value_unskilled}[{subtotal_unskilled}]（技能無）";
            }
            else
            {
                output = $"d20ロール（ポシビリティ） ＞ {output_modifier}+1d20[{dice_str}] ＞ {value_skilled}[{subtotal_skilled}]";
            }

            return Result.CreateBuilder(output).Build();
        }

        private Result? GetBonusDamageDiceCommandResult(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)(BD)(([+-]\d+)*)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var number_bonus_die = int.Parse(m.Groups[1].Value);
            var (value_modifier, output_modifier) = GetTorgEternityModifier(m.Groups[3].Value);

            string output;
            if (number_bonus_die <= 0)
            {
                output = "エラーです。xBD (x≧1) として下さい";
            }
            else
            {
                var (value_roll, output_roll) = GetTorgEternityDamageBonusDice(number_bonus_die, randomizer);
                var output_value = value_roll + value_modifier;
                output = $"ボーナスダメージロール({number_bonus_die}BD{output_modifier}) ＞ {value_roll}[{output_roll}]{output_modifier} ＞ {output_value}ダメージ";
            }

            return Result.CreateBuilder(output).Build();
        }

        private Result? GetSuccessLevelDiceCommandResult(string command)
        {
            var m = Regex.Match(command, @"^(RT|Result)(-*\d+([+-]\d+)*)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var value = !string.IsNullOrEmpty(m.Groups[2].Value)
                ? ArithmeticEvaluator.Eval(m.Groups[2].Value, RoundType) ?? 0
                : 0;

            string output;
            if (value < 0)
            {
                output = "Failure.";
            }
            else
            {
                output = GetTorgEternitySuccessLevel(value);
            }
            output = $"成功レベル表[{value}] ＞ {output}";

            return Result.CreateBuilder(output).Build();
        }

        private Result? GetDamageResultDiceCommandResult(string command)
        {
            var m = Regex.Match(command, @"^(DT|Damage)(-*\d+([+-]\d+)*)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var value = !string.IsNullOrEmpty(m.Groups[2].Value)
                ? ArithmeticEvaluator.Eval(m.Groups[2].Value, RoundType) ?? 0
                : 0;

            var output = GetTorgEternityDamageResult(value);
            output = $"ダメージ結果表[{value}] ＞ {output}";

            return Result.CreateBuilder(output).Build();
        }

        private Result? GetRollBonusDiceCommandResult(string command)
        {
            var m = Regex.Match(command, @"^(BT|Bonus|Total)(\d+)(([+-]\d+)*)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var value_roll = int.Parse(m.Groups[2].Value);
            var output_bonus = GetTorgEternityBonus(value_roll);
            var (value_modifier, output_modifier) = GetTorgEternityModifier(m.Groups[3].Value);

            string output;
            if (value_roll <= 1)
            {
                output = $"ロールボーナス表[{value_roll}] ＞ Mishap!!";
            }
            else if (string.IsNullOrEmpty(output_modifier))
            {
                output = $"ロールボーナス表[{value_roll}] ＞ {output_bonus}";
            }
            else
            {
                var value_result = output_bonus + value_modifier;
                output = $"ロールボーナス表[{value_roll}]{output_modifier} ＞ {output_bonus}[{value_roll}]{output_modifier} ＞ {value_result}";
            }

            return Result.CreateBuilder(output).Build();
        }

        private string? GetTorgEternityTableResult(int value, (int threshold, string text)[] table)
        {
            string? output = null;
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

        private int GetTorgEternityTableResultInt(int value, (int threshold, int bonus)[] table)
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

        private (int value_modifier, string output_modifier) GetTorgEternityModifier(string string_modifier)
        {
            if (string.IsNullOrEmpty(string_modifier))
            {
                return (0, "");
            }
            else
            {
                var value_modifier = ArithmeticEvaluator.Eval(string_modifier, RoundType) ?? 0;
                var output_modifier = string.Format("{0:+#;-#;+0}", value_modifier);
                return (value_modifier, output_modifier);
            }
        }

        private (int skilled, int unskilled, string dice_str, int mishap) TorgEternityDice(bool check_pos, bool check_mishap, IRandomizer randomizer)
        {
            var isSkilledCritical = true;
            var isCritical = true;
            var skilled = 0;
            var unskilled = 0;
            var mishap = 0;
            var dice_str = "";

            while (isSkilledCritical)
            {
                if (dice_str.Length > 0)
                {
                    dice_str += ",";
                }

                var dice_n = randomizer.RollOnce(20);

                if (check_pos)
                {
                    if (dice_n < 10)
                    {
                        dice_str += $"{dice_n}→10";
                        dice_n = 10;
                        isSkilledCritical = false;
                    }
                    else
                    {
                        dice_str += dice_n.ToString();
                    }
                }
                else
                {
                    dice_str += dice_n.ToString();
                }

                skilled += dice_n;
                if (isCritical)
                {
                    unskilled += dice_n;
                }

                if (dice_n == 20)
                {
                    isCritical = false;
                }
                else if (dice_n != 10)
                {
                    isSkilledCritical = false;
                    isCritical = false;
                    if (check_mishap && dice_n == 1)
                    {
                        mishap = 1;
                    }
                }

                check_pos = false;
                check_mishap = false;
            }

            return (skilled, unskilled, dice_str, mishap);
        }

        private (int value_roll, string output_roll) GetTorgEternityDamageBonusDice(int number, IRandomizer randomizer)
        {
            var value_roll = 0;
            var output_roll = "";

            if (number > 0)
            {
                while (number > 0)
                {
                    if (output_roll.Length > 0)
                    {
                        output_roll += ",";
                    }

                    var dice_value = randomizer.RollOnce(6);
                    var dice_text = dice_value.ToString();

                    if (dice_value == 6)
                    {
                        dice_value = 5;
                        dice_text = "5∞";
                        number += 1;
                    }

                    value_roll += dice_value;
                    output_roll += dice_text;
                    number -= 1;
                }
            }
            else
            {
                output_roll = "0";
            }

            return (value_roll, output_roll);
        }

        private string GetTorgEternitySuccessLevel(int value)
        {
            var success_table = new (int threshold, string text)[] {
                (0, "Success - Standard."), (5, "Success - Good!"), (10, "Success - Outstanding!!")
            };
            return GetTorgEternityTableResult(value, success_table) ?? "Success - Standard.";
        }

        private string GetTorgEternityDamageResult(int value)
        {
            var damage_table = new (int threshold, string text)[] {
                (-50, "ノーダメージ"), (-5, "1ショック"), (0, "2ショック"),
                (5, "1レベル負傷 + 2ショック"), (10, "2レベル負傷 + 4ショック"),
                (15, "3レベル負傷 + 6ショック"), (20, "4レベル負傷 + 8ショック"),
                (25, "5レベル負傷 + 10ショック"), (30, "6レベル負傷 + 12ショック"),
                (35, "7レベル負傷 + 14ショック"), (40, "8レベル負傷 + 16ショック"),
                (45, "9レベル負傷 + 18ショック"), (50, "10レベル負傷 + 20ショック")
            };
            return GetTorgEternityTableResult(value, damage_table) ?? "ノーダメージ";
        }

        private int GetTorgEternityBonus(int value)
        {
            var bonus_table = new (int threshold, int bonus)[] {
                (1, -10), (2, -8), (3, -6), (5, -4), (7, -2), (9, -1), (11, 0),
                (13, 1), (15, 2), (16, 3), (17, 4), (18, 5), (19, 6), (20, 7)
            };

            var bonus = GetTorgEternityTableResultInt(value, bonus_table);

            if (value > 20)
            {
                var over_value_bonus = (value - 21) / 5 + 1;
                bonus += over_value_bonus;
            }

            return bonus;
        }

    }
}
