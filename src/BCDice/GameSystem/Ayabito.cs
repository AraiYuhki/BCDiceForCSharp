using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// あやびと
    /// </summary>
    public sealed class Ayabito : GameSystemBase
    {
        public static readonly Ayabito Instance = new Ayabito();

        private static readonly Dictionary<string, object> TABLES = new Dictionary<string, object>();

        public override string Id => "Ayabito";
        public override string Name => "あやびと";
        public override string SortKey => "あやひと";
        public override RoundType RoundType => RoundType.Ceiling;
        public override bool SortBarabaraDice => true;

        public override string HelpMessage => @"
        ・判定コマンド(xAB±y@c$d>=z)
          x：サイコロの数(10以上の場合9個振り、それ以降を成功数2として加算する)
          ±y：成功数への補正(省略可)
          c：クリティカル値(@ごと省略可。省略時は6)
          d：出目を2として数える数の最小値($ごと省略可。省略時は6)
          z：目標値(妨害値など。>=ごと省略可)
          (例) 4AB
               11AB>=5
               5AB+1
               6AB@5>=3
        ";

        private static readonly Regex AbRegex = new Regex(
            @"^(\d+)AB([+-]\d+)?(?:@(\d+))?(?:\$(\d+))?(>=(\d+))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return CheckAction(command, randomizer) ?? RollTables(command, TABLES);
        }

        private Result? CheckAction(string command, IRandomizer randomizer)
        {
            var m = AbRegex.Match(command);
            if (!m.Success)
                return null;

            int prefixNum = int.Parse(m.Groups[1].Value);
            int modify = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
            int criticalTarget = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 6;
            int additionTarget = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 6;
            int? target = m.Groups[6].Success ? (int?)int.Parse(m.Groups[6].Value) : null;

            int diceCnt;
            int overModify;
            if (prefixNum < 10)
            {
                diceCnt = prefixNum;
                overModify = 0;
            }
            else
            {
                diceCnt = 9;
                overModify = prefixNum - 9;
            }

            var diceArr = randomizer.RollBarabara(diceCnt, 6).OrderBy(x => x).ToList();
            string diceStr = string.Join(",", diceArr);
            bool hasCritical = diceArr.Any(x => x >= criticalTarget);
            int successCnt = diceArr.Count(x => x >= 4) + diceArr.Count(x => x >= additionTarget) + overModify * 2;
            bool hasFumble = successCnt == 0 && diceArr.Contains(1);
            if (hasFumble)
                successCnt = 0;
            else
                successCnt += modify;

            bool result = target == null ? successCnt >= 1 : successCnt >= target.Value;
            string modifyStr = overModify > 0 ? $"+{overModify * 2}" : "";
            string text = $"({diceCnt}B6>=4){modifyStr} ＞ [{diceStr}]{modifyStr} ＞ 成功数{successCnt} ＞ {(result ? "成功" : "失敗")}{(hasCritical ? "(クリティカル)" : "")}{(hasFumble ? "(ファンブル)" : "")}";

            return Result.CreateBuilder(text)
                .SetCritical(hasCritical)
                .SetFumble(hasFumble)
                .SetSuccess(result)
                .SetFailure(!result)
                .SetRands(randomizer.RandResults)
                .Build();
        }
    }
}
