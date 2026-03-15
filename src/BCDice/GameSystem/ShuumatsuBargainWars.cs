using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 終末買い物戦争
    /// </summary>
    public sealed class ShuumatsuBargainWars : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly ShuumatsuBargainWars Instance = new ShuumatsuBargainWars();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();

        // nBGk+y>=t  (n:dice count, k:kokorone, y:correction optional, t:target)
        private static readonly Regex BgRegex = new Regex(
            @"^(\d+)BG(\d+)([+-]\d+)?>=(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "ShuumatsuBargainWars";

        /// <inheritdoc/>
        public override string Name => "終末買い物戦争";

        /// <inheritdoc/>
        public override string SortKey => "しゆうまつはあけんうおおす";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・行為判定 （nBGk+y>=t）n:ダイス数、k:心根、y:修正値（省略可)、t:目標値
          例）3BG1>=3 2BG3+1>=4 4BG5-1>=3
        ・アイテム表
          ・RT 回復系アイテム表
          ・CT 便利系アイテム表
          ・WT 武器系アイテム表
          ・WG ワゴン(全アイテムランダム)
        ・ET イベント表
        ・TT トラブル表
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollBg(command, randomizer) ?? RollTables(command, TABLES);
        }

        private Result? RollBg(string command, IRandomizer randomizer)
        {
            var match = BgRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            int times = int.Parse(match.Groups[1].Value);
            int kokorone = int.Parse(match.Groups[2].Value);
            int correction = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
            int target = int.Parse(match.Groups[4].Value);

            int[] diceArray = randomizer.RollBarabara(times, 6);
            var diceList = diceArray.OrderBy(x => x).ToList();

            int success = diceList.Count(number => number >= target - correction);
            int getVitality = diceList.Count(v => v == kokorone);

            Result result;
            if (diceList.Count(v => v == 6) >= 2)
            {
                result = Result.CreateBuilder($"スペシャル！ 成功度{success + 1}、活力{getVitality}獲得").SetCritical(true).SetSuccess(true).Build();
            }
            else if (diceList.All(x => x == 1))
            {
                result = Result.CreateBuilder("ファンブル 活力をすべて失う").SetFumble(true).SetFailure(true).Build();
            }
            else
            {
                result = Result.CreateBuilder($"成功度{success}、活力{getVitality}獲得").Build();
            }

            string fullText = $"({command}) ＞ [{string.Join(",", diceList)}] ＞ {result.Text}";
            return Result.CreateBuilder(fullText)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .Build();
        }

    }
}
