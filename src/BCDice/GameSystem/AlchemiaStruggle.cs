using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Table;

namespace BCDice.GameSystem
{
    public sealed class AlchemiaStruggle : GameSystemBase
    {
        public static readonly AlchemiaStruggle Instance = new AlchemiaStruggle();

        public override string Id => "AlchemiaStruggle";
        public override string Name => "\u30a2\u30eb\u30b1\u30df\u30a2\u30fb\u30b9\u30c8\u30e9\u30b0\u30eb";
        public override string SortKey => "\u3042\u308b\u3051\u307f\u3042\u3059\u3068\u3089\u304f\u308b";
        public override RoundType RoundType => RoundType.Ceiling;
        public override bool SortBarabaraDice => true;

        private static readonly Regex AsReg = new Regex(
            @"^(\d+)AS(\d+)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex UlReg = new Regex(
            @"^(\d+)UL$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Dictionary<string, string> ALIAS = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "CELE", "CElement" }, { "CALC", "CAlchemia" }, { "CINF", "CInformant" },
            { "CINN", "CInnocence" }, { "CACQ", "CAcquired" },
            { "ARS", "ArticleS" }, { "ARM", "ArticleM" }, { "ARL", "ArticleL" },
            { "PCI", "PCInformation" }, { "REA", "Reason" }, { "ASS", "Associate" }, { "CON", "Contact" },
        };

        private static readonly Dictionary<string, SimpleTable> TABLES = BuildTables();

