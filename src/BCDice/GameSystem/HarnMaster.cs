using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class HarnMaster : GameSystemBase
    {
        public static readonly HarnMaster Instance = new HarnMaster();

        public override string Id => "HarnMaster";
        public override string Name => "\u30cf\u30fc\u30f3\u30de\u30b9\u30bf\u30fc";
        public override string SortKey => "\u306f\u3042\u3093\u307e\u3059\u305f\u3042";

        private static readonly Regex ShkReg = new Regex(@"^SHK(\d*),(\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SlhReg = new Regex(@"^SLH(U|D)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var mShk = ShkReg.Match(command);
            if (mShk.Success)
            {
                int toughness = mShk.Groups[1].Value != "" ? int.Parse(mShk.Groups[1].Value) : 0;
                int damage = int.Parse(mShk.Groups[2].Value);
                return Result.Success(GetCheckShockResult(damage, toughness, randomizer));
            }

            var mSlh = SlhReg.Match(command);
            if (mSlh.Success)
            {
                string? type = mSlh.Groups[1].Success ? mSlh.Groups[1].Value.ToUpper() : null;
                return Result.Success(GetStrikeLocationHuman(type, randomizer));
            }

            return null;
        }

        private static string GetCheckShockResult(int damage, int toughness, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(damage, 6);
            int dice = diceList.Sum();
            string diceText = string.Join(",", diceList);
            string result = dice <= toughness ? "\u6210\u529f" : "\u5931\u6557";
            return "\u30b7\u30e7\u30c3\u30af\u5224\u5b9a(\u30c0\u30e1\u30fc\u30b8:" + damage + ", \u8010\u4e45\u529b:" + toughness + ") \uff1e (" + dice + "[" + diceText + "]) \uff1e " + result;
        }

        private static string GetStrikeLocationHuman(string? type, IRandomizer randomizer)
        {
            string typeName;
            (int Max, string Part)[] table;

            if (type == "U")
            {
                typeName = "\u547d\u4e2d\u90e8\u4f4d(\u4eba\u578b \u4e0a\u6bb5)";
                table = UpperTable;
            }
            else if (type == "D")
            {
                typeName = "\u547d\u4e2d\u90e8\u4f4d(\u4eba\u578b \u4e0b\u6bb5)";
                table = DownTable;
            }
            else
            {
                typeName = "\u547d\u4e2d\u90e8\u4f4d(\u4eba\u578b \u4e2d\u6bb5)";
                table = NormalTable;
            }

            int number = randomizer.RollOnce(100);
            string part = GetTableByNumber(number, table);
            part = GetLocationSide(part, number);
            part = GetFaceLocation(part, number, randomizer);

            return typeName + " \uff1e (" + number + ")" + part;
        }

        private static string GetLocationSide(string part, int number)
        {
            if (!part.StartsWith("*")) return part;
            string side = number % 2 == 1 ? "\u5de6" : "\u53f3";
            return side + part.Substring(1);
        }

        private static string GetFaceLocation(string part, int number, IRandomizer randomizer)
        {
            if (!part.EndsWith("+")) return part;
            var faceTable = new (int Max, string Part)[]
            {
                (15, "\u984e"), (30, "*\u76ee"), (64, "*\u983c"), (80, "\u9f3b"), (90, "*\u8033"), (100, "\u53e3")
            };
            int fn = randomizer.RollOnce(100);
            string faceLocation = GetTableByNumber(fn, faceTable);
            faceLocation = GetLocationSide(faceLocation, fn);
            return part.Substring(0, part.Length - 1) + " \uff1e (" + fn + ")" + faceLocation;
        }

        private static string GetTableByNumber(int number, (int Max, string Part)[] table)
        {
            foreach (var entry in table)
                if (number <= entry.Max) return entry.Part;
            return table[table.Length - 1].Part;
        }

        private static readonly (int Max, string Part)[] UpperTable = new[]
        {
            (15, "\u982d\u90e8"), (30, "\u9854+"), (45, "\u9996"), (57, "*\u80a9"),
            (69, "*\u4e0a\u8155"), (73, "*\u8098"), (81, "*\u524d\u8155"), (85, "*\u624b"),
            (95, "\u80f8\u90e8"), (100, "\u8179\u90e8")
        };

        private static readonly (int Max, string Part)[] NormalTable = new[]
        {
            (5, "\u982d\u90e8"), (10, "\u9854+"), (15, "\u9996"), (27, "*\u80a9"),
            (33, "*\u4e0a\u8155"), (35, "*\u8098"), (39, "*\u524d\u8155"), (43, "*\u624b"),
            (60, "\u80f8\u90e8"), (70, "\u8179\u90e8"), (74, "\u80a1\u9593"),
            (80, "*\u81c0\u90e8"), (88, "*\u819f"), (90, "*\u819d"),
            (96, "*\u8129"), (100, "*\u8db3")
        };

        private static readonly (int Max, string Part)[] DownTable = new[]
        {
            (6, "*\u524d\u8155"), (12, "*\u624b"), (19, "\u80f8\u90e8"), (29, "\u8179\u90e8"),
            (35, "\u80a1\u9593"), (49, "*\u81c0\u90e8"), (70, "*\u819f"),
            (78, "*\u819d"), (92, "*\u8129"), (100, "*\u8db3")
        };
    }
}
