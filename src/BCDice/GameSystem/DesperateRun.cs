using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// Desperate Run TRPG
    /// </summary>
    public sealed class DesperateRun : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly DesperateRun Instance = new DesperateRun();

        private static readonly Regex CheckRollRegex = new Regex(
            @"^RC(\d+)([-+]\d+)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "DesperateRun";

        /// <inheritdoc/>
        public override string Name => "Desperate Run TRPG";

        /// <inheritdoc/>
        public override string SortKey => "てすへれいとらんTRPG";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・難易度算出コマンド　DDC
        ・判定コマンド　RCx　or　RCx+y　or　RCx-y（x＝難易度、y=修正値（省略可能））
        ・アクシデント表　ACT
        ・初期アイテム表　ITEMT
        ・動機表　DOUKIT
        ・死亡フラグ台詞表　FLAGT
        ・途中参加動機表　ENTRYT
        ・途中参加道中表　ROADT
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return CheckRoll(command, randomizer)
                ?? DdcTable(command, randomizer)
                ?? RollTables(command, TABLES);
        }

        private Result? CheckRoll(string command, IRandomizer randomizer)
        {
            var m = CheckRollRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var target = int.Parse(m.Groups[1].Value);
            var modifyNumber = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;

            var dice = randomizer.RollBarabara(2, 6);
            var d1 = dice[0];
            var d2 = dice[1];
            var diceTotal = d1 + d2;
            var total = diceTotal + modifyNumber;

            var modifierStr = modifyNumber != 0 ? $"　修正値：{modifyNumber}" : "";

            Result result;
            if (d1 == d2)
            {
                result = Result.CreateBuilder("ゾロ目！【Critical】").SetSuccess(true).SetCritical(true).Build();
            }
            else if (diceTotal == 7)
            {
                result = Result.CreateBuilder("ダイスの出目が表裏！【Fumble】").SetFumble(true).SetFailure(true).Build();
            }
            else if (total >= target)
            {
                result = Result.CreateBuilder($"{total}、難易度以上！【Success】").SetSuccess(true).Build();
            }
            else
            {
                result = Result.CreateBuilder($"{total}、難易度未満！【Miss】").SetFailure(true).Build();
            }

            var sequence = new string[]
            {
                $"判定　難易度：{target}{modifierStr}",
                $"出目：{d1}、{d2}",
                result.Text
            };

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .Build();
        }

        private Result? DdcTable(string command, IRandomizer randomizer)
        {
            if (!string.Equals(command, "DDC", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var dice = randomizer.RollBarabara(2, 6);
            var d1 = dice[0];
            var d2 = dice[1];
            var sorted = new[] { d1, d2 }.OrderBy(x => x).ToArray();
            var smaller = sorted[0];
            var larger = sorted[1];
            var difference = larger - smaller;

            return Result.CreateBuilder($"難易度決定 ＞ 出目：{d1}、{d2} ＞ {larger}-{smaller}={difference} ＞ 難易度{5 + difference}").Build();
        }

        // TABLES は Ruby 版のテーブル定義に対応（省略 - 別途テーブル実装が必要）
        private static readonly Dictionary<string, object> TABLES = new Dictionary<string, object>();

    }
}
