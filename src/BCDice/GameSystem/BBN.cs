using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// BBNTRPG
    /// </summary>
    public sealed class BBN : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly BBN Instance = new BBN();

        /// <inheritdoc/>
        public override string Id => "BBN";

        /// <inheritdoc/>
        public override string Name => "BBNTRPG";

        /// <inheritdoc/>
        public override string SortKey => "ひいひいえぬTRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定(xBN±y>=z[c,f])
        　xD6の判定。クリティカル、ファンブルの自動判定を行います。
        　1Dのクリティカル値とファンブル値は1。2Dのクリティカル値とファンブル値は2。
        　nDのクリティカル値とファンブル値は n/2 の切り上げ。
        　クリティカルとファンブルが同時に発生した場合、クリティカルを優先。
        　x：xに振るダイス数を入力。
        　y：yに修正値を入力。省略可能。
          z：zに目標値を入力。省略可能。
          c：クリティカルに必要なダイス目「6」の数の増減。省略可能。
          f：ファンブルに必要なダイス目「1」の数の増減。省略可能。
        　例） 3BN+4　3BN>=8　3BN+1>=10[-1] 3BN+1>=10[,1] 3BN+1>=10[1,1]
        ";

        // パース結果を保持するフィールド
        private int _rollTimes;
        private string _modifyStr = "";
        private int _modify;
        private int? _difficulty;
        private int _critical;
        private int _fumble;

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            if (!Parse(command))
            {
                return null;
            }

            var diceList = randomizer.RollBarabara(_rollTimes, 6);
            var dice = diceList.Sum();
            var diceStr = string.Join(",", diceList);
            var total = dice + _modify;

            var sequence = new List<string>
            {
                $"({command})",
                $"{dice}[{diceStr}]{_modifyStr}",
                total.ToString()
            };

            if (IsCritical(diceList))
            {
                sequence.Add("クリティカル！");
                sequence.AddRange(AdditionalRoll(diceList.Count(v => v == 6), total, randomizer));
            }
            else if (IsFumble(diceList))
            {
                sequence.Add("ファンブル！");
            }
            else if (_difficulty.HasValue)
            {
                sequence.Add(total >= _difficulty.Value ? "成功" : "失敗");
            }

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetRands(randomizer.RandResults)
                .Build();
        }

        /// <summary>
        /// コマンド文字列をパースする
        /// </summary>
        private bool Parse(string command)
        {
            var m = Regex.Match(command, @"^(\d+)BN([+-]\d+)?(>=(([+-]?\d+)))?(\[([+-]?\d+)?(,([+-]?\d+))?\])?");
            if (!m.Success)
            {
                return false;
            }

            _rollTimes = int.Parse(m.Groups[1].Value);
            _modifyStr = m.Groups[2].Success ? m.Groups[2].Value : "";
            _modify = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
            _difficulty = m.Groups[4].Success ? (int?)int.Parse(m.Groups[4].Value) : null;

            var b = CriticalBase(_rollTimes);
            _critical = b + (m.Groups[7].Success ? int.Parse(m.Groups[7].Value) : 0);
            _fumble = b + (m.Groups[9].Success ? int.Parse(m.Groups[9].Value) : 0);

            return true;
        }

        /// <summary>
        /// 振るダイスの数からクリティカルとファンブルの基本値を算出する
        /// </summary>
        private int CriticalBase(int rollTimes)
        {
            switch (rollTimes)
            {
                case 1:
                case 2:
                    return rollTimes;
                default:
                    return (int)Math.Ceiling(rollTimes / 2.0);
            }
        }

        /// <summary>クリティカルか判定</summary>
        private bool IsCritical(int[] diceList)
        {
            return diceList.Count(v => v == 6) >= _critical;
        }

        /// <summary>ファンブルか判定</summary>
        private bool IsFumble(int[] diceList)
        {
            return diceList.Count(v => v == 1) >= _fumble;
        }

        /// <summary>
        /// クリティカルの追加ロールをする
        /// </summary>
        private IEnumerable<string> AdditionalRoll(int additionalDice, int total, IRandomizer randomizer)
        {
            var sequence = new List<string>();
            var rerollCount = 0;

            while (additionalDice > 0 && rerollCount < 10)
            {
                rerollCount++;
                var dl = randomizer.RollBarabara(additionalDice, 6);
                var dt = dl.Sum();
                var ds = string.Join(",", dl);
                additionalDice = dl.Count(v => v == 6);

                sequence.Add($"{total}+{dt}[{ds}]");
                if (additionalDice > 0)
                {
                    sequence.Add("追加クリティカル！");
                }
                total += dt;
            }

            if (additionalDice > 0)
            {
                sequence.Add("無限ループ防止のため中断");
            }

            sequence.Add(total.ToString());
            if (_difficulty.HasValue)
            {
                sequence.Add(total >= _difficulty.Value ? "成功" : "失敗");
            }

            return sequence;
        }
    }
}
