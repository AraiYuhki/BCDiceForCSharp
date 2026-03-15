using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Arithmetic;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 六門世界RPG セカンドエディション
    /// </summary>
    public sealed class RokumonSekai2 : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly RokumonSekai2 Instance = new RokumonSekai2();

        /// <inheritdoc/>
        public override string Id => "RokumonSekai2";

        /// <inheritdoc/>
        public override string Name => "六門世界RPG セカンドエディション";

        /// <inheritdoc/>
        public override string SortKey => "ろくもんせかいRPG2";

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定
        aRSm<=t
        能力値a,修正値m,目標値tで判定ロールを行います。
        Rコマンド(3R6m<=t[a])に読み替えます。
        成功度、評価、ボーナスダイスを自動表示します。
        　例) 3RS+1<=9　3R6+1<=9[3]
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var str = ReplaceText(command);
            var m = Regex.Match(str, @"3R6([+\-\d]*)<=(\d+)\[(\d+)\]", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var modText = m.Groups[1].Value;
            var target = int.Parse(m.Groups[2].Value);
            var abl = int.Parse(m.Groups[3].Value);

            int mod = 0;
            if (!string.IsNullOrEmpty(modText))
            {
                mod = ArithmeticEvaluator.Eval(modText, RoundType) ?? 0;
            }

            var (dstr, suc, sum) = Rokumon2Roll(mod, target, abl, randomizer);
            var output = $"{sum}[{dstr}] ＞ {suc} ＞ 評価{Rokumon2SucRank(suc)}";

            if (suc != 0)
            {
                output += $"(+{suc}d6)";
            }

            output = $"({str}) ＞ {output}";

            return Result.CreateBuilder(output).Build();
        }

        private string ReplaceText(string str)
        {
            str = Regex.Replace(str, @"(\d+)RS([+-][+\-\d]+)<=(\d+)", m => $"3R6{m.Groups[2].Value}<={m.Groups[3].Value}[{m.Groups[1].Value}]", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"(\d+)RS<=(\d+)", m => $"3R6<={m.Groups[2].Value}[{m.Groups[1].Value}]", RegexOptions.IgnoreCase);
            return str;
        }

        private (string dicestr, int suc, int sum) Rokumon2Roll(int mod, int target, int abl, IRandomizer randomizer)
        {
            var suc = 0;

            var dice = randomizer.RollBarabara(3 + Math.Abs(mod), 6).OrderBy(x => x).ToList();
            var dicestr = string.Join(",", dice);

            for (var i = 0; i < Math.Abs(mod); i++)
            {
                if (mod < 0)
                {
                    dice.RemoveAt(0);
                }
                else
                {
                    dice.RemoveAt(dice.Count - 1);
                }
            }

            var cnt5 = 0;
            var cnt2 = 0;
            var sum = 0;

            foreach (var die1 in dice)
            {
                if (die1 >= 5)
                {
                    cnt5 += 1;
                }
                if (die1 <= 2)
                {
                    cnt2 += 1;
                }
                if (die1 <= abl)
                {
                    suc += 1;
                }
                sum += die1;
            }

            if (sum < target)
            {
                suc += 2;
            }
            else if (sum == target)
            {
                suc += 1;
            }

            if (cnt5 >= 3)
            {
                suc = 0;
            }
            if (cnt2 >= 3)
            {
                suc = 5;
            }

            return (dicestr, suc, sum);
        }

        private string Rokumon2SucRank(int suc)
        {
            var suc_rank = new[] { "E", "D", "C", "B", "A", "S" };
            if (suc >= 0 && suc < suc_rank.Length)
            {
                return suc_rank[suc];
            }
            return "S";
        }

    }
}
