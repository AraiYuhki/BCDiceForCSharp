using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class GoblinSlayer : GameSystemBase
    {
        public static readonly GoblinSlayer Instance = new GoblinSlayer();

        public override string Id => "GoblinSlayer";
        public override string Name => "\u30b4\u30d6\u30ea\u30f3\u30b9\u30ec\u30a4\u30e4\u30fcTRPG";
        public override string SortKey => "\u3053\u3075\u308a\u3093\u3059\u308c\u3044\u3084\u3042TRPG";
        public override RoundType RoundType => RoundType.Ceiling;

        private const int CRITICAL = 12;
        private const int FUMBLE = 2;

        private static readonly Regex GsReg = new Regex(
            @"^GS([-+]?\d+)?(?:(?:([@#])([-+]?\d+))(?:([@#])([-+]?\d+))?)?(?:(>=?)(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex McpiReg = new Regex(
            @"^MCPI(\+?\d+)?\$(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex DbReg = new Regex(
            @"^DB(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            if (Regex.IsMatch(command, @"^GS", RegexOptions.IgnoreCase))
                return GetCheckResult(command, randomizer);
            if (Regex.IsMatch(command, @"^MCPI", RegexOptions.IgnoreCase))
                return MurmurChantPrayInvoke(command, randomizer);
            if (Regex.IsMatch(command, @"^DB", RegexOptions.IgnoreCase))
                return DamageBonus(command, randomizer);
            return null;
        }

        private Result? GetCheckResult(string command, IRandomizer randomizer)
        {
            var m = GsReg.Match(command);
            if (!m.Success) return null;

            if (m.Groups[2].Success && m.Groups[4].Success &&
                m.Groups[2].Value == m.Groups[4].Value)
                return null;

            int basis = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
            string cmpOp = m.Groups[6].Success ? m.Groups[6].Value : "";
            int target = m.Groups[7].Success ? int.Parse(m.Groups[7].Value) : 0;
            bool withoutCompare = !m.Groups[6].Success;

            string opt1 = m.Groups[2].Success ? m.Groups[2].Value : "";
            string opt1Val = m.Groups[3].Success ? m.Groups[3].Value : "";
            string opt2 = m.Groups[4].Success ? m.Groups[4].Value : "";
            string opt2Val = m.Groups[5].Success ? m.Groups[5].Value : "";

            var (threshCrit, threshFumb) = CalcThreshold(opt1, opt1Val, opt2, opt2Val);

            var diceList = randomizer.RollBarabara(2, 6);
            int total = diceList.Sum();
            string diceText = string.Join(",", diceList);
            int achievement = basis + total;
            bool fumble = total <= threshFumb;
            bool critical = total >= threshCrit;

            string result = "";
            if (!withoutCompare || !(fumble && critical))
                result = " \uff1e " + ResultStr(achievement, target, cmpOp, fumble, critical);

            string basisStr = basis == 0 ? "" : basis + " + ";
            string text = "(" + command + ") \uff1e " + basisStr + total + "[" + diceText + "] \uff1e " + achievement + result;
            return Result.Success(text);
        }

        private static (int critical, int fumble) CalcThreshold(string opt1, string opt1Val, string opt2, string opt2Val)
        {
            string? critStr = opt1 == "@" ? (opt1Val != "" ? opt1Val : null) : (opt2Val != "" ? opt2Val : null);
            string? fumbStr = opt1 == "@" ? (opt2Val != "" ? opt2Val : null) : (opt1Val != "" ? opt1Val : null);
            int crit = critStr != null ? int.Parse(critStr) : CRITICAL;
            int fumb = fumbStr != null ? int.Parse(fumbStr) : FUMBLE;
            return (crit, fumb);
        }

        private Result? MurmurChantPrayInvoke(string command, IRandomizer randomizer)
        {
            var m = McpiReg.Match(command);
            if (!m.Success) return null;

            int luck = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
            int volition = int.Parse(m.Groups[2].Value);

            if (volition >= 12)
                return Result.Success("\u56e0\u679c\u70b9\u304c12\u70b9\u4ee5\u4e0a\u306e\u5834\u5408\u3001\u56e0\u679c\u70b9\u306f\u4f7f\u7528\u3067\u304d\u307e\u305b\u3093\u3002");

            var diceList = randomizer.RollBarabara(2, 6);
            int total = diceList.Sum();
            string diceText = string.Join(",", diceList);
            int achievement = total + luck;
            string luckStr = luck == 0 ? "" : "+" + luck;
            string result = ResultStr(achievement, volition, ">=", false, false);
            string text = "\u7948\u5ff5(2d6" + luckStr + ") \uff1e " + total + "[" + diceText + "]" + luckStr +
                          " \uff1e " + achievement + " \uff1e " + result +
                          ", \u56e0\u679c\u70b9\uff1a" + volition + "\u70b9 \u2192 " + (volition + 1) + "\u70b9";
            return Result.Success(text);
        }

        private Result? DamageBonus(string command, IRandomizer randomizer)
        {
            var m = DbReg.Match(command);
            if (!m.Success) return null;

            int num = int.Parse(m.Groups[1].Value);
            string fmt = "\u547d\u4e2d\u5224\u5b9a\u306e\u52b9\u529b\u5024\u306b\u3088\u308b\u30dc\u30fc\u30ca\u30b9 \uff1e ";
            if (num < 15)
                return Result.Success(fmt + "\u306a\u3057");

            int times;
            if (num >= 40) times = 5;
            else if (num >= 30) times = 4;
            else if (num >= 25) times = 3;
            else if (num >= 20) times = 2;
            else times = 1;

            var diceList = randomizer.RollBarabara(times, 6);
            int total = diceList.Sum();
            string diceText = string.Join(",", diceList);
            return Result.Success(fmt + total + "[" + diceText + "] \uff1e " + total);
        }

        private static string ResultStr(int achievement, int target, string cmpOp, bool fumble, bool critical)
        {
            if (fumble) return "\u5927\u5931\u6557";
            if (critical) return "\u5927\u6210\u529f";
            if (cmpOp == ">=") return achievement >= target ? "\u6210\u529f" : "\u5931\u6557";
            return achievement > target ? "\u6210\u529f" : "\u5931\u6557";
        }
    }
}
