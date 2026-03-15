using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ペルソナTRPG-O
    /// </summary>
    public sealed class PersonaO : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly PersonaO Instance = new PersonaO();

        /// <inheritdoc/>
        public override string Id => "PersonaO";

        /// <inheritdoc/>
        public override string Name => "ペルソナTRPG-O";

        /// <inheritdoc/>
        public override string SortKey => "へるそなTRPGO";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・基本判定
        　PTx@y　x：目標値、y：クリティカル値（省略時は5）
        　例）PT60　PT90@10

        ・ダメージ計算
        　nPD+(x+y*2)%(z-a)-b　n：ダイス個数、x：スキル固定値、y：ボーナス、z：バフ倍率、a：耐性、b：敵防御力
        　nPD+(x+y*2)までがスキルによる素のダメージ、zおよびaは計算式を入れてよい。

        　例）ソニックパンチ、力B2点、
        　　　タルカジャがかかっており、打撃耐性あり、
        　　　目標の物理防御力は2点

        　　　2PD+(20+2*2)%(100+50-50)-2
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollAttack(command, randomizer) ?? RollDamage(command, randomizer);
        }

        private Result? RollAttack(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^PT(-?\d+)?(@(-?\d+))?$");
            if (!m.Success)
            {
                return null;
            }

            int successRate = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
            int criticalBorder = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 5;
            int diceValue = randomizer.RollOnce(100);

            Result result;
            string resultText;
            if (diceValue <= criticalBorder)
            {
                resultText = "クリティカル";
                result = Result.CreateBuilder($"D100<={successRate}@{criticalBorder} ＞ {diceValue} ＞ {resultText}")
                    .SetCritical(true)
                    .SetSuccess(true)
                    .SetRands(randomizer.RandResults)
                    .SetDetailedRands(randomizer.DetailedRandResults)
                    .Build();
            }
            else if (diceValue <= successRate)
            {
                resultText = "成功";
                result = Result.CreateBuilder($"D100<={successRate}@{criticalBorder} ＞ {diceValue} ＞ {resultText}")
                    .SetSuccess(true)
                    .SetRands(randomizer.RandResults)
                    .SetDetailedRands(randomizer.DetailedRandResults)
                    .Build();
            }
            else
            {
                resultText = "失敗";
                result = Result.CreateBuilder($"D100<={successRate}@{criticalBorder} ＞ {diceValue} ＞ {resultText}")
                    .SetFailure(true)
                    .SetRands(randomizer.RandResults)
                    .SetDetailedRands(randomizer.DetailedRandResults)
                    .Build();
            }

            return result;
        }

        private Result? RollDamage(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)PD\+(-?\d+)%(-?\d+)-(\d+)$");
            if (!m.Success)
            {
                return null;
            }

            int dice = int.Parse(m.Groups[1].Value);
            int kotei = int.Parse(m.Groups[2].Value);
            int hosei = int.Parse(m.Groups[3].Value);
            int bougyo = int.Parse(m.Groups[4].Value);
            var diceList = randomizer.RollBarabara(dice, 10);
            int diceSum = diceList.Sum();
            int dmg = diceSum + (int)(hosei * kotei / 100.0) - bougyo;
            string text = $"{dice}D10+{kotei}＊{hosei}%-{bougyo} ＞ [{string.Join(",", diceList)}]+{kotei}＊{hosei}%-{bougyo} ＞ {dmg} ダメージ！";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

    }
}
