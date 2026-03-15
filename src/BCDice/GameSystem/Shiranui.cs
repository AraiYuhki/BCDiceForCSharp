using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 不知火
    /// </summary>
    public sealed class Shiranui : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Shiranui Instance = new Shiranui();

        /// <inheritdoc/>
        public override string Id => "Shiranui";

        /// <inheritdoc/>
        public override string Name => "不知火";

        /// <inheritdoc/>
        public override string SortKey => "しらぬい";

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■∞D66ダイスロール
        「 ∞D66 」または「 ID66 」
        （ ID は Infinite D の略です）

        □行動力や攻撃力の指定
        「 x+∞D66 」または「 x+ID66 」
        （ x は行動力や攻撃力）

        □鬼火の使用について
        鬼火を使用する∞D66は、ダイスボットでサポートしていません。

        ■おみくじを引く
        OMKJ
        ";

        private static readonly Regex INFINITE_D66_ROLL_REG = new Regex(
            @"^((\d+)\+)?(∞|I)D66$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = INFINITE_D66_ROLL_REG.Match(command);
            if (m.Success)
            {
                int? fixedScore = m.Groups[2].Success ? (int?)int.Parse(m.Groups[2].Value) : null;
                return RollInfiniteD66(fixedScore, randomizer);
            }
            else
            {
                return RollTables(command, randomizer);
            }
        }

        private Result RollInfiniteD66(int? fixedScore, IRandomizer randomizer)
        {
            var steps = new List<InfiniteD66Step>();

            while (steps.Count == 0 || steps.Last().ToContinueDiceroll())
            {
                // 個別の出目をあつかうので、 roll_d66 ではなく roll_barabara を使う
                var dices = randomizer.RollBarabara(2, 6).OrderBy(x => x).ToArray();
                steps.Add(new InfiniteD66Step(dices));
            }

            bool isFailure = steps.First().Score() == 0; // 「しくじり」か？
            int total = isFailure ? 0 : steps.Sum(s => s.Score()) + (fixedScore ?? 0);

            string resultText = $"({MakeCommandText(fixedScore)})";
            resultText += " ＞ " + string.Join(" ＞ ", steps.Select(s => s.ToS()));
            if (isFailure)
            {
                resultText += " ＞ しくじり";
            }
            else
            {
                if (steps.Count > 1 || fixedScore != null)
                {
                    resultText += " ＞ " + ScoreExpressionText(steps, fixedScore);
                }
                resultText += " ＞ " + total.ToString();
            }

            return Result.CreateBuilder(resultText)
                .SetCritical(steps.Count > 1)
                .SetFailure(isFailure)
                .SetFumble(isFailure)
                .Build();
        }

        private static string MakeCommandText(int? fixedScore)
        {
            return fixedScore == null ? "∞D66" : $"{fixedScore}+∞D66";
        }

        private static string ScoreExpressionText(List<InfiniteD66Step> steps, int? fixedScore)
        {
            string text = string.Join("+", steps.Select(s => s.Score().ToString()));
            if (fixedScore != null)
            {
                text = $"{fixedScore}+({text})";
            }
            return text;
        }

        private Result? RollTables(string command, IRandomizer randomizer)
        {
            // おみくじテーブル OMKJ
            if (command == "OMKJ")
            {
                int roll = randomizer.RollOnce(6);
                string[] table = new[]
                {
                    "大凶［御利益１］――このみくじにあたる人は、凶運から逃れることができぬ者なり。まさに凶運にその身をゆだねてこそ、浮かぶ瀬もあれ。……これより上演中に演者が振る［∞Ｄ66］で初めて⚀⚀が出たら、御利益を使っても振り直しができない。",
                    "凶［御利益２］――このみくじにあたる人は、吉兆を逃す定めにある。まさに、天の与うるを取らざれば反ってその咎めを受く。……これより上演中に演者が振る［∞Ｄ66］で初めて⚅⚅が出たら、強制的に１回の振り直しをする。",
                    "小吉［御利益３］――このみくじにあたる人は、神使の機嫌を損ねている。神使が何に怒り、何に苛立っているのかは、まさに神のみぞ知る。……神使の機嫌が突然、悪くなる。これより上演中に神使は何かと理由をつけてはシラヌイの前から立ち去ろうとする。",
                    "中吉［御利益４］――このみくじにあたる人は、神使の機嫌を良くすることを行った者なり。神使が何に喜び、なぜ機嫌が良いのか、まさに神のみぞ知る。……神使の機嫌がすこぶる良くなる。これより上演中に神使は上機嫌となり、シラヌイに何かにつけて話しかけてくれる。",
                    "吉［御利益５］――このみくじにあたる人は、悪運を幸運へと変える道を進む者なり。まさに禍福は糾える縄の如し。……これより上演中に演者が振る［∞Ｄ66］で初めて⚀⚀が出たら、御利益を消費することなく、１回の振り直しをする。",
                    "大吉［御利益６］――このみくじにあたる人は、思いもよらぬ幸運に巡り合う者なり。まさに、暗き道より出て、気づけば月の光あり。……これより上演中に演者が振る［∞Ｄ66］で１回だけ、サイコロの出目を⚅⚅に変えてよい。",
                };
                string result = $"おみくじ(1D6) ＞ {roll} ＞ {table[roll - 1]}";
                return Result.CreateBuilder(result).Build();
            }

            return null;
        }

        /// <summary>
        /// ∞D66の1ステップを表すクラス
        /// </summary>
        private class InfiniteD66Step
        {
            private readonly int[] _dices;

            public InfiniteD66Step(int[] dices)
            {
                _dices = (int[])dices.Clone();
            }

            public int Score()
            {
                if (IsRepdigit())
                {
                    int digit = _dices[0];
                    return digit == 1 ? 0 : digit * 10;
                }
                else
                {
                    return _dices[0] * 10 + _dices[1];
                }
            }

            public bool IsRepdigit()
            {
                return _dices[0] == _dices[1];
            }

            public bool ToContinueDiceroll()
            {
                return IsRepdigit() && _dices[0] != 1;
            }

            public string ToS()
            {
                return $"[{_dices[0]},{_dices[1]}]";
            }
        }
    }
}
