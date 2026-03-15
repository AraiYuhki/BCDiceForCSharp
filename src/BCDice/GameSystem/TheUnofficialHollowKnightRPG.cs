using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// The Unofficial Hollow Knight RPG
    /// </summary>
    public sealed class TheUnofficialHollowKnightRPG : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly TheUnofficialHollowKnightRPG Instance = new TheUnofficialHollowKnightRPG();

        /// <inheritdoc/>
        public override string Id => "TheUnofficialHollowKnightRPG";

        /// <inheritdoc/>
        public override string Name => "The Unofficial Hollow Knight RPG";

        /// <inheritdoc/>
        public override string SortKey => "しあんおふいしやるほろうないとRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・能力値判定　[n]AD[+b][#r][>=t]
        　n: 能力値。小数可。省略不可。
        　b: ボーナス、ペナルティダイス。省略可。
        　r: 追加リロールダイス数。省略可。
        　t: 目標値。>=含めて省略可。
        　成功数を判定。
        　例）1AD, 2.5AD, 1.5AD+1, 2AD#1, 2.5AD+2#2>=4

        ・イニシアチブ　[n]INTI[+b][#r]
        　n: イニシアチブに使う能力値。省略不可。
          b: ボーナス、ペナルティダイス。省略可。
          r: 追加リロールダイス数。省略可。
        　振り直しを行ったうえでイニシアチブ値を計算。
        　例）1INTI, 2.5INTI, 1.5INTI+1, 2INTI#1, 2.5INTI+2#2
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return AbilityRoll(command, randomizer) ?? InitiativeRoll(command, randomizer);
        }

        private string NumberWithSignFromInt(int number)
        {
            if (number == 0)
            {
                return "";
            }
            else if (number > 0)
            {
                return $"+{Math.Abs(number)}";
            }
            else
            {
                return $"-{Math.Abs(number)}";
            }
        }

        private string NumberWithRerollFromInt(int number)
        {
            if (number == 0)
            {
                return "";
            }
            else if (number > 0)
            {
                return $"#{number}";
            }
            else
            {
                return number.ToString();
            }
        }

        private Result? AbilityRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+\.?\d*)?AD([+-](\d+))?(#(\d*))?(>=(\d+))?");
            if (!m.Success)
            {
                return null;
            }

            var num_of_die = m.Groups[1].Success ? Convert.ToDouble(m.Groups[1].Value) : 0.0;
            var bonus = m.Groups[3].Success ? Convert.ToInt32(m.Groups[3].Value) : 0;
            if (m.Groups[2].Success && m.Groups[2].Value.StartsWith("-"))
            {
                bonus = -bonus;
            }
            var reroll = m.Groups[5].Success ? Convert.ToInt32(m.Groups[5].Value) : 0;
            var difficulty = m.Groups[7].Success ? Convert.ToInt32(m.Groups[7].Value) : 0;

            string dice_command;
            // 小数部が 0 以外 (例: 2.5) かどうかチェック
            if (Regex.IsMatch(num_of_die.ToString("G"), @"\.[1-9]+"))
            {
                dice_command = $"{num_of_die}AD{NumberWithSignFromInt(bonus)}{NumberWithRerollFromInt(reroll)}";
                reroll += 1;
            }
            else
            {
                dice_command = $"{(int)num_of_die}AD{NumberWithSignFromInt(bonus)}{NumberWithRerollFromInt(reroll)}";
            }

            if (difficulty == 0)
            {
                difficulty = 5;
            }
            else
            {
                dice_command += $">={difficulty}";
            }

            // ダイスをロールする
            int[] values = randomizer.RollBarabara((int)num_of_die + bonus, 6);
            // 成功数
            int result = values.Count(num => num >= difficulty);
            int failed_roll = (int)num_of_die - result;

            // ロール結果テキスト
            string rolled_text = "[" + string.Join(",", values) + "]";

            var reroll_values = new List<int>();

            if (reroll == 1)
            {
                reroll_values.Add(randomizer.RollOnce(6));
            }
            else if (reroll > 1)
            {
                reroll_values.AddRange(randomizer.RollBarabara(reroll, 6));
            }

            int reroll_result = reroll_values.Count(num => num >= difficulty);
            if (failed_roll < reroll_result)
            {
                reroll_result = failed_roll;
            }
            result += reroll_result;

            // リロール結果をテキストに追加
            if (reroll_values.Count > 0)
            {
                rolled_text += " Reroll [" + string.Join(",", reroll_values) + "]";
            }

            return Result.CreateBuilder($"({dice_command}).Build() > {rolled_text} > {result}成功")
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private Result? InitiativeRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+\.?\d*)?(INTI|inti)([+-](\d+))?(#(\d+))?");
            if (!m.Success)
            {
                return null;
            }

            var grace = m.Groups[1].Success ? Convert.ToDouble(m.Groups[1].Value) : 0.0;
            var bonus = m.Groups[4].Success ? Convert.ToInt32(m.Groups[4].Value) : 0;
            if (m.Groups[3].Success && m.Groups[3].Value.StartsWith("-"))
            {
                bonus = -bonus;
            }
            var reroll = m.Groups[6].Success ? Convert.ToInt32(m.Groups[6].Value) : 0;

            string dice_command;
            if (Regex.IsMatch(grace.ToString("G"), @"\.[1-9]+"))
            {
                dice_command = $"({grace}INTI{NumberWithSignFromInt(bonus)}{NumberWithRerollFromInt(reroll)})";
                reroll += 1;
            }
            else
            {
                dice_command = $"({(int)grace}INTI{NumberWithSignFromInt(bonus)}{NumberWithRerollFromInt(reroll)})";
            }

            int[] values = randomizer.RollBarabara((int)grace + bonus, 6);

            var revalue = new List<int>();
            if (reroll != 0)
            {
                revalue.AddRange(randomizer.RollBarabara(reroll, 6));
            }
            revalue = revalue.OrderBy(x => x).ToList();

            int result = 0;
            string res_text = "[";

            foreach (var value in values)
            {
                if (revalue.Count == 0)
                {
                    res_text += value.ToString();
                    result += value;
                }
                else
                {
                    bool is_min = false;
                    int index = -1;
                    foreach (var re in revalue)
                    {
                        index += 1;
                        if (re <= value)
                        {
                            continue;
                        }
                        // re > value: このリロールダイスが元の値より大きい
                        res_text += $"{value}<<{re}";
                        result += re;
                        revalue.RemoveAt(index);
                        is_min = true;
                        break;
                    }
                    if (!is_min)
                    {
                        res_text += value.ToString();
                        result += value;
                    }
                }

                res_text += ",";
            }

            // 末尾のカンマを削除
            if (res_text.Length > 1 && res_text.EndsWith(","))
            {
                res_text = res_text.Substring(0, res_text.Length - 1);
            }
            res_text += "]";

            return Result.CreateBuilder($"{dice_command} > {res_text} > {result}")
                .SetRands(randomizer.RandResults)
                .Build();
        }

    }
}
