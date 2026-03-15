using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Command;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ダークブレイズ
    /// </summary>
    public sealed class DarkBlaze : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly DarkBlaze Instance = new DarkBlaze();

        /// <inheritdoc/>
        public override string Id => "DarkBlaze";

        /// <inheritdoc/>
        public override string Name => "ダークブレイズ";

        /// <inheritdoc/>
        public override string SortKey => "たあくふれいす";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・行為判定　(DBxy#n)
        　行為判定専用のコマンドです。
        　""DB(能力)(技能)#(修正)""でロールします。Rコマンド(3R6+n[x,y]>=m mは難易度)に読替をします。
        　クリティカルとファンブルも自動で処理されます。
        　DB@x@y#m と DBx,y#m にも対応しました。
        　例）DB33　　　DB32#-1　　　DB@3@1#1　　　DB3,2　　　DB23#1>=4　　　3R6+1[3,3]>=4

        ・掘り出し袋表　(BTx)
        　""BT(ダイス数)""で掘り出し袋表を自動で振り、結果を表示します。
        　例）BT1　　　BT2　　　BT[1...3]
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^BT(\d)?$", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                var dice = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 1;
                return GetHoridasibukuroTable(dice, randomizer);
            }

            return CheckRoll(command, randomizer);
        }

        private string ReplaceText(string str)
        {
            if (!Regex.IsMatch(str, @"DB", RegexOptions.IgnoreCase))
            {
                return str;
            }
            str = Regex.Replace(str, @"DB(\d),(\d)", m => $"DB{m.Groups[1].Value}{m.Groups[2].Value}");
            str = Regex.Replace(str, @"DB@(\d)@(\d)", m => $"DB{m.Groups[1].Value}{m.Groups[2].Value}");
            str = Regex.Replace(str, @"DB(\d)(\d)(#(\d[+\-\d]*))", m => $"3R6+{m.Groups[4].Value}[{m.Groups[1].Value},{m.Groups[2].Value}]");
            str = Regex.Replace(str, @"DB(\d)(\d)(#([+\-\d]*))", m => $"3R6{m.Groups[4].Value}[{m.Groups[1].Value},{m.Groups[2].Value}]");
            str = Regex.Replace(str, @"DB(\d)(\d)", m => $"3R6[{m.Groups[1].Value},{m.Groups[2].Value}]");
            return str;
        }

        private Result? CheckRoll(string str, IRandomizer randomizer)
        {
            str = ReplaceText(str);
            var m = Regex.Match(str, @"(^|\s)S?(3[rR]6([+\-\d]+)?(\[(\d+),(\d+)\])(([>=]+)(\d+))?)(\s|$)", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var matchedStr = m.Groups[2].Value;
            var mod = 0;
            var abl = 1;
            var skl = 1;
            CompareOperator? cmpOp = null;
            var diff = 0;

            if (m.Groups[3].Success && m.Groups[3].Value.Length > 0)
            {
                mod = ArithmeticEvaluator.Eval(m.Groups[3].Value, RoundType) ?? 0;
            }
            if (m.Groups[4].Success && m.Groups[4].Value.Length > 0)
            {
                abl = int.Parse(m.Groups[5].Value);
                skl = int.Parse(m.Groups[6].Value);
            }
            if (m.Groups[7].Success && m.Groups[7].Value.Length > 0)
            {
                cmpOp = Normalize.ComparisonOperator(m.Groups[8].Value);
                diff = int.Parse(m.Groups[9].Value);
            }

            var (total, out_str) = GetDice(mod, abl, skl, randomizer);
            var output = $"({matchedStr}) ＞ {out_str}";

            if (cmpOp != null)
            {
                output += CompareResult(total, cmpOp, diff) ? " ＞ 成功" : " ＞ 失敗";
            }

            return Result.CreateBuilder(output).Build();
        }

        private (int total, string output) GetDice(int mod, int abl, int skl, IRandomizer randomizer)
        {
            var total = 0;
            var crit = 0;
            var fumble = 0;
            var dice_c = 3 + Math.Abs(mod);
            var dice_arr = randomizer.RollBarabara(dice_c, 6).OrderBy(x => x).ToList();
            var dice_str = string.Join(",", dice_arr);

            for (var i = 0; i < 3; i++)
            {
                var ch = dice_arr[i];
                if (mod < 0)
                {
                    ch = dice_arr[dice_c - i - 1];
                }
                if (ch <= abl)
                {
                    total += 1;
                }
                if (ch <= skl)
                {
                    total += 1;
                }
                if (ch <= 2)
                {
                    crit += 1;
                }
                if (ch >= 5)
                {
                    fumble += 1;
                }
            }

            var resultText = "";
            if (crit >= 3)
            {
                resultText = " ＞ クリティカル";
                total = 6 + skl;
            }
            if (fumble >= 3)
            {
                resultText = " ＞ ファンブル";
                total = 0;
            }

            var output = $"{total}[{dice_str}]{resultText}";
            return (total, output);
        }

        private Result? GetHoridasibukuroTable(int dice, IRandomizer randomizer)
        {
            var output = "1";
            var material_kind = new[] { "蟲甲", "金属", "金貨", "植物", "獣皮", "竜鱗", "レアモノ", "レアモノ" };
            var magic_stone = new[] { "火炎石", "雷撃石", "氷結石" };

            var num1 = randomizer.RollSum(2, 6);
            var num2 = randomizer.RollSum(dice, 6);

            if (num1 <= 4)
            {
                num2 = randomizer.RollOnce(6);
                var magic_stone_result = magic_stone[(int)(num2 / 2) - 1];
                output = $"《{magic_stone_result}》を{dice}個獲得";
            }
            else if (num1 == 7)
            {
                output = $"《金貨》を{num2}枚獲得";
            }
            else
            {
                var type = material_kind[num1 - 5];
                if (num2 <= 3)
                {
                    output = $"《{type} I》を1個獲得";
                }
                else if (num2 <= 5)
                {
                    output = $"《{type} I》を2個獲得";
                }
                else if (num2 <= 7)
                {
                    output = $"《{type} I》を3個獲得";
                }
                else if (num2 <= 9)
                {
                    output = $"《{type} II》を1個獲得";
                }
                else if (num2 <= 11)
                {
                    output = $"《{type} I》を2個《{type} II》を1個獲得";
                }
                else if (num2 <= 13)
                {
                    output = $"《{type} I》を2個《{type} II》を2個獲得";
                }
                else if (num2 <= 15)
                {
                    output = $"《{type} III》を1個獲得";
                }
                else if (num2 <= 17)
                {
                    output = $"《{type} II》を2個《{type} III》を1個獲得";
                }
                else
                {
                    output = $"《{type} II》を2個《{type} III》を2個獲得";
                }
            }

            if (output != "1")
            {
                output = $"掘り出し袋表[{num1},{num2}] ＞ {output}";
            }

            return Result.CreateBuilder(output).Build();
        }

        private static bool CompareResult(int left, CompareOperator? op, int right) => op switch
        {
            CompareOperator.GreaterThanOrEqual => left >= right,
            CompareOperator.LessThanOrEqual => left <= right,
            CompareOperator.GreaterThan => left > right,
            CompareOperator.LessThan => left < right,
            CompareOperator.Equal => left == right,
            CompareOperator.NotEqual => left != right,
            _ => throw new ArgumentException($"Unknown operator: {op}")
        };

    }
}
