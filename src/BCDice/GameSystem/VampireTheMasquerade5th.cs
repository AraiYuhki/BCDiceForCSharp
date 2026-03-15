using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// Vampire: The Masquerade 5th Edition
    /// </summary>
    public sealed class VampireTheMasquerade5th : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly VampireTheMasquerade5th Instance = new VampireTheMasquerade5th();

        /// <inheritdoc/>
        public override string Id => "VampireTheMasquerade5th";

        /// <inheritdoc/>
        public override string Name => "Vampire: The Masquerade 5th Edition";

        /// <inheritdoc/>
        public override string SortKey => "うあんはいあさますかれえと5";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定コマンド(nVMFx+x または nVMIxHx)
          VMFコマンドはHungerダイスとダイスプールを個別に指定する。
          VMIコマンドはHungerダイスをダイスプールの内数として指定する。

            例：難易度2、9ダイスプールでHungerダイス3個の場合、それぞれ以下のようなコマンドとなる。
            2VMF6+3
            2VMI9H3

          難易度指定：成功数のカウント、判定成功と失敗、Critical処理、Critical Win、Total Failureのチェックを行う
                     （Hungerダイスがある場合）Messy CriticalとBestial Failureチェックを行う
          例) (難易度)VMF(通常ダイス)+(Hungerダイス)
              (難易度)VMF(通常ダイス)
              (難易度)VMI(通常ダイス)H(Hungerダイス)
              (難易度)VMI(通常ダイス)

          難易度省略：成功数のカウント、判定失敗、Critical処理、Total Failure、（Hungerダイスがある場合）Bestial Failureチェックを行う
                      判定成功、Messy Criticalのチェックを行わない
                      Critical Win、（Hungerダイスがある場合）Bestial Failure、Messy Criticalのヒントを出力
          例) VMF(通常ダイス)+(Hungerダイス)
              VMF(通常ダイス)
              VMI(通常ダイス)H(Hungerダイス)
              VMI(通常ダイス)

          難易度0指定：Critical処理と成功数のカウントを行い、全てのチェックを行わない
          例) 0VMF(通常ダイス)+(Hungerダイス)
              0VMF(通常ダイス)
              0VMI(通常ダイス)+(Hungerダイス)
              0VMI(通常ダイス)

        ";

        private const int DIFFICULTY_INDEX = 1;
        private const int DICE_POOL_HUNGER_DICE_NO_INCLUDED_INDEX = 5;
        private const int HUNGER_DICE_NO_INCLUDED_INDEX = 7;
        private const int COMMAND_HUNGER_DICE_INCLUDED_INDEX = 9;
        private const int DICE_POOL_HUNGER_DICE_INCLUDED_INDEX = 10;
        private const int HUNGER_DICE_INCLUDED_INDEX = 12;

        /// <summary>
        /// 難易度に指定可能な特殊値: 判定成功にかかわるチェックを行わない
        /// </summary>
        private const int NOT_CHECK_SUCCESS = -1;

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"\A(\d+)?(((VMF)(\d+)(\+(\d+))?)|((VMI)(\d+)(H(\d+))?))$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var (dicePool, hungerDicePool) = GetDicePools(m);
            if (dicePool < 0)
            {
                return Result.CreateBuilder("ダイスプール0のときにHungerダイスは指定できません。").Build();
            }
            if (hungerDicePool > 5)
            {
                return Result.CreateBuilder("Hungerダイス指定は5ダイスが最大です。").Build();
            }

            var (diceText, successDice, tenDice, _) = MakeDiceRoll(dicePool, randomizer);
            var resultText = $"({dicePool}D10";

            int hungerTenDice;
            int hungerBotchDice;

            if (hungerDicePool >= 0)
            {
                var (hungerDiceText, hungerSuccessDice, hungerTenDiceVal, hungerBotchDiceVal) = MakeDiceRoll(hungerDicePool, randomizer);
                tenDice += hungerTenDiceVal;
                successDice += hungerSuccessDice;
                hungerTenDice = hungerTenDiceVal;
                hungerBotchDice = hungerBotchDiceVal;
                resultText = $"{resultText}+{hungerDicePool}D10) ＞ [{diceText}]+[{hungerDiceText}] ";
            }
            else
            {
                hungerTenDice = 0;
                hungerBotchDice = 0;
                resultText = $"{resultText}) ＞ [{diceText}] ";
            }

            successDice += GetCriticalSuccess(tenDice);

            var difficulty = m.Groups[DIFFICULTY_INDEX].Success && !string.IsNullOrEmpty(m.Groups[DIFFICULTY_INDEX].Value)
                ? int.Parse(m.Groups[DIFFICULTY_INDEX].Value)
                : NOT_CHECK_SUCCESS;

            return GetRollResult(resultText, successDice, tenDice, hungerTenDice, hungerBotchDice, difficulty);
        }

        private (int dicePool, int hungerDicePool) GetDicePools(Match m)
        {
            var hungerDiceIncludedCommand = m.Groups[COMMAND_HUNGER_DICE_INCLUDED_INDEX].Value;
            int dicePool;
            int hungerDicePool;

            if (!string.IsNullOrEmpty(hungerDiceIncludedCommand) && hungerDiceIncludedCommand == "VMI")
            {
                // Hunger Diceを内数処理する場合
                hungerDicePool = !m.Groups[HUNGER_DICE_INCLUDED_INDEX].Success || string.IsNullOrEmpty(m.Groups[HUNGER_DICE_INCLUDED_INDEX].Value)
                    ? -1
                    : int.Parse(m.Groups[HUNGER_DICE_INCLUDED_INDEX].Value);
                var dicePoolValue = int.Parse(m.Groups[DICE_POOL_HUNGER_DICE_INCLUDED_INDEX].Value);
                dicePool = dicePoolValue - (hungerDicePool < 0 ? 0 : hungerDicePool);
                if (dicePoolValue > 0 && hungerDicePool >= dicePoolValue)
                {
                    // 1以上のダイスプール、かつ、Hungerダイスがダイスプール以上のとき、ダイスプールが全てHungerダイスになる
                    dicePool = 0;
                    hungerDicePool = dicePoolValue;
                }
            }
            else
            {
                // Hunger DiceがPLによる内数指定の場合
                hungerDicePool = !m.Groups[HUNGER_DICE_NO_INCLUDED_INDEX].Success || string.IsNullOrEmpty(m.Groups[HUNGER_DICE_NO_INCLUDED_INDEX].Value)
                    ? -1
                    : int.Parse(m.Groups[HUNGER_DICE_NO_INCLUDED_INDEX].Value);
                dicePool = int.Parse(m.Groups[DICE_POOL_HUNGER_DICE_NO_INCLUDED_INDEX].Value);
            }

            return (dicePool, hungerDicePool);
        }

        private Result? GetRollResult(string resultText, int successDice, int tenDice, int hungerTenDice, int hungerBotchDice, int difficulty)
        {
            resultText = $"{resultText} 成功数={successDice}";
            var isCritical = tenDice >= 2;

            if (difficulty > 0)
            {
                resultText = $"{resultText} 難易度={difficulty}";
                if (successDice >= difficulty)
                {
                    resultText = $"{resultText} 差分={successDice - difficulty}";
                    if (hungerTenDice > 0 && isCritical)
                    {
                        return Result.CreateBuilder($"{resultText}：判定成功! [Messy Critical]").SetSuccess(true).SetCritical(true).Build();
                    }
                    else if (isCritical)
                    {
                        return Result.CreateBuilder($"{resultText}：判定成功! [Critical Win]").SetSuccess(true).SetCritical(true).Build();
                    }
                    return Result.CreateBuilder($"{resultText}：判定成功!").SetSuccess(true).Build();
                }
                else
                {
                    if (hungerBotchDice > 0)
                    {
                        return Result.CreateBuilder($"{resultText}：判定失敗! [Bestial Failure]").SetFumble(true).SetFailure(true).Build();
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
                    if (hungerBotchDice > 0)
                    {
                        return Result.CreateBuilder($"{resultText}：判定失敗! [Bestial Failure]").SetFumble(true).SetFailure(true).Build();
                    }
                    return Result.CreateBuilder($"{resultText}：判定失敗! [Total Failure]").SetFumble(true).SetFailure(true).Build();
                }
                else
                {
                    if (hungerBotchDice > 0)
                    {
                        resultText = $"{resultText}\n　判定失敗なら [Bestial Failure]";
                    }
                    if (hungerTenDice > 0 && isCritical)
                    {
                        resultText = $"{resultText}\n　判定成功なら [Messy Critical]";
                    }
                    else if (isCritical)
                    {
                        resultText = $"{resultText}\n　判定成功なら [Critical Win]";
                    }
                    return Result.CreateBuilder(resultText).Build();
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
            var diceText = string.Join(",", diceList);
            var successDice = diceList.Count(x => x >= 6);
            var tenDice = diceList.Count(x => x == 10);
            var botchDice = diceList.Count(x => x == 1);
            return (diceText, successDice, tenDice, botchDice);
        }

    }
}
