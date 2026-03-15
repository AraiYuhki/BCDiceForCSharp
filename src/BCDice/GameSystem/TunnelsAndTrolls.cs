using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// トンネルズ＆トロールズ
    /// </summary>
    public sealed class TunnelsAndTrolls : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly TunnelsAndTrolls Instance = new TunnelsAndTrolls();

        /// <inheritdoc/>
        public override string Id => "TunnelsAndTrolls";

        /// <inheritdoc/>
        public override string Name => "トンネルズ＆トロールズ";

        /// <inheritdoc/>
        public override string SortKey => "とんねるすあんととろおるす";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・行為判定　(nD6+x>=nLV)
        失敗、成功、自動失敗の自動判定とゾロ目の振り足し経験値の自動計算を行います。
        SAVEの難易度を「レベル」で表記することが出来ます。
        例えば「2Lv」と書くと「25」に置換されます。
        判定時以外は悪意ダメージを表示します。
        バーサークとハイパーバーサーク用に専用コマンドが使えます。
        例）2D6+1>=1Lv
        　 (2D6+1>=20) ＞ 7[2,5]+1 ＞ 8 ＞ 失敗
        　判定時にはゾロ目を自動で振り足します。

        ・バーサークとハイパーバーサーク　(nBS+x or nHBS+x)
        　""(ダイス数)BS(修正値)""でバーサーク、""(ダイス数)HBS(修正値)""でハイパーバーサークでロールできます。
        　最初のダイスの読替は、個別の出目はそのままで表示。
        　下から２番目の出目をずらした分だけ合計にマイナス修正を追加して表示します。
        ";

        // Instance fields for roll_action_dice
        private List<int[]> _diceList;
        private int _diceTotal;
        private int _count6;

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            if (Regex.IsMatch(command, @"^\d+D\d+", RegexOptions.IgnoreCase))
            {
                return RollAction(command, randomizer);
            }

            var str = ReplaceText(command);

            var output = "1";

            var m = Regex.Match(str, @"(^|\s)S?((\d+)[rR]6([+\-\d]*)(\[(\w+)\])?)(\s|$)", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            str = m.Groups[2].Value;
            var dice_c = int.Parse(m.Groups[3].Value);
            var bonus = 0;
            if (m.Groups[4].Success && m.Groups[4].Value.Length > 0)
            {
                bonus = ArithmeticEvaluator.Eval(m.Groups[4].Value, RoundType.Floor) ?? 0;
            }
            var isHyperBerserk = false;
            if (m.Groups[5].Success && Regex.IsMatch(m.Groups[6].Value, @"[Hh]"))
            {
                isHyperBerserk = true;
            }
            var dice_arr = new List<int>();
            var dice_now = 0;
            var dice_str = "";
            var isFirstLoop = true;
            var n_max = 0;
            var bonus2 = 0;

            dice_arr.Add(dice_c);

            while (true)
            {
                var dice_wk = dice_arr[0];
                dice_arr.RemoveAt(0);

                var dice_list = randomizer.RollBarabara(dice_wk, 6).OrderBy(x => x).ToArray();
                var rollTotal = dice_list.Sum();
                var rollDiceMaxCount = dice_list.Count(x => x == 6);

                if (dice_wk >= 2)
                {
                    var dice_num = dice_list;
                    var diceType = 6;

                    var dice_face = new int[diceType];
                    for (var _i = 0; _i < diceType; _i++)
                    {
                        dice_face[_i] = 0;
                    }

                    foreach (var dice_o in dice_num)
                    {
                        dice_face[dice_o - 1] += 1;
                    }

                    foreach (var dice_o in dice_face)
                    {
                        var d = dice_o;
                        if (d >= 2)
                        {
                            if (isHyperBerserk)
                            {
                                d += 1;
                            }
                            dice_arr.Add(d);
                        }
                    }

                    if (isFirstLoop && dice_arr.Count == 0)
                    {
                        var min1 = 0;
                        var min2 = 0;
                        for (var i = 0; i < diceType; i++)
                        {
                            var index = diceType - i - 1;
                            if (dice_face[index] > 0)
                            {
                                min2 = min1;
                                min1 = index;
                            }
                        }

                        bonus2 = -(min2 - min1);
                        if (min2 == 5)
                        {
                            rollDiceMaxCount -= 1;
                        }

                        if (isHyperBerserk)
                        {
                            dice_arr.Add(3);
                        }
                        else
                        {
                            dice_arr.Add(2);
                        }
                    }
                }

                dice_now += rollTotal;
                if (dice_str != "")
                {
                    dice_str += "][";
                }
                dice_str += string.Join(",", dice_list);
                n_max += rollDiceMaxCount;
                isFirstLoop = false;

                if (dice_arr.Count == 0)
                {
                    break;
                }
            }

            var total_n = dice_now + bonus + bonus2;

            dice_str = $"[{dice_str}]";
            output = $"{dice_now}{dice_str}";

            if (bonus2 < 0)
            {
                output += bonus2.ToString();
            }

            if (bonus > 0)
            {
                output += $"+{bonus}";
            }
            else if (bonus < 0)
            {
                output += bonus.ToString();
            }

            if (Regex.IsMatch(output, @"[^\d\[\]]+"))
            {
                output = $"({str}) ＞ {output} ＞ {total_n}";
            }
            else
            {
                output = $"({str}) ＞ {total_n}";
            }

            if (n_max > 0)
            {
                output += $" ＞ 悪意{n_max}";
            }

            return Result.CreateBuilder(output).Build();
        }

        private string ReplaceText(string str)
        {
            if (Regex.IsMatch(str, @"BS", RegexOptions.IgnoreCase))
            {
                str = Regex.Replace(str, @"(\d+)HBS([^\d\s][+\-\d]+)", m => $"{m.Groups[1].Value}R6{m.Groups[2].Value}[H]", RegexOptions.IgnoreCase);
                str = Regex.Replace(str, @"(\d+)HBS", m => $"{m.Groups[1].Value}R6[H]", RegexOptions.IgnoreCase);
                str = Regex.Replace(str, @"(\d+)BS([^\d\s][+\-\d]+)", m => $"{m.Groups[1].Value}R6{m.Groups[2].Value}", RegexOptions.IgnoreCase);
                str = Regex.Replace(str, @"(\d+)BS", m => $"{m.Groups[1].Value}R6", RegexOptions.IgnoreCase);
            }
            return str;
        }

        private Result? RollAction(string command, IRandomizer randomizer)
        {
            command = Regex.Replace(command, @"\d+LV", m => (int.Parse(Regex.Match(m.Value, @"\d+").Value) * 5 + 15).ToString(), RegexOptions.IgnoreCase);

            // Simple parsing: match nD6+modifier>=target or nD6+modifier>=?
            var cmdMatch = Regex.Match(command, @"^(\d+)D6([+\-]\d+)?(?:>=([\d?]+))?$", RegexOptions.IgnoreCase);
            if (!cmdMatch.Success)
            {
                return null;
            }

            var times = int.Parse(cmdMatch.Groups[1].Value);
            var modifyNumber = cmdMatch.Groups[2].Success ? int.Parse(cmdMatch.Groups[2].Value) : 0;
            string? targetStr = cmdMatch.Groups[3].Success ? cmdMatch.Groups[3].Value : null;

            RollActionDice(times, randomizer);
            var total = _diceTotal + modifyNumber;

            string? target = null;
            bool isQuestionTarget = false;
            int targetNumber = 0;
            if (targetStr == "?")
            {
                isQuestionTarget = true;
                target = "?";
            }
            else if (targetStr != null)
            {
                targetNumber = int.Parse(targetStr);
                target = targetStr;
            }

            var sequence = new List<string>();
            sequence.Add($"({command})");

            var interimStr = InterimExpr(modifyNumber, _diceTotal);
            if (interimStr != null)
            {
                sequence.Add(interimStr);
            }

            sequence.Add(total.ToString());

            var actionResultStr = ActionResult(total, _diceTotal, target, isQuestionTarget, targetStr != null ? (int?)targetNumber : null);
            if (actionResultStr != null)
            {
                sequence.Add(actionResultStr);
            }

            var additionalStr = AdditionalResult(_count6);
            if (additionalStr != null)
            {
                sequence.Add(additionalStr);
            }

            return Result.CreateBuilder(string.Join(" ＞ ", sequence)).Build();
        }

        private void RollActionDice(int times, IRandomizer randomizer)
        {
            var dice_list = randomizer.RollBarabara(times, 6).OrderBy(x => x).ToArray();
            _diceList = new List<int[]> { dice_list };
            while (SameAllDice(dice_list))
            {
                dice_list = randomizer.RollBarabara(times, 6).OrderBy(x => x).ToArray();
                _diceList.Add(dice_list);
            }

            var dice_list_flatten = _diceList.SelectMany(x => x).ToArray();
            _diceTotal = dice_list_flatten.Sum();
            _count6 = dice_list_flatten.Count(x => x == 6);
        }

        private bool SameAllDice(int[] dice_list)
        {
            return dice_list.Length > 1 && dice_list.Distinct().Count() == 1;
        }

        private string? InterimExpr(int modifyNumber, int dice_total)
        {
            if (_diceList.SelectMany(x => x).Count() == 1 && modifyNumber == 0)
            {
                return null;
            }

            var diceListStr = string.Join("", _diceList.Select(ds => $"[{string.Join(",", ds)}]"));
            var modifier = Format.Modifier(modifyNumber);
            return $"{dice_total}{diceListStr}{modifier}";
        }

        private string? ActionResult(int total, int dice_total, string? target, bool isQuestionTarget, int? targetNumber)
        {
            if (dice_total == 3)
            {
                return "自動失敗";
            }
            else if (target == null)
            {
                return null;
            }
            else if (isQuestionTarget)
            {
                return SuccessLevel(total, dice_total);
            }
            else if (targetNumber.HasValue && total >= targetNumber.Value)
            {
                return $"成功 ＞ 経験値{ExperiencePoint(targetNumber.Value, dice_total)}";
            }
            else
            {
                return "失敗";
            }
        }

        private string SuccessLevel(int total, int dice_total)
        {
            var level = (total - 15) / 5;
            if (level <= 0)
            {
                return $"失敗 ＞ 経験値{dice_total}";
            }
            else
            {
                return $"{level}Lv成功 ＞ 経験値{dice_total}";
            }
        }

        private string ExperiencePoint(int target_number, int dice_total)
        {
            var ep = 1.0 * (target_number - 15) / 5 * dice_total;
            if (ep <= 0)
            {
                return "0";
            }
            else if (ep == (int)ep)
            {
                return ((int)ep).ToString();
            }
            else
            {
                return ep.ToString("F1");
            }
        }

        private string? AdditionalResult(int count_6)
        {
            if (count_6 > 0)
            {
                return $"悪意{count_6}";
            }
            return null;
        }

    }
}
