using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class ArsMagica : GameSystemBase
    {
        public static readonly ArsMagica Instance = new ArsMagica();
        public override string Id => "ArsMagica";
        public override string Name => "アルスマギカ";
        public override string SortKey => "あるすまきか";
        private static readonly Regex ArsReg = new Regex(
            @"^ArS(\d+)?((?:[+-]-?\d+)+)?(?:([>=]+)(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex R10Reg = new Regex(
            @"^1R10((?:[+-]-?\d+)+)?(?:\[(\d+)\])?(?:([>=]+)(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            int botch = 1, bonus = 0, diff = 0;
            string? cmpOp = null;
            bool hasDiff = false;

            var mArs = ArsReg.Match(command);
            var mR10 = R10Reg.Match(command);
            if (!mArs.Success && !mR10.Success) return null;

            if (mArs.Success)
            {
                botch = mArs.Groups[1].Success ? int.Parse(mArs.Groups[1].Value) : 1;
                bonus = mArs.Groups[2].Success ? EvalModifier(mArs.Groups[2].Value) : 0;
                cmpOp = mArs.Groups[3].Success ? mArs.Groups[3].Value : null;
                if (mArs.Groups[4].Success) { diff = int.Parse(mArs.Groups[4].Value); hasDiff = true; }
            }
            else
            {
                bonus = mR10.Groups[1].Success ? EvalModifier(mR10.Groups[1].Value) : 0;
                botch = mR10.Groups[2].Success ? int.Parse(mR10.Groups[2].Value) : 1;
                cmpOp = mR10.Groups[3].Success ? mR10.Groups[3].Value : null;
                if (mR10.Groups[4].Success) { diff = int.Parse(mR10.Groups[4].Value); hasDiff = true; }
            }

            string modStr = FormatMod(bonus);
            string cmpStr = cmpOp != null && hasDiff ? cmpOp + diff : "";
            string expr = "1R10" + modStr + "[" + botch + "]" + cmpStr;
            var sb = new StringBuilder();
            sb.Append("(" + expr + ") ＞ ");

            int die = randomizer.RollOnce(10) - 1;
            int total = 0;
            bool isBotch = false;

            if (die == 0)
            {
                int count0 = 0;
                var diceN = new List<int>();
                for (int i = 0; i < botch; i++)
                {
                    int bd = randomizer.RollOnce(10) - 1;
                    if (bd == 0) count0++;
                    diceN.Add(bd);
                }
                sb.Append("0[0," + string.Join(",", diceN) + "]");
                if (count0 != 0)
                {
                    sb.Append(count0 > 1 ? " ＞ " + count0 + "Botch!" : " ＞ Botch!");
                    cmpOp = null; isBotch = true;
                }
                else
                {
                    if (bonus > 0) sb.Append("+" + bonus + " ＞ " + bonus);
                    else if (bonus < 0) sb.Append(bonus + " ＞ " + bonus);
                    else sb.Append(" ＞ 0");
                    total = bonus;
                }
            }
            else if (die == 1)
            {
                int critMul = 1;
                var critDice = new List<int>();
                while (die == 1)
                {
                    critMul *= 2;
                    die = randomizer.RollOnce(10);
                    critDice.Add(die);
                }
                total = die * critMul;
                sb.Append(total + "[1," + string.Join(",", critDice) + "]");
                total += bonus;
                if (bonus > 0) sb.Append("+" + bonus + " ＞ " + total);
                else if (bonus < 0) sb.Append(bonus + " ＞ " + total);
            }
            else
            {
                total = die + bonus;
                if (bonus > 0) sb.Append(die + "+" + bonus + " ＞ " + total);
                else if (bonus < 0) sb.Append("" + die + bonus + " ＞ " + total);
                else sb.Append(total.ToString());
            }

            if (cmpOp == ">=" && hasDiff)
                sb.Append(total >= diff ? " ＞ 成功" : " ＞ 失敗");

            if (isBotch) return Result.Failure(sb.ToString());
            if (cmpOp != null && hasDiff)
                return total >= diff ? Result.Success(sb.ToString()) : Result.Failure(sb.ToString());
            return Result.Success(sb.ToString());
        }

        private static int EvalModifier(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            int total = 0;
            foreach (var part in System.Text.RegularExpressions.Regex.Split(s, @"(?=[+-])"))
                if (!string.IsNullOrEmpty(part))
                    total += int.Parse(part);
            return total;
        }

        private static string FormatMod(int m) { return m == 0 ? "" : (m > 0 ? "+" + m : m.ToString()); }
    }
}
