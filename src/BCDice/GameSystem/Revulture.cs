using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class Revulture : GameSystemBase
    {
        public static readonly Revulture Instance = new Revulture();

        public override string Id => "Revulture";
        public override string Name => "光砕のリヴァルチャー";
        public override string SortKey => "こうさいのりうあるちやあ";

        private static readonly Regex AttackReg = new Regex(
            @"^(\d+([+/]\d+)*)?AT(TACK|K)?(<=([1-6](\+\d)*))?((\[>?=\d+:\+\d+\])+)?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex AdditionalDmgReg = new Regex(
            @"\[(>?=)(\d+):\+(\d+)\]",
            RegexOptions.Compiled);

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = AttackReg.Match(command);
            if (!m.Success) return null;
            string? diceExpr = m.Groups[1].Success ? m.Groups[1].Value : null;
            string? borderExpr = m.Groups[5].Success ? m.Groups[5].Value : null;
            string? addDmgRules = m.Groups[7].Success ? m.Groups[7].Value : null;
            return RollAttack(diceExpr, borderExpr, addDmgRules, randomizer);
        }

        private Result? RollAttack(string? diceExpr, string? borderExpr, string? addDmgRules, IRandomizer randomizer)
        {
            int diceCount = EvalArith(diceExpr);
            int? border = borderExpr != null ? (int?)Math.Max(1, Math.Min(6, EvalArith(borderExpr))) : null;

            string cmd = MakeCommandText(diceCount, border, addDmgRules);

            if (diceCount <= 0)
                return Result.Failure($"{cmd} ＞ ダイス数が 0 です");
            if (border == null && addDmgRules != null)
                return Result.Failure($"{cmd} ＞ 目標値が指定されていないため、追加ダメージを算出できません");

            var dices = randomizer.RollBarabara(diceCount, 6).OrderBy(x => x).ToList();
            int critCount = dices.Count(v => v == 1);
            int? hitCount = border.HasValue
                ? (int?)(dices.Count(d => d <= border.Value) + critCount)
                : null;
            int? damage = CalcDamage(hitCount, addDmgRules);

            var parts = new List<string> { cmd, string.Join(",", dices) };
            if (critCount > 0) parts.Add($"クリティカル {critCount}");
            if (hitCount.HasValue) parts.Add($"ヒット数 {hitCount.Value}");
            if (damage.HasValue) parts.Add($"ダメージ {damage.Value}");

            string text = string.Join($" ＞ ", parts);
            bool success = hitCount.HasValue && hitCount.Value > 0;
            return Result.CreateBuilder(text)
                .SetCondition(success)
                .SetCritical(critCount > 0)
                .Build();
        }

        private static int EvalArith(string? expr)
        {
            if (string.IsNullOrEmpty(expr)) return 0;
            // Simple addition/division evaluator for dice expressions like 3+2, 10/2
            if (expr.Contains('/'))
            {
                var parts = expr.Split('/');
                int a = EvalArith(parts[0]);
                int b = EvalArith(parts[1]);
                return b != 0 ? (int)Math.Floor((double)a / b) : 0;
            }
            if (expr.Contains('+'))
            {
                return expr.Split('+').Select(p => EvalArith(p)).Sum();
            }
            return int.TryParse(expr, out int v) ? v : 0;
        }

        private static string MakeCommandText(int diceCount, int? border, string? addDmgRules)
        {
            string cmd = $"{diceCount}attack";
            if (border.HasValue) cmd += $"<=={border.Value}";
            if (addDmgRules != null) cmd += addDmgRules;
            return $"({cmd})";
        }

        private int? CalcDamage(int? hitCount, string? addDmgRules)
        {
            if (addDmgRules == null) return null;
            if (!hitCount.HasValue) return null;
            int damage = hitCount.Value;
            foreach (Match m in AdditionalDmgReg.Matches(addDmgRules))
            {
                string cmp = m.Groups[1].Value;
                int threshold = int.Parse(m.Groups[2].Value);
                int add = int.Parse(m.Groups[3].Value);
                bool applies = cmp == "=" ? hitCount.Value == threshold : hitCount.Value >= threshold;
                if (applies) damage += add;
            }
            return damage;
        }
    }
}
