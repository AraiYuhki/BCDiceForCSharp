using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Arithmetic;
using BCDice.Core;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 砂塵戦機アーガス
    /// </summary>
    public sealed class SajinsenkiAGuS : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly SajinsenkiAGuS Instance = new SajinsenkiAGuS();

        /// <inheritdoc/>
        public override string Id => "SajinsenkiAGuS";

        /// <inheritdoc/>
        public override string Name => "砂塵戦機アーガス";

        /// <inheritdoc/>
        public override string SortKey => "さしんせんきああかす";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・一般判定Lv（チャンス出目0→判定0） nAG+x
        　　　nは習得レベル、Lv0の場合nの省略可能。xは判定値修正（数式による修正可）、省略した場合はレベル修正0
        　　　例）AG:習得レベル0の一般技能、1AG+1:習得レベル1・判定値修正+1の技能、AG+2-1：習得レベル0・判定値修正2-1の技能、(1-1)AG：習得レベル1・レベル修正-1の技能

        ・適正距離での命中判定（チャンス出目0→判定0、HR算出）OM+y@z
        　　　yは命中補正値（数式可）、zはクリティカル値。クリティカル値省略時は0
        　　　HRの算出時には、HRが大きくなる場合に出目0を10に読み替えます。
        　　　例）OM+18-6@2:命中補正値+18-6でクリティカル値2、適正距離の判定

        ・非適正距離での命中判定（チャンス出目0→判定0、HR算出）NM+y@z
        　　　yは命中補正値（数式可）、zはクリティカル値。クリティカル値省略時は0
        　　　HRの算出時には、HRが大きくなる場合に出目0を10に読み替えます。
        　　　例）NM+4-3:命中補正値+4-3で非適正距離の判定

        ・クリティカル表 CR

        ※通常の1D10などの10面ダイスにおいて出目10の読み替えはしません。コマンドのみです。

        ";

        private static readonly Regex IppanRegex = new Regex(
            @"^(-?\d+)?AG((?:[-+]\d+)*)$",
            RegexOptions.Compiled);

        private static readonly Regex HitCheckRegex = new Regex(
            @"^(OM|NM)((?:[-+]\d+)*)(?:@(\d+))?$",
            RegexOptions.Compiled);

        private static readonly TableCollection TABLES = CreateTables();

        private static TableCollection CreateTables()
        {
            var tables = new TableCollection();
            tables.Add(new SimpleTable(
                "クリティカル表",
                "CR",
                "1：「小破」ダメージ+［5］。耐久値-［1］",
                "2：「小破」ダメージ+［5］。耐久値-［1］",
                "3：「小破」ダメージ+［5］。耐久値-［1］",
                "4：「小破」ダメージ+［5］。耐久値-［1］",
                "5：「兵装」損壊を受けるごとに［1D10］を振り、出目に応じた部位の兵装とオプションが《脱落》",
                "6：「上体」攻撃系能力［白兵/ 火器/ 索敵］は各［- 損壊Lv］",
                "7：「脚部」行動系・防御系能力［Iv 値（イニシア値）/ 最大MP/ 回避］は各［- 損壊Lv］",
                "8：「搭乗者」搭乗者の〈最大HP〉および〈HP〉は［-（4 ×損壊Lv）］",
                "9：「搭乗者」搭乗者の〈最大HP〉および〈HP〉は［-（4 ×損壊Lv）］",
                "0：「小破」ダメージ+［5］。耐久値-［1］"
            ));
            return tables;
        }

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollIppan(command, randomizer)
                ?? RollHitCheck(command, randomizer)
                ?? TABLES.Eval(command, randomizer);
        }

        private Result? RollIppan(string command, IRandomizer randomizer)
        {
            var m = IppanRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            int level = string.IsNullOrEmpty(m.Groups[1].Value) ? 0 : int.Parse(m.Groups[1].Value);
            int x = Arithmetic.ArithmeticEvaluator.Eval(m.Groups[2].Value, RoundType) ?? 0;
            int target = level <= 0 ? 7 + x : 10 + level + x;

            int[] diceList = randomizer.RollBarabara(2, 10).Select(d => d == 10 ? 0 : d).ToArray();
            int total = diceList.Sum();
            int successLevel = 1 + diceList.Count(val => val <= level);

            bool isSuccess = total <= target;

            var sequence = new List<string>();
            sequence.Add($"(2D10<={target})");
            sequence.Add($"{total}[{string.Join(",", diceList)}]");
            if (diceList.Contains(0))
            {
                sequence.Add("チャンス");
            }
            sequence.Add(isSuccess ? $"成功(+{successLevel})" : "失敗");

            string text = string.Join(" ＞ ", sequence);

            return Result.CreateBuilder(text)
                .SetCondition(isSuccess)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result? RollHitCheck(string command, IRandomizer randomizer)
        {
            var m = HitCheckRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            string cmd = m.Groups[1].Value;
            int modifyNumber = Arithmetic.ArithmeticEvaluator.Eval(m.Groups[2].Value, RoundType) ?? 0;
            int critical = string.IsNullOrEmpty(m.Groups[3].Value) ? 0 : int.Parse(m.Groups[3].Value);

            if (cmd == "OM")
            {
                return RollOm(modifyNumber, critical, randomizer);
            }
            else if (cmd == "NM")
            {
                return RollNm(modifyNumber, critical, randomizer);
            }

            return null;
        }

        private Result? RollOm(int target, int critical, IRandomizer randomizer)
        {
            int[] diceList = randomizer.RollBarabara(2, 10)
                .Select(v => v == 10 ? 0 : v)
                .OrderByDescending(v => v)
                .ToArray();

            int total = diceList.Sum();
            int criticals = diceList.Count(v => v <= critical);
            int hr = CalcHr(target, diceList);

            bool isSuccess = total <= target;

            var sequence = new List<string>();
            sequence.Add($"(2D10<={target})");
            sequence.Add($"{total}[{string.Join(",", diceList)}]");
            if (diceList.Contains(0))
            {
                sequence.Add("チャンス");
            }
            sequence.Add(isSuccess ? $"成功（HR={hr}、クリティカル{criticals}）" : "失敗");

            string text = string.Join(" ＞ ", sequence);

            return Result.CreateBuilder(text)
                .SetCondition(isSuccess)
                .SetCritical(criticals >= 1)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result? RollNm(int target, int critical, IRandomizer randomizer)
        {
            int[] diceList = randomizer.RollBarabara(3, 10)
                .Select(v => v == 10 ? 0 : v)
                .OrderByDescending(v => v)
                .ToArray();

            int[] chosenDiceList = diceList.Take(2).ToArray();
            int total = chosenDiceList.Sum();
            int criticals = chosenDiceList.Count(v => v <= critical);
            int hr = CalcHr(target, chosenDiceList);

            bool isSuccess = total <= target;

            var sequence = new List<string>();
            sequence.Add($"(3D10<={target})");
            sequence.Add($"{total}[{diceList[0]},{diceList[1]}&{diceList[2]}]");
            if (chosenDiceList.Contains(0))
            {
                sequence.Add("チャンス");
            }
            sequence.Add(isSuccess ? $"成功（HR={hr}、クリティカル{criticals}）" : "失敗");

            string text = string.Join(" ＞ ", sequence);

            return Result.CreateBuilder(text)
                .SetCondition(isSuccess)
                .SetCritical(criticals >= 1)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static int CalcHr(int target, int[] chosenDiceList)
        {
            int total = chosenDiceList.Sum();
            int a = Math.Abs(target - total);
            int b = Math.Abs(target - total - chosenDiceList.Count(v => v == 0) * 10);
            return Math.Max(a, b);
        }
    }
}
