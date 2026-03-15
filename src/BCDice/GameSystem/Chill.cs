using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// Chill
    /// </summary>
    public sealed class Chill : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Chill Instance = new Chill();

        /// <inheritdoc/>
        public override string Id => "Chill";

        /// <inheritdoc/>
        public override string Name => "Chill";

        /// <inheritdoc/>
        public override string SortKey => "ちる";

        /// <inheritdoc/>
        public override string HelpMessage => @"
・ストライク・ランク　(SRx)
　""SRストライク・ランク""の形で記入します。
　ストライク・ランク・チャートに従って自動でダイスロールを行い、
　負傷とスタミナロスを計算します。
　ダイスロールと同様に、他のプレイヤーに隠れてロールすることも可能です。
　例）SR7　　　sr13　　　SR(7+4)　　　Ssr10
";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollStrikeRankResult(command, randomizer);
        }

        /// <summary>
        /// 1d100の判定結果を返す
        /// </summary>
        private Result? Result1d100(int total, int _diceTotal, string cmpOp, int target)
        {
            if (target.ToString() == "?")
            {
                return null;
            }
            if (cmpOp != "<=")
            {
                return null;
            }

            if (total >= 100)
            {
                return Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
            }
            else if (total > target)
            {
                return Result.CreateBuilder("失敗").SetFailure(true).Build();
            }
            else if (total >= (int)(target * 0.9))
            {
                return Result.CreateBuilder("Ｌ成功").SetSuccess(true).Build();
            }
            else if (total >= target / 2)
            {
                return Result.CreateBuilder("Ｍ成功").SetSuccess(true).Build();
            }
            else if (total >= target / 10)
            {
                return Result.CreateBuilder("Ｈ成功").SetSuccess(true).Build();
            }
            else
            {
                return Result.CreateBuilder("Ｃ成功").SetSuccess(true).SetCritical(true).Build();
            }
        }

        private Result? RollStrikeRankResult(string str, IRandomizer randomizer)
        {
            var wounds = 0;
            var staLoss = 0;
            var dice = "";
            var diceAdd = "";
            var diceStr = "";

            var m = Regex.Match(str, @"(^|\s)[sS]?(SR|sr)(\d+)($|\s)");
            if (!m.Success)
            {
                return null;
            }

            var strikeRank = int.Parse(m.Groups[3].Value);

            if (strikeRank < 14)
            {
                var (sl, d, da, ds) = CheckStrikeRank(strikeRank, randomizer);
                staLoss = sl;
                dice = d;
                diceAdd = da;
                diceStr = ds;

                var (w, dw, dwa, dws) = CheckStrikeRank(strikeRank - 3, randomizer);
                wounds = w;
                dice = dice + ", " + dw;
                diceAdd += ", " + dwa;
                diceStr = diceStr + ", " + dws;
            }
            else
            {
                var (sl, _d, da, ds) = CheckStrikeRank(13, randomizer);
                staLoss = sl;
                diceAdd = da;
                diceStr = ds;

                var diceList = randomizer.RollBarabara(4, 10);
                wounds = diceList.Sum();
                var diceWs = string.Join(",", diceList);

                dice = "5d10*3, 4d10+" + ((strikeRank - 13) * 2).ToString() + "d10";
                diceAdd += ", " + wounds.ToString();
                diceStr = $"{diceStr}, {diceWs}";

                var diceList2 = randomizer.RollBarabara((strikeRank - 13) * 2, 10);
                var woundsWk = diceList2.Sum();
                var diceWs2 = string.Join(",", diceList2);

                diceStr += $"+{diceWs2}";
                diceAdd += $"+{woundsWk}";
                wounds += woundsWk;
            }

            var output = $"{diceStr} ＞ {diceAdd} ＞ スタミナ損失{staLoss}, 負傷{wounds}";

            str += ":" + dice;

            if (string.IsNullOrEmpty(output))
            {
                return null;
            }

            output = $"({str}) ＞ {output}";

            return Result.CreateBuilder(output).Build();
        }

        private (int damage, string dice, string diceAdd, string diceStr) CheckStrikeRank(int strikeRank, IRandomizer randomizer)
        {
            int[] diceList = null;
            int total = 0;
            var dice = "";
            var diceAdd = "";
            var diceStr = "";
            var damage = 0;

            if (strikeRank < 1)
            {
                damage = 0;
                diceStr = "-";
                diceAdd = "-";
                dice = "-";
            }
            else if (strikeRank < 2)
            {
                dice = "0or1";
                damage = randomizer.RollOnce(2);
                diceStr = damage.ToString();
                damage -= 1;
                diceAdd = damage.ToString();
            }
            else if (strikeRank < 3)
            {
                dice = "1or2";
                damage = randomizer.RollOnce(2);
                diceStr = damage.ToString();
                diceAdd = damage.ToString();
            }
            else if (strikeRank < 4)
            {
                dice = "1d5";
                damage = randomizer.RollOnce(5);
                diceStr = damage.ToString();
                diceAdd = damage.ToString();
            }
            else if (strikeRank < 10)
            {
                dice = (strikeRank - 3).ToString() + "d10";
                diceList = randomizer.RollBarabara(strikeRank - 3, 10);
                damage = diceList.Sum();
                diceAdd = damage.ToString();
                diceStr = string.Join(",", diceList);
            }
            else if (strikeRank < 13)
            {
                dice = (strikeRank - 6).ToString() + "d10*2";
                diceList = randomizer.RollBarabara(strikeRank - 6, 10);
                total = diceList.Sum();
                diceAdd = $"{total}*2";
                damage = total * 2;
                diceStr = $"({string.Join(",", diceList)})*2";
            }
            else
            {
                dice = "5d10*3";
                diceList = randomizer.RollBarabara(5, 10);
                total = diceList.Sum();
                diceAdd = $"{total}*3";
                damage = total * 3;
                diceStr = $"({string.Join(",", diceList)})*3";
            }

            return (damage, dice, diceAdd, diceStr);
        }
    }
}
