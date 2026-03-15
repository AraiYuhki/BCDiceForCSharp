using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// フィアスコ
    /// </summary>
    public sealed class Fiasco : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Fiasco Instance = new Fiasco();

        private static readonly Regex WDWRegex = new Regex(@"^W\d+W\d+$", RegexOptions.Compiled);
        private static readonly Regex BDBRegex = new Regex(@"^B\d+B\d+$", RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "Fiasco";

        /// <inheritdoc/>
        public override string Name => "フィアスコ";

        /// <inheritdoc/>
        public override string SortKey => "ふいあすこ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
          ・判定コマンド(FSx, WxBx)
            相関図・転落要素用(FSx)：相関図や転落要素のためにx個ダイスを振り、出目ごとに分類する
            黒白差分判定用(WxBx)  ：転落、残響のために白ダイス(W指定)と黒ダイス(B指定)で差分を求める
              ※ WとBは片方指定(Bx, Wx)、入替指定(WxBx,BxWx)可能
        ";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var result = RollFs(command, randomizer)
                ?? RollWhiteBlack(command, randomizer)
                ?? RollWhiteBlackSingle(command, randomizer);
            return result;
        }

        private Result? RollFs(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^FS(\d+)$");
            if (!m.Success)
            {
                return null;
            }

            var diceCount = int.Parse(m.Groups[1].Value);
            var diceList = randomizer.RollBarabara(diceCount, 6);

            var bucket = new int[7]; // index 0 unused, 1-6 counts
            foreach (var val in diceList)
            {
                bucket[val] += 1;
            }

            var text = $"1 => {bucket[1]}, 2 => {bucket[2]}, 3 => {bucket[3]}, 4 => {bucket[4]}, 5 => {bucket[5]}, 6 => {bucket[6]}";
            return Result.CreateBuilder(text).Build();
        }

        private Result? RollWhiteBlackSingle(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^([WB])(\d+)$");
            if (!m.Success)
            {
                return null;
            }

            var a = new Side(GetColor(m.Groups[1].Value), int.Parse(m.Groups[2].Value));
            var rollText = a.Roll(randomizer);

            var text = $"{rollText} ＞ {a.ColorName}{a.Total}";
            return Result.CreateBuilder(text).Build();
        }

        private Result? RollWhiteBlack(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^([WB])(\d+)([WB])(\d+)$");
            if (!m.Success)
            {
                return null;
            }

            if (WDWRegex.IsMatch(command))
            {
                return Result.CreateBuilder($"{command}：白ダイスが重複しています").Build();
            }
            else if (BDBRegex.IsMatch(command))
            {
                return Result.CreateBuilder($"{command}：黒ダイスが重複しています").Build();
            }

            var a = new Side(GetColor(m.Groups[1].Value), int.Parse(m.Groups[2].Value));
            var resultA = a.Roll(randomizer);

            var b = new Side(GetColor(m.Groups[3].Value), int.Parse(m.Groups[4].Value));
            var resultB = b.Roll(randomizer);

            var text = $"{resultA} {resultB} ＞ {a.Diff(b)}";
            return Result.CreateBuilder(text).Build();
        }

        private string GetColor(string c)
        {
            return c == "W" ? "白" : "黒";
        }

        private class Side
        {
            private readonly string _color;
            private readonly int _count;
            private int[] _diceList = Array.Empty<int>();

            public string ColorName => _color;
            public int Total { get; private set; }

            public Side(string color, int count)
            {
                _color = color;
                _count = count;
            }

            public string Roll(IRandomizer randomizer)
            {
                _diceList = _count == 0 ? new[] { 0 } : randomizer.RollBarabara(_count, 6);
                Total = _diceList.Sum();
                return $"{_color}{Total}[{string.Join(",", _diceList)}]";
            }

            public string Diff(Side other)
            {
                if (Total == other.Total)
                {
                    return "0";
                }
                else if (Total > other.Total)
                {
                    return $"{_color}{Total - other.Total}";
                }
                else
                {
                    return $"{other.ColorName}{other.Total - Total}";
                }
            }
        }
    }
}
