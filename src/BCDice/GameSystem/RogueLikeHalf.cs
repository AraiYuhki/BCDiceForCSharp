using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class RogueLikeHalf : GameSystemBase
    {
        public static readonly RogueLikeHalf Instance = new RogueLikeHalf();
        public override string Id => "RogueLikeHalf";
        public override string Name => "ローグライクハーフ";
        public override string SortKey => "ろおくらいくはあふ";
        public override bool SortBarabaraDice => true;

        private static readonly Regex RhReg = new Regex(
            @"^RH([+-]\d+)*(>=(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex D33Reg = new Regex(
            @"^D33([+-]\d+)*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex NttReg = new Regex(
            @"^NTT([+-]\d+)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly string[] NttTable = new[]
        {
            "金貨10枚",
            "1ｄ６枚の金貨",
            "2ｄ６枚の金貨（下限は金貨5枚）",
            "1個のアクセサリー（1ｄ６×1ｄ６枚の金貨と同等の価値）",
            "1個の宝石・小（1ｄ６×5枚の金貨と同等の価値。下限は金貨15枚の価値）",
            "1個の宝石・大（2ｄ６×5枚の金貨と同等の価値。下限は金貨30枚の価値）",
            "【魔法の宝物表】でダイスロールを行うこと。"
        };

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return ResoluteAction(command, randomizer) ?? ResoluteD33(command, randomizer) ?? RollNtt(command, randomizer);
        }

        private static string WithSymbol(int n)
        {
            return n == 0 ? "+0" : (n > 0 ? "+" + n : n.ToString());
        }

        private Result? ResoluteAction(string command, IRandomizer randomizer)
        {
            var m = RhReg.Match(command);
            if (!m.Success) return null;

            int modify = 0;
            foreach (Capture cap in m.Groups[1].Captures)
                modify += int.Parse(cap.Value);
            int target = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 4;

            int die = randomizer.RollOnce(6);
            int total = die + modify;

            Result result;
            if (die == 6) result = Result.CreateBuilder("クリティカル").SetCritical(true).SetSuccess(true).Build();
            else if (die == 1) result = Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
            else if (total >= target) result = Result.CreateBuilder("成功").SetSuccess(true).Build();
            else result = Result.CreateBuilder("失敗").SetFailure(true).Build();

            string cmdTxt = "(RH" + WithSymbol(modify) + ">=" + target + ")";
            string text = string.Join(" ＞ ", new[]{ cmdTxt, "["+die+"]"+WithSymbol(modify), total.ToString(), result.Text });
            return Result.CreateBuilder(text).SetCondition(result.IsSuccess).SetCritical(result.IsCritical).SetFumble(result.IsFumble).Build();
        }


        private Result? ResoluteD33(string command, IRandomizer randomizer)
        {
            var m = D33Reg.Match(command);
            if (!m.Success) return null;

            int modify = 0;
            foreach (System.Text.RegularExpressions.Capture cap in m.Groups[1].Captures)
                modify += int.Parse(cap.Value);

            var dice = randomizer.RollBarabara(2, 3);
            string diceText = string.Join("", dice);
            int diceTotal = dice[0] * 3 + dice[1] + modify;
            if (diceTotal > 12) diceTotal = 12;
            if (diceTotal < 4) diceTotal = 4;

            int tens = diceTotal / 3;
            int units = diceTotal % 3;
            if (units == 0) { tens -= 1; units = 3; }
            int total = tens * 10 + units;

            string text;
            if (modify != 0)
                text = string.Join(" ＞ ", new[] { "(" + command + ")", diceText + WithSymbol(modify), total.ToString() });
            else
                text = string.Join(" ＞ ", new[] { "(" + command + ")", diceText });

            return Result.Success(text);
        }

        private Result? RollNtt(string command, IRandomizer randomizer)
        {
            var m = NttReg.Match(command);
            if (!m.Success) return null;

            int modify = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;

            int roll = randomizer.RollSum(1, 6);
            int index = roll + modify;
            if (index < 1) index = 1;
            if (index > NttTable.Length) index = NttTable.Length;

            string entry = NttTable[index - 1];
            string text = "宝物表:" + index + ":" + entry;
            if (modify != 0)
                text = "(宝物表 1D6" + WithSymbol(modify) + ") ＞ " + text;
            else
                text = "(宝物表 1D6) ＞ " + text;

            return Result.Success(text);
        }
    }
}
