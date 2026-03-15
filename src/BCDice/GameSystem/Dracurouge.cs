using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ドラクルージュ
    /// 行い判定（DR）と抗い判定（DRR）を実装。
    /// </summary>
    public sealed class Dracurouge : GameSystemBase
    {
        public static readonly Dracurouge Instance = new Dracurouge();

        private static readonly Regex ConductRegex = new Regex(
            @"^DR(\d*[1-9])?(\+\d+)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ResistRegex = new Regex(
            @"^DRR(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CorruptionRegex = new Regex(
            @"^CT(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override string Id => "Dracurouge";
        public override string Name => "ドラクルージュ";
        public override string SortKey => "とらくるうしゆ";
        public override D66SortType D66SortType => D66SortType.NoSort;

        public override string HelpMessage => @"・行い判定（DRx+y）
　x：振るサイコロの数（省略時４）、y：渇き修正（省略時０）
　例） DR　DR6　DR+1　DR5+2
・抗い判定（DRRx）
　x：振るサイコロの数
　例） DRR3
・堕落表（CTx） x：渇き （例） CT3
・D66ダイスあり
";

        // 堕落表
        private static readonly (int threshold, string text)[] CorruptionTable = new[]
        {
            (0, "堕落は止まった。……だが、この先も繰り返されるだろう。"),
            (1, "忌まわしい欲望が湧き上がり、あなたを衝き動かす。"),
            (3, "あなたは過ちを犯す。それはあなたを蝕む毒だ。"),
            (5, "肉体が変容し、恐ろしい力を得る。しかし代償は大きい。"),
            (6, "あなたの魂は引き裂かれ、別の何かが流れ込む。"),
            (7, "あなたの中の何かが壊れる。二度と元には戻らない。"),
            (8, "あなたを繋ぎ止めていたものが崩れていく。"),
            (99, "あなたは完全に堕落し、闇の眷属となった。"),
        };

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var result = RollConductDice(command, randomizer);
            if (result != null) return result;

            result = RollResistDice(command, randomizer);
            if (result != null) return result;

            result = RollCorruptionTable(command, randomizer);
            if (result != null) return result;

            return null;
        }

        private Result? RollConductDice(string command, IRandomizer randomizer)
        {
            var m = ConductRegex.Match(command);
            if (!m.Success) return null;

            int diceCount = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 4;
            int thirstyPoint = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;

            var diceList = randomizer.RollBarabara(diceCount, 6).OrderBy(x => x).ToList();

            // 栄光ダイスのカウント（1のペアと6のペアを10に変換）
            int gloryDice = CountGloryDice(diceList);
            for (int i = 0; i < gloryDice; i++)
            {
                diceList.Add(10);
            }

            // 渇き修正の適用
            string? thirstyExpr = null;
            if (thirstyPoint > 0)
            {
                thirstyExpr = ApplyThirstyPoint(diceList, thirstyPoint);
            }

            string modStr = thirstyPoint > 0 ? $"+{thirstyPoint}" : "";

            var parts = new List<string>
            {
                $"({command})",
                $"{diceCount}D6{modStr}",
            };

            if (thirstyExpr != null)
            {
                parts.Add(thirstyExpr);
            }

            parts.Add($"[ {string.Join(", ", diceList)} ]");

            string text = string.Join(" ＞ ", parts);

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static int CountGloryDice(List<int> diceList)
        {
            int oneCount = diceList.Count(d => d == 1);
            int sixCount = diceList.Count(d => d == 6);
            return (oneCount / 2) + (sixCount / 2);
        }

        private static string? ApplyThirstyPoint(List<int> diceList, int thirstyPoint)
        {
            if (thirstyPoint == 0) return null;

            // 6以下の最後のダイスに渇き修正を加える
            int idx = -1;
            for (int i = diceList.Count - 1; i >= 0; i--)
            {
                if (diceList[i] <= 6)
                {
                    idx = i;
                    break;
                }
            }

            if (idx < 0) return null;

            var textList = diceList.Select(d => d.ToString()).ToList();
            textList[idx] += $"+{thirstyPoint}";
            diceList[idx] += thirstyPoint;

            return $"[ {string.Join(", ", textList)} ]";
        }

        private Result? RollResistDice(string command, IRandomizer randomizer)
        {
            var m = ResistRegex.Match(command);
            if (!m.Success) return null;

            int diceCount = int.Parse(m.Groups[1].Value);
            if (diceCount == 0) diceCount = 4;

            var diceList = randomizer.RollBarabara(diceCount, 6).OrderBy(x => x).ToArray();
            int total = diceList.Sum();

            string text = $"({command}) ＞ {diceCount}D6 ＞ [ {string.Join(", ", diceList)} ] ＞ {total}";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result? RollCorruptionTable(string command, IRandomizer randomizer)
        {
            var m = CorruptionRegex.Match(command);
            if (!m.Success) return null;

            int modify = int.Parse(m.Groups[1].Value);

            var diceList = randomizer.RollBarabara(2, 6);
            int number = diceList.Sum();
            string numberText = string.Join(",", diceList);
            int index = number - modify;

            string resultText = GetCorruptionText(index);

            string text = $"2D6[{numberText}]-{modify} ＞ 堕落表({index}) ＞ {resultText}";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static string GetCorruptionText(int index)
        {
            for (int i = CorruptionTable.Length - 1; i >= 0; i--)
            {
                if (index >= CorruptionTable[i].threshold)
                {
                    return CorruptionTable[i].text;
                }
            }
            return CorruptionTable[0].text;
        }
    }
}