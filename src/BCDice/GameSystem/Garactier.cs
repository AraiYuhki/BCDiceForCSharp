using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ガラクティア
    /// </summary>
    public sealed class Garactier : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Garactier Instance = new Garactier();

        /// <inheritdoc/>
        public override string Id => "Garactier";

        /// <inheritdoc/>
        public override string Name => "ガラクティア";

        /// <inheritdoc/>
        public override string SortKey => "からくていあ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ガラクティアVer1.04
        x:基準値
        y:目標値
        x, yについては四則演算の入力が可能

        ■達成値の算出(GRx)
          クリティカル・ファンブルの判定、達成値の表示を行う。
        ■通常判定(GRx>=y)
          通常の判定を行う。
        ■命中判定(GRHx>=y)
          命中判定を行う。
        ■回避判定(GRDx>=y)
          回避判定を行う。
        ■抵抗判定(GRMx>=y)
          抵抗判定を行う。
        ■探索成功マスレベル算出(GRSx)
          探索・索敵判定時の最大成功マスレベル(ML)を算出する
        ■ 表
          アイテム決定表(ITMn)
            ランクnのアイテム表を振る。例)ITM2 ランク２アイテム表
            上級アイテムの判定も行います。
          命中部位決定表(BUI)
            命中時の部位を決定します。
          増強可能施設決定表(SST)
            報告フェイズの増強可能施設を決定します。
        ";

        private static readonly Regex GrsRegex = new Regex(@"^GRS", RegexOptions.Compiled);
        private static readonly Regex GrHdmRegex = new Regex(@"^GR[HDM]", RegexOptions.Compiled);
        private static readonly Regex GrRegex = new Regex(@"^GR", RegexOptions.Compiled);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return CmdGr(command, randomizer) ?? RollItem(command, randomizer);
        }

        private Result? CmdGr(string command, IRandomizer randomizer)
        {
            if (GrsRegex.IsMatch(command))
            {
                return RollSearch(command, randomizer);
            }
            else if (GrHdmRegex.IsMatch(command))
            {
                return RollTarget(command, randomizer);
            }
            else if (GrRegex.IsMatch(command))
            {
                return RollGr(command, randomizer);
            }
            return null;
        }

        private Result? RollSearch(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^GRS([+-/*\d]+)?$");
            if (!m.Success)
            {
                return null;
            }

            var modifier = Arithmetic.ArithmeticEvaluator.Eval(m.Groups[1].Value ?? "", RoundType.Floor) ?? 0;
            var diceResult = RollDiceWithModifier(modifier, randomizer);
            var r = DetermineNoTargetResult("S", diceResult.Total, diceResult.Critical, diceResult.Fumble);

            var text = $"({m.Groups[0].Value}) ＞ {diceResult.DiceSum}[{string.Join(",", diceResult.DiceList)}]{m.Groups[1].Value} ＞ {diceResult.Total} ＞ {r.Text}";
            return Result.CreateBuilder(text)
                .SetSuccess(r.IsSuccess)
                .SetFailure(r.IsFailure)
                .SetCritical(r.IsCritical)
                .SetFumble(r.IsFumble)
                .Build();
        }

        private Result? RollTarget(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^GR([HDM])([+-/*\d]+)?(?:>=?([+-/*\d]+)+)$");
            if (!m.Success)
            {
                return null;
            }

            var rollType = m.Groups[1].Value;
            var modifier = Arithmetic.ArithmeticEvaluator.Eval(m.Groups[2].Value ?? "", RoundType.Floor) ?? 0;
            var target = Arithmetic.ArithmeticEvaluator.Eval(m.Groups[3].Value ?? "", RoundType.Floor) ?? 0;
            var diceResult = RollDiceWithModifier(modifier, randomizer);
            var r = DetermineTargetResult(rollType, diceResult.Total, target, diceResult.Critical, diceResult.Fumble);

            var text = $"({m.Groups[0].Value}) ＞ {diceResult.DiceSum}[{string.Join(",", diceResult.DiceList)}]{m.Groups[2].Value} ＞ {diceResult.Total} ＞ {r.Text}";
            return Result.CreateBuilder(text)
                .SetSuccess(r.IsSuccess)
                .SetFailure(r.IsFailure)
                .SetCritical(r.IsCritical)
                .SetFumble(r.IsFumble)
                .Build();
        }

        private Result? RollGr(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^GR([+-/*\d]+)?(>=)?\(?([+-/*\d]+)?\)?$");
            if (!m.Success)
            {
                return null;
            }

            var modifier = Arithmetic.ArithmeticEvaluator.Eval(m.Groups[1].Value ?? "", RoundType.Floor) ?? 0;
            var targetFlag = !string.IsNullOrEmpty(m.Groups[2].Value);
            var diceResult = RollDiceWithModifier(modifier, randomizer);

            Result r;
            if (targetFlag)
            {
                var target = Arithmetic.ArithmeticEvaluator.Eval(m.Groups[3].Value ?? "", RoundType.Floor) ?? 0;
                r = DetermineTargetResult("", diceResult.Total, target, diceResult.Critical, diceResult.Fumble);
            }
            else
            {
                r = DetermineNoTargetResult("", diceResult.Total, diceResult.Critical, diceResult.Fumble);
            }

            var text = $"({m.Groups[0].Value}) ＞ {diceResult.DiceSum}[{string.Join(",", diceResult.DiceList)}]{m.Groups[1].Value} ＞ {diceResult.Total} ＞ {r.Text}";
            return Result.CreateBuilder(text)
                .SetSuccess(r.IsSuccess)
                .SetFailure(r.IsFailure)
                .SetCritical(r.IsCritical)
                .SetFumble(r.IsFumble)
                .Build();
        }

        private class DiceRollResult
        {
            public int[] DiceList { get; set; } = Array.Empty<int>();
            public int DiceSum { get; set; }
            public int Total { get; set; }
            public bool Critical { get; set; }
            public bool Fumble { get; set; }
        }

        private DiceRollResult RollDiceWithModifier(int modifier, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(2, 6);
            var diceSum = diceList.Sum();
            var total = diceSum + modifier;
            var criticalFlag = diceList.Count(d => d == 6) == 2;
            var fumbleFlag = diceList.Count(d => d == 1) == 2;
            return new DiceRollResult
            {
                DiceList = diceList,
                DiceSum = diceSum,
                Total = total,
                Critical = criticalFlag,
                Fumble = fumbleFlag,
            };
        }

        private Result DetermineTargetResult(string rollType, int total, int target, bool critical, bool fumble)
        {
            switch (rollType)
            {
                case "H":
                    if (critical)
                        return Result.CreateBuilder("クリティカル命中").SetSuccess(true).SetCritical(true).Build();
                    else if (fumble)
                        return Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
                    else if (total >= target + 4)
                        return Result.CreateBuilder("急所命中").SetSuccess(true).Build();
                    else if (total >= target)
                        return Result.CreateBuilder("命中").SetSuccess(true).Build();
                    else
                        return Result.CreateBuilder("失敗").SetFailure(true).Build();

                case "D":
                    if (critical)
                        return Result.CreateBuilder("クリティカル").SetSuccess(true).SetCritical(true).Build();
                    else if (fumble)
                        return Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
                    else if (total >= target)
                        return Result.CreateBuilder("回避成功").SetSuccess(true).Build();
                    else if (total >= target - 4)
                        return Result.CreateBuilder("半減命中").SetFailure(true).Build();
                    else
                        return Result.CreateBuilder("失敗").SetFailure(true).Build();

                case "M":
                    // 抵抗判定は基準値以上(激情)のほうが悪い効果のことが多いためResultを反転[6,6]の場合にファンブル
                    if (critical)
                        return Result.CreateBuilder("必ず激情").SetFumble(true).SetFailure(true).Build();
                    else if (fumble)
                        return Result.CreateBuilder("必ず平静").SetSuccess(true).SetCritical(true).Build();
                    else if (total >= target)
                        return Result.CreateBuilder("激情").SetFailure(true).Build();
                    else
                        return Result.CreateBuilder("平静").SetSuccess(true).Build();

                default:
                    if (critical)
                        return Result.CreateBuilder("クリティカル").SetSuccess(true).SetCritical(true).Build();
                    else if (fumble)
                        return Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
                    else if (total >= target)
                        return Result.CreateBuilder("成功").SetSuccess(true).Build();
                    else
                        return Result.CreateBuilder("失敗").SetFailure(true).Build();
            }
        }

        private Result DetermineNoTargetResult(string rollType, int total, bool critical, bool fumble)
        {
            switch (rollType)
            {
                case "S":
                    if (critical)
                        return Result.CreateBuilder("クリティカル").SetSuccess(true).SetCritical(true).Build();
                    else if (fumble)
                        return Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
                    else
                    {
                        var successLevel = (total - 4) / 2;
                        if (successLevel >= 11)
                            successLevel = 11;
                        else if (successLevel <= 0)
                            successLevel = 1;
                        return Result.CreateBuilder($"成功ML {successLevel}").Build();
                    }

                default:
                    if (critical)
                        return Result.CreateBuilder("クリティカル").SetSuccess(true).SetCritical(true).Build();
                    else if (fumble)
                        return Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
                    else
                        return Result.CreateBuilder($"達成値 {total}").Build();
            }
        }

        private Result? RollItem(string command, IRandomizer randomizer)
        {
            if (!command.Contains("ITM"))
            {
                return null;
            }

            // TODO: Implement ITEM_TABLES D66Table roll
            // In Ruby, this calls roll_tables(command, ITEM_TABLES) which rolls on D66 tables.
            // For now, return null as table infrastructure is not yet ported.
            return null;
        }
    }
}
