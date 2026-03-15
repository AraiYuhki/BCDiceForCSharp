using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Arithmetic;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class Irisbane : GameSystemBase
    {
        public static readonly Irisbane Instance = new Irisbane();

        public override string Id => "Irisbane";
        public override string Name => "\u77b3\u9038\u3089\u3055\u306c\u30a4\u30ea\u30b9\u30d9\u30a4\u30f3";
        public override string SortKey => "\u3072\u3068\u307f\u305d\u3089\u3055\u306c\u3044\u308a\u3059\u3078\u3044\u3093";
        public override RoundType RoundType => RoundType.Ceiling;
        public override bool SortBarabaraDice => true;

        private static readonly Regex AttackReg = new Regex(
            @"^AT(?:TACK|K)?([+\-*/()\d]+)@([+\-*/()\d]+)<=([+\-*/()\d]+)(?:\[([+-])([+\-*/()\d]+)\])?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Dictionary<string, string> ALIAS = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "SSI", "SCENESITUATION" },
        };

        private static readonly string[] SituationTable = new[]
        {
            "\u3010\u65e5\u5e38\u3011\u4f55\u4e00\u3064\u5909\u308f\u308b\u3053\u3068\u306e\u7121\u3044\u65e5\u3005\u306e\u4e00\u5e55\u3002",
            "\u3010\u6e96\u5099\u3011\u4f55\u304b\u3092\u70ba\u3059\u305f\u3081\u306e\u7528\u610f\u3092\u3059\u308b\u4e00\u5e55\u3002",
            "\u3010\u8da3\u5473\u3011\u81ea\u5206\u306e\u6642\u9593\u3092\u3001\u6709\u52b9\u6d3b\u7528\u3057\u3066\u3044\u308b\u4e00\u5e55\u3002",
            "\u3010\u55ab\u8336\u3011\u4e00\u606f\u5165\u308c\u3001\u55dc\u597d\u54c1\u3092\u55ab\u3080\u6642\u306e\u4e00\u5e55\u3002",
            "\u3010\u935b\u9149\u3011\u4f53\u3092\u9039\u3048\u3001\u5fc3\u3092\u990a\u3046\u4fee\u7df4\u306e\u4e00\u5e55\u3002",
            "\u3010\u8077\u52d9\u3011\u5f79\u5272\u306e\u5143\u3001\u4ed5\u4e8b\u306b\u7cbe\u3092\u51fa\u3059\u6642\u306e\u4e00\u5e55\u3002",
            "\u3010\u79fb\u52d5\u3011\u4f55\u51e6\u304b\u304b\u3089\u4f55\u51e6\u304b\u3078\u3068\u5411\u304b\u3046\u4e00\u5e55\u3002",
            "\u3010\u5893\u524d\u3011\u6545\u4eba\u304c\u7720\u308b\u5834\u6240\u3078\u3068\u8d74\u304f\u4e00\u5e55\u3002",
            "\u3010\u64cd\u4f5c\u3011\u4f55\u304b\u3092\u64cd\u308a\u3001\u671b\u307f\u3092\u679c\u305f\u3057\u3066\u3044\u308b\u4e00\u5e55\u3002",
            "\u3010\u98df\u4e8b\u3011\u4f55\u304b\u3092\u7ce7\u3068\u3057\u3001\u5df1\u306e\u529b\u3092\u84c4\u3048\u308b\u4e00\u5e55\u3002",
            "\u3010\u4f11\u606f\u3011\u65e5\u3005\u306e\u5408\u9593\u306e\u3001\u60a3\u3044\u306e\u4e00\u5e55\u3002",
            "\u3010\u5922\u5e7b\u3011\u73fe\u5b9f\u306b\u5b58\u5728\u3057\u306a\u3044\u4f55\u304b\u3078\u3068\u8019\u308b\u4e00\u5e55\u3002",
        };

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            if (ALIAS.TryGetValue(command, out var aliased))
                command = aliased;

            if (command.Equals("SCENESITUATION", StringComparison.OrdinalIgnoreCase))
            {
                int d1 = randomizer.RollOnce(6);
                int d2 = randomizer.RollOnce(6);
                int row = d1 <= 3 ? 0 : 6;
                int col = d2 - 1;
                string entry = SituationTable[row + col];
                return Result.Success("\u30b7\u30c1\u30e5\u30a8\u30fc\u30b7\u30e7\u30f3(" + d1 + d2 + ") \uff1e " + entry);
            }

            return RollAttack(command, randomizer);
        }

        private Result? RollAttack(string command, IRandomizer randomizer)
        {
            var m = AttackReg.Match(command);
            if (!m.Success) return null;

            int? power = ArithmeticEvaluator.Eval(m.Groups[1].Value, RoundType.Ceiling);
            int? diceCount = ArithmeticEvaluator.Eval(m.Groups[2].Value, RoundType.Ceiling);
            int? border = ArithmeticEvaluator.Eval(m.Groups[3].Value, RoundType.Ceiling);
            if (power == null || diceCount == null || border == null) return null;

            string? modOp = m.Groups[4].Success ? m.Groups[4].Value : null;
            int? modVal = null;
            if (m.Groups[5].Success)
            {
                modVal = ArithmeticEvaluator.Eval(m.Groups[5].Value, RoundType.Ceiling);
                if (modVal == null) return null;
            }
            if (modOp != null && modVal == null) return null;

            int p = Math.Max(0, power.Value);
            int b = Math.Max(1, Math.Min(6, border.Value));
            int dc = diceCount.Value;

            string cmdText = "(ATTACK" + p + "@" + dc + "<=" + b;
            if (modOp != null) cmdText += "[" + modOp + modVal + "]";
            cmdText += ")";

            if (dc <= 0)
                return Result.Success(cmdText + " \uff1e \u5224\u5b9a\u6570\u304c 0 \u3067\u3059");

            var dices = randomizer.RollBarabara(dc, 6).OrderBy(x => x).ToList();
            int successCount = dices.Count(d => d <= b);
            int damage = successCount * p;

            var parts = new List<string> { cmdText, string.Join(",", dices), "\u6210\u529f\u30c0\u30a4\u30b9\u6570 " + successCount };
            if (successCount > 0)
            {
                parts.Add("\u00d7 \u653b\u6483\u529b " + p);
                if (modOp != null && modVal != null)
                {
                    parts.Add("\u30c0\u30e1\u30fc\u30b8 " + damage + modOp + modVal);
                    damage = modOp == "+" ? damage + modVal.Value : damage - modVal.Value;
                    if (damage < 0) damage = 0;
                    parts.Add(damage.ToString());
                }
                else
                {
                    parts.Add("\u30c0\u30e1\u30fc\u30b8 " + damage);
                }
            }

            string text = string.Join(" \uff1e ", parts);
            return Result.CreateBuilder(text).SetCondition(successCount > 0).Build();
        }
    }
}
