using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// エンゼルギア 天使大戦TRPG The 2nd Editon
    /// </summary>
    public sealed class AngelGear : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly AngelGear Instance = new AngelGear();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "AngelGear";

        /// <inheritdoc/>
        public override string Name => "エンゼルギア 天使大戦TRPG The 2nd Editon";

        /// <inheritdoc/>
        public override string SortKey => "えんせるきあ2";

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定　nAG[s][±a]
        []内は省略可能。
        n:判定値
        s:技能値
        a:修正
        （例）
        12AG 10AG3±20

        ・感情表　ET
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)AG(\d+)?(([+-]\d+)*)$");
            if (m.Success)
            {
                return ResoluteAction(
                    Convert.ToInt32(m.Groups[1].Value),
                    m.Groups[2].Success ? (int?)int.Parse(m.Groups[2].Value) : null,
                    m.Groups[3].Value,
                    command,
                    randomizer);
            }

            return RollTable(command, TABLES, randomizer);
        }

        private Result ResoluteAction(int numDice, int? skillValue, string modify, string command, IRandomizer randomizer)
        {
            string gospel = "";
            var dice = randomizer.RollBarabara(numDice, 6).OrderBy(x => x);
            var diceText = string.Join(",", dice);
            var modifyN = 0;
            var success = 0;
            if (skillValue.HasValue)
            {
                success = dice.Count(val => val <= skillValue.Value);
                if (!string.IsNullOrEmpty(modify))
                {
                    modifyN = ArithmeticEvaluator.Eval(modify, RoundType.Floor) ?? 0;
                }
            }

            if (success + modifyN >= 100)
            {
                gospel = "(福音発生)";
            }

            string modifyStr = modifyN >= 0 ? $"+{modifyN}" : modifyN.ToString();
            var output = $"({command}) ＞ {success}[{diceText}]{modifyStr} ＞ 成功数: {success + modifyN}{gospel}";
            if (success + modifyN >= 100)
            {
                return Result.CreateBuilder(output).SetCritical(true).SetSuccess(true).Build();
            }
            else if (success + modifyN > 0)
            {
                return Result.CreateBuilder(output).SetSuccess(true).Build();
            }
            else
            {
                return Result.CreateBuilder(output).SetFailure(true).Build();
            }
        }

    }
}