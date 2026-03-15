using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class AFF2e : GameSystemBase
    {
        public static readonly AFF2e Instance = new AFF2e();

        public override string Id => "AFF2e";
        public override string Name => "ADVANCED FIGHTING FANTASY 2nd Edition";
        public override string SortKey => "\u3042\u3068\u306f\u3093\u3059\u3068\u3075\u3042\u3044\u3066\u3044\u3093\u304f\u3075\u3042\u3093\u305f\u3057\u30012";

        private static readonly Regex AffReg = new Regex(
            @"^FF(.*)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex AfrReg = new Regex(
            @"^FR(.*)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex AfdReg = new Regex(
            @"^FD(.*)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TermNumReg = new Regex(
            @"[+-]?\d+",
            RegexOptions.Compiled);

        private static readonly Regex SlotReg = new Regex(
            @"^\[(.+)\]",
            RegexOptions.Compiled);

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            Match m;

            m = AffReg.Match(command);
            if (m.Success)
            {
                string term = m.Groups[1].Value;
                int diff = EvalTerm(term);
                string diceCommand = "2D6<=" + diff;
                var diceList = randomizer.RollBarabara(2, 6);
                int total = diceList.Sum();
                string diceStr = string.Join(",", diceList);
                string expr = total + "[" + diceStr + "]";
                string succ = SuccessfulOrFailed(total, diff);
                string text = string.Join(" \uff1e ", new[] { "(" + diceCommand + ")", expr, succ });
                return Result.Success(text);
            }

            m = AfrReg.Match(command);
            if (m.Success)
            {
                string term = m.Groups[1].Value;
                int corr = EvalTerm(term);
                string diceCommand = "2D6" + ExplicitSign(corr);
                var diceList = randomizer.RollBarabara(2, 6);
                int total = diceList.Sum();
                string diceStr = string.Join(",", diceList);
                string expr = total + "[" + diceStr + "]" + ExplicitSign(corr);
                string? crit = Critical(total);
                int finalTotal = total + corr;
                var parts = new List<string> { "(" + diceCommand + ")", expr };
                if (crit != null) parts.Add(crit);
                parts.Add(finalTotal.ToString());
                string text = string.Join(" \uff1e ", parts);
                return Result.Success(text);
            }

            m = AfdReg.Match(command);
            if (m.Success)
            {
                string term = m.Groups[1].Value;
                var slotMatch = SlotReg.Match(term);
                if (!slotMatch.Success)
                    return Result.Success("\u30c0\u30e1\u30fc\u30b8\u30b9\u30ed\u30c3\u30c8\u306f\u5fc5\u9808\u3067\u3059\u3002");

                string afterSlot = term.Substring(slotMatch.Length);
                var damageSlots = slotMatch.Groups[1].Value.Split(',').Select(t => EvalTerm(t)).ToList();
                if (damageSlots.Count != 7)
                    return Result.Success("\u30c0\u30e1\u30fc\u30b8\u30b9\u30ed\u30c3\u30c8\u306e\u9577\u3055\u306b\u8aa4\u308a\u304c\u3042\u308a\u307e\u3059\u3002");

                int corr = EvalTerm(afterSlot);
                string diceCommand = "1D6" + ExplicitSign(corr);
                int total = randomizer.RollOnce(6);
                string expr = total + ExplicitSign(corr);
                int slotNumber = Clamp(total + corr, 1, 7);
                int damage = damageSlots[slotNumber - 1];
                string text = string.Join(" \uff1e ", new[]
                {
                    "(" + diceCommand + ")",
                    expr,
                    (total + corr).ToString(),
                    damage + "\u30c0\u30e1\u30fc\u30b8"
                });
                return Result.Success(text);
            }

            return null;
        }

        private static string ExplicitSign(int i) { return i >= 0 ? "+" + i : i.ToString(); }

        private static int EvalTerm(string term)
        {
            int value = 0;
            foreach (Match m in TermNumReg.Matches(term))
                value += int.Parse(m.Value);
            return value;
        }

        private static string SuccessfulOrFailed(int total, int diff)
        {
            if (total == 2) return diff <= 1 ? "\u6210\u529f\uff08\u5927\u6210\u529f\u3067\u306f\u306a\u3044\uff09" : "\u5927\u6210\u529f\uff01";
            if (total == 12) return diff >= 12 ? "\u5931\u6557\uff08\u5927\u5931\u6557\u3067\u306f\u306a\u3044\uff09" : "\u5927\u5931\u6557\uff01";
            return total <= diff ? "\u6210\u529f" : "\u5931\u6557";
        }

        private static string? Critical(int total)
        {
            if (total == 2) return "\u30d5\u30a1\u30f3\u30d6\u30eb\uff01";
            if (total == 12) return "\u5f37\u6253\uff01";
            return null;
        }

        private static int Clamp(int i, int min, int max)
        {
            if (i < min) return min;
            if (i > max) return max;
            return i;
        }
    }
}
