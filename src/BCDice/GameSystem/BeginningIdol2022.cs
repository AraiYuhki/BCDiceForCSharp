using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ビギニングアイドル（2022年改訂版）
    /// </summary>
    public sealed class BeginningIdol2022 : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly BeginningIdol2022 Instance = new BeginningIdol2022();

        /// <inheritdoc/>
        public override string Id => "BeginningIdol2022";

        /// <inheritdoc/>
        public override string Name => "ビギニングアイドル（2022年改訂版）";

        /// <inheritdoc/>
        public override string SortKey => "ひきにんくあいとる2022";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        これは、2022年に大判サイズで発売された『駆け出しアイドルRPG ビギニングアイドル 基本ルールブック』に対応したコマンドです。

        ・行為判定　BIn@c#f+m>=t
        　nD6をダイスロールし、行為判定に成功したかを出力します。スペシャルとファンブルの判定も行います。
        　　n: ダイス数（省略時 2)
        　　c: スペシャル値（省略時 12)
        　　f: ファンブル値（省略時 2)
        　　m: 修正値（省略可)
        　　t: 目標値

        ・パフォーマンス判定　PDn+m
        　nD6をダイスロールし、パフォーマンス値を出力します。パーフェクトミラクルとミラクルの判定も行います。
        　　n: ダイス数
        　　m: 修正値（省略可)

        ・シンフォニー　xxxPDn+m
        　nD6をダイスロールし、場に残っているダイスを加味してパフォーマンス値を出力します。
        　パーフェクトミラクルとミラクルシンクロの判定も行います。
        　　xxx: 場に残っているダイスの出目を列挙したもの
        　　n: ダイス数
        　　m: 修正値（省略可)
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollSkillCheck(command, randomizer) ?? RollPerformanceCheck(command, randomizer) ?? RollSymphonyCheck(command, randomizer);
        }

        private Result? RollSkillCheck(string command, IRandomizer randomizer)
        {
            // Parse BI command: BIn@c#f+m>=t
            var m = Regex.Match(command, @"^BI(\d+)?(?:@(\d+))?(?:#(\d+))?([+\-]\d+)?(?:>=(\d+))?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var dice_times = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 2;
            var critical = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 12;
            var fumble = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 2;
            var modifyNumber = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 0;
            if (!m.Groups[5].Success)
            {
                return null;
            }
            var targetNumber = int.Parse(m.Groups[5].Value);

            var dice_list = randomizer.RollBarabara(dice_times, 6).OrderBy(x => x).ToArray();
            var dice_total = dice_list.Sum();
            var is_critical = dice_total >= critical;
            var is_fumble = !is_critical && dice_total <= fumble;
            var total = dice_total + modifyNumber;

            Result result;
            if (is_critical)
            {
                result = Result.CreateBuilder("スペシャル(PCは【思い出】を1つ獲得する)").SetSuccess(true).SetCritical(true).Build();
            }
            else if (is_fumble)
            {
                result = Result.CreateBuilder("ファンブル(【思い出】を1つ獲得し、ファンブル表を振る)").SetFumble(true).SetFailure(true).Build();
            }
            else if (total >= targetNumber)
            {
                result = Result.CreateBuilder("成功").SetSuccess(true).Build();
            }
            else
            {
                result = Result.CreateBuilder("失敗").SetFailure(true).Build();
            }

            var parsedStr = $"BI{dice_times}@{critical}#{fumble}{Format.Modifier(modifyNumber)}>={targetNumber}";
            var text = $"({parsedStr}) ＞ {dice_total}[{string.Join(",", dice_list)}]{Format.Modifier(modifyNumber)} ＞ {total} ＞ {result.Text}";

            return Result.CreateBuilder(text)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .Build();
        }

        private Result? RollPerformanceCheck(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^PD(\d+)([+-]\d+)?$");
            if (!m.Success)
            {
                return null;
            }

            var suffix_number = int.Parse(m.Groups[1].Value);
            var modifier = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
            var is_extension = suffix_number >= 7;
            var dice_times = is_extension ? 6 : suffix_number;
            var extension_bonus = is_extension ? suffix_number - dice_times : 0;

            if (dice_times <= 0)
            {
                return null;
            }

            var dice_list = randomizer.RollBarabara(dice_times, 6).OrderBy(x => x).ToArray();
            var uniqed = SelectUniqs(dice_list).OrderBy(x => x).ToArray();

            var is_perfect_miracle = uniqed.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 });
            var is_miracle = uniqed.Length == 0;

            string result_label;
            if (is_perfect_miracle)
            {
                result_label = $"【パーフェクトミラクル】{30 + extension_bonus + modifier}";
            }
            else if (is_miracle)
            {
                result_label = $"【ミラクル】{10 + extension_bonus + modifier}";
            }
            else
            {
                result_label = (uniqed.Sum() + extension_bonus + modifier).ToString();
            }

            if (is_extension)
            {
                result_label += $" (エクステンション: {extension_bonus}個まで振りなおし可能)";
            }

            var parts = new List<string>();
            parts.Add($"({command})");
            parts.Add("パフォーマンス判定");
            parts.Add($"[{string.Join(",", dice_list)}]{Format.Modifier(extension_bonus)}{Format.Modifier(modifier)}");
            if (dice_list.Length != uniqed.Length)
            {
                parts.Add($"[{string.Join(",", uniqed)}]{Format.Modifier(extension_bonus)}{Format.Modifier(modifier)}");
            }
            parts.Add(result_label);

            var text = string.Join(" ＞ ", parts);

            return Result.CreateBuilder(text)
                .SetCritical(is_perfect_miracle || is_miracle)
                .Build();
        }

        private int[] SelectUniqs(int[] array)
        {
            return array.GroupBy(x => x)
                .Where(g => g.Count() == 1)
                .Select(g => g.Key)
                .ToArray();
        }

        private Result? RollSymphonyCheck(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^([1-6]+)PD([1-6])([+-]\d+)?$");
            if (!m.Success)
            {
                return null;
            }

            var carries = m.Groups[1].Value.ToCharArray().Select(c => int.Parse(c.ToString())).OrderBy(x => x).ToArray();
            var dice_times = int.Parse(m.Groups[2].Value);
            var modifier = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 0;

            var dice_list = randomizer.RollBarabara(dice_times, 6).OrderBy(x => x).ToArray();
            var combined = carries.Concat(dice_list).ToArray();
            var uniqed = SelectUniqs(combined).OrderBy(x => x).ToArray();

            var is_perfect_miracle = uniqed.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 });
            var is_miracle_synchro = uniqed.Length == 0;

            string result_label;
            if (is_perfect_miracle)
            {
                result_label = $"【パーフェクトミラクル】{30 + modifier}";
            }
            else if (is_miracle_synchro)
            {
                result_label = $"【ミラクルシンクロ】{20 + modifier}";
            }
            else
            {
                result_label = (uniqed.Sum() + modifier).ToString();
            }

            var text = string.Join(" ＞ ", new[]
            {
                $"({command})",
                "シンフォニー",
                $"[{string.Join(",", carries)}],[{string.Join(",", dice_list)}]{Format.Modifier(modifier)}",
                $"[{string.Join(",", uniqed)}]{Format.Modifier(modifier)}",
                result_label,
            });

            return Result.CreateBuilder(text)
                .SetCritical(is_perfect_miracle || is_miracle_synchro)
                .Build();
        }

    }
}
