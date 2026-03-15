using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// Ventangle
    /// </summary>
    public sealed class Ventangle : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Ventangle Instance = new Ventangle();

        private const int DefaultSpecialValue = 12;
        private const int DefaultFumbleValue = 2;
        private const int DefaultDiceNum = 2;

        // VTn@s#f$g[+-m]>=T
        // n  = suffix number (dice count, optional, default 2)
        // @s = critical/special value (optional, default 12)
        // #f = fumble value (optional, default 2)
        // $g = gap value (optional)
        // [+-m] = modifier (optional)
        // >=T = comparison operator and target number (optional)
        private static readonly Regex VtRegex = new Regex(
            @"^VT(\d+)?(?:@(\d+))?(?:#(\d+))?(?:\$(\d+))?(?:([+-]\d+))?(?:>=([\d]+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "Ventangle";

        /// <inheritdoc/>
        public override string Name => "Ventangle";

        /// <inheritdoc/>
        public override string SortKey => "うえんたんくる";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        基本書式 VTn@s#f$g>=T n=ダイス数（省略時2） s=スペシャル値（省略時12） f=ファンブル値（省略時2） g=レベルギャップ判定値（省略可） T=目標値（省略可）

        例：
        VT        デフォルトのスペシャル値・ファンブル値の判定を行う
        VT@10#3   スペシャル値10、ファンブル値3の判定を行う
        VT3@10#3  スペシャル値10、ファンブル値3の判定を、アドバンテージを1点消費してダイス3つで行う

        VT>=5         デフォルトのスペシャル値・ファンブル値で目標値5の判定を行う
        VT@10#3>=5    スペシャル値10、ファンブル値3で目標値5の判定を行う
        VT@10#3$5>=5  スペシャル値10、ファンブル値3で目標値5の判定を行う。この際達成値が目標値より5以上大きい場合、ギャップボーナスを表示する
        VT3@10#3>=5   スペシャル値10、ファンブル値3で目標値5の判定を、アドバンテージを1点消費してダイス3つで行う
        VT3@10#3$4>=5 スペシャル値10、ファンブル値3で目標値5の判定を、アドバンテージを1点消費してダイス3つで行う。この際達成値が目標値より4以上大きい場合、ギャップボーナスを表示する
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            Debug("eval_game_system_specific_command Begin");

            var m = VtRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            int diceNum = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : DefaultDiceNum;
            if (diceNum < DefaultDiceNum)
            {
                return null;
            }

            int special = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : DefaultSpecialValue;
            int fumble = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : DefaultFumbleValue;
            int? dollar = m.Groups[4].Success ? (int?)int.Parse(m.Groups[4].Value) : null;
            int modifyNumber = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : 0;
            int? targetNumber = m.Groups[6].Success ? (int?)int.Parse(m.Groups[6].Value) : null;

            var diceList = randomizer.RollBarabara(diceNum, 6);

            int[] usingList;
            if (diceNum > DefaultDiceNum)
            {
                // Keep top 2 dice in original order
                var indexed = diceList.Select((v, i) => (value: v, index: i)).ToList();
                var top2 = indexed.OrderByDescending(x => x.value).Take(2).OrderBy(x => x.index).ToList();
                usingList = top2.Select(x => x.value).ToArray();
            }
            else
            {
                usingList = diceList;
            }

            int diceTotal = usingList.Sum();
            int total = diceTotal + modifyNumber;

            Result result = Compare(diceTotal, total, targetNumber, special, fumble, randomizer);

            // Build command string representation
            var specialStr = m.Groups[2].Success ? $"@{special}" : "";
            var fumbleStr = m.Groups[3].Success ? $"#{fumble}" : "";
            var dollarStr = m.Groups[4].Success ? $"${dollar}" : "";
            var modStr = modifyNumber > 0 ? $"+{modifyNumber}" : (modifyNumber < 0 ? modifyNumber.ToString() : "");
            var targetStr = targetNumber.HasValue ? $">={targetNumber}" : "";
            var suffixStr = diceNum != DefaultDiceNum ? diceNum.ToString() : "";
            var cmdStr = $"({command})";

            var diceListStr = $"[{string.Join(",", diceList)}]";
            string? advantageStr = diceNum > DefaultDiceNum ? $"[{string.Join(",", usingList)}]" : null;
            string? modifierStr = modifyNumber != 0 ? $"{diceTotal}{(modifyNumber > 0 ? "+" : "")}{modifyNumber}" : null;

            string? levelGapStr = null;
            if (targetNumber.HasValue && dollar.HasValue && result.IsSuccess)
            {
                int gap = total - targetNumber.Value;
                if (gap >= dollar.Value)
                {
                    levelGapStr = $"ギャップボーナス({gap})";
                }
            }

            var sequence = new List<string?> { cmdStr, diceListStr, advantageStr, modifierStr, total.ToString(), result.Text, levelGapStr }
                .Where(x => x != null)
                .Cast<string>();
            var fullText = string.Join(" ＞ ", sequence);

            result = Result.CreateBuilder(fullText)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .SetRands(randomizer.RandResults)
                .Build();

            return result;
        }

        private static Result Compare(int diceTotal, int total, int? targetNumber, int special, int fumble, IRandomizer randomizer)
        {
            if (diceTotal <= fumble)
            {
                return Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
            }

            if (diceTotal >= special)
            {
                return Result.CreateBuilder("スペシャル").SetCritical(true).SetSuccess(true).Build();
            }

            if (targetNumber.HasValue)
            {
                if (total >= targetNumber.Value)
                {
                    return Result.CreateBuilder("成功").SetSuccess(true).Build();
                }
                else
                {
                    return Result.CreateBuilder("失敗").SetFailure(true).Build();
                }
            }

            return Result.CreateBuilder("").Build();
        }

    }
}
