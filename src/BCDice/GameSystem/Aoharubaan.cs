using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// あおはるばーんっ
    /// </summary>
    public sealed class Aoharubaan : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Aoharubaan Instance = new Aoharubaan();

        private static readonly Regex JudgeRollReg = new Regex(
            @"^(1d6?|d6)(\+\d+)?(>=|=>)(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Dictionary<string, string> Alias = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "KR", "KREACTION" },
        };

        private static readonly Dictionary<string, object> Tables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public override string Id => "Aoharubaan";

        /// <inheritdoc/>
        public override string Name => "あおはるばーんっ";

        /// <inheritdoc/>
        public override string SortKey => "あおはるはあんつ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        カレカノ反応表（ KR, KReaction ）
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            string resolvedCommand = Alias.TryGetValue(command, out string? aliasValue) ? aliasValue : command;
            var m = JudgeRollReg.Match(resolvedCommand);
            if (m.Success)
            {
                return RollJudge(m.Groups[2].Value, m.Groups[4].Value, randomizer);
            }

            return RollTables(resolvedCommand, Tables);
        }

        private Result? RollJudge(string modifierExpression, string borderExpression, IRandomizer randomizer)
        {
            int? modifierNullable = !string.IsNullOrEmpty(modifierExpression)
                ? ArithmeticEvaluator.Eval(modifierExpression, RoundType.Floor)
                : null;
            int modifier = modifierNullable ?? 0;
            int border = Convert.ToInt32(borderExpression);

            string commandText = MakeCommandText(modifierNullable, border);

            int dice = randomizer.RollOnce(6);
            int score = dice + modifier;

            bool isSuccess = score >= border;
            bool isRight = isSuccess && score == border;
            bool isExcellent = isSuccess && score >= 7;

            var resultElements = new List<string>();
            resultElements.Add(isSuccess ? "成功" : "失敗");
            if (isRight)
            {
                resultElements.Add("ピタリ賞");
            }
            if (isExcellent)
            {
                resultElements.Add("限界突破");
            }

            var messageElements = new List<string>();
            messageElements.Add(commandText);
            if (modifierNullable.HasValue)
            {
                messageElements.Add($"{dice}+{modifier}");
            }
            messageElements.Add(score.ToString());
            messageElements.Add(string.Join(" ＆ ", resultElements));

            string text = string.Join(" ＞ ", messageElements);

            return Result.CreateBuilder(text)
                .SetCondition(isSuccess)
                .SetCritical(isRight || isExcellent)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private string MakeCommandText(int? modifier, int border)
        {
            string cmd = "1D6";
            if (modifier.HasValue)
            {
                cmd = $"{cmd}+{modifier.Value}";
            }
            cmd = $"{cmd}>={border}";
            return $"({cmd})";
        }

    }
}
