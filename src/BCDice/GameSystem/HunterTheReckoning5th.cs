using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// Hunter: The Reckoning 5th Edition
    /// </summary>
    public sealed class HunterTheReckoning5th : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly HunterTheReckoning5th Instance = new HunterTheReckoning5th();

        /// <inheritdoc/>
        public override string Id => "HunterTheReckoning5th";

        /// <inheritdoc/>
        public override string Name => "Hunter: The Reckoning 5th Edition";

        /// <inheritdoc/>
        public override string SortKey => "はんあたされこにんく5";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定コマンド(nHRFx+x)
          注意：難易度は必要成功数を表す

          難易度指定：成功数のカウント、判定成功と失敗、Critical処理、Critical Win、Total Failureのチェックを行う
                     （Desperationダイスがある場合）OverreachとDespairの発生チェックを行う
          例) (難易度)HRF(通常ダイス)+(Desperationダイス)
              (難易度)HRF(通常ダイス)

          難易度省略：成功数のカウント、判定失敗、Critical処理、Total Failure、（Desperationダイスがある場合）Despairチェックを行う
                      判定成功、Overreachのチェックを行わない
                      Critical Win、（Desperationダイスがある場合）Despair、Overreachのヒントを出力
          例) HRF(通常ダイス)+(Desperationダイス)
              HRF(通常ダイス)

          難易度0指定：全てのチェックを行わない
          例) 0HRF(通常ダイス)+(Desperationダイス)
              0HRF(通常ダイス)

        ";

        private static readonly Regex COMMAND_REG = new Regex(
            @"^(\d+)?(HRF)(\d+)(\+(\d+))?$",
            RegexOptions.Compiled);

        private const int DIFFICULTY_INDEX = 1;
        private const int DICE_POOL_INDEX = 3;
        private const int DESPERATION_DICE_INDEX = 5;

        // 難易度に指定可能な特殊値
        private const int NOT_CHECK_SUCCESS = -1; // 判定成功にかかわるチェックを行わない(判定失敗に関わるチェックは行う)

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = COMMAND_REG.Match(command);
            if (!m.Success)
            {
                return null;
            }

            int dicePool = int.Parse(m.Groups[DICE_POOL_INDEX].Value);
            var (diceText, successDice, tenDice, _) = MakeDiceRoll(dicePool, randomizer);
            string resultText = $"({dicePool}D10";

            int desperatonTenDice = 0;
            int desperatonBotchDice = 0;

            if (m.Groups[DESPERATION_DICE_INDEX].Success)
            {
                int desperatonDicePool = int.Parse(m.Groups[DESPERATION_DICE_INDEX].Value);
                if (desperatonDicePool > 5)
                {
                    return Result.CreateBuilder("Desperationダイス指定は5ダイスが最大です。").Build();
                }

                var (desperatonDiceText, desperatonSuccessDice, dTenDice, dBotchDice) = MakeDiceRoll(desperatonDicePool, randomizer);
                desperatonTenDice = dTenDice;
                desperatonBotchDice = dBotchDice;
                tenDice += desperatonTenDice;
                successDice += desperatonSuccessDice;

                resultText = $"{resultText}+{desperatonDicePool}D10) ＞ [{diceText}]+[{desperatonDiceText}] ";
            }
            else
            {
                resultText = $"{resultText}) ＞ [{diceText}] ";
            }

            successDice += GetCriticalSuccess(tenDice);

            int difficulty = m.Groups[DIFFICULTY_INDEX].Success ? int.Parse(m.Groups[DIFFICULTY_INDEX].Value) : NOT_CHECK_SUCCESS;

            return GetRollResult(resultText, successDice, tenDice, desperatonTenDice, desperatonBotchDice, difficulty);
        }

        private Result GetRollResult(string resultText, int successDice, int tenDice, int desperatonTenDice, int desperatonBotchDice, int difficulty)
        {
            resultText = $"{resultText} 成功数={successDice}";
            bool isCritical = tenDice >= 2;
            string desperationResult = "";

            if (difficulty > 0)
            {
                resultText = $"{resultText} 難易度={difficulty}";

                if (successDice >= difficulty)
                {
                    resultText = $"{resultText} 差分={successDice - difficulty}";

                    if (desperatonBotchDice > 0)
                    {
                        desperationResult = " [Overreach or Despair?]";
                    }

                    if (isCritical)
                    {
                        return Result.CreateBuilder($"{resultText}：判定成功! [Critical Win]{desperationResult}").SetSuccess(true).SetCritical(true).Build();
                    }
                    else
                    {
                        return Result.CreateBuilder($"{resultText}：判定成功!{desperationResult}").SetSuccess(true).Build();
                    }
                }
                else
                {
                    if (desperatonBotchDice > 0)
                    {
                        return Result.CreateBuilder($"{resultText}：判定失敗! [Despair]").SetFumble(true).SetFailure(true).Build();
                    }
                    if (successDice == 0)
                    {
                        return Result.CreateBuilder($"{resultText}：判定失敗! [Total Failure]").SetFumble(true).SetFailure(true).Build();
                    }

                    return Result.CreateBuilder($"{resultText}：判定失敗!").SetFailure(true).Build();
                }
            }
            else if (difficulty < 0)
            {
                if (successDice == 0)
                {
                    if (desperatonBotchDice > 0)
                    {
                        return Result.CreateBuilder($"{resultText}：判定失敗! [Despair]").SetFumble(true).SetFailure(true).Build();
                    }

                    return Result.CreateBuilder($"{resultText}：判定失敗! [Total Failure]").SetFumble(true).SetFailure(true).Build();
                }
                else
                {
                    if (desperatonBotchDice > 0)
                    {
                        resultText = $"{resultText}\n　判定失敗なら [Despair]";
                        desperationResult = " [Overreach or Despair?]";
                    }

                    if (isCritical)
                    {
                        resultText = $"{resultText}\n　判定成功なら [Critical Win]";
                    }
                    else if (desperatonBotchDice > 0)
                    {
                        resultText = $"{resultText}\n　判定成功なら";
                    }

                    return Result.CreateBuilder($"{resultText}{desperationResult}").Build();
                }
            }

            // 難易度0指定(=全ての判定チェックを行わない)
            return Result.CreateBuilder(resultText).Build();
        }

        private int GetCriticalSuccess(int tenDice)
        {
            // 10の目が2個毎に追加2成功
            return (tenDice / 2) * 2;
        }

        private (string diceText, int successDice, int tenDice, int botchDice) MakeDiceRoll(int dicePool, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(dicePool, 10);
            string diceText = string.Join(",", diceList);
            int successDice = diceList.Count(x => x >= 6);
            int tenDice = diceList.Count(x => x == 10);
            int botchDice = diceList.Count(x => x == 1);
            return (diceText, successDice, tenDice, botchDice);
        }
    }
}