        private static Dictionary<string, SimpleTable> BuildTables()
        {
            var t = new Dictionary<string, SimpleTable>(StringComparer.OrdinalIgnoreCase);

            t["CElement"] = new SimpleTable("\u5947\u8de1\u306e\u89e6\u5a92\uff08\u30a8\u30ec\u30e1\u30f3\u30c8\uff09", "1D6", new[]
            {
                "\u30ef\u30f3\u30c9", "\u6c34\u6676\u7389", "\u30ab\u30fc\u30c9", "\u30b9\u30c6\u30c3\u30ad", "\u624b\u93e1", "\u5b9d\u77f3",
            });
            t["CAlchemia"] = new SimpleTable("\u5947\u8de1\u306e\u89e6\u5a92\uff08\u30a2\u30eb\u30b1\u30df\u30a2\uff09", "1D6", new[]
            {
                "\u6307\u8f2a", "\u30d6\u30ec\u30b9\u30ec\u30c3\u30c8", "\u30a4\u30e4\u30ea\u30f3\u30b0", "\u30cd\u30c3\u30af\u30ec\u30b9", "\u30d6\u30ed\u30fc\u30c1", "\u30d8\u30a2\u30d4\u30f3",
            });
            t["CInformant"] = new SimpleTable("\u5947\u8de1\u306e\u89e6\u5a92\uff08\u30a4\u30f3\u30d5\u30a9\u30fc\u30de\u30f3\u30c8\uff09", "1D6", new[]
            {
                "\u30b9\u30de\u30fc\u30c8\u30d5\u30a9\u30f3", "\u30bf\u30d6\u30ec\u30c3\u30c8", "\u30ce\u30fc\u30c8\u30d1\u30bd\u30b3\u30f3", "\u7121\u7dda\u6a5f\uff08\u30c8\u30e9\u30f3\u30b7\u30fc\u30d0\u30fc\uff09", "\u30a6\u30a7\u30a2\u30e9\u30d6\u30eb\u30c7\u30d0\u30a4\u30b9", "\u643a\u5e2f\u30b2\u30fc\u30e0\u6a5f",
            });
            t["CInnocence"] = new SimpleTable("\u5947\u8de1\u306e\u89e6\u5a92\uff08\u30a4\u30ce\u30bb\u30f3\u30b9\uff09", "1D6", new[]
            {
                "\u624b\u888b", "\u7b1b", "\u9774", "\u9234", "\u62e1\u58f0\u5668", "\u5f26\u697d\u5668",
            });
            t["CAcquired"] = new SimpleTable("\u5947\u8de1\u306e\u89e6\u5a92\uff08\u30a2\u30af\u30ef\u30a4\u30e4\u30fc\u30c9\uff09", "1D6", new[]
            {
                "\u30dc\u30bf\u30f3", "\u97f3\u58f0", "\u30e2\u30fc\u30b7\u30e7\u30f3", "\u8133\u6ce2", "\u8a18\u9332\u5a92\u4f53", "\uff21\uff29",
            });
            t["Reason"] = new SimpleTable("\u7406\u7531\u8868", "1D6", new[]
            {
                "\u4e0d\u4fe1\u611f \u2015\u2015 \u884c\u52d5\u3084\u8a00\u52d5\u306b\u306a\u306b\u304b\u91c8\u7136\u3068\u3057\u306a\u3044\u90e8\u5206\u3092\u611f\u3058\u308b\u3002\u767d\u9ed2\u306f\u3063\u304d\u308a\u3055\u305b\u3088\u3046\u3002",
                "\u597d\u5947\u5fc3 \u2015\u2015 \u76f8\u624b\u306e\u3053\u3068\u3092\u77e5\u308a\u305f\u3044\u3068\u639b\u304d\u7acb\u3066\u3089\u308c\u308b\u3002\u77e5\u308a\u305f\u3044\u6c17\u6301\u3061\u3092\u62bc\u3055\u3048\u3089\u308c\u306a\u3044\u3002",
                "\u5e87\u8b77\u611f \u2015\u2015 \u77e5\u53e4\u306e\u59ff\u3092\u91cd\u306d\u3066\u5b88\u308a\u305f\u304f\u306a\u3063\u3066\u3057\u307e\u3046\u3002\u4fe1\u983c\u95a2\u4fc2\u3092\u541b\u3068\u7bc9\u304f\u305f\u3081\u3001\u8e0f\u307f\u8fbc\u3093\u3060\u3068\u3053\u308d\u307e\u3067\u77e5\u3063\u3066\u304a\u304d\u305f\u3044\u3002",
                "\u5acc\u60aa\u611f \u2015\u2015 \u7406\u7531\u306f\u306a\u3044\u3051\u3069\u6c17\u306b\u98df\u308f\u306a\u3044\u3002\u60c5\u5831\u306e\u30a2\u30c9\u30d0\u30f3\u30c6\u30fc\u30b8\u3092\u63e1\u308b\u3053\u3068\u3067\u512a\u4f4d\u306b\u7acb\u3066\u308b\u306f\u305a\u3060\u3002",
                "\u504f\u611f \u2015\u2015 \u611b\u3086\u3048\u306b\u77e5\u308a\u305f\u304f\u306a\u3063\u3066\u3057\u307e\u3046\u3002\u541b\u306e\u601d\u8003\u3001\u76ee\u7684\u3001\u611f\u60c5\u306e\u3059\u3079\u3066\u3092\u624b\u306b\u5165\u308c\u305f\u3044\u3002",
                "\u76f4\u611f \u2015\u2015 \u6839\u62e0\u306f\u306a\u3044\u304c\u3001\u306a\u306b\u304b\u96a0\u3057\u3066\u3044\u308b\u6c17\u304c\u3059\u308b\u3002\u4e00\u304b\u516b\u304b\u3001\u52dd\u8ca0\u306b\u51fa\u3088\u3046\u3002",
            });
            t["Contact"] = new SimpleTable("\u63a5\u89e6\u306e\u304d\u3063\u304b\u3051\u8868", "1D6", new[]
            {
                "\u4f53\u52e2\u3092\u5d29\u3059 \u2015\u2015 \u8ee2\u3073\u305d\u3046\u306b\u306a\u3063\u305f\u3068\u3053\u308d\u3092\u652f\u3048\u308b\u3001\u652f\u3048\u3089\u308c\u308b\u3002",
                "\u4ed8\u7740\u7269\u3092\u3068\u308b \u2015\u2015 \u9aea\u3084\u670d\u306b\u3064\u3044\u3066\u3044\u308b\u30b4\u30df\u3001\u6c5a\u308c\u3092\u3068\u3063\u3066\u3042\u3052\u308b\u3002",
                "\u601d\u308f\u305a\u624b\u304c\u51fa\u308b \u2015\u2015 \u8a00\u8449\u3088\u308a\u5148\u306b\u3001\u5f37\u3081\u306b\u624b\u304c\u51fa\u3066\u3057\u307e\u3046\u3002",
                "\u7269\u3054\u3057\u306b\u89e6\u308c\u308b \u2015\u2015 \u7269\u3092\u6e21\u3059\u3001\u62fe\u3046\u969b\u306b\u6307\u5148\u540c\u58eb\u304c\u3076\u3064\u304b\u308b\u3002",
                "\u53cb\u597d\u306e\u30b5\u30a4\u30f3 \u2015\u2015 \u80a9\u3092\u7d44\u3080\u3001\u63e1\u624b\u3092\u3059\u308b\u3001\u30cf\u30b0\u3092\u3059\u308b\u306a\u3069\u3002",
                "\u30b1\u30a2\u3092\u3057\u3066\u3042\u3052\u308b \u2015\u2015 \u9aea\u3092\u3068\u304b\u3059\u3001\u80a9\u3092\u3082\u3080\u3001\u982d\u3092\u6491\u3067\u308b\u3002\u76f8\u624b\u3092\u52b4\u3063\u3066\u3059\u308b\u884c\u70ba\u5168\u822c\u3002",
            });

            return t;
        }

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            if (ALIAS.TryGetValue(command, out var aliased))
                command = aliased;

