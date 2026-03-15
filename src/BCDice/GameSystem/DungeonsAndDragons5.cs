using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class DungeonsAndDragons5 : GameSystemBase
    {
        public static readonly DungeonsAndDragons5 Instance = new DungeonsAndDragons5();
        public override string Id => "DungeonsAndDragons5";
        public override string Name => "ダンジョンズ＆ドラゴンズ第5版";
        public override string SortKey => "たんしよんすあんととらこんす5";
        public override string HelpMessage => @"
        ・攻撃ロール AT[x][@c][>=t][y]
        ・能力値判定 AR[x][>=t][y]
        ・両手持ちのダメージ 2HnDx[m]
        ";

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return AttackRoll(command, randomizer) ?? AbilityRoll(command, randomizer) ?? TwohandsDamageRoll(command, randomizer);
        }

        private string NumberWithSignFromInt(int number)
        {
            if (number == 0) return "";
            if (number > 0) return "+" + number.ToString();
            return number.ToString();
        }

        private Result? AttackRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^AT([-+]\d+)?(@(\d+))?(>=(\d+))?([AD]?)$");
            if (!m.Success) return null;

            var modify = m.Groups[1].Success ? Convert.ToInt32(m.Groups[1].Value) : 0;
            var criticalNo = m.Groups[3].Success ? Convert.ToInt32(m.Groups[3].Value) : 0;
            var difficulty = m.Groups[5].Success ? Convert.ToInt32(m.Groups[5].Value) : 0;
            var advantage = m.Groups[6].Value;

            var diceCommand = "AT" + NumberWithSignFromInt(modify);
            if (criticalNo > 0)
                diceCommand += "@" + criticalNo.ToString();
            else
                criticalNo = 20;
            if (difficulty > 0)
                diceCommand += ">=" + difficulty.ToString();
            if (!string.IsNullOrEmpty(advantage))
                diceCommand += advantage;

            var output = new List<string> { "(" + diceCommand + ")" };

            int usedie;
            string rollDie;
            if (string.IsNullOrEmpty(advantage))
            {
                usedie = randomizer.RollOnce(20);
                rollDie = usedie.ToString();
            }
            else
            {
                var dice = randomizer.RollBarabara(2, 20);
                rollDie = "[" + string.Join(",", dice) + "]";
                usedie = advantage == "A" ? dice.Max() : dice.Min();
            }

            if (modify != 0)
            {
                output.Add(rollDie + NumberWithSignFromInt(modify));
                output.Add((usedie + modify).ToString());
            }
            else
            {
                if (!string.IsNullOrEmpty(advantage))
                    output.Add(rollDie);
                output.Add(usedie.ToString());
            }

            bool isCritical = false;
            bool isFumble = false;
            bool isSuccess = false;

            if (usedie >= criticalNo)
            {
                isCritical = true;
                isSuccess = true;
                output.Add(Translate("critical"));
            }
            else if (usedie == 1)
            {
                isFumble = true;
                output.Add(Translate("fumble"));
            }
            else if (difficulty > 0)
            {
                if (usedie + modify >= difficulty)
                {
                    isSuccess = true;
                    output.Add(Translate("success"));
                }
                else
                {
                    output.Add(Translate("failure"));
                }
            }

            var builder = Result.CreateBuilder(string.Join(" ▶ ", output))
                .SetRands(randomizer.RandResults);
            if (difficulty > 0 || isCritical || isFumble)
                builder = builder.SetCondition(isSuccess);
            if (isCritical)
                builder = builder.SetCritical(true);
            if (isFumble)
                builder = builder.SetFumble(true);
            return builder.Build();
        }

        private Result? AbilityRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^AR([-+]\d+)?(>=(\d+))?([AD]?)$");
            if (!m.Success) return null;

            var modify = m.Groups[1].Success ? Convert.ToInt32(m.Groups[1].Value) : 0;
            var difficulty = m.Groups[3].Success ? Convert.ToInt32(m.Groups[3].Value) : 0;
            var advantage = m.Groups[4].Value;

            var diceCommand = "AR" + NumberWithSignFromInt(modify);
            if (difficulty > 0)
                diceCommand += ">=" + difficulty.ToString();
            if (!string.IsNullOrEmpty(advantage))
                diceCommand += advantage;

            var output = new List<string> { "(" + diceCommand + ")" };

            int usedie;
            string rollDie;
            if (string.IsNullOrEmpty(advantage))
            {
                usedie = randomizer.RollOnce(20);
                rollDie = usedie.ToString();
            }
            else
            {
                var dice = randomizer.RollBarabara(2, 20);
                rollDie = "[" + string.Join(",", dice) + "]";
                usedie = advantage == "A" ? dice.Max() : dice.Min();
            }

            if (modify != 0)
            {
                output.Add(rollDie + NumberWithSignFromInt(modify));
                output.Add((usedie + modify).ToString());
            }
            else
            {
                if (!string.IsNullOrEmpty(advantage))
                    output.Add(rollDie);
                output.Add(usedie.ToString());
            }

            bool isSuccess = false;
            if (difficulty > 0)
            {
                if (usedie + modify >= difficulty)
                {
                    isSuccess = true;
                    output.Add(Translate("success"));
                }
                else
                {
                    output.Add(Translate("failure"));
                }
            }

            var builder = Result.CreateBuilder(string.Join(" ▶ ", output))
                .SetRands(randomizer.RandResults);
            if (difficulty > 0)
                builder = builder.SetCondition(isSuccess);
            return builder.Build();
        }

        private Result? TwohandsDamageRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^2H(\d+)D(\d+)([+-]\d+)?$");
            if (!m.Success) return null;

            var diceCount = Convert.ToInt32(m.Groups[1].Value);
            var diceNumber = Convert.ToInt32(m.Groups[2].Value);
            var modify = m.Groups[3].Success ? Convert.ToInt32(m.Groups[3].Value) : 0;
            var modStr = NumberWithSignFromInt(modify);

            var output = new List<string> { "(2H" + diceCount.ToString() + "D" + diceNumber.ToString() + modStr + ")" };
            var dice = randomizer.RollBarabara(diceCount, diceNumber);
            output.Add("[" + string.Join(",", dice) + "]" + modStr);

            var exDice = new List<int>();
            var newDice = new List<int>();
            var sumDice = 0;

            foreach (var num in dice)
            {
                if (num > 2) { sumDice += num; exDice.Add(num); }
                else { var oneDie = randomizer.RollOnce(diceNumber); sumDice += oneDie; newDice.Add(oneDie); }
            }

            if (newDice.Count > 0)
                output.Add("[" + string.Join(",", exDice) + "][" + string.Join(",", newDice) + "]" + modStr);
            output.Add((sumDice + modify).ToString());

            return Result.CreateBuilder(string.Join(" ▶ ", output))
                .SetRands(randomizer.RandResults)
                .Build();
        }
    }
}
