using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// Werewolf: The Apocalypse 5th Edition
    /// </summary>
    public sealed class WerewolfTheApocalypse5th : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly WerewolfTheApocalypse5th Instance = new WerewolfTheApocalypse5th();

        /// <inheritdoc/>
        public override string Id => "WerewolfTheApocalypse5th";

        /// <inheritdoc/>
        public override string Name => "Werewolf: The Apocalypse 5th Edition";

        /// <inheritdoc/>
        public override string SortKey => "わあうふるしあほかりふす5";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定コマンド(nWAFx+x または nWAIxRx)
          WAFコマンドはRageダイスとダイスプールを個別に指定する。
          WAIコマンドはRageダイスをダイスプールの内数として指定する。

            例：難易度2、9ダイスプールでRageダイス3個の場合、それぞれ以下のようなコマンドとなる。
            2WAF6+3
            2WAI9R3

          難易度指定：成功数のカウント、判定成功と失敗、（Rageダイスがある場合）Brutal outcome、Critical処理、Total Failure、Critical Winのチェックを行う
          例) (難易度)WAF(通常ダイス)+(Rageダイス)
              (難易度)WAF(通常ダイス)
              (難易度)WAI(通常ダイス)R(Rageダイス)
              (難易度)WAI(通常ダイス)

          難易度省略：成功数のカウント、判定失敗、（Rageダイスがある場合）Brutal outcome、Critical処理、Total Failureのチェックを行う
                      判定成功チェックを行わない
                      Critical Winのヒントを出力
          例) WAF(通常ダイス)+(Rageダイス)
              WAF(通常ダイス)
              WAI(通常ダイス)R(Rageダイス)
              WAI(通常ダイス)

          難易度0指定：Critical処理と成功数のカウントを行い、全てのチェックを行わない
          例) 0WAF(通常ダイス)+(Rageダイス)
              0WAF(通常ダイス)
              0WAI(通常ダイス)+(Rageダイス)
              0WAI(通常ダイス)

        ";

        private static readonly Regex COMMAND_REG = new Regex(
            @"^(\d+)?(((WAF)(\d+)(\+(\d+))?)|((WAI)(\d+)(R(\d+))?))$",
            RegexOptions.Compiled);

        private const int DIFFICULTY_INDEX = 1;
        private const int DICE_POOL_RAGE_DICE_NO_INCLUDED_INDEX = 5;
        private const int RAGE_DICE_NO_INCLUDED_INDEX = 7;
        private const int COMMAND_RAGE_DICE_INCLUDED_INDEX = 9;
        private const int DICE_POOL_RAGE_DICE_INCLUDED_INDEX = 10;
        private const int RAGE_DICE_INCLUDED_INDEX = 12;

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

            var (dicePool, rageDicePool) = GetDicePools(m);
            if (dicePool < 0)
            {
                return Result.CreateBuilder("ダイスプール0のときにRageダイスは指定できません。").Build();
            }
            if (rageDicePool > 5)
            {
                return Result.CreateBuilder("5を超えるRageダイス指定はできません。").Build();
            }

            var (diceText, successDice, tenDice, _) = MakeDiceRoll(dicePool, randomizer);
            string resultText = $"({dicePool}D10";

            int rageTenDice = 0;
            int brutalOutcome = 0;

            if (rageDicePool >= 0)
            {
                var (rageDiceText, rageSuccessDice, rTenDice, brutalResultDice) = MakeDiceRoll(rageDicePool, randomizer);
                brutalOutcome = brutalResultDice / 2;
                rageTenDice = rTenDice;
                tenDice += rageTenDice;
                successDice += rageSuccessDice;

                resultText = $"{resultText}+{rageDicePool}D10) ＞ [{diceText}]+[{rageDiceText}] ";
            }
            else
            {
                resultText = $"{resultText}) ＞ [{diceText}] ";
            }

            successDice += GetCriticalSuccess(tenDice);

            int difficulty = m.Groups[DIFFICULTY_INDEX].Success ? int.Parse(m.Groups[DIFFICULTY_INDEX].Value) : NOT_CHECK_SUCCESS;

            return GetRollResult(resultText, successDice, tenDice, rageTenDice, brutalOutcome, difficulty);
        }

        private (int dicePool, int rageDicePool) GetDicePools(Match m)
        {
            int dicePool;
            int rageDicePool;

            string rageDiceIncludedCommand = m.Groups[COMMAND_RAGE_DICE_INCLUDED_INDEX].Value;
            if (!string.IsNullOrEmpty(rageDiceIncludedCommand) && rageDiceIncludedCommand == "WAI")
            {
                // Rage Diceを内数処理するの場合
                rageDicePool = m.Groups[RAGE_DICE_INCLUDED_INDEX].Success ? int.Parse(m.Groups[RAGE_DICE_INCLUDED_INDEX].Value) : -1;
                int dicePoolValue = int.Parse(m.Groups[DICE_POOL_RAGE_DICE_INCLUDED_INDEX].Value);
                dicePool = dicePoolValue - (rageDicePool < 0 ? 0 : rageDicePool);
                if (dicePoolValue > 0 && rageDicePool >= dicePoolValue)
                {
                    // 1 以上のダイスプール、かつ、Rageダイスがダイスプール以上のとき、ダイスプールが全てRageダイスになる。
                    dicePool = 0;
                    rageDicePool = dicePoolValue;
                }
            }
            else
            {
                // Rage DiceがPLによる内数指定の場合
                rageDicePool = m.Groups[RAGE_DICE_NO_INCLUDED_INDEX].Success ? int.Parse(m.Groups[RAGE_DICE_NO_INCLUDED_INDEX].Value) : -1;
                dicePool = int.Parse(m.Groups[DICE_POOL_RAGE_DICE_NO_INCLUDED_INDEX].Value);
            }

            return (dicePool, rageDicePool);
        }

        private Result GetRollResult(string resultText, int successDice, int tenDice, int rageTenDice, int brutalOutcome, int difficulty)
        {
            bool isCritical = tenDice >= 2;

            if (brutalOutcome > 0 && difficulty != 0)
            {
                successDice += 4;
                resultText = $"{resultText} [Brutal outcome] 自動失敗、または 成功数={successDice}";
            }
            else
            {
                resultText = $"{resultText} 成功数={successDice}";
            }

            if (difficulty > 0)
            {
                resultText = $"{resultText} 難易度={difficulty}";
                if (successDice >= difficulty)
                {
                    resultText = $"{resultText} 差分={successDice - difficulty}";

                    if (isCritical)
                    {
                        var resultData = Result.CreateBuilder($"{resultText}：判定成功! [Critical Win]").SetSuccess(true).SetCritical(true).Build();
                        return brutalOutcome > 0 ? Result.CreateBuilder(resultData.Text).Build() : resultData;
                    }
                    var resultDataSuccess = Result.CreateBuilder($"{resultText}：判定成功!").SetSuccess(true).Build();
                    return brutalOutcome > 0 ? Result.CreateBuilder(resultDataSuccess.Text).Build() : resultDataSuccess;
                }
                else
                {
                    if (successDice == 0)
                    {
                        return Result.CreateBuilder($"{resultText}：判定失敗! [Total Failure]").SetFumble(true).SetFailure(true).Build();
                    }
                    else
                    {
                        return Result.CreateBuilder($"{resultText}：判定失敗!").SetFailure(true).Build();
                    }
                }
            }
            else if (difficulty < 0)
            {
                if (successDice == 0)
                {
                    return Result.CreateBuilder($"{resultText}：判定失敗! [Total Failure]").SetFumble(true).SetFailure(true).Build();
                }
                else
                {
                    if (isCritical)
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

        private (string diceText, int successDice, int tenDice, int brutalResultDice) MakeDiceRoll(int dicePool, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(dicePool, 10);
            string diceText = string.Join(",", diceList);
            int successDice = diceList.Count(x => x >= 6);
            int tenDice = diceList.Count(x => x == 10);
            int brutalResultDice = diceList.Count(x => x == 1) + diceList.Count(x => x == 2);
            return (diceText, successDice, tenDice, brutalResultDice);
        }
    }
}