            return TryRollAlchemia(command, randomizer)
                ?? TryRollUldice(command, randomizer)
                ?? RollTableCommand(command, randomizer);
        }

        private Result? TryRollAlchemia(string command, IRandomizer randomizer)
        {
            var m = AsReg.Match(command);
            if (!m.Success) return null;

            int rollCount = int.Parse(m.Groups[1].Value);

            if (!m.Groups[2].Success)
            {
                var diceList = randomizer.RollBarabara(rollCount, 6);
                return Result.Success(MakeRollText(diceList));
            }
            else
            {
                int pickCount = int.Parse(m.Groups[2].Value);
                var diceList = randomizer.RollBarabara(rollCount, 6);
                var pickedList = PickMaximum(diceList, pickCount);
                return Result.Success(MakeRollAndPickText(diceList, pickCount, pickedList));
            }
        }

        private Result? TryRollUldice(string command, IRandomizer randomizer)
        {
            var m = UlReg.Match(command);
            if (!m.Success) return null;

            int rollCount = int.Parse(m.Groups[1].Value);
            var diceList = randomizer.RollBarabara(rollCount, 6).OrderBy(x => x).ToList();
            string diceText = string.Join(",", diceList);
            string result = string.Join(", ", diceList.GroupBy(x => x).Select(g => "No." + g.Key + ": " + g.Count() + "\u500b"));
            string text = string.Join(" \uff1e ", new[] { "(" + rollCount + "D6)", "[" + diceText + "]", result });
            return Result.Success(text);
        }

        private Result? RollTableCommand(string command, IRandomizer randomizer)
        {
            if (!TABLES.TryGetValue(command, out var table)) return null;
            return table.Roll(randomizer);
        }

        private static int[] PickMaximum(int[] diceList, int pickCount)
        {
            if (diceList.Length <= pickCount) return diceList;
            return diceList.OrderBy(x => x).Skip(diceList.Length - pickCount).ToArray();
        }

        private static string MakeRollText(int[] diceList)
        {
            return "(" + diceList.Length + "D6) \uff1e " + MakeDiceText(diceList);
        }

        private static string MakeRollAndPickText(int[] rolledList, int pickCount, int[] pickedList)
        {
            return "(" + rolledList.Length + "D6|>" + pickCount + "D6) \uff1e " +
                   MakeDiceText(rolledList) + " >> " + MakeDiceText(pickedList) + " \uff1e " + pickedList.Sum();
        }

        private static string MakeDiceText(int[] diceList)
        {
            return "[" + string.Join(", ", diceList.OrderBy(x => x)) + "]";
        }
    }
}
