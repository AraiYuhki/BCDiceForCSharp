using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ウタカゼ
    /// </summary>
    public sealed class Utakaze : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Utakaze Instance = new Utakaze();

        /// <inheritdoc/>
        public override string Id => "Utakaze";

        /// <inheritdoc/>
        public override string Name => "ウタカゼ";

        /// <inheritdoc/>
        public override string SortKey => "うたかせ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・行為判定ロール（nUK）
          n個のサイコロで行為判定ロール。ゾロ目の最大個数を成功レベルとして表示。nを省略すると2UK扱い。
          例）3UK ：サイコロ3個で行為判定
          例）UK  ：サイコロ2個で行為判定
        ・難易度付き行為判定ロール（nUK>=t）
          tに難易度を指定した行為判定ロール。
          成功レベルと難易度tを比べて成否を判定します。
          例）6UK>=3 ：サイコロ6個で行為判定して、成功レベル3が出れば成功。
        ・クリティカルコール付き行為判定ロール（nUK@c or nUKc）
          cに「龍のダイス目」を指定した行為判定ロール。
          ゾロ目ではなく、cと同じ値の出目数x2が成功レベルとなります。難易度の指定も可能です。
          例）3UK@5 ：龍のダイス「月」でクリティカルコール宣言したサイコロ3個の行為判定
         ・対抗判定ロール(nUR[@c], nUO[@c]) n:ダイス数 c:クリティカルコール
         　行為判定ロールと同様にロールするが、最期に成功レベルとセット数から求めたマジックナンバーが表示される。
         　マジックナンバーの大きいものが成功、同値は引き分け。
         　ダイスは18個まで対応。
        ";

        private static readonly Dictionary<int, string> DRAGON_DICE_NAME = new Dictionary<int, string>
        {
            { 1, "風" },
            { 2, "雨" },
            { 3, "雲" },
            { 4, "影" },
            { 5, "月" },
            { 6, "歌" },
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return CheckRoll(command, randomizer) ?? OpposedRoll(command, randomizer);
        }

        private Result? CheckRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)?UK(@?(\d))?(>=(\d+))?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var baseCount = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 2;
            var crit = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 0;
            var diff = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : 0;

            baseCount = GetValue(baseCount);
            crit = GetValue(crit);

            if (baseCount < 1)
            {
                return null;
            }
            if (crit > 6)
            {
                crit = 6;
            }

            var diceList = randomizer.RollBarabara(baseCount, 6).OrderBy(x => x).ToArray();
            var result = GetRollResult(diceList, crit, diff);

            var sequence = new List<string>
            {
                command,
                $"({baseCount}D6)",
                $"[{string.Join(",", diceList)}]",
                result.Text
            };

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .Build();
        }

        private Result GetRollResult(int[] diceList, int crit, int diff)
        {
            var (success, maxnum, setCount) = GetSuccessInfo(diceList, crit);

            var sequence = new List<string>();

            if (IsDragonDice(crit))
            {
                sequence.Add($"龍のダイス「{DRAGON_DICE_NAME[crit]}」({crit})を使用");
            }

            if (success)
            {
                sequence.Add($"成功レベル:{maxnum} ({setCount}セット)");
            }
            else
            {
                sequence.Add("失敗");
                return Result.CreateBuilder(string.Join(" ＞ ", sequence)).SetFailure(true).Build();
            }

            if (diff == 0)
            {
                return Result.CreateBuilder(string.Join(" ＞ ", sequence)).SetSuccess(true).Build();
            }
            else
            {
                if (maxnum >= diff)
                {
                    sequence.Add("成功");
                    return Result.CreateBuilder(string.Join(" ＞ ", sequence)).SetSuccess(true).Build();
                }
                else
                {
                    sequence.Add("失敗");
                    return Result.CreateBuilder(string.Join(" ＞ ", sequence)).SetFailure(true).Build();
                }
            }
        }

        private Result? OpposedRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)?U[RO](@?(\d))?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var baseCount = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 2;
            var crit = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 0;

            baseCount = GetValue(baseCount);
            crit = GetValue(crit);

            if (baseCount < 1 || baseCount > 18)
            {
                return null;
            }
            if (crit > 6)
            {
                crit = 6;
            }

            var diceList = randomizer.RollBarabara(baseCount, 6).OrderBy(x => x).ToArray();
            var result = GetOpposedRollResult(diceList, crit);

            var sequence = new List<string>
            {
                command,
                $"({baseCount}D6)",
                $"[{string.Join(",", diceList)}]",
                result.Text
            };

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .Build();
        }

        private Result GetOpposedRollResult(int[] diceList, int crit)
        {
            var (success, maxnum, setCount) = GetSuccessInfo(diceList, crit);

            var sequence = new List<string>();

            if (IsDragonDice(crit))
            {
                sequence.Add($"龍のダイス「{DRAGON_DICE_NAME[crit]}」({crit})を使用");
            }

            if (success)
            {
                sequence.Add($"成功レベル:{maxnum} ({setCount}セット)");
                sequence.Add("(" + maxnum.ToString("D2") + setCount.ToString() + ")");
                return Result.CreateBuilder(string.Join(" ＞ ", sequence)).SetSuccess(true).Build();
            }
            else
            {
                sequence.Add("(000)");
                return Result.CreateBuilder(string.Join(" ＞ ", sequence)).SetFailure(true).Build();
            }
        }

        private (bool success, int maxnum, int setCount) GetSuccessInfo(int[] diceList, int crit)
        {
            var diceCountHash = GetDiceCountHash(diceList, crit);

            var maxnum = 0;
            var successDiceList = new List<int>();
            var countThreshold = IsDragonDice(crit) ? 1 : 2;

            foreach (var kvp in diceCountHash)
            {
                var dice = kvp.Key;
                var count = kvp.Value;

                if (count > maxnum)
                {
                    maxnum = count;
                }
                if (count >= countThreshold)
                {
                    successDiceList.Add(dice);
                }
            }

            if (successDiceList.Count <= 0)
            {
                return (false, 0, 0);
            }

            if (IsDragonDice(crit))
            {
                maxnum *= 2;
            }

            return (true, maxnum, successDiceList.Count);
        }

        private Dictionary<int, int> GetDiceCountHash(int[] diceList, int critical)
        {
            return diceList
                .Where(dice => IsNormalDice(critical) || dice == critical)
                .GroupBy(x => x)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private bool IsNormalDice(int crit)
        {
            return !IsDragonDice(crit);
        }

        private bool IsDragonDice(int crit)
        {
            return crit != 0;
        }

        private int GetValue(int number)
        {
            if (number > 100)
            {
                return 0;
            }
            return number;
        }
    }
}
