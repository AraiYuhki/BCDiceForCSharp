using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// バケノカワ
    /// </summary>
    public sealed class Bakenokawa : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Bakenokawa Instance = new Bakenokawa();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "Bakenokawa";

        /// <inheritdoc/>
        public override string Name => "バケノカワ";

        /// <inheritdoc/>
        public override string SortKey => "はけのかわ";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・行為判定
          xBKy@z
            x：振るダイスの数(省略可、省略した場合は2)
            y：振るダイスの面数
            z：スペシャル値(@ごと省略可、省略した場合は12)
          （例）BK10
          　　　4BK6
          　　　2BK6@10

        ・各種表
          今の関係表 NRT
          カイブツ時代からの因縁表 KKT
          調査演出表
            カイブツ RTK
            バケノカワ RTB
          コラボテーマ表
            カイブツ CTK
            バケノカワ CTB
          ファンブル表 FT
        ";


        private static readonly Regex BkRegex = new Regex(
            @"^(\d+)?BK(\d+)(?:@(\d+))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return CheckAction(command, randomizer) ?? RollTables(command, TABLES);
        }

        private Result? CheckAction(string command, IRandomizer randomizer)
        {
            var m = BkRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var target = 4;
            var diceCnt = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 2;
            var diceFaces = int.Parse(m.Groups[2].Value);
            var specialTarget = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 12;

            var diceArr = randomizer.RollBarabara(diceCnt, diceFaces).OrderBy(x => x).ToList();
            var diceStr = string.Join(",", diceArr);
            var diceSum = diceArr.Sum();
            var hasSpecial = diceSum >= specialTarget;
            var hasFumble = diceSum <= 2;
            var success = diceArr.Count(x => x >= target) >= 1;

            return Result.CreateBuilder($"({diceCnt}B{diceFaces}>={target}).Build() ＞ [{diceStr}] ＞ {diceSum} ＞ {(success ? "成功" : "失敗")}{(hasSpecial ? "(スペシャル)" : "")}{(hasFumble ? "(ファンブル)" : "")}")
                .SetCritical(hasSpecial)
                .SetFumble(hasFumble)
                .SetSuccess(success)
                .SetFailure(!success)
                .SetRands(randomizer.RandResults)
                .Build();
        }

    }
}