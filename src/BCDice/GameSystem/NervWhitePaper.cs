using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 新世紀エヴァンゲリオンRPG NERV白書/使徒降臨
    /// </summary>
    public sealed class NervWhitePaper : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly NervWhitePaper Instance = new NervWhitePaper();

        /// <inheritdoc/>
        public override string Id => "NervWhitePaper";

        /// <inheritdoc/>
        public override string Name => "新世紀エヴァンゲリオンRPG NERV白書/使徒降臨";

        /// <inheritdoc/>
        public override string SortKey => "しんせいきえうあんけりおんああるひいしいねるふはくしよしとこおりん";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■通常ロール(NR)：成功、失敗、絶対成功、絶対失敗を表示します。
        例) NR

        ■長所ロール(NA)：成功、失敗、絶対成功、絶対失敗を表示します。
        例) NA

        ■短所ロール(ND)：成功、失敗、絶対成功、絶対失敗を表示します。
        例) ND

        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return ResoluteRegularAction(command, randomizer) ?? ResoluteAdvantageAction(command, randomizer) ?? ResoluteDisadvantageAction(command, randomizer);

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result ResoluteRegularAction(string command, IRandomizer randomizer)

        {
            var m = Regex.Match(command, @"NR");
            if (m.Success) {
                return null;
            }
            else
            {
                return null;
            }
            var dices = randomizer.RollBarabara(2, 6);
            var dice_text = string.Join(",", dices);
            var dice_add = dices.Sum();
            var output = $"(NR) ＞ {dice_text}";
            if (dice_add == 7)
            {
                output = " ＞ 絶対成功";
                return Result.CreateBuilder(output).SetCritical(true).SetSuccess(true).Build();
            }
            else
            {
                if (dice_add == 2)
                {
                    output = " ＞ 絶対失敗";
                    return Result.CreateBuilder(output).SetFumble(true).SetFailure(true).Build();
                }
                else
                {
                    if (dice_add == 12)
                    {
                        output = " ＞ 絶対失敗";
                        return Result.CreateBuilder(output).SetFumble(true).SetFailure(true).Build();
                    }
                    else
                    {
                        if (dice_add % 2 == 0)
                        {
                            output = " ＞ 失敗";
                            return Result.CreateBuilder(output).SetFailure(true).Build();
                        }
                        else
                        {
                            output = " ＞ 成功";
                            return Result.CreateBuilder(output).SetSuccess(true).Build();
                        }
                    }
                }
            }
        }

        private Result ResoluteAdvantageAction(string command, IRandomizer randomizer)

        {
            var m = Regex.Match(command, @"NA");
            if (m.Success) {
                return null;
            }
            else
            {
                return null;
            }
            var dices = randomizer.RollBarabara(2, 6);
            var dice_text = string.Join(",", dices);
            var dice_add = dices.Sum();
            var output = $"(NA) ＞ {dice_text}";
            if (dice_add == 7)
            {
                output = " ＞ 絶対成功";
                return Result.CreateBuilder(output).SetCritical(true).SetSuccess(true).Build();
            }
            else
            {
                if (dice_add == 2)
                {
                    output = " ＞ 絶対失敗";
                    return Result.CreateBuilder(output).SetFumble(true).SetFailure(true).Build();
                }
                else
                {
                    if (dice_add == 12)
                    {
                        output = " ＞ 絶対失敗";
                        return Result.CreateBuilder(output).SetFumble(true).SetFailure(true).Build();
                    }
                    else
                    {
                        if (dices[0] == dices[1])
                        {
                            output = " ＞ 失敗";
                            return Result.CreateBuilder(output).SetFailure(true).Build();
                        }
                        else
                        {
                            output = " ＞ 成功";
                            return Result.CreateBuilder(output).SetSuccess(true).Build();
                        }
                    }
                }
            }
        }

        private Result ResoluteDisadvantageAction(string command, IRandomizer randomizer)

        {
            var m = Regex.Match(command, @"ND");
            if (m.Success) {
                return null;
            }
            else
            {
                return null;
            }
            var dices = randomizer.RollBarabara(2, 6);
            var dice_text = string.Join(",", dices);
            var dice_add = dices.Sum();
            var output = $"(ND) ＞ {dice_text}";
            if (dice_add == 7)
            {
                output = " ＞ 絶対成功";
                return Result.CreateBuilder(output).SetCritical(true).SetSuccess(true).Build();
            }
            else
            {
                if (dice_add == 2)
                {
                    output = " ＞ 絶対失敗";
                    return Result.CreateBuilder(output).SetFumble(true).SetFailure(true).Build();
                }
                else
                {
                    if (dice_add == 12)
                    {
                        output = " ＞ 絶対失敗";
                        return Result.CreateBuilder(output).SetFumble(true).SetFailure(true).Build();
                    }
                    else
                    {
                        if (dice_add != 7)
                        {
                            output = " ＞ 失敗";
                            return Result.CreateBuilder(output).SetFailure(true).Build();
                        }
                    }
                }
            }
        }

    }
}