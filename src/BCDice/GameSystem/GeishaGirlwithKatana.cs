using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class GeishaGirlwithKatana : GameSystemBase
    {
        public static readonly GeishaGirlwithKatana Instance = new GeishaGirlwithKatana();

        public override string Id => "GeishaGirlwithKatana";
        public override string Name => "\u30b2\u30a4\u30b7\u30e3\u30fb\u30ac\u30fc\u30eb\u30fb\u30a6\u30a3\u30ba\u30fb\u30ab\u30bf\u30ca";
        public override string SortKey => "\u3051\u3044\u3057\u3084\u304b\u3042\u308b\u3046\u3044\u3059\u304b\u305f\u306a";

        private static readonly Regex GlReg = new Regex(@"^GL$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex GkReg = new Regex(@"^GK(#(\d+))?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Dictionary<string, string?> YakuTable = new Dictionary<string, string?>
        {
            { "1,2,3", "\u81ea\u52d5\u5931\u6557\uff0f\u81ea\u5206\u306e\u88c5\u7532\u52b9\u679c\u7121\u3057\u3067\u30c0\u30e1\u30fc\u30b8\u3092\u53d7\u3051\u3066\u3057\u307e\u3046" },
            { "4,5,6", "\u81ea\u52d5\u6210\u529f\uff0f\u6575\u306e\u88c5\u7532\u3092\u7121\u8996\u3057\u3066\u30c0\u30e1\u30fc\u30b8\u3092\u4e0e\u3048\u308b" },
            { "1,1,1", "10\u500d\u6210\u529f YP\u304c10\u5897\u52a0\uff0f10\u500d\u30c0\u30e1\u30fc\u30b8 YP\u304c10\u5897\u52a0" },
            { "2,2,2", "2\u500d\u6210\u529f\uff0f2\u500d\u30c0\u30e1\u30fc\u30b8" },
            { "3,3,3", "3\u500d\u6210\u529f\uff0f3\u500d\u30c0\u30e1\u30fc\u30b8" },
            { "4,4,4", "4\u500d\u6210\u529f\uff0f4\u500d\u30c0\u30e1\u30fc\u30b8" },
            { "5,5,5", "5\u500d\u6210\u529f\uff0f5\u500d\u30c0\u30e1\u30fc\u30b8" },
            { "6,6,6", "6\u500d\u6210\u529f\uff0f6\u500d\u30c0\u30e1\u30fc\u30b8" },
        };

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            if (GlReg.IsMatch(command))
                return Result.Success(GetResultText("\u30c1\u30e7\u30e0\u30d0\uff01\uff01"));

            var m = GkReg.Match(command);
            if (!m.Success) return null;

            string? chombaStr = m.Groups[2].Success ? m.Groups[2].Value : null;
            if (IsChomba(chombaStr, randomizer))
                return Result.Success(GetResultText("\u30c1\u30e7\u30e0\u30d0\uff01\uff01"));

            var diceList = randomizer.RollBarabara(3, 6).OrderBy(x => x).ToList();

            string? yaku = GetYaku(diceList);
            if (yaku != null)
                return Result.Success(GetResultTextByDice(diceList, "\u300c\u5f79\u300d" + yaku));

            var (deme, zorome) = GetDemeZorome(diceList);
            if (deme == 0)
                return Result.Success(GetResultTextByDice(diceList, "\u5931\u6557"));

            string yp = zorome == 1 ? " YP\u304c1\u5897\u52a0" : "";
            return Result.Success(GetResultTextByDice(diceList, "\u9054\u6210\u5024" + deme + yp));
        }

        private static bool IsChomba(string? chombaStr, IRandomizer randomizer)
        {
            int threshold = chombaStr != null ? int.Parse(chombaStr) : 5;
            return randomizer.RollOnce(100) <= threshold;
        }

        private static string? GetYaku(List<int> diceList)
        {
            string key = string.Join(",", diceList);
            return YakuTable.TryGetValue(key, out var v) ? v : null;
        }

        private static (int deme, int zorome) GetDemeZorome(List<int> diceList)
        {
            if (diceList[0] == diceList[1]) return (diceList[2], diceList[0]);
            if (diceList[1] == diceList[2]) return (diceList[0], diceList[1]);
            return (0, 0);
        }

        private static string GetResultTextByDice(List<int> diceList, string result)
            => GetResultText(string.Join(",", diceList) + " \uff1e " + result);

        private static string GetResultText(string result)
            => "(3B6) \uff1e " + result;
    }
}
