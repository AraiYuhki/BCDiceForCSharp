using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class DiceOfTheDead : GameSystemBase
    {
        public static readonly DiceOfTheDead Instance = new DiceOfTheDead();

        public override string Id => "DiceOfTheDead";
        public override string Name => "\u30c0\u30a4\u30b9\u30fb\u30aa\u30d6\u30fb\u30b6\u30fb\u30c7\u30c3\u30c9";
        public override string SortKey => "\u305f\u3044\u3059\u304a\u3075\u3055\u3066\u3064\u3068";
        public override D66SortType D66SortType => D66SortType.Ascending;

        private static readonly Regex BioReg = new Regex(@"^BIO(\d+)?$", RegexOptions.Compiled);
        private static readonly Regex ZmbReg = new Regex(@"^ZMB(\+(\d+))?$", RegexOptions.Compiled);

        private static readonly int[] ZombieMaxValues = new[] { 5,6,7,8,9,10,11,12,13,14,15,16,17,18 };
        private static readonly string[] ZombieTexts = new[]
        {
            "\uff15\u4ee5\u4e0b\uff1a\u5f71\u97ff\u306a\u3057",
            "\uff16\uff1a\u4efb\u610f\u306e\u90e8\u4f4d\u3092\uff11\u70b9\u56de\u5fa9",
            "\uff17\uff1a\u3008\u30a2\u30a4\u30c6\u30e0\u3009\u6b66\u5668\u3092\uff11\u3064\u305d\u306e\u5834\u306b\u843d\u3068\u3059",
            "\uff18\uff1a\u3008\u30a2\u30a4\u30c6\u30e0\u3009\u4fbf\u5229\u9053\u5177\uff11\u3064\u3092\u305d\u306e\u5834\u306b\u843d\u3068\u3059",
            "\uff19\uff1a\u3008\u30a2\u30a4\u30c6\u30e0\u3009\u6d88\u8017\u54c1\uff11\u3064\u3092\u305d\u306e\u5834\u306b\u843d\u3068\u3059",
            "\uff11\uff10\uff1a\u8155\u306e\u50b7\u304c\u5e83\u304c\u308b\u3002\u300c\u90e8\u4f4d\uff1a\u300e\u8155\u300f\u300d\uff11\u70b9\u30c0\u30e1\u30fc\u30b8",
            "\uff11\uff11\uff1a\u8db3\u306e\u50b7\u304c\u5e83\u304c\u308b\u3002\u300c\u90e8\u4f4d\uff1a\u300e\u8db3\u300f\u300d\uff11\u70b9\u30c0\u30e1\u30fc\u30b8",
            "\uff11\uff12\uff1a\u982d\u306e\u50b7\u304c\u5e83\u304c\u308b\u3002\u300c\u90e8\u4f4d\uff1a\u300e\u982d\u300f\u300d\uff11\u70b9\u30c0\u30e1\u30fc\u30b8",
            "\uff11\uff13\uff1a\u300e\u30be\u30f3\u30d3\u5316\u8868\u300f\u304c\u65b0\u305f\u306b\u9069\u7528\u3055\u308c\u308b\u307e\u3067\u300c\u300e\u611f\u67d3\u5ea6\u300f\uff0b\uff11\u30de\u30b9\u300d\u306e\u52b9\u679c\u3092\u53d7\u3051\u308b",
            "\uff11\uff14\uff1a\u5373\u5ea7\u306b\u81ea\u5206\u4ee5\u5916\u306e\u5473\u65b9\uff11\u4eba\u306e\u30b9\u30ed\u30c3\u30c8\u5185\u306e\u3008\u30a2\u30a4\u30c6\u30e0\u3009\uff11\u3064\u3092\u30e9\u30f3\u30c0\u30e0\u306b\u6368\u3066\u3055\u305b\u308b",
            "\uff11\uff15\uff1a\u5473\u65b9\uff11\u4eba\u306b\u7d20\u624b\u3067\u653b\u6483\u3092\u884c\u3046",
            "\uff11\uff16\uff1a\u5373\u5ea7\u306b\u611f\u67d3\u5ea6\u304c\uff11\u4e0a\u6607\u3059\u308b",
            "\uff11\uff17\uff1a\u6b21\u306e\u30bf\u30fc\u30f3\u306e\u307f\u3001\u3059\u3079\u3066\u306e\u300e\u80fd\u529b\u5024\u300f\u3092\uff12\u500d\u306b\u3059\u308b",
            "\uff11\uff18\u4ee5\u4e0a\uff1a\u81ea\u5206\u4ee5\u5916\u306e\u5473\u65b9\uff11\u4eba\u306b\u3067\u304d\u308b\u9650\u308a\u5168\u529b\u3067\u653b\u6483\u3092\u884c\u3046\u3002\u3008\u30a2\u30a4\u30c6\u30e0\u3009\u3082\u53ef\u80fd\u306a\u9650\u308a\u4f7f\u7528\u3059\u308b",
        };

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var mBio = BioReg.Match(command);
            if (mBio.Success)
            {
                int rollTimes = mBio.Groups[1].Success ? int.Parse(mBio.Groups[1].Value) : 1;
                string text = CheckInfection(rollTimes, randomizer);
                return Result.CreateBuilder(text).SetSecret(true).Build();
            }

            var mZmb = ZmbReg.Match(command);
            if (mZmb.Success)
            {
                int value = mZmb.Groups[2].Success ? int.Parse(mZmb.Groups[2].Value) : 0;
                string text = RollZombie(value, randomizer);
                return Result.CreateBuilder(text).SetSecret(true).Build();
            }

            return null;
        }

        private static string CheckInfection(int rollTimes, IRandomizer randomizer)
        {
            var sb = new System.Text.StringBuilder("\u611f\u67d3\u5ea6\u8868");
            for (int i = 0; i < rollTimes; i++)
            {
                int d1 = randomizer.RollOnce(6);
                int d2 = randomizer.RollOnce(6);
                sb.Append("\u3000\uff1e\u3000\u51fa\u76ee\uff1a" + d1 + "\u3001" + d2 + "\u3000");
                int idx1 = (int)Math.Ceiling(d1 / 2.0) - 1;
                int idx2 = (int)Math.Ceiling(d2 / 2.0) - 1;
                string[][] table = new string[][]
                {
                    new string[] { "\u300c\u53f3\u4e0b\uff08\u300e\u8db3\u300f\uff0b\uff11\uff09\u300d", "\u300c\u53f3\u4e2d\uff08\u300e\u8db3\u300f\uff0b\uff11\uff09\u300d", "\u300c\u53f3\u4e0a\uff08\u300e\u8db3\u300f\uff0b\uff11\uff09\u300d" },
                    new string[] { "\u300c\u4e2d\u4e0b\uff08\u300e\u8155\u300f\uff0b\uff11\uff09\u300d", "\u300c\u771f\u4e2d\uff08\u300e\u8155\u300f\uff0b\uff11\uff09\u300d", "\u300c\u4e2d\u4e0a\uff08\u300e\u8155\u300f\uff0b\uff11\uff09\u300d" },
                    new string[] { "\u300c\u5de6\u4e0b\uff08\u300e\u982d\u300f\uff0b\uff11\uff09\u300d", "\u300c\u5de6\u4e2d\uff08\u300e\u982d\u300f\uff0b\uff11\uff09\u300d", "\u300c\u5de6\u4e0a\uff08\u300e\u982d\u300f\uff0b\uff11\uff09\u300d" },
                };
                sb.Append(table[idx1][idx2]);
            }
            return sb.ToString();
        }

        private static string RollZombie(int value, IRandomizer randomizer)
        {
            int d1 = randomizer.RollOnce(6);
            int d2 = randomizer.RollOnce(6);
            int diceTotal = d1 + d2 + value;

            int index = diceTotal;
            if (index < ZombieMaxValues[0]) index = ZombieMaxValues[0];
            if (index > ZombieMaxValues[ZombieMaxValues.Length - 1]) index = ZombieMaxValues[ZombieMaxValues.Length - 1];

            string text = ZombieTexts[ZombieTexts.Length - 1];
            for (int i = 0; i < ZombieMaxValues.Length; i++)
            {
                if (index <= ZombieMaxValues[i]) { text = ZombieTexts[i]; break; }
            }

            return "\u30be\u30f3\u30d3\u5316\u8868\u3000\uff1e\u3000\u51fa\u76ee\uff1a" + d1 + "\uff0b" + d2 +
                   "\u3000\u611f\u67d3\u5ea6\uff1a" + value + "\u3000\u5408\u8a08\u5024\uff1a" + diceTotal +
                   "\u3000\uff1e\u3000" + text;
        }
    }
}
