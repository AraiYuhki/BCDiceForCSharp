using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class CodeLayerd : GameSystemBase
    {
        public static readonly CodeLayerd Instance = new CodeLayerd();
        public override string Id => "CodeLayerd";
        public override string Name => "コード：レイヤード";
        public override string SortKey => "こおとれいやあと";
        public override string HelpMessage => @"
        ・行為判定（nCL@m[c]+x）
        ";

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return CheckRollCommand(command, randomizer);
        }

        private Result? CheckRollCommand(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"([+-]?\d+)?CL([+-]\d+)?(@(\d))?(\[(\d+)\])?([+-]\d+)?(>=(\d+))?", RegexOptions.IgnoreCase);
            if (!m.Success) return null;

            var baseVal = m.Groups[1].Success ? Convert.ToInt32(m.Groups[1].Value) : 1;
            var modifier1 = m.Groups[2].Success ? Convert.ToInt32(m.Groups[2].Value) : 0;
            var targetVal = m.Groups[4].Success ? Convert.ToInt32(m.Groups[4].Value) : 6;
            var criticalTarget = m.Groups[6].Success ? Convert.ToInt32(m.Groups[6].Value) : 1;
            var modifier2 = m.Groups[7].Success ? Convert.ToInt32(m.Groups[7].Value) : 0;
            var hasDiff = m.Groups[9].Success;
            var diff = hasDiff ? Convert.ToInt32(m.Groups[9].Value) : 0;
            var modifier = modifier1 + modifier2;

            return CheckRoll(command, baseVal, targetVal, criticalTarget, hasDiff ? (int?)diff : null, modifier, randomizer);
        }

        private Result? CheckRoll(string command, int baseVal, int targetVal, int criticalTarget, int? diff, int modifier, IRandomizer randomizer)
        {
            if (baseVal <= 0) { criticalTarget = 0; baseVal = 1; }
            if (targetVal > 10) targetVal = 10;

            var diceList = randomizer.RollBarabara(baseVal, 10).OrderBy(x => x).ToList();
            var successCount = diceList.Count(x => x <= targetVal);
            var criticalCount = criticalTarget > 0 ? diceList.Count(x => x <= criticalTarget) : 0;
            var isCritical = criticalCount > 0;
            var successTotal = successCount + criticalCount + modifier;

            var modText = modifier > 0 ? "+" + modifier.ToString() : (modifier < 0 ? modifier.ToString() : "");
            var sb = new System.Text.StringBuilder();
            sb.Append(command + " ▶ ");
            sb.Append("(" + baseVal.ToString() + "d10" + modText + ") ▶ ");
            sb.Append("[" + string.Join(",", diceList) + "]" + modText + " ▶ ");
            if (targetVal != 6) sb.Append("判定値[" + targetVal.ToString() + "] ");
            if (criticalTarget != 1) sb.Append("クリティカル値[" + criticalTarget.ToString() + "] ");
            sb.Append("達成値[" + successCount.ToString() + "]");

            if (successCount <= 0)
            {
                sb.Append(" ▶ ファンブル！");
                return Result.CreateBuilder(sb.ToString())
                    .SetRands(randomizer.RandResults)
                    .SetFumble(true).SetFailure(true).Build();
            }

            if (isCritical) sb.Append("+クリティカル[" + criticalCount.ToString() + "]");
            sb.Append(modText);
            if (isCritical || modifier != 0) sb.Append("=[" + successTotal.ToString() + "]");

            if (!diff.HasValue)
            {
                sb.Append(" ▶ " + successTotal.ToString());
                return Result.CreateBuilder(sb.ToString()).SetRands(randomizer.RandResults).SetCritical(isCritical).Build();
            }
            else if (successTotal >= diff.Value)
            {
                sb.Append(" ▶ 成功");
                return Result.CreateBuilder(sb.ToString()).SetRands(randomizer.RandResults).SetCritical(isCritical).SetSuccess(true).Build();
            }
            else
            {
                sb.Append(" ▶ 失敗");
                return Result.CreateBuilder(sb.ToString()).SetRands(randomizer.RandResults).SetCritical(isCritical).SetFailure(true).Build();
            }
        }
    }
}
