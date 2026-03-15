using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ザ・ループTRPG
    /// </summary>
    public sealed class TalesFromTheLoop : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly TalesFromTheLoop Instance = new TalesFromTheLoop();

        /// <inheritdoc/>
        public override string Id => "TalesFromTheLoop";

        /// <inheritdoc/>
        public override string Name => "ザ・ループTRPG";

        /// <inheritdoc/>
        public override string SortKey => "さるうふTRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ 判定コマンド(nTFLx-x+x or nTFLx+x-x)
          (必要成功数)TFL(判定ダイス数)+/-(修正ダイス数)

        ※ 必要成功数と修正ダイス数は省略可能

        例1) 必要成功数1、判定ダイスは能力値3
              1TFL3

        例2）必要成功数不明、あるいはダイスボットの成功判定を使わない、判定ダイスは能力値3+技能1で4、アイテムの修正+1
              TFL4+1

        例3）必要成功数1、判定ダイスは能力値2+技能1で3、コンディションにチェックが2つ、アイテムの修正+1
              1TFL3-2+1
             あるいは以下のようにカッコ書きで内訳を詳細に記述することも可能。
              1TFL(2+1)-(1+1)+1
             修正ダイスのプラスとマイナスは逆でもよい。
              1TFL(2+1)+1-(1+1)
        ";

        private static readonly Regex COMMAND_REG = new Regex(
            @"^(\d+)?TFL(\d+)([+\-]\d+)*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = COMMAND_REG.Match(command);
            if (!m.Success)
            {
                return null;
            }

            // コマンド文字列からdifficulty と dice_pool を取得
            // Ruby: difficulty, dice_pool = parsed.command.split("TFL", 2).map(&:to_i)
            var parts = command.Split(new[] { "TFL" }, 2, StringSplitOptions.None);
            int difficulty = string.IsNullOrEmpty(parts[0]) ? 0 : int.Parse(parts[0]);

            // dice_pool の後ろに +/- の修正がある場合があるので、数式を解析
            // 簡易的にTFL以降の全体を評価
            string afterTFL = parts.Length > 1 ? parts[1] : "0";
            int dicePool = EvalSimpleExpression(afterTFL);

            if (dicePool <= 0)
            {
                dicePool = 1;
            }

            var (abilityDiceText, successDice) = MakeDiceRoll(dicePool, randomizer);
            return MakeDiceRollText(difficulty, dicePool, abilityDiceText, successDice);
        }

        /// <summary>
        /// 簡易的な加減算式を評価する (例: "3-2+1" => 2)
        /// </summary>
        private static int EvalSimpleExpression(string expr)
        {
            int result = 0;
            int sign = 1;
            int current = 0;
            bool hasDigit = false;

            foreach (char c in expr)
            {
                if (c >= '0' && c <= '9')
                {
                    current = current * 10 + (c - '0');
                    hasDigit = true;
                }
                else if (c == '+' || c == '-')
                {
                    if (hasDigit)
                    {
                        result += sign * current;
                        current = 0;
                        hasDigit = false;
                    }
                    sign = c == '+' ? 1 : -1;
                }
                // カッコなどは無視（数字のみ抽出）
            }
            if (hasDigit)
            {
                result += sign * current;
            }
            return result;
        }

        private Result? MakeDiceRollText(int difficulty, int dicePool, string abilityDiceText, int successDice)
        {
            var diceCountText = $"({dicePool}D6) ＞ {abilityDiceText} 成功数:{successDice}";
            int pushDice = dicePool - successDice;
            string rerollCommand = "";

            if (pushDice > 0)
            {
                diceCountText = $"{diceCountText} 振り直し可能:{pushDice}";
                rerollCommand = $"\n振り直しコマンド: TFL{pushDice}";
            }

            if (difficulty > 0)
            {
                if (successDice >= difficulty)
                {
                    return Result.CreateBuilder($"{diceCountText} 成功！{rerollCommand}").SetSuccess(true).Build();
                }
                else
                {
                    return Result.CreateBuilder($"{diceCountText} 失敗！{rerollCommand}").SetFailure(true).Build();
                }
            }

            return Result.CreateBuilder($"{diceCountText}{rerollCommand}").Build();
        }

        private (string diceText, int successDice) MakeDiceRoll(int dicePool, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(dicePool, 6);
            int successDice = diceList.Count(x => x == 6);
            string diceText = $"[{string.Join(",", diceList)}]";
            return (diceText, successDice);
        }
    }
}
