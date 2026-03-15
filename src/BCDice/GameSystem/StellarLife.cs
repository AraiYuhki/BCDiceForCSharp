using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ステラーライフTRPG
    /// </summary>
    public sealed class StellarLife : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly StellarLife Instance = new StellarLife();

        private static readonly Regex DdaDRegex = new Regex(
            @"^(\d+)?DA([\d+*-]*\d)?\[(\d+),(\d+)(,(\d+))?\]$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "StellarLife";

        /// <inheritdoc/>
        public override string Name => "ステラーライフTRPG";

        /// <inheritdoc/>
        public override string SortKey => "すてらあらいふTRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ◆判定　nDAc[s,d,t]　n:ダイス数　c:各種修正　s:1成功（省略不可）　d:2成功（省略不可）　t:3成功（ダイス目一致時のみ　省略時:無し）
        　例）2DA-3[7,10]
        ◆船名ランダム表
        　・船名接頭辞表　VPFT
        　・船名前半表　VNFT
        　・船名後半表　VNRT
        　・アバターアルファベット表①　AAFT
        　・アバターアルファベット表②　AAST
        ◆ヴォヤージュ各種表
        　・ランダムNPC艦表　RNST
        　・ランダムイベント表　RET
        　・お宝特徴表　TRST
        　・お宝形容表　TRAT
        　・お宝外見表　TRMT
        　・お宝物品表　TROT
        ◆ステラーライフお題表
        　・超未来の技術　TET
        　・超未来のエンタメ　ENT
        　・超未来の文化　CUT
        　・超未来の自然　NAT
        　・超未来の宇宙船内　INT
        ";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var output = GetJudgeResult(command, randomizer);
            if (output != null)
            {
                return output;
            }

            return null;
        }

        private Result? GetJudgeResult(string command, IRandomizer randomizer)
        {
            var match = DdaDRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            var number = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 1;
            var correction = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
            var single = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 4;
            var @double = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 8;
            var triple = match.Groups[6].Success ? int.Parse(match.Groups[6].Value) : -1;

            if (number <= 0)
            {
                number = 1;
            }

            var success = 0;
            var dicetext = "";
            var corrected = "";

            for (var i = 0; i < number; i++)
            {
                var dice = randomizer.RollOnce(10);
                if (dice == 10)
                {
                    dice = 0;
                }

                if (dice + correction >= single || dice == triple)
                {
                    success += 1;
                }
                if (dice + correction >= @double || dice == triple)
                {
                    success += 1;
                }
                if (dice == triple)
                {
                    success += 1;
                }

                if (dicetext == "")
                {
                    dicetext = dice.ToString();
                    corrected = (dice + correction).ToString();
                }
                else
                {
                    dicetext = dicetext + "," + dice.ToString();
                    corrected = corrected + "," + (dice + correction).ToString();
                }
            }

            string result;
            if (correction == 0)
            {
                result = $"({dicetext}) ＞ 成功数{success}";
            }
            else if (correction > 0)
            {
                result = $"({dicetext}) +{correction} ＞ ({corrected}) ＞ 成功数{success}";
            }
            else
            {
                result = $"({dicetext}) {correction} ＞ ({corrected}) ＞ 成功数{success}";
            }

            return Result.CreateBuilder(result).Build();
        }
    }
}
