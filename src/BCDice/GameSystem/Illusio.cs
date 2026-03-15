using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 晃天のイルージオ
    /// </summary>
    public sealed class Illusio : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Illusio Instance = new Illusio();

        private static readonly Regex CommandRegex = new Regex(
            @"(\d+)?IL([1-6]{0,6})(P)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "Illusio";

        /// <inheritdoc/>
        public override string Name => "晃天のイルージオ";

        /// <inheritdoc/>
        public override string SortKey => "こうてんのいるうしお";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        判定：[n]IL(BNo)[P]

        []内のコマンドは省略可能。
        「n」でダイス数を指定。省略時は「1」。
        (BNo)でブロックナンバーを指定。「236」のように記述。順不同可。
        コマンド末に「P」を指定で、(BNo)のパリィ判定。（一応、複数指定可）

        【書式例】
        ・6IL236 → 6dでブロックナンバー「2,3,6」の判定。
        ・IL4512 → 1dでブロックナンバー「1,2,4,5」の判定。
        ・2IL1P → 2dでパリィナンバー「1」の判定。
        ";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = CommandRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var diceCount = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 1;
            var blockNoStr = m.Groups[2].Success ? m.Groups[2].Value : "";
            var blockNo = blockNoStr
                .Select(c => c - '0')
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
            var isParry = m.Groups[3].Success;

            return CheckRoll(diceCount, blockNo, isParry, randomizer);
        }

        private Result? CheckRoll(int diceCount, int[] blockNo, bool isParry, IRandomizer randomizer)
        {
            var diceArray = randomizer.RollBarabara(diceCount, 6).OrderBy(x => x).ToArray();
            var diceText = string.Join(",", diceArray);

            var resultArray = new List<string>();
            var success = 0;

            foreach (var i in diceArray)
            {
                if (blockNo.Contains(i))
                {
                    resultArray.Add("×");
                }
                else
                {
                    resultArray.Add(i.ToString());
                    success += 1;
                }
            }

            var blockText = string.Join(",", blockNo);
            var blockText2 = isParry ? "Parry" : "Block";
            var resultText = string.Join(",", resultArray);

            var result = $"{diceCount}D6({blockText2}:{blockText}) ＞ {diceText} ＞ {resultText} ＞ ";

            if (!isParry)
            {
                return Result.CreateBuilder($"{result}成功数：{success}")
                    .SetRands(randomizer.RandResults)
                    .SetDetailedRands(randomizer.DetailedRandResults)
                    .Build();
            }

            if (success < diceCount)
            {
                return Result.CreateBuilder($"{result}パリィ成立！　次の非ダメージ2倍。")
                    .SetSuccess(true)
                    .SetRands(randomizer.RandResults)
                    .SetDetailedRands(randomizer.DetailedRandResults)
                    .Build();
            }
            else
            {
                return Result.CreateBuilder($"{result}成功数：{success}　パリィ失敗")
                    .SetFailure(true)
                    .SetRands(randomizer.RandResults)
                    .SetDetailedRands(randomizer.DetailedRandResults)
                    .Build();
            }
        }
    }
}
