using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// EDGE FLIPPERS
    /// </summary>
    public sealed class EdgeFlippers : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly EdgeFlippers Instance = new EdgeFlippers();

        /// <inheritdoc/>
        public override string Id => "EdgeFlippers";

        /// <inheritdoc/>
        public override string Name => "EDGE FLIPPERS";

        /// <inheritdoc/>
        public override string SortKey => "えつしふりつはあす";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ 汎用コマンド
          xHD+ySD=t,t,t
          人間ダイスと怪異ダイスをロールして、特定の出目があるか判定します。
          x: 人間ダイスの数
          y: 怪異ダイスの数
          t: 必要な出目（省略化）

        ■ 拮抗判定
          xHD+ySD=Xi
          人間ダイスと怪異ダイスをロールして、ゾロ目が必要数あるか判定します。
          i: 必要なゾロ目の個数

        ■ 全力判定
          xHFD>=t
          xSFD>=t
          x: ダイスの数
          t: 目標値（省略時20）

        ■ 存在判定
          EXIST -> 2HD+2SD

        ■ 都市判定
          xHCD -> xHD=1,2,3,4

        ■ 超常判定
          nSPD -> nSD=7,8,9,10

        ■ 術式判定
          TD

        ■ ランダム表
          BAET：【人間】にも【怪異】にも影響のある後遺症
          HAET：【人間寄り】の時に影響のある後遺症
          SAET：【怪異寄り】の時に影響のある後遺症
        ";

        private static readonly Dictionary<string, object> TABLES = new Dictionary<string, object>
        {
            {
                "BAET", new SimpleTable(
                    "【人間】にも【怪異】にも影響のある後遺症",
                    "1D12",
                    new[]
                    {
                        "鏡に映らない：鏡や水面に姿が映らなくなる。他者から見えなくなるわけではない",
                        "部分的に透明：身体のどこかが透明になる",
                        "部分的欠落：身体のどこかが消えて触ることもできなくなる。生命維持に影響はない",
                        "角が生える：角が生えてくる。元から角がある場合はさらに追加",
                        "しっぽ：動物のしっぽが生える",
                        "けもみみ：動物の耳が頭に生える",
                        "影がなくなる：自分の影がなくなってしまう",
                        "涙が止まらない：ずっと涙が流れ続けて止まらなくなる",
                        "視界の端に何かいる：常に視界の端に何かがいて、踊っている",
                        "耳鳴り：ずっと耳鳴りがやまない。たまに幻聴がする",
                        "美しい幻聴：時々他人の声が現実離れした美しい声に変わって聞こえる",
                        "皮膚の下のうごめき：皮膚の下でずっと何かが蠢いているように感じられる",
                    }
                )
            },
            {
                "HAET", new SimpleTable(
                    "【人間寄り】の時に影響のある後遺症",
                    "1D6",
                    new[]
                    {
                        "幻肢の感覚：存在しない追加の腕や翼の感覚がある",
                        "声変わり：普段とは違う声になってしまう",
                        "怪異の片鱗：【怪異寄り】のときの容姿の一部が【人間寄り】の時にも出てしまう",
                        "幻覚：時折やけに生々しい幻覚が見えるようになる",
                        "恐怖：あらゆるものが怖く見える",
                        "怪異言語：【人間】の領域には存在しない謎の言語が理解できる",
                    }
                )
            },
            {
                "SAET", new SimpleTable(
                    "【怪異寄り】の時に影響のある後遺症",
                    "1D6",
                    new[]
                    {
                        "吸精体質：他者に触れると生命力を吸収してしまう。特に仲の良い相手だとより強くなる",
                        "威圧する視線：あなたの視線は周囲に強烈な威圧感を与えてしまう",
                        "キメラ：あなたの容姿の一部が、他の【越境者】の【怪異寄り】のときの容姿のものと同じになる",
                        "異なる怪異：あなた本来の【怪異】の姿とは違う姿に変化してしまう",
                        "ポルターガイスト：周囲でラップ音や部品の浮遊といった怪現象が多発する",
                        "人間そのもの：【怪異寄り】のときでも人間そのものの容姿になってしまう",
                    }
                )
            }
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollHs(command, randomizer) ?? RollAlias(command, randomizer) ?? RollFullpower(command, randomizer) ?? RollTechnical(command, randomizer) ?? RollTables(command, TABLES);
        }

        private Result? RollHs(string command, IRandomizer randomizer)
        {
            var cmd = HSCommand.Parse(command);
            if (cmd == null)
            {
                return null;
            }

            return cmd.Roll(randomizer);
        }

        private Result? RollAlias(string command, IRandomizer randomizer)
        {
            if (command == "EXIST")
            {
                return RollHs("2HD+2SD", randomizer);
            }

            var m = Regex.Match(command, @"^(\d+)(HCD|SPD)$");
            if (!m.Success)
            {
                return null;
            }

            var n = int.Parse(m.Groups[1].Value);
            switch (m.Groups[2].Value)
            {
                case "HCD":
                    return RollHs($"{n}HD=1,2,3,4", randomizer);
                case "SPD":
                    return RollHs($"{n}SD=7,8,9,10", randomizer);
                default:
                    return null;
            }
        }

        private Result? RollFullpower(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)([HS]FD)>=(\d+)$");
            if (!m.Success)
            {
                // コマンドが [HS]FD の形式だが目標値省略の場合も試す
                var m2 = Regex.Match(command, @"^(\d+)([HS]FD)$");
                if (!m2.Success)
                {
                    return null;
                }
                // 目標値省略時は20をデフォルトとする
                return RollFullpowerInternal(int.Parse(m2.Groups[1].Value), m2.Groups[2].Value, 20, randomizer);
            }

            return RollFullpowerInternal(int.Parse(m.Groups[1].Value), m.Groups[2].Value, int.Parse(m.Groups[3].Value), randomizer);
        }

        private Result? RollFullpowerInternal(int n, string commandName, int target, IRandomizer randomizer)
        {
            if (n == 0)
            {
                return null;
            }

            var side = commandName == "HFD" ? 6 : 10;
            var dice = randomizer.RollBarabara(n, side).OrderBy(x => x).ToArray();
            var sum = dice.Sum();
            var tries = new List<int[]> { dice };

            while (sum < target)
            {
                dice = randomizer.RollBarabara(n, side).OrderBy(x => x).ToArray();
                tries.Add(dice);
                sum += dice.Sum();
            }

            var text = $"({n}{commandName}>={target})";
            foreach (var ds in tries)
            {
                text += $" ＞ {ds.Sum()}[{string.Join(",", ds)}]";
            }

            var isSuccess = tries.Count < 5;
            text += $" ＞ {sum}({tries.Count}回)";
            text += isSuccess ? " ＞ 成功" : " ＞ 失敗";

            return Result.CreateBuilder(text)
                .SetCondition(isSuccess)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private Result? RollTechnical(string command, IRandomizer randomizer)
        {
            if (command != "TD")
            {
                return null;
            }

            var value = randomizer.RollOnce(10);
            var isSuccess = value <= 2;

            var text = $"(TD) ＞ {value} ＞ {(isSuccess ? "成功" : "失敗")}";

            return Result.CreateBuilder(text)
                .SetCondition(isSuccess)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        /// <summary>
        /// HD/SD コマンドの解析と実行を行う内部クラス
        /// </summary>
        private class HSCommand
        {
            private readonly int _h;
            private readonly int _s;
            private readonly int[]? _targets;
            private readonly int? _sameValue;

            private int[] _hv = Array.Empty<int>();
            private int[] _sv = Array.Empty<int>();
            private bool? _isSuccess;

            private HSCommand(int h, int s, int[]? targets, int? sameValue)
            {
                _h = h;
                _s = s;
                _targets = targets;
                _sameValue = sameValue;
            }

            public static HSCommand? Parse(string command)
            {
                var m = Regex.Match(command, @"^(?:(\d+)HD(?:\+(\d+)SD)?|(\d+)SD(?:\+(\d+)HD)?)(?:=(?:([\d,]+)|X(\d+)))?$");
                if (!m.Success)
                {
                    return null;
                }

                var h = (m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0);
                if (h == 0)
                {
                    h = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 0;
                }

                var s = (m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0);
                if (s == 0)
                {
                    s = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 0;
                }

                var targets = ParseTarget(m.Groups[5].Value);
                var sameValue = m.Groups[6].Success ? (int?)int.Parse(m.Groups[6].Value) : null;

                if (h == 0 && s == 0)
                {
                    return null;
                }

                return new HSCommand(h, s, targets, sameValue);
            }

            private static int[]? ParseTarget(string str)
            {
                if (string.IsNullOrEmpty(str))
                {
                    return null;
                }

                return str.Split(',')
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => int.Parse(s))
                    .OrderBy(x => x)
                    .ToArray();
            }

            public Result Roll(IRandomizer randomizer)
            {
                _hv = randomizer.RollBarabara(_h, 6).OrderBy(x => x).ToArray();
                _sv = randomizer.RollBarabara(_s, 10).OrderBy(x => x).ToArray();
                var values = _hv.Concat(_sv).OrderBy(x => x).ToArray();

                if (_targets != null)
                {
                    _isSuccess = Subset(values, _targets);
                }
                else if (_sameValue.HasValue)
                {
                    _isSuccess = values.GroupBy(x => x).Any(g => g.Count() >= _sameValue.Value);
                }
                else
                {
                    _isSuccess = null;
                }

                var parts = new List<string> { Expr(), ResultValues() };
                var label = ResultLabel();
                if (label != null)
                {
                    parts.Add(label);
                }

                var text = string.Join(" ＞ ", parts.Where(x => !string.IsNullOrEmpty(x)));

                var builder = Result.CreateBuilder(text)
                    .SetRands(randomizer.RandResults);

                if (_isSuccess.HasValue)
                {
                    builder.SetCondition(_isSuccess.Value);
                }

                return builder.Build();
            }

            private bool Subset(int[] superset, int[] subset)
            {
                if (subset.Length == 0)
                {
                    return true;
                }

                var i = 0;
                foreach (var a in superset)
                {
                    var b = subset[i];
                    if (b == a)
                    {
                        i += 1;
                        if (i == subset.Length)
                        {
                            return true;
                        }
                    }
                    else if (a > b)
                    {
                        return false;
                    }
                }

                return false;
            }

            private string Expr()
            {
                return $"({ExprBody()}{ExprCond()})";
            }

            private string ExprBody()
            {
                if (_h != 0 && _s != 0)
                {
                    return $"{_h}HD+{_s}SD";
                }
                else if (_h != 0)
                {
                    return $"{_h}HD";
                }
                else
                {
                    return $"{_s}SD";
                }
            }

            private string ExprCond()
            {
                if (_targets != null)
                {
                    return $"={string.Join(",", _targets)}";
                }
                else if (_sameValue.HasValue)
                {
                    return $"=X{_sameValue.Value}";
                }
                else
                {
                    return "";
                }
            }

            private string ResultValues()
            {
                var d6 = _hv.Length > 0 ? $"D6[{string.Join(",", _hv)}]" : null;
                var d10 = _sv.Length > 0 ? $"D10[{string.Join(",", _sv)}]" : null;
                return string.Join(" ", new[] { d6, d10 }.Where(x => x != null));
            }

            private string? ResultLabel()
            {
                switch (_isSuccess)
                {
                    case true:
                        return "成功";
                    case false:
                        return "失敗";
                    default:
                        return null;
                }
            }
        }
    }
}
