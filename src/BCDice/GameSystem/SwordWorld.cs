using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ソード・ワールドRPG
    /// </summary>
    public sealed class SwordWorld : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly SwordWorld Instance = new SwordWorld();

        /// <summary>
        /// レーティング表の種類 (0=通常, 1=完全版)
        /// </summary>
        private int _ratingTable = 0;

        /// <inheritdoc/>
        public override string Id => "SwordWorld";

        /// <inheritdoc/>
        public override string Name => "ソード・ワールドRPG";

        /// <inheritdoc/>
        public override string SortKey => "そおとわあると";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・SW　レーティング表　(Kx[c]+m$f) (x:キー, c:クリティカル値, m:ボーナス, f:出目修正)
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return Rating(command, randomizer);
        }

        /// <summary>
        /// 2D6の判定結果を返す
        /// </summary>
        protected Result? EvalResult2D6(int total, int diceTotal, IReadOnlyList<int> diceList, CompareOperator cmpOp, int? target, IRandomizer randomizer)
        {
            if (diceTotal >= 12)
            {
                return Result.CreateBuilder("クリティカル").SetSuccess(true).SetCritical(true).Build();
            }
            else if (diceTotal <= 2)
            {
                return Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
            }
            else if (cmpOp != CompareOperator.GreaterThanOrEqual || target == null)
            {
                return null;
            }
            else if (total >= target.Value)
            {
                return Result.CreateBuilder("成功").SetSuccess(true).Build();
            }
            else
            {
                return Result.CreateBuilder("失敗").SetFailure(true).Build();
            }
        }

        /// <summary>
        /// レーティング表処理
        /// </summary>
        /// <remarks>
        /// TODO: RatingParser が未実装のため、レーティングコマンドのパースは未対応。
        /// RatingParser 実装後に完成させる。
        /// </remarks>
        private Result? Rating(string command, IRandomizer randomizer)
        {
            // TODO: RatingParser is not yet implemented.
            // Once RatingParser is available, this method should parse the command
            // and perform the full rating table lookup logic as in the Ruby version.
            // For now, return null to indicate this command is not handled.
            return null;
        }

        /// <summary>
        /// SW2.0 レーティング表データを取得する
        /// </summary>
        internal string[] GetSW20RatingTable()
        {
            var rate_sw2_0 = new[] {
                "*,0,0,0,1,2,2,3,3,4,4", "*,0,0,0,1,2,3,3,3,4,4", "*,0,0,0,1,2,3,4,4,4,4",
                "*,0,0,1,1,2,3,4,4,4,5", "*,0,0,1,2,2,3,4,4,5,5", "*,0,1,1,2,2,3,4,5,5,5",
                "*,0,1,1,2,3,3,4,5,5,5", "*,0,1,1,2,3,4,4,5,5,6", "*,0,1,2,2,3,4,4,5,6,6",
                "*,0,1,2,3,3,4,4,5,6,7", "*,1,1,2,3,3,4,5,5,6,7", "*,1,2,2,3,3,4,5,6,6,7",
                "*,1,2,2,3,4,4,5,6,6,7", "*,1,2,3,3,4,4,5,6,7,7", "*,1,2,3,4,4,4,5,6,7,8",
                "*,1,2,3,4,4,5,5,6,7,8", "*,1,2,3,4,4,5,6,7,7,8", "*,1,2,3,4,5,5,6,7,7,8",
                "*,1,2,3,4,5,6,6,7,7,8", "*,1,2,3,4,5,6,7,7,8,9", "*,1,2,3,4,5,6,7,8,9,10",
                "*,1,2,3,4,6,6,7,8,9,10", "*,1,2,3,5,6,6,7,8,9,10", "*,2,2,3,5,6,7,7,8,9,10",
                "*,2,3,4,5,6,7,7,8,9,10", "*,2,3,4,5,6,7,8,8,9,10", "*,2,3,4,5,6,8,8,9,9,10",
                "*,2,3,4,6,6,8,8,9,9,10", "*,2,3,4,6,6,8,9,9,10,10", "*,2,3,4,6,7,8,9,9,10,10",
                "*,2,4,4,6,7,8,9,10,10,10", "*,2,4,5,6,7,8,9,10,10,11", "*,3,4,5,6,7,8,10,10,10,11",
                "*,3,4,5,6,8,8,10,10,10,11", "*,3,4,5,6,8,9,10,10,11,11", "*,3,4,5,7,8,9,10,10,11,12",
                "*,3,5,5,7,8,9,10,11,11,12", "*,3,5,6,7,8,9,10,11,12,12", "*,3,5,6,7,8,10,10,11,12,13",
                "*,4,5,6,7,8,10,11,11,12,13", "*,4,5,6,7,9,10,11,11,12,13", "*,4,6,6,7,9,10,11,12,12,13",
                "*,4,6,7,7,9,10,11,12,13,13", "*,4,6,7,8,9,10,11,12,13,14", "*,4,6,7,8,10,10,11,12,13,14",
                "*,4,6,7,9,10,10,11,12,13,14", "*,4,6,7,9,10,10,12,13,13,14", "*,4,6,7,9,10,11,12,13,13,15",
                "*,4,6,7,9,10,12,12,13,13,15", "*,4,6,7,10,10,12,12,13,14,15", "*,4,6,8,10,10,12,12,13,15,15",
                "*,5,7,8,10,10,12,12,13,15,15", "*,5,7,8,10,11,12,12,13,15,15", "*,5,7,9,10,11,12,12,14,15,15",
                "*,5,7,9,10,11,12,13,14,15,16", "*,5,7,10,10,11,12,13,14,16,16", "*,5,8,10,10,11,12,13,15,16,16",
                "*,5,8,10,11,11,12,13,15,16,17", "*,5,8,10,11,12,12,13,15,16,17", "*,5,9,10,11,12,12,14,15,16,17",
                "*,5,9,10,11,12,13,14,15,16,18", "*,5,9,10,11,12,13,14,16,17,18", "*,5,9,10,11,13,13,14,16,17,18",
                "*,5,9,10,11,13,13,15,17,17,18", "*,5,9,10,11,13,14,15,17,17,18", "*,5,9,10,12,13,14,15,17,18,18",
                "*,5,9,10,12,13,15,15,17,18,19", "*,5,9,10,12,13,15,16,17,19,19", "*,5,9,10,12,14,15,16,17,19,19",
                "*,5,9,10,12,14,16,16,17,19,19", "*,5,9,10,12,14,16,17,18,19,19", "*,5,9,10,13,14,16,17,18,19,20",
                "*,5,9,10,13,15,16,17,18,19,20", "*,5,9,10,13,15,16,17,19,20,21", "*,6,9,10,13,15,16,18,19,20,21",
                "*,6,9,10,13,16,16,18,19,20,21", "*,6,9,10,13,16,17,18,19,20,21", "*,6,9,10,13,16,17,18,20,21,22",
                "*,6,9,10,13,16,17,19,20,22,23", "*,6,9,10,13,16,18,19,20,22,23", "*,6,9,10,13,16,18,20,21,22,23",
                "*,6,9,10,13,17,18,20,21,22,23", "*,6,9,10,14,17,18,20,21,22,24", "*,6,9,11,14,17,18,20,21,23,24",
                "*,6,9,11,14,17,19,20,21,23,24", "*,6,9,11,14,17,19,21,22,23,24", "*,7,10,11,14,17,19,21,22,23,25",
                "*,7,10,12,14,17,19,21,22,24,25", "*,7,10,12,14,18,19,21,22,24,25", "*,7,10,12,15,18,19,21,22,24,26",
                "*,7,10,12,15,18,19,21,23,25,26", "*,7,11,13,15,18,19,21,23,25,26", "*,7,11,13,15,18,20,21,23,25,27",
                "*,8,11,13,15,18,20,22,23,25,27", "*,8,11,13,16,18,20,22,23,25,28", "*,8,11,14,16,18,20,22,23,26,28",
                "*,8,11,14,16,19,20,22,23,26,28", "*,8,12,14,16,19,20,22,24,26,28", "*,8,12,15,16,19,20,22,24,27,28",
                "*,8,12,15,17,19,20,22,24,27,29", "*,8,12,15,18,19,20,22,24,27,30"
            };
            return rate_sw2_0;
        }

        /// <summary>
        /// レーティング表をパースして2次元配列に変換する
        /// </summary>
        internal List<int>[] GetNewRates(string[] rate_sw2_0)
        {
            var rate_3 = new List<int>();
            var rate_4 = new List<int>();
            var rate_5 = new List<int>();
            var rate_6 = new List<int>();
            var rate_7 = new List<int>();
            var rate_8 = new List<int>();
            var rate_9 = new List<int>();
            var rate_10 = new List<int>();
            var rate_11 = new List<int>();
            var rate_12 = new List<int>();
            var zeroArray = new List<int>();

            foreach (var rateText in rate_sw2_0)
            {
                var rate_arr = rateText.Split(',');
                zeroArray.Add(0);
                rate_3.Add(int.Parse(rate_arr[1]));
                rate_4.Add(int.Parse(rate_arr[2]));
                rate_5.Add(int.Parse(rate_arr[3]));
                rate_6.Add(int.Parse(rate_arr[4]));
                rate_7.Add(int.Parse(rate_arr[5]));
                rate_8.Add(int.Parse(rate_arr[6]));
                rate_9.Add(int.Parse(rate_arr[7]));
                rate_10.Add(int.Parse(rate_arr[8]));
                rate_11.Add(int.Parse(rate_arr[9]));
                rate_12.Add(int.Parse(rate_arr[10]));
            }

            if (_ratingTable == 1)
            {
                rate_12[31] = rate_12[32] = rate_12[33] = 10;
            }

            var newRates = new List<int>[] {
                zeroArray, zeroArray, zeroArray, rate_3, rate_4, rate_5,
                rate_6, rate_7, rate_8, rate_9, rate_10, rate_11, rate_12
            };

            return newRates;
        }

        /// <summary>
        /// ダイスロール (2D6)
        /// </summary>
        private (int total, string diceText) RollDice(IRandomizer randomizer)
        {
            var dice_list = randomizer.RollBarabara(2, 6);
            var total = dice_list.Sum();
            var dice_str = string.Join(",", dice_list);
            return (total, dice_str);
        }

    }
}
