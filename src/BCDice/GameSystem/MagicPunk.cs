using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// マジックパンクTRPG
    /// </summary>
    public sealed class MagicPunk : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly MagicPunk Instance = new MagicPunk();

        /// <inheritdoc/>
        public override string Id => "MagicPunk";

        /// <inheritdoc/>
        public override string Name => "マジックパンクTRPG";

        /// <inheritdoc/>
        public override string SortKey => "ましつくはんくTRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ 判定 (nMPm)
        nD20のダイスロールをして、m以下の目があれば成功。
        mと同じ目があればジャックポット(自動成功)。
        すべての目が1ならバッドビート(自動失敗)。
        ■ チャレンジ判定 (nMPmCx)
        通常の判定に加えてチャレンジ値x以上の目が必要になる。
        ■ ダイス数0 (0MPmCx)
        修正によりダイス数が0になった場合は2d20のダイスロールを行う。
        2つの目からより悪い結果になる方を採用する。
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollMp(command, randomizer);

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result? RollMp(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d*)MP(\d+)(C?)(\d*)$");
            if (!m.Success)
            {
                return null;
            }

            var dices = string.IsNullOrEmpty(m.Groups[1].Value) ? 1 : int.Parse(m.Groups[1].Value);
            var spec = int.Parse(m.Groups[2].Value);
            var opt1 = m.Groups[3].Value;
            var arg1 = string.IsNullOrEmpty(m.Groups[4].Value) ? 0 : int.Parse(m.Groups[4].Value);
            var isZero = dices == 0;
            var challenge = opt1 == "C" ? arg1 : 0;

            var dice_list = randomizer.RollBarabara(isZero ? 2 : dices, 20);

            // 通常は1つ成功なら成功(any?)、0ダイス時はすべて成功したとき成功(all?)
            bool check = isZero
                ? dice_list.All(d => d <= spec && challenge <= d)
                : dice_list.Any(d => d <= spec && challenge <= d);
            // ジャックポット判定
            bool isJp = isZero
                ? dice_list.All(d => d == spec)
                : dice_list.Any(d => d == spec);
            // バッドビート判定: 通常はすべて1なら失敗(all?)、0ダイス時は1つでも1があれば失敗(any?)
            bool isBb = isZero
                ? dice_list.Any(d => d == 1)
                : dice_list.All(d => d == 1);

            string result;
            if (isBb)
            {
                isJp = false;
                check = false;
                result = "失敗(BB)";
            }
            else if (isJp)
            {
                check = true;
                result = "成功(JP)";
            }
            else if (check)
            {
                var filtered = dice_list.Where(d => d <= spec);
                var value = isZero ? filtered.Min() : filtered.Max();
                result = $"成功({value})";
            }
            else
            {
                result = "失敗";
            }

            return Result.CreateBuilder($"({dices}MP{spec}C{challenge}).Build() > [{string.Join(",", dice_list)}] > {result}")
                .SetFumble(isBb)
                .SetCritical(isJp)
                .SetCondition(check)
                .SetRands(randomizer.RandResults)
                .Build();
        }

    }
}