using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// アニマラス
    /// </summary>
    public sealed class AniMalus : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly AniMalus Instance = new AniMalus();

        /// <inheritdoc/>
        public override string Id => "AniMalus";

        /// <inheritdoc/>
        public override string Name => "アニマラス";

        /// <inheritdoc/>
        public override string SortKey => "あにまらす";

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ステータスのダイス判定　n[+-b]AM<=t,x        n:能力値 b:修正値(省略可能) t:成功値 x:必要成功数
        例)3AM<=2,1: ダイスを3個振って、成功値2,必要成功数1で判定。その結果(成功数,成功・失敗,クリティカル,ファンブル)を表示

        ■探索技能のダイス判定　[+-b]AI<=t,x        t:探索技能レベル b:修正値(省略可能) x:必要成功数
        例)AI<=3,1: ダイスを3個振って、探索技能レベル3,必要成功数1で判定。その結果(成功数,成功・失敗,クリティカル,ファンブル)を表示

        ■攻撃判定　[+-b]AA<=t       t:戦闘技能レベル b:修正値(省略可能)
        例)AA<=3: ダイスを3個振って、戦闘技能レベル3で判定。その結果(成功・失敗,ダメージ,クリティカル,ファンブル)を表示

        ■防御判定　[+-b]AG=t        t:攻撃技能レベル b:修正値(省略可能)
        例)AG=2: ダイスを3個振って、攻撃技能レベル2で判定。その結果(成功・失敗,ダメージ軽減,クリティカル,ファンブル)を表示

        ■回避判定　[+-b]AD=t        t:攻撃技能レベル b:修正値(省略可能)
        例)AD=3: ダイスを1個振って、攻撃技能レベル3で判定。その結果(成功・失敗)を表示
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return ResoluteAction(command, randomizer)
                ?? ResoluteInvestigation(command, randomizer)
                ?? ResoluteAttacking(command, randomizer)
                ?? ResoluteGuarding(command, randomizer)
                ?? ResoluteDodging(command, randomizer);
        }

        private string WithSymbol(int number)
        {
            if (number == 0)
            {
                return "";
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
        /// ステータスの判定
        /// </summary>
        private Result? ResoluteAction(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)([-+]\d+)?AM<=(\d+),(\d)$");
            if (!m.Success)
            {
                return null;
            }

            var numDice = int.Parse(m.Groups[1].Value);
            var numBonus = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
            var numTarget = int.Parse(m.Groups[3].Value);
            var numSuccess = int.Parse(m.Groups[4].Value);

            var dice = randomizer.RollBarabara(numDice + numBonus, 6).OrderBy(x => x).ToArray();
            var diceText = string.Join(",", dice);
            var successNum = dice.Count(val => val <= numTarget);
            var isCritical = dice.Contains(1) && dice.Contains(2) && dice.Contains(3);
            var isFumble = dice.Contains(4) && dice.Contains(5) && dice.Contains(6);

            var isSuccess = successNum >= numSuccess;

            var sequence = new List<string>
            {
                $"({numDice}{WithSymbol(numBonus)}AM<={numTarget},{numSuccess})",
                diceText,
                $"成功数{successNum}",
                isSuccess ? "成功" : "失敗"
            };
            if (isCritical)
            {
                sequence.Add("クリティカル");
            }
            if (isFumble)
            {
                sequence.Add("ファンブル");
            }

            var text = string.Join(" ＞ ", sequence);
            return Result.CreateBuilder(text)
                .SetCondition(isSuccess)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .Build();
        }

        /// <summary>
        /// 探索技能の判定
        /// </summary>
        private Result? ResoluteInvestigation(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^([-+]\d+)?AI<=(\d+),(\d)$");
            if (!m.Success)
            {
                return null;
            }

            var numBonus = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
            var numTarget = int.Parse(m.Groups[2].Value);
            var numSuccess = int.Parse(m.Groups[3].Value);

            var dice = randomizer.RollBarabara(3 + numBonus, 6).OrderBy(x => x).ToArray();
            var diceText = string.Join(",", dice);
            var successNum = dice.Count(val => val <= numTarget);
            var isCritical = dice.Contains(1) && dice.Contains(2) && dice.Contains(3);
            var isFumble = dice.Contains(4) && dice.Contains(5) && dice.Contains(6);

            var isSuccess = successNum >= numSuccess;

            var sequence = new List<string>
            {
                $"({WithSymbol(numBonus)}AI<={numTarget},{numSuccess})",
                diceText,
                $"成功数{successNum}",
                isSuccess ? "成功" : "失敗"
            };
            if (isCritical)
            {
                sequence.Add("クリティカル");
            }
            if (isFumble)
            {
                sequence.Add("ファンブル");
            }

            var text = string.Join(" ＞ ", sequence);
            return Result.CreateBuilder(text)
                .SetCondition(isSuccess)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .Build();
        }

        /// <summary>
        /// 攻撃技能の判定
        /// </summary>
        private Result? ResoluteAttacking(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^([-+]\d+)?AA<=(\d+)$");
            if (!m.Success)
            {
                return null;
            }

            var numBonus = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
            var numTarget = int.Parse(m.Groups[2].Value);

            var dice = randomizer.RollBarabara(3 + numBonus, 6).OrderBy(x => x).ToArray();
            var diceText = string.Join(",", dice);
            var successNum = dice.Count(val => val <= numTarget);
            var isCritical = dice.Contains(1) && dice.Contains(2) && dice.Contains(3);
            var isFumble = dice.Contains(4) && dice.Contains(5) && dice.Contains(6);

            var damage1 = dice.Max();
            var damage2 = dice.Max();
            foreach (var idx in Enumerable.Range(1, numTarget))
            {
                if (dice.Count(x => x == idx) > 1)
                {
                    var nowDamage = damage1 + 3 * (dice.Count(x => x == idx) - 1);
                    if (damage2 < nowDamage)
                    {
                        damage2 = nowDamage;
                    }
                }
            }

            var isSuccess = successNum > 0;

            var sequence = new List<string>
            {
                $"({WithSymbol(numBonus)}AA<={numTarget})",
                diceText,
                $"成功数{successNum}",
                isSuccess ? "成功" : "失敗"
            };
            if (isSuccess)
            {
                sequence.Add($"最大ダメージ({damage2})");
            }
            if (isCritical)
            {
                sequence.Add("クリティカル");
            }
            if (isFumble)
            {
                sequence.Add("ファンブル");
            }

            var text = string.Join(" ＞ ", sequence);
            return Result.CreateBuilder(text)
                .SetCondition(isSuccess)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .Build();
        }

        /// <summary>
        /// 防御技能の判定
        /// </summary>
        private Result? ResoluteGuarding(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^([-+]\d+)?AG=(\d+)$");
            if (!m.Success)
            {
                return null;
            }

            var numBonus = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
            var numTarget = int.Parse(m.Groups[2].Value);

            var dice = randomizer.RollBarabara(3 + numBonus, 6).OrderBy(x => x).ToArray();
            var diceText = string.Join(",", dice);
            var successNum = dice.Count(x => x == numTarget);
            var isCritical = dice.Contains(1) && dice.Contains(2) && dice.Contains(3);
            var isFumble = dice.Contains(4) && dice.Contains(5) && dice.Contains(6);

            var isSuccess = successNum > 0;

            var sequence = new List<string>
            {
                $"({WithSymbol(numBonus)}AG={numTarget})",
                diceText,
                $"成功数{successNum}",
                isSuccess ? $"成功 ＞ ダメージ軽減({successNum * 2})" : "失敗"
            };
            if (isCritical)
            {
                sequence.Add("クリティカル");
            }
            if (isFumble)
            {
                sequence.Add("ファンブル");
            }

            var text = string.Join(" ＞ ", sequence);
            return Result.CreateBuilder(text)
                .SetCondition(isSuccess)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .Build();
        }

        /// <summary>
        /// 回避技能の判定
        /// </summary>
        private Result? ResoluteDodging(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^([-+]\d+)?AD=(\d+)$");
            if (!m.Success)
            {
                return null;
            }

            var numBonus = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
            var numTarget = int.Parse(m.Groups[2].Value);

            var dice = randomizer.RollBarabara(1 + numBonus, 6);
            var diceText = string.Join(",", dice);
            var successNum = dice.Count(x => x == numTarget);

            var isSuccess = successNum > 0;

            var sequence = new List<string>
            {
                $"({WithSymbol(numBonus)}AD={numTarget})",
                diceText,
                $"成功数{successNum}",
                isSuccess ? "成功(ダメージ無効)" : "失敗"
            };

            var text = string.Join(" ＞ ", sequence);
            return Result.CreateBuilder(text)
                .SetCondition(isSuccess)
                .Build();
        }

    }
}
