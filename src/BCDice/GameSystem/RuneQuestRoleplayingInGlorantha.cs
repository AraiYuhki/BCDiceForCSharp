using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ルーンクエスト：ロールプレイング・イン・グローランサ
    /// </summary>
    public sealed class RuneQuestRoleplayingInGlorantha : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly RuneQuestRoleplayingInGlorantha Instance = new RuneQuestRoleplayingInGlorantha();


        private static readonly Regex RqgRegex = new Regex(
            @"RQG",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ResRegex = new Regex(
            @"RES",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RsaRegex = new Regex(
            @"RSA",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "RuneQuestRoleplayingInGlorantha";

        /// <inheritdoc/>
        public override string Name => "ルーンクエスト：ロールプレイング・イン・グローランサ";

        /// <inheritdoc/>
        public override string SortKey => "るうんくえすと4";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定コマンド 決定的成功、効果的成功、ファンブルを含めた判定を行う。
        RQG<=成功率      (基本書式)
        RQG成功率        (省略記法)

        例1：RQG<=80    （技能値80で判定）
        例2：RQG<=80+20 （技能値100で判定）
        例3：RQG80      （省略書式で技能値80の判定）
        例4：RQG80+20   （省略書式で技能値100の判定）

        ・抵抗判定コマンド（能動-受動） 決定的成功、効果的成功、ファンブルを含めた判定を行う。
        RES(能動能力-受動能力)m増強値
        増強値は省略可能。

        例1：RES(9-11)    (能動能力9 vs 受動能力11で判定)
        例2：RES(9-11)m20 (能動能力9 vs 受動能力11、+20%の増強が能動側に入る判定)
        例3：RES(9)m50    (能動能力と受動能力の差が9で、+50%の増強が能動側に入る判定)

        ・抵抗判定コマンド(能動側のみ) 決定的成功、効果的成功、ファンブルは含めず判定を行う。
        RSA(能動能力)m増強値
        増強値は省略可能。

        例1：RSA(9)       (能動能力9で判定)
        例2：RSA(9)m20    (能動能力9で判定、+20%の増強が能動側に入る判定)

        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            Match match;

            match = RqgRegex.Match(command);
            if (match.Success)
            {
                return DoAbilityRoll(command, randomizer);
            }

            match = ResRegex.Match(command);
            if (match.Success)
            {
                return DoResistanceRoll(command, randomizer);
            }

            match = RsaRegex.Match(command);
            if (match.Success)
            {
                return DoResistanceActiveCharacteristicRoll(command, randomizer);
            }


            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result? DoAbilityRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"\A(RQG)((<=)?([+-/*\d]+))?$");
            if (!m.Success)
            {
                return null;
            }
            var rollValue = randomizer.RollOnce(100);
            if (!m.Groups[4].Success || string.IsNullOrEmpty(m.Groups[4].Value))
            {
                return Result.CreateBuilder($"(1D100).Build() ＞ {rollValue}").Build();
            }
            var abilityValue = ArithmeticEvaluator.Eval(m.Groups[4].Value, RoundType.Round);
            var resultPrefixStr = $"(1D100<={abilityValue}) ＞";
            if (abilityValue == 0)
            {
                return Result.CreateBuilder($"{resultPrefixStr} 失敗").SetFailure(true).Build();
            }
            var resultStr = $"{resultPrefixStr} {rollValue} ＞";
            return GetRollResult(resultStr, abilityValue ?? 0, rollValue, randomizer);
        }

        private Result? DoResistanceRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"\A(RES)([+-/*\d]+)(M([+-/*\d]+))?$");
            if (!m.Success)
            {
                return null;
            }
            if (!m.Groups[2].Success || string.IsNullOrEmpty(m.Groups[2].Value))
            {
                return null;
            }
            var differenceValue = ArithmeticEvaluator.Eval(m.Groups[2].Value, RoundType.Round);
            if (differenceValue < -10)
            {
                differenceValue = -10;
            }
            var resistanceValue = 50 + differenceValue * 5;
            if (m.Groups[4].Success && !string.IsNullOrEmpty(m.Groups[4].Value))
            {
                resistanceValue += ArithmeticEvaluator.Eval(m.Groups[4].Value, RoundType.Round);
            }
            var rollValue = randomizer.RollOnce(100);
            var resultStr = $"(1D100<={resistanceValue}) ＞ {rollValue} ＞";
            return GetRollResult(resultStr, resistanceValue ?? 0, rollValue, randomizer);
        }

        private Result? DoResistanceActiveCharacteristicRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"\A(RSA)(\d+)(M([+-/*\d]+))?$");
            if (!m.Success)
            {
                return null;
            }
            if (!m.Groups[2].Success || string.IsNullOrEmpty(m.Groups[2].Value))
            {
                return null;
            }
            var activeAbilityValue = Convert.ToInt32(m.Groups[2].Value);
            if (activeAbilityValue == 0)
            {
                return Result.CreateBuilder("0は指定できません。").Build();
            }
            var modifyValue = (m.Groups[4].Success && !string.IsNullOrEmpty(m.Groups[4].Value)) ? ArithmeticEvaluator.Eval(m.Groups[4].Value, RoundType.Round) : 0;
            var rollValue = randomizer.RollOnce(100);
            var activeValue = activeAbilityValue * 5 + modifyValue;
            var resultPrefixStr = $"(1D100<={activeValue}) ＞ {rollValue} ＞";
            var noteStr = "決定的成功、効果的成功、ファンブルは未処理。必要なら確認すること。";
            if (rollValue >= 96)
            {
                return Result.CreateBuilder($"{resultPrefixStr} 失敗\n{noteStr}").SetFailure(true).Build();
            }
            else if (rollValue <= 5 || rollValue <= modifyValue)
            {
                return Result.CreateBuilder($"{resultPrefixStr} 成功\n{noteStr}").SetSuccess(true).Build();
            }
            else
            {
                return Result.CreateBuilder($"{resultPrefixStr} 相手側能力値{activeAbilityValue + (50 + modifyValue - rollValue) / 5}まで成功\n{noteStr}").Build();
            }
        }

        private Result? GetRollResult(string resultStr, int successValue, int rollValue, IRandomizer randomizer)
        {
            var criticalValue = (int)Math.Round(Convert.ToDouble(successValue) / 20);
            var specialValue = (int)Math.Round(Convert.ToDouble(successValue) / 5);
            var fumbleValue = (int)Math.Round((100 - Convert.ToDouble(successValue)) / 20);
            if (rollValue == 1 || rollValue <= criticalValue)
            {
                return Result.CreateBuilder($"{resultStr} 決定的成功").SetCritical(true).SetSuccess(true).Build();
            }
            else if (rollValue == 100 || rollValue >= 100 - fumbleValue + 1)
            {
                return Result.CreateBuilder($"{resultStr} ファンブル").SetFumble(true).SetFailure(true).Build();
            }
            else if (rollValue >= 96 || (rollValue > successValue && rollValue > 5))
            {
                return Result.CreateBuilder($"{resultStr} 失敗").SetFailure(true).Build();
            }
            else if (rollValue <= specialValue)
            {
                return Result.CreateBuilder($"{resultStr} 効果的成功").SetSuccess(true).Build();
            }
            else if (rollValue <= 5 || rollValue <= successValue)
            {
                return Result.CreateBuilder($"{resultStr} 成功").SetSuccess(true).Build();
            }
            else
            {
                return Result.CreateBuilder($"{resultStr} エラー").SetFailure(true).Build();
            }
        }

    }
}
