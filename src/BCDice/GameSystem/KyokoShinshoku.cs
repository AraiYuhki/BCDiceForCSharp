using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Arithmetic;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 虚構侵蝕TRPG
    /// </summary>
    public sealed class KyokoShinshoku : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly KyokoShinshoku Instance = new KyokoShinshoku();

        /// <inheritdoc/>
        public override string Id => "KyokoShinshoku";

        /// <inheritdoc/>
        public override string Name => "虚構侵蝕TRPG";

        /// <inheritdoc/>
        public override string SortKey => "きよこうしんしよくTRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定
        　ダイスを指定数ダイスロールして、最も高い出目を出力します。難易度を指定すると成否を判定します。オプションでA、Dをつけると、［有利］［不利］の条件で振れます（A=［有利］、D=［不利］）。
        KS(x,y)
        x：ダイスサイズ。1=D4（能力値1、2以上の出目が出ていたとしても最大1）／2=D4（能力値2、3以上の出目が出ていたとしても最大2）／3=D4（能力値3、出目4が出ていたとしても最大3）／4=D4／6=D6／8=D8／10=D10／12=D12／20=D20
        y：ダイス数（省略：1）

        KS(x,y)>=z
        x：ダイスサイズ。1=D4（能力値1、2以上の出目が出ていたとしても最大1）／2=D4（能力値2、3以上の出目が出ていたとしても最大2）／3=D4（能力値3、出目4が出ていたとしても最大3）／4=D4／6=D6／8=D8／10=D10／12=D12／20=D20
        y：ダイス数（省略：1）
        z：難易度

        KS(x,y)A>=z（［有利］：KS(x,y)の判定を２回行い、それぞれの結果のより大きい方が結果となります）
        x：ダイスサイズ。1=D4（能力値1、2以上の出目が出ていたとしても最大1）／2=D4（能力値2、3以上の出目が出ていたとしても最大2）／3=D4（能力値3、出目4が出ていたとしても最大3）／4=D4／6=D6／8=D8／10=D10／12=D12／20=D20
        y：ダイス数（省略：1）
        z：難易度

        KS(x,y)D>=z（［不利］：KS(x,y)の判定を２回行い、それぞれの結果のより小さい方が結果となります）
        x：ダイスサイズ。1=D4（能力値1、2以上の出目が出ていたとしても最大1）／2=D4（能力値2、3以上の出目が出ていたとしても最大2）／3=D4（能力値3、出目4が出ていたとしても最大3）／4=D4／6=D6／8=D8／10=D10／12=D12／20=D20
        y：ダイス数（省略：1）
        z：難易度

        ・観測ロール
        　［現実乖離］の段階に応じたダイスを指定数ダイスロールして、最も高い出目を出力します。
        KR(x)
        x=［現実乖離］の段階（1=D4／2=D6／3=D8／4=D10／5=D12／6=D20）

        KR(x,y)　観測ロール（リアリティラインあり）
        x=［現実乖離］の段階（1=D4／2=D6／3=D8／4=D10／5=D12／6=D20）
        y=［リアリティライン］のレベル（3=1個／2=2個／1=3個）

        ・虚構の収束の侵蝕度減少ロール
        　［現実乖離］の段階に応じたダイスを指定数ダイスロールして、その合計を出力します。
        KRS(x,y)
        x=［現実乖離］の段階（1=D4／2=D6／3=D8／4=D10／5=D12／6=D20）
        y=ダイスの個数
        ";

        private static readonly Dictionary<int, int> DICE_SIZE_TO_SIDES = new Dictionary<int, int>
        {
            { 1, 4 },
            { 2, 4 },
            { 3, 4 },
            { 4, 4 },
            { 6, 6 },
            { 8, 8 },
            { 10, 10 },
            { 12, 12 },
            { 20, 20 },
        };

        private static readonly int[] GENJITU_KAIRI_TO_SIDES = { 4, 6, 8, 10, 12, 20 };

        private static readonly Dictionary<int, int> REALITY_LINE_TO_TIMES = new Dictionary<int, int>
        {
            { 3, 1 },
            { 2, 2 },
            { 1, 3 },
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollCheck(command, randomizer) ?? RollKansoku(command, randomizer) ?? RollShusoku(command, randomizer);
        }

        private Result? RollCheck(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^KS(?:\(([-+\d]+),([-+\d]+)?\)|(\d+))([AD]?)(?:>=([-+\d]+))?$");
            if (!m.Success)
            {
                return null;
            }

            int? diceSize = m.Groups[1].Success
                ? ArithmeticEvaluator.Eval(m.Groups[1].Value, RoundType)
                : ArithmeticEvaluator.Eval(m.Groups[3].Value, RoundType);
            int? times = m.Groups[2].Success
                ? ArithmeticEvaluator.Eval(m.Groups[2].Value, RoundType)
                : 1;
            int? target = m.Groups[5].Success
                ? ArithmeticEvaluator.Eval(m.Groups[5].Value, RoundType)
                : (int?)null;

            var advantage = m.Groups[4].Value;

            if (diceSize == null || !DICE_SIZE_TO_SIDES.ContainsKey(diceSize.Value))
            {
                return null;
            }
            var sides = DICE_SIZE_TO_SIDES[diceSize.Value];

            if (times == null)
            {
                return null;
            }

            int rollCount = string.IsNullOrEmpty(advantage) ? 1 : 2;
            var rolls = new List<(int[] DiceList, int Value)>();
            for (int i = 0; i < rollCount; i++)
            {
                rolls.Add(RollCheckOnce(times.Value, diceSize.Value, sides, randomizer));
            }

            var values = rolls.Select(v => v.Value).ToArray();

            int value;
            if (advantage == "A")
            {
                value = values.Max();
            }
            else if (advantage == "D")
            {
                value = values.Min();
            }
            else
            {
                value = values.First();
            }

            Result result;
            if (value == 1)
            {
                result = Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
            }
            else if (target.HasValue && value < target.Value)
            {
                result = Result.CreateBuilder("失敗").SetFailure(true).Build();
            }
            else if (target.HasValue && value == sides)
            {
                result = Result.CreateBuilder("クリティカル").SetSuccess(true).SetCritical(true).Build();
            }
            else if (target.HasValue && value >= target.Value)
            {
                result = Result.CreateBuilder("成功").SetSuccess(true).Build();
            }
            else
            {
                result = Result.CreateBuilder("").Build();
            }

            var parts = new List<string>();
            if (target.HasValue)
            {
                parts.Add($"(KS({diceSize},{times}){advantage}>={target})");
            }
            else
            {
                parts.Add($"(KS({diceSize},{times}){advantage})");
            }

            var formattedRolls = FormatRolls(rolls);
            if (formattedRolls != null)
            {
                parts.Add(formattedRolls);
            }

            parts.Add(value.ToString());

            if (!string.IsNullOrEmpty(result.Text))
            {
                parts.Add(result.Text);
            }

            var text = string.Join(" ＞ ", parts);

            return Result.CreateBuilder(text)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .Build();
        }

        private (int[] DiceList, int Value) RollCheckOnce(int times, int diceSize, int sides, IRandomizer randomizer)
        {
            int[] diceList;
            int value;
            if (times < 1)
            {
                diceList = randomizer.RollBarabara(2, sides).OrderBy(x => x).ToArray();
                value = Math.Max(1, Math.Min(diceSize, diceList.Min()));
            }
            else
            {
                diceList = randomizer.RollBarabara(times, sides).OrderBy(x => x).ToArray();
                value = Math.Max(1, Math.Min(diceSize, diceList.Max()));
            }

            return (diceList, value);
        }

        private string? FormatRolls(List<(int[] DiceList, int Value)> rolls)
        {
            if (rolls.Count == 1 && rolls.First().DiceList.Length == 1)
            {
                return null;
            }

            return string.Join(", ", rolls.Select(v =>
                v.DiceList.Length == 1
                    ? v.Value.ToString()
                    : $"{v.Value}[{string.Join(",", v.DiceList)}]"
            ));
        }

        private Result? RollKansoku(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^KR(?:(\d+)|\((\d),(\d)\))$");
            if (!m.Success)
            {
                return null;
            }

            int diceSize = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : int.Parse(m.Groups[2].Value);
            int? realityLine = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : (int?)null;

            if (realityLine.HasValue && (realityLine.Value > 3 || realityLine.Value < 1))
            {
                return null;
            }

            if (diceSize < 1 || diceSize > GENJITU_KAIRI_TO_SIDES.Length)
            {
                return null;
            }
            var sides = GENJITU_KAIRI_TO_SIDES[diceSize - 1];

            int times;
            if (realityLine.HasValue && REALITY_LINE_TO_TIMES.ContainsKey(realityLine.Value))
            {
                times = REALITY_LINE_TO_TIMES[realityLine.Value];
            }
            else
            {
                times = 1;
            }

            var diceList = randomizer.RollBarabara(times, sides).OrderBy(x => x).ToArray();
            var value = diceList.Max();

            var cmd = realityLine.HasValue ? $"KR({diceSize},{realityLine})" : $"KR({diceSize})";

            string text;
            if (times == 1)
            {
                text = $"({cmd}) ＞ {value}";
            }
            else
            {
                text = $"({cmd}) ＞ {value}[{string.Join(",", diceList)}] ＞ {value}";
            }

            return Result.CreateBuilder(text).Build();
        }

        private Result? RollShusoku(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^KRS(?:\((\d),([-+\d]+)\))$");
            if (!m.Success)
            {
                return null;
            }

            var diceSize = int.Parse(m.Groups[1].Value);
            int? times = m.Groups[2].Success ? ArithmeticEvaluator.Eval(m.Groups[2].Value, RoundType) : null;

            if (diceSize < 1 || diceSize > GENJITU_KAIRI_TO_SIDES.Length)
            {
                return null;
            }
            var sides = GENJITU_KAIRI_TO_SIDES[diceSize - 1];

            if (times == null)
            {
                return null;
            }

            var diceList = randomizer.RollBarabara(times.Value, sides);
            var value = diceList.Sum();

            string text;
            if (times.Value == 1)
            {
                text = $"(KRS({diceSize},{times})) ＞ {value}";
            }
            else
            {
                text = $"(KRS({diceSize},{times})) ＞ {value}[{string.Join(",", diceList)}] ＞ {value}";
            }

            return Result.CreateBuilder(text).Build();
        }
    }
}
