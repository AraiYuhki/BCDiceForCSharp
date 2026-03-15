using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// リミナル
    /// </summary>
    public sealed class Liminal : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Liminal Instance = new Liminal();

        /// <inheritdoc/>
        public override string Id => "Liminal";

        /// <inheritdoc/>
        public override string Name => "リミナル";

        /// <inheritdoc/>
        public override string SortKey => "りみなる";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■技能判定　LMx+b>=t+m   x:技能レベル b:ボーナス t:難易度 m:敵の技能レベル(対抗判定)

        例)LM2>=8:  技能レベル2,難易度8で技能判定し、その結果を表示。(クリティカル成功も表示)
           LM3+2>=9:技能レベル3,ボーナス+2,難易度9で技能判定し、その結果を表示。( 〃 )
           LM0>=8:  技能なし,難易度8で技能判定する。(難易度+2は自動的に足されます)

        ■イニシアティヴ判定　LIx+b>=t+m   x:認識力レベル b:ボーナス t:難易度 m:敵の認識力レベル
        例)LI2>=8+2:  認識力レベル2,難易度8,敵認識力レベル2で技能判定し、その結果を表示。
           LI0>=8+2:  認識力なし,難易度8,敵認識力レベル2で技能判定する。(難易度加算なし)
        ";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return ResoluteAction(command, randomizer) ?? ResoluteInitiative(command, randomizer);
        }

        private Result? ResoluteAction(string command, IRandomizer randomizer)
        {
            // LMx+b>=t の形式をパースする
            var m = Regex.Match(command, @"^LM(\d+)([+-]\d+)?>=(\d+(?:[+-]\d+)*)$");
            if (!m.Success)
            {
                return null;
            }

            var skillLevel = int.Parse(m.Groups[1].Value);
            var bonus = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
            var difficulty = EvalDifficulty(m.Groups[3].Value);

            var dice = randomizer.RollBarabara(2, 6);
            var diceTotal = dice.Sum();
            var total = diceTotal + skillLevel + bonus;

            if (skillLevel == 0)
            {
                difficulty += 2;
            }

            var isSuccess = total >= difficulty;
            var isCritical = total >= difficulty + 5;
            var isFumble = diceTotal == 2;

            if (isFumble)
            {
                isCritical = false;
                isSuccess = false;
            }

            string statusLabel;
            if (isFumble)
            {
                statusLabel = "1ゾロ";
            }
            else if (isCritical)
            {
                statusLabel = "クリティカル";
            }
            else
            {
                statusLabel = isSuccess ? "成功" : "失敗";
            }

            var sequence = new[]
            {
                $"(LM{skillLevel}{WithSymbol(bonus)}>={difficulty})",
                $"{diceTotal}[{string.Join(",", dice)}]{WithSymbol(skillLevel + bonus)}",
                total.ToString(),
                statusLabel,
            };

            var text = string.Join(" ＞ ", sequence);

            var builder = Result.CreateBuilder(text)
                .SetCondition(isSuccess)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .SetRands(randomizer.RandResults);

            return builder.Build();
        }

        private Result? ResoluteInitiative(string command, IRandomizer randomizer)
        {
            // LIx+b>=t の形式をパースする
            var m = Regex.Match(command, @"^LI(\d+)([+-]\d+)?>=(\d+(?:[+-]\d+)*)$");
            if (!m.Success)
            {
                return null;
            }

            var skillLevel = int.Parse(m.Groups[1].Value);
            var bonus = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
            var difficulty = EvalDifficulty(m.Groups[3].Value);

            var dice = randomizer.RollBarabara(2, 6);
            var diceTotal = dice.Sum();
            var total = diceTotal + skillLevel + bonus;

            // イニシアティヴ判定では技能レベル0でも難易度加算なし

            var isSuccess = total >= difficulty;
            var isCritical = total >= difficulty + 5;
            var isFumble = diceTotal == 2;

            if (isFumble)
            {
                isCritical = false;
                isSuccess = false;
            }

            string statusLabel;
            if (isFumble)
            {
                statusLabel = "1ゾロ";
            }
            else if (isCritical)
            {
                statusLabel = "クリティカル";
            }
            else
            {
                statusLabel = isSuccess ? "成功" : "失敗";
            }

            var sequence = new[]
            {
                $"(LI{skillLevel}{WithSymbol(bonus)}>={difficulty})",
                $"{diceTotal}[{string.Join(",", dice)}]{WithSymbol(skillLevel + bonus)}",
                total.ToString(),
                statusLabel,
            };

            var text = string.Join(" ＞ ", sequence);

            var builder = Result.CreateBuilder(text)
                .SetCondition(isSuccess)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .SetRands(randomizer.RandResults);

            return builder.Build();
        }

        private string WithSymbol(int number)
        {
            if (number == 0)
            {
                return "+0";
            }
            else if (number > 0)
            {
                return $"+{number}";
            }
            else
            {
                return number.ToString();
            }
        }

        /// <summary>
        /// "8+2" のような難易度文字列を評価して合計値を返す
        /// </summary>
        private int EvalDifficulty(string expr)
        {
            var total = 0;
            var matches = Regex.Matches(expr, @"[+-]?\d+");
            foreach (Match m in matches)
            {
                total += int.Parse(m.Value);
            }
            // 先頭が符号なしの場合、最初のマッチは正の数として扱われる
            if (total == 0 && expr.Length > 0 && char.IsDigit(expr[0]))
            {
                // Regex.Matches already handles this correctly
            }
            return total;
        }
    }
}
