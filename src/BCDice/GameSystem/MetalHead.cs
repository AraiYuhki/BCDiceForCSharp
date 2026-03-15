using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Arithmetic;
using BCDice.Core;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// メタルヘッド
    /// </summary>
    public sealed class MetalHead : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly MetalHead Instance = new MetalHead();

        private static readonly Regex CrcRegex = new Regex(
            @"\ACRC(\w)(\d+)\z",
            RegexOptions.Compiled);

        private static readonly Regex HrRegex = new Regex(
            @"\AHR<=(.+)",
            RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "MetalHead";

        /// <inheritdoc/>
        public override string Name => "メタルヘッド";

        /// <inheritdoc/>
        public override string SortKey => "めたるへつと";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・アビリティロール  AR>=目標値
        ・スキルロール      SR<=目標値(%)
        ・命中判定ロール    HR<=目標値(%)

          例）AR>=5
          例）SR<=(40+25)
          例）HR<=(50-10)

          これらのロールで成否、絶対成功/絶対失敗、クリティカル/アクシデントを自動判定します。

        ・クリティカルチャート  CC
        ・アクシデントチャート  射撃・投擲用:ACL  格闘用:ACS
        ・戦闘結果チャート      CRCsn   s=耐久レベル(SUV) n=数値

          例）CRCA61 SUV=Aを対象とした数値61(62に変換される)の戦闘結果を参照する。
          例）CRCM98 対物で数値98の戦闘結果を参照する。
        ";

        private static readonly Dictionary<string, ITable> TABLES = new Dictionary<string, ITable>
        {
            {
                "CC", new RangeTable(
                    "クリティカルチャート",
                    "CC",
                    1, 10,
                    new (int, int, string)[]
                    {
                        (1, 1, "相手は知覚系に多大なダメージを受けた。PERを1にして頭部にHWのダメージ、および心理チェック。"),
                        (2, 2, "相手の運動神経を断ち切った。DEXを1にして腕部にHWのダメージ、および心理チェック。さらに腕に持っていた武器などは落としてしまう。"),
                        (3, 3, "相手の移動手段は完全に奪われた。REFを1にして脚部にHWダメージと心理チェック。また、次回からのこちらの攻撃は必ず命中する。"),
                        (4, 5, "相手の急所に命中。激痛のため気絶した上、胴にHWダメージ。"),
                        (6, 6, "効果的な一撃。胴にHWダメージ。心理チェック。"),
                        (7, 7, "効果的な一撃。胴にMOダメージ。心理チェック。"),
                        (8, 10, "君の一撃は相手の中枢を完全に破壊した。即死である。"),
                    })
            },
            {
                "ACL", new RangeTable(
                    "アクシデントチャート（射撃・投擲）",
                    "ACL",
                    1, 10,
                    new (int, int, string)[]
                    {
                        (1, 7, "ささいなミス。特にペナルティーはない。"),
                        (8, 8, "不発、またはジャム。弾を取り出さねばならない物は次のターンは射撃できない。"),
                        (9, 9, "ささいな故障。可能なら次のターンから個別武器のスキルロールで修理を行える。"),
                        (10, 10, "武器の暴発、または爆発。頭部HWの心理効果ロール。さらに、その武器は破壊されPERとDEXのどちらか、または両方に計2ポイントのマイナスを与える。（遠隔操作の場合、射手への被害は無し）"),
                    })
            },
            {
                "ACS", new RangeTable(
                    "アクシデントチャート（格闘）",
                    "ACS",
                    1, 10,
                    new (int, int, string)[]
                    {
                        (1, 3, "足を滑らせて転倒し、起き上がるまで相手に+20の命中修正を与える。"),
                        (4, 6, "手を滑らせて、武器を落とす。素手の時は関係ない。"),
                        (7, 9, "使用武器の破壊。素手戦闘のときはMWのダメージを受ける。"),
                        (10, 10, "手を滑らせ、不幸にも武器は飛んでいき、5m以内に人がいれば誰かに刺さるか、または打撃を与えるかもしれない。ランダムに決定し、普通どおり判定を続ける。素手のときは関係ない。"),
                    })
            },
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // テーブルコマンドを試す
            if (TABLES.TryGetValue(command, out var table))
            {
                return table.Roll(randomizer);
            }

            // CRCコマンド
            var crcMatch = CrcRegex.Match(command);
            if (crcMatch.Success)
            {
                var suv = crcMatch.Groups[1].Value;
                var num = crcMatch.Groups[2].Value;
                return MhCrcTable(suv, num);
            }

            // HR<=コマンド
            var hrMatch = HrRegex.Match(command);
            if (hrMatch.Success)
            {
                var target = ArithmeticEvaluator.Eval(hrMatch.Groups[1].Value, RoundType);
                if (target == null)
                {
                    return null;
                }
                return RollHit(target.Value, randomizer);
            }

            return null;
        }

        /// <summary>
        /// 2D6の判定結果を返す
        /// </summary>
        // TODO: Hook up to AddDice common command when base class supports EvalResult2D6
        private Result? EvalResult2D6(int total, int diceTotal, IReadOnlyList<int> diceList, CompareOperator cmpOp, int target, IRandomizer randomizer)
        {
            if (cmpOp != CompareOperator.GreaterThanOrEqual)
            {
                return null;
            }

            if (diceTotal >= 12)
            {
                return Result.CreateBuilder("絶対成功").SetSuccess(true).SetCritical(true).Build();
            }
            else if (diceTotal <= 2)
            {
                return Result.CreateBuilder("絶対失敗").SetFumble(true).SetFailure(true).Build();
            }

            return null;
        }

        /// <inheritdoc/>
        public override string ChangeText(string text)
        {
            text = Regex.Replace(text, @"^(S)?AR", m => $"{m.Groups[1].Value}2D6", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"^(S)?SR", m => $"{m.Groups[1].Value}1D100", RegexOptions.IgnoreCase);
            return text;
        }

        private Result? RollHit(int target, IRandomizer randomizer)
        {
            var total = randomizer.RollOnce(100);
            var resultText = GetHitResult(total, total, target);

            var text = $"(1D100<={target}) ＞ {total}{resultText}";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private string GetHitResult(int totalN, int diceN, int diff)
        {
            var diceValue = totalN % 100;
            var dice1 = diceValue % 10;

            if (totalN > diff)
            {
                return " ＞ 失敗";
            }

            if (dice1 == 1)
            {
                return " ＞ 成功（クリティカル）";
            }
            if (dice1 == 0)
            {
                return " ＞ 失敗（アクシデント）";
            }

            return " ＞ 成功";
        }

        private Result? MhCrcTable(string suv, string num)
        {
            var headerParts = new List<string> { "戦闘結果チャート", num };
            var separator = " ＞ ";

            suv = suv.ToUpper();
            var numbuf = int.Parse(num);
            if (numbuf < 1)
            {
                return Result.CreateBuilder(string.Join(separator, new List<string>(headerParts) { "数値が不正です" })).Build();
            }

            var numD1 = numbuf % 10;
            if (numD1 == 1)
            {
                numbuf += 1;
            }
            if (numD1 == 0)
            {
                numbuf -= 1;
            }
            numD1 = numbuf % 10;

            var tablePoint = new string?[] { null, null, "腕部", "腕部", "脚部", "脚部", "胴部", "胴部", "胴部", "頭部" };

            var tableDamage = new Dictionary<string, List<(string Label, int Threshold)>>
            {
                { "S", new List<(string, int)> { ("N", 2), ("LW", 16), ("MD", 46), ("MW", 56), ("HD", 76), ("HW", 96), ("MO", 106), ("K", 116) } },
                { "A", new List<(string, int)> { ("LW", 2), ("MW", 46), ("HW", 76), ("MO", 96), ("K", 116) } },
                { "B", new List<(string, int)> { ("LW", 2), ("MW", 36), ("HW", 66), ("MO", 96), ("K", 106) } },
                { "C", new List<(string, int)> { ("LW", 2), ("MW", 26), ("HW", 66), ("MO", 86), ("K", 106) } },
                { "D", new List<(string, int)> { ("LW", 2), ("MW", 26), ("HW", 46), ("MO", 76), ("K", 96) } },
                { "E", new List<(string, int)> { ("LW", 2), ("MW", 26), ("HW", 39), ("MO", 54), ("K", 76) } },
                { "F", new List<(string, int)> { ("LW", 2), ("MW", 16), ("HW", 39), ("MO", 54), ("K", 66) } },
                { "G", new List<(string, int)> { ("LW", 2), ("MW", 6), ("HW", 16), ("MO", 26), ("K", 39) } },
                { "M", new List<(string, int)> { ("0", 2), ("1", 22), ("2", 42), ("3", 62), ("4", 82), ("5", 92), ("6", 102), ("8", 112) } },
            };

            if (!tableDamage.ContainsKey(suv))
            {
                var errorParts = new List<string>(headerParts)
                {
                    $"耐久レベル(SUV)[{suv}]",
                    "耐久レベル(SUV)の値が不正です"
                };
                return Result.CreateBuilder(string.Join(separator, errorParts)).Build();
            }

            var damageLevel = "";
            foreach (var entry in tableDamage[suv])
            {
                if (entry.Threshold <= numbuf)
                {
                    damageLevel = entry.Label;
                }
            }

            var resultParts = new List<string>();

            if (numbuf != int.Parse(num))
            {
                resultParts.Add(numbuf.ToString());
            }

            if (suv == "M")
            {
                resultParts.Add("耐物");
                resultParts.Add($"HP[{damageLevel}]");
            }
            else
            {
                resultParts.Add($"耐久レベル(SUV)[{suv}]");
                resultParts.Add($"部位[{tablePoint[numD1]}] ： 損傷種別[{damageLevel}]");
            }

            var allParts = new List<string>(headerParts);
            allParts.AddRange(resultParts);
            return Result.CreateBuilder(string.Join(separator, allParts)).Build();
        }
    }
}
