using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// トワイライトガンスモーク
    /// </summary>
    public sealed class TwilightGunsmoke : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly TwilightGunsmoke Instance = new TwilightGunsmoke();

        /// <inheritdoc/>
        public override string Id => "TwilightGunsmoke";

        /// <inheritdoc/>
        public override string Name => "トワイライトガンスモーク";

        /// <inheritdoc/>
        public override string SortKey => "とわいらいとかんすもおく";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.NoSort;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定
        　・通常判定　　　　　　2D6+m>=t[c,f]
        　　修正値m,目標値t,クリティカル値c,ファンブル値fで判定ロールを行います。
        　　クリティカル値、ファンブル値は省略可能です。([]ごと省略できます)
        　　自動成功、自動失敗、成功、失敗を自動表示します。
        ・各種表
        　・邂逅表　　CT
        　・オープニングチャート
        　　リアリスティック　OPR｜シネマティック　OPC
        　・エンディングチャート
        　　リアリスティック　EDR｜シネマティック　EDC
        　・情報収集チャート
        　　荒野　RWL｜ウェブ　RWB｜ストリート　RST｜上流　RUP
        　・ドロップチャート
        　　コーポレイト　DCP｜バンデッド　DBD｜クリミナル　DCR｜ニンジャ　DNJ
        　　ロボ　DRB｜武装車輛　DBS｜ターレット　DTR｜メルカバ　DMK
        　　ヘリ　DHL｜マシンライフ　DML｜ゾンビ　DZB｜ミュータント　DMT
        　　BM／飛竜科　DHR｜BM／巨爪科　DKS｜フィーンド　DFD
        ・D66ダイスあり
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var ret = CheckRoll(command, randomizer);
            if (ret != null)
            {
                return ret;
            }
            return null;
        }

        private Result? CheckRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^2D6([+\-\d]*)>=(\d+)(\[(\d+)?(,(\d+))?\])?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            int modify_number = m.Groups[1].Value.Length > 0
                ? (Arithmetic.ArithmeticEvaluator.Eval(m.Groups[1].Value, RoundType.Floor) ?? 0)
                : 0;
            int target = Convert.ToInt32(m.Groups[2].Value);
            int critical = m.Groups[4].Success && m.Groups[4].Value.Length > 0
                ? Convert.ToInt32(m.Groups[4].Value)
                : 12;
            int fumble = m.Groups[6].Success && m.Groups[6].Value.Length > 0
                ? Convert.ToInt32(m.Groups[6].Value)
                : 2;

            int[] dice_array = randomizer.RollBarabara(2, 6);
            var dice_list = dice_array.OrderBy(x => x).ToList();
            int dice_value = dice_list.Sum();
            string dice_str = string.Join(",", dice_list);

            int total = dice_value + modify_number;

            string result;
            if (dice_value >= critical)
            {
                result = "自動成功";
            }
            else if (dice_value <= fumble)
            {
                result = "自動失敗";
            }
            else
            {
                result = total >= target ? "成功" : "失敗";
            }

            string modifierStr = modify_number > 0 ? $"+{modify_number}" : (modify_number < 0 ? modify_number.ToString() : "");
            var sequence = new[] { $"({command})", $"{dice_value}[{dice_str}]{modifierStr}", total.ToString(), result };
            string text = string.Join(" ＞ ", sequence);

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .Build();
        }
    }
}
