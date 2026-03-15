using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// SRSじゃない世界樹の迷宮TRPG
    /// </summary>
    public sealed class NSSQ : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly NSSQ Instance = new NSSQ();

        /// <inheritdoc/>
        public override string Id => "NSSQ";

        /// <inheritdoc/>
        public override string Name => "SRSじゃない世界樹の迷宮TRPG";

        /// <inheritdoc/>
        public override string SortKey => "えすああるえすしやないせかいしゆのめいきゆうTRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ 判定 (xSQ±y>=z)
          xD6の判定。3つ以上振ったとき、出目の高い2つを表示します。絶対成功、絶対失敗も計算します。
          2つのサイコロを使用して出目に1があった場合は、FPの獲得も表示します。3つ以上使用した場合は表示しません。
          ±y: yに修正値を入力。±の計算に対応。省略可能。
          z: 目標値。省略可能。

        ■ ダメージロール (xDR(C)(+)y)
          xD6のダメージロール。クリティカルヒットの自動判定を行います。Cを付けるとクリティカルアップ状態で計算できます。+を付けるとクリティカルヒット時のダイスが8個になります。
          x: xに振るダイス数を入力。
          y: yに耐性を入力。
          例) 5DR3 5DRC4 5DRC+4

        ■ 回復ロール (xHRy)
          xD6の回復ロール。クリティカルヒットが発生しません。
          x: xに振るダイス数を入力。
          y: yに耐性を入力。省略した場合3。
          例) 2HR 10HR2

        ■ 採集ロール (TC±z,SC±z,GC±z)
          少しだけ(T)、そこそこ(S)、ガッツリ(G)採取採掘伐採を行います。
          z: zに追加でロールする回数を入力。省略可能。
          例) TC SC+1 GC-1
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollSq(command, randomizer) ?? DamageRoll(command, randomizer) ?? HealRoll(command, randomizer) ?? CollectingRoll(command, randomizer);
        }

        private static int EvalModifier(string expr)
        {
            if (string.IsNullOrEmpty(expr)) return 0;
            int result = 0;
            foreach (Match m in Regex.Matches(expr, @"[+\-]\d+"))
            {
                result += int.Parse(m.Value);
            }
            return result;
        }

        private static string FormatModifier(int value)
        {
            if (value == 0) return "";
            return value > 0 ? $"+{value}" : value.ToString();
        }

        private Result? RollSq(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"(\d+)SQ([+\-\d]+)?(([>=]+)(\d+))?", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var diceCount = int.Parse(m.Groups[1].Value);
            var modifier = EvalModifier(m.Groups[2].Value);
            int? target = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : (int?)null;

            var diceList = randomizer.RollBarabara(diceCount, 6);
            var largestTwo = diceList.OrderByDescending(x => x).Take(2).ToArray();
            var total = largestTwo.Sum() + modifier;
            var num1 = diceList.Count(x => x == 1);

            Result result;
            if (largestTwo.SequenceEqual(new[] { 6, 6 }))
            {
                result = Result.CreateBuilder(" ＞ 絶対成功！").SetSuccess(true).SetCritical(true).Build();
            }
            else if (largestTwo.SequenceEqual(new[] { 1, 1 }))
            {
                result = Result.CreateBuilder(" ＞ 絶対失敗！").SetFumble(true).SetFailure(true).Build();
            }
            else if (target.HasValue && total >= target.Value)
            {
                result = Result.CreateBuilder(" ＞ 成功").SetSuccess(true).Build();
            }
            else if (target.HasValue && total < target.Value)
            {
                result = Result.CreateBuilder(" ＞ 失敗").SetFailure(true).Build();
            }
            else
            {
                result = Result.CreateBuilder("").Build();
            }

            // ダイス数が2個の場合は1の出目の数だけ【FP】を獲得できる
            var fpResult = (diceCount == 2 && num1 >= 1) ? $" (【FP】{num1}獲得)" : "";

            var sequence = new[]
            {
                $"({command})",
                $"[{string.Join(",", diceList)}]{FormatModifier(modifier)}",
                $"{total}[{string.Join(",", largestTwo)}]{result.Text}{fpResult}",
            };

            var text = string.Join(" ＞ ", sequence);
            return Result.CreateBuilder(text)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private Result? DamageRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"(\d+)DR(C)?(\+)?(\d+)", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var diceCount = int.Parse(m.Groups[1].Value);
            var criticalUp = m.Groups[2].Success;
            var increaseCriticalDice = m.Groups[3].Success;
            var resist = int.Parse(m.Groups[4].Value);

            var diceList = randomizer.RollBarabara(diceCount, 6);
            var normalDamage = Damage(diceList, resist);

            var resultText = $"({command}) ＞ [{string.Join(",", diceList)}]{resist}";

            var criticalTarget = criticalUp ? 1 : 2;

            if (diceList.Count(x => x == 6) - diceList.Count(x => x == 1) >= criticalTarget)
            {
                resultText += CriticalDamageRoll(increaseCriticalDice, resist, normalDamage, randomizer);
                return Result.CreateBuilder(resultText).SetSuccess(true).SetCritical(true).Build();
            }
            else
            {
                resultText += $" ＞ {normalDamage}ダメージ";
                return normalDamage > 0 ? Result.CreateBuilder(resultText).SetSuccess(true).Build() : Result.CreateBuilder(resultText).SetFailure(true).Build();
            }
        }

        private string CriticalDamageRoll(bool increaseCriticalDice, int resist, int normalDamage, IRandomizer randomizer)
        {
            var diceCount = increaseCriticalDice ? 8 : 4;
            var diceList = randomizer.RollBarabara(diceCount, 6);
            var criticalDamage = Damage(diceList, resist);
            return $" ＞ クリティカルヒット！ ＞ ({diceCount}DR{resist}) ＞ [{string.Join(",", diceList)}]{resist} ＞ {normalDamage + criticalDamage}ダメージ";
        }

        private Result? HealRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)HR(\d+)?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var diceCount = int.Parse(m.Groups[1].Value);
            var resist = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 3;

            var diceList = randomizer.RollBarabara(diceCount, 6);
            var healAmount = Damage(diceList, resist);
            var resultText = $"({command}) ＞ [{string.Join(",", diceList)}]{resist} ＞ {healAmount}回復";

            return healAmount > 0 ? Result.CreateBuilder(resultText).SetSuccess(true).Build() : Result.CreateBuilder(resultText).SetFailure(true).Build();
        }

        private int Damage(int[] diceList, int resist)
        {
            return diceList.Count(x => x > resist);
        }

        private Result? CollectingRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"([TSG])C([+\-\d]+)?", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var type = m.Groups[1].Value.ToUpper();
            var modifier = EvalModifier(m.Groups[2].Value);

            int aattoParam;
            switch (type)
            {
                case "T":
                    aattoParam = 3;
                    break;
                case "S":
                    aattoParam = 4;
                    break;
                case "G":
                    aattoParam = 5;
                    break;
                default:
                    return null;
            }

            var rollTimes = aattoParam - 2 + modifier;
            if (rollTimes <= 0)
            {
                return null;
            }

            var results = new List<string>();
            for (int i = 0; i < rollTimes; i++)
            {
                var diceList = randomizer.RollBarabara(2, 6);
                var dice = diceList.Sum();
                results.Add($"({command}) ＞ {dice}[{string.Join(",", diceList)}]: {ResultCollecting(i, dice, aattoParam)}");
            }

            return Result.CreateBuilder(string.Join("\n", results))
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private string ResultCollecting(int i, int dice, int aatto)
        {
            if (dice <= aatto && aatto - 2 > i)
            {
                return "！ああっと！";
            }
            else if (aatto - 2 <= i)
            {
                return "成功（追加分）";
            }
            else
            {
                return "成功";
            }
        }

    }
}
