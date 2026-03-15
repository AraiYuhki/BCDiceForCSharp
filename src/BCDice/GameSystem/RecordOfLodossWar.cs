using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ロードス島戦記RPG
    /// </summary>
    public sealed class RecordOfLodossWar : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly RecordOfLodossWar Instance = new RecordOfLodossWar();

        private static readonly Regex CommandRegex = new Regex(
            @"^(LWD|LW)(<=(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "RecordOfLodossWar";

        /// <inheritdoc/>
        public override string Name => "ロードス島戦記RPG";

        /// <inheritdoc/>
        public override string SortKey => "ろおとすとうせんきRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ●判定
        　LW<=(目標値)で判定。
        　達成値が目標値の1/10(端数切り上げ)以下であれば大成功。1～10であれば自動成功。
        　91～100であれば自動失敗となります。

        ●回避判定
        　LWD<=(目標値)で回避判定。この時出目が51以上で自動失敗となります。

        　判定と回避判定は、どちらもコマンドだけの場合、出目の表示と自動成功と自動失敗の判定のみを行います。
        ";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = CommandRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            string cmdName = m.Groups[1].Value.ToUpperInvariant();
            string? cmpOp = m.Groups[2].Success ? "<=" : null;
            int? targetNumber = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : (int?)null;

            int autoFailure = cmdName == "LWD" ? 51 : 91;
            int critical = targetNumber.HasValue ? (int)Math.Ceiling((double)targetNumber.Value / 10) : 0;

            int diceValue = randomizer.RollOnce(100);

            string? result;
            if (diceValue >= autoFailure)
            {
                result = $"自動失敗({autoFailure})";
            }
            else if (targetNumber.HasValue && diceValue <= critical)
            {
                result = $"大成功({critical})";
            }
            else if (diceValue <= 10)
            {
                result = "自動成功";
            }
            else if (cmpOp != null && targetNumber.HasValue)
            {
                result = diceValue <= targetNumber.Value ? "成功" : "失敗";
            }
            else
            {
                result = null;
            }

            var sequence = new List<string>();
            sequence.Add($"(1D100{cmpOp}{targetNumber})");
            sequence.Add(diceValue.ToString());
            if (result != null)
            {
                sequence.Add(result);
            }

            string text = string.Join(" ＞ ", sequence);

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }
    }
}
