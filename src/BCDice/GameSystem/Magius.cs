using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// MAGIUS
    /// </summary>
    public sealed class Magius : GameSystemBase
    {
        public static readonly Magius Instance = new Magius();

        public override string Id => "Magius";
        public override string Name => "MAGIUS";
        public override string SortKey => "まきうす";
        public override bool SortBarabaraDice => true;

        public override string HelpMessage => @"
        ■能力値判定　MA+x>=t        x:修正値 t:目標値
        例)MA>=7: ダイスを2個振って、その結果を表示

        ■技能値判定　MS+x>=t        x:修正値 t:目標値
        例)MS>=7: ダイスを3個振って、そのうち上位2つを採用し、結果を表示

        ";

        private static readonly Regex AbilityRegex = new Regex(
            @"^MA([+-]\d+)*>=(\d+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SkillRegex = new Regex(
            @"^MS([+-]\d+)*>=(\d+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return ResoluteAbilityAction(command, randomizer) ?? ResoluteSkillAction(command, randomizer);
        }

        private string WithSymbol(int number)
        {
            if (number == 0) return "";
            if (number > 0) return $"+{number}";
            return number.ToString();
        }

        private Result? ResoluteAbilityAction(string command, IRandomizer randomizer)
        {
            var m = AbilityRegex.Match(command);
            if (!m.Success)
                return null;

            int modify = m.Groups[1].Success && !string.IsNullOrEmpty(m.Groups[1].Value)
                ? ArithmeticEvaluator.Eval(m.Groups[1].Value, RoundType) ?? 0
                : 0;
            int target = int.Parse(m.Groups[2].Value);

            var dices = randomizer.RollBarabara(2, 6).OrderBy(x => x).ToList();
            string diceText = string.Join(",", dices);
            int diceAdd = dices.Sum();
            int total = diceAdd + modify;

            bool isSuccess = total >= target;
            string resultText = isSuccess ? Translate("success") : Translate("failure");

            var sequence = new List<string>
            {
                $"({command})",
                $"[{diceText}]{WithSymbol(modify)}",
                total.ToString(),
                resultText,
            };

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetCondition(isSuccess)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private Result? ResoluteSkillAction(string command, IRandomizer randomizer)
        {
            var m = SkillRegex.Match(command);
            if (!m.Success)
                return null;

            int modify = m.Groups[1].Success && !string.IsNullOrEmpty(m.Groups[1].Value)
                ? ArithmeticEvaluator.Eval(m.Groups[1].Value, RoundType) ?? 0
                : 0;
            int target = int.Parse(m.Groups[2].Value);

            var dices = randomizer.RollBarabara(3, 6).OrderBy(x => x).ToList();
            string diceText = string.Join(",", dices);
            int diceAdd = dices[1] + dices[2];
            int total = diceAdd + modify;

            bool isSuccess = total >= target;
            string resultText = isSuccess ? Translate("success") : Translate("failure");

            var sequence = new List<string>
            {
                $"({command})",
                $"[{diceText}]{WithSymbol(modify)}",
                total.ToString(),
                resultText,
            };

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetCondition(isSuccess)
                .SetRands(randomizer.RandResults)
                .Build();
        }
    }
}
