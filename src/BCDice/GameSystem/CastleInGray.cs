using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 灰色城綺譚
    /// </summary>
    public sealed class CastleInGray : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly CastleInGray Instance = new CastleInGray();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "CastleInGray";

        /// <inheritdoc/>
        public override string Name => "灰色城綺譚";

        /// <inheritdoc/>
        public override string SortKey => "はいいろしようきたん";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ 色占い (BnWm)
        n: 黒
        m: 白
        n, m は1～12の異なる整数

        例) B12W7
        例) B5W12

        ■ 悪意の渦による占い (MALn)
        n: 悪意の渦
        n は1～12の整数

        ■ その他
        ・感情表 ET
        ・暗示表(黒) BIT
        ・暗示表(白) WIT
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollColor(command, randomizer) ?? RollMal(command, randomizer) ?? RollTables(command, TABLES);
        }

        private Result? RollColor(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^B(\d{1,2})W(\d{1,2})$");
            if (!m.Success)
            {
                return null;
            }

            var black = Convert.ToInt32(m.Groups[1].Value);
            var white = Convert.ToInt32(m.Groups[2].Value);
            if (!(black >= 1 && black <= 12 && white >= 1 && white <= 12))
            {
                return null;
            }

            var value = randomizer.RollOnce(12);
            string resultText;
            if (black == white)
            {
                resultText = ColorText(black, white, value, "白と黒は重ねられません");
            }
            else if (white > black)
            {
                resultText = ColorText(black, white, value, black <= value && value < white ? "黒" : "白");
            }
            else
            {
                resultText = ColorText(black, white, value, white <= value && value < black ? "白" : "黒");
            }

            return Result.CreateBuilder(resultText)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private string ColorText(object black, object white, object value, object result)
        {
            return $"色占い(黒{black}白{white}) ＞ [{value}] ＞ {result}";
        }

        private Result? RollMal(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^MAL(\d{1,2})$");
            if (!m.Success)
            {
                return null;
            }

            var mal = Convert.ToInt32(m.Groups[1].Value);
            if (!(mal >= 1 && mal <= 12))
            {
                return null;
            }

            var value = randomizer.RollOnce(12);
            var result = value <= mal ? "黒" : "白";
            string text = $"悪意の渦({mal}) ＞ [{value}] ＞ {result}";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .Build();
        }

    }
}
