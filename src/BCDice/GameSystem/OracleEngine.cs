using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// オラクルエンジン
    /// </summary>
    public sealed class OracleEngine : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly OracleEngine Instance = new OracleEngine();


        private static readonly Regex DClRegex = new Regex(
            @"\d+CL.*",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex DD6DRegex = new Regex(
            @"\d+D6.*\$[+-]?\d.*",
            RegexOptions.Compiled);

        private static readonly Regex DR6Regex = new Regex(
            @"\d+R6",
            RegexOptions.Compiled);

        // Parses: xCL[67]?[+/-modify][>=target]
        private static readonly Regex ClutchParseRegex = new Regex(
            @"^(\d+CL[67]?)([+-]\d+)?(>=(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Parses: xR6[+/-modify][@critical][#fumble][$dollar][>=target]
        private static readonly Regex RRollParseRegex = new Regex(
            @"^(\d+R6)([+-]\d+)?(?:@([+-]?\d+))?(?:#([+-]?\d+))?(?:\$([+-]?\d+))?(>=(\d+))?$",
            RegexOptions.Compiled);

        // Parses: xD6[+/-modify][$dollar]
        private static readonly Regex DamageParseRegex = new Regex(
            @"^(\d+D6)([+-]\d+)?\$([+-]?\d+)$",
            RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "OracleEngine";

        /// <inheritdoc/>
        public override string Name => "オラクルエンジン";

        /// <inheritdoc/>
        public override string SortKey => "おらくるえんしん";

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
          ・クラッチロール （xCL+y>=z)
          ダイスをx個振り、1個以上目標シフトzに到達したか判定します。修正yは全てのダイスにかかります。
          成功した時は目標シフトを、失敗した時はダイスの最大値-1シフトを返します
          zが指定されないときは、ダイスをx個を振り、それに修正yしたものを返します。
          通常、最低シフトは1、最大シフトは6です。目標シフトもそろえられます。
          また、CLの後に7を入れ、(xCL7+y>=z)と入力すると最大シフトが7になります。
         ・判定 (xR6+y@c#f$b>=z)
          ダイスをx個振り、大きいもの2つだけを見て達成値を算出し、成否を判定します。修正yは達成値にかかります。
          ダイスブレイクとしてbを、クリティカル値としてcを、ファンブル値としてfを指定できます。
          それぞれ指定されない時、0,12,2になります。
          クリティカル値の上限はなし、下限は2。ファンブル値の上限は12、下限は0。
          zが指定されないとき、達成値の算出のみ行います。
         ・ダメージロールのダイスブレイク (xD6+y$b)
          ダイスをx個振り、合計値を出します。修正yは合計値にかかります。
          ダイスブレイクとしてbを指定します。合計値は0未満になりません。
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            if (DClRegex.IsMatch(command))
            {
                return ClutchRoll(command, randomizer);
            }

            if (DD6DRegex.IsMatch(command))
            {
                return DamageRoll(command, randomizer);
            }

            if (DR6Regex.IsMatch(command))
            {
                return RRoll(command, randomizer);
            }

            return null;
        }

        private Result? ClutchRoll(string str, IRandomizer randomizer)
        {
            var m = ClutchParseRegex.Match(str);
            if (!m.Success)
            {
                return null;
            }

            string commandPart = m.Groups[1].Value; // e.g. "3CL7" or "2CL"
            int modifyNumber = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
            bool hasCmpOp = m.Groups[3].Success;
            int targetNumber = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 0;

            var clParts = commandPart.ToUpper().Split(new[] { "CL" }, StringSplitOptions.None);
            int times = int.Parse(clParts[0]);
            int maxShift = (clParts.Length > 1 && !string.IsNullOrEmpty(clParts[1])) ? int.Parse(clParts[1]) : 6;

            if (hasCmpOp)
            {
                targetNumber = Clamp(targetNumber, 1, maxShift);
            }
            if (times == 0)
            {
                return null;
            }

            var diceList = randomizer.RollBarabara(times, 6).Select(x => Clamp(x + modifyNumber, 1, maxShift)).OrderBy(x => x).ToList();

            var maxShiftStr = maxShift == 7 ? "7" : "";
            var cmpOpStr = hasCmpOp ? ">=" : "";
            var modifyNumberStr = Format.Modifier(modifyNumber);
            var targetStr = hasCmpOp ? targetNumber.ToString() : "";
            var exprClutch = $"({times}CL{maxShiftStr}{modifyNumberStr}{cmpOpStr}{targetStr})";

            var afterShift = diceList.Last();
            string resultClutch;
            if (!hasCmpOp)
            {
                resultClutch = $"シフト{afterShift}";
            }
            else if (afterShift >= targetNumber)
            {
                resultClutch = $"成功 シフト{targetNumber}";
            }
            else
            {
                afterShift -= 1;
                if (afterShift < 1)
                {
                    afterShift = 1;
                }
                resultClutch = $"失敗 シフト{afterShift}";
            }

            var sequence = new[]
            {
                exprClutch,
                $"[{string.Join(", ", diceList)}]",
                resultClutch,
            };

            return Result.CreateBuilder(string.Join(" ＞ ", sequence)).Build();
        }

        private static int Clamp(int i, int min, int max)
        {
            if (i < min)
            {
                return min;
            }
            else if (i > max)
            {
                return max;
            }
            else
            {
                return i;
            }
        }

        private Result? RRoll(string str, IRandomizer randomizer)
        {
            var m = RRollParseRegex.Match(str);
            if (!m.Success)
            {
                return null;
            }

            string commandPart = m.Groups[1].Value; // e.g. "3R6"
            int modifyNumber = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
            int? criticalRaw = m.Groups[3].Success ? (int?)int.Parse(m.Groups[3].Value) : null;
            int? fumbleRaw = m.Groups[4].Success ? (int?)int.Parse(m.Groups[4].Value) : null;
            int? dollarRaw = m.Groups[5].Success ? (int?)int.Parse(m.Groups[5].Value) : null;
            bool hasCmpOp = m.Groups[6].Success;
            int targetNumber = m.Groups[7].Success ? int.Parse(m.Groups[7].Value) : 0;

            int times = int.Parse(commandPart.Replace("R6", ""));
            if (times == 0)
            {
                return null;
            }

            int critical = NormalizeCritical(criticalRaw ?? 12, str);
            int fumble = NormalizeFumble(fumbleRaw ?? 2, str);
            int breakCount = Math.Abs(dollarRaw ?? 0);

            var diceList = randomizer.RollBarabara(times, 6).OrderBy(x => x).ToList();

            // pop(breakCount) = 末尾からbreakCount個を取り出す
            List<int> diceBroken;
            if (breakCount > 0 && breakCount < diceList.Count)
            {
                diceBroken = diceList.Skip(diceList.Count - breakCount).ToList();
                diceList = diceList.Take(diceList.Count - breakCount).ToList();
            }
            else if (breakCount >= diceList.Count)
            {
                diceBroken = new List<int>(diceList);
                diceList = new List<int>();
            }
            else
            {
                diceBroken = new List<int>();
            }

            // ブレイク後のダイスから最大値2つの合計がダイスの値
            int diceTotal = diceList.OrderByDescending(x => x).Take(2).Sum();
            int total = diceTotal + modifyNumber;

            // expr_r
            var modifyNumberStr = Format.Modifier(modifyNumber);
            var criticalStr = (critical == 12) ? "" : $"c[{critical}]";
            var fumbleStr = (fumble == 2) ? "" : $"f[{fumble}]";
            var brakStr = (breakCount == 0) ? "" : $"b[{breakCount}]";
            var cmpOpStr = hasCmpOp ? ">=" : "";
            var targetStr = hasCmpOp ? targetNumber.ToString() : "";
            var exprR = $"({times}R6{modifyNumberStr}{criticalStr}{fumbleStr}{brakStr}{cmpOpStr}{targetStr})";

            // dice_result_r
            var modifyNumberText = Format.Modifier(modifyNumber);
            string diceResultR;
            if (diceBroken.Count == 0)
            {
                diceResultR = $"{diceTotal}[{string.Join(", ", diceList)}]{modifyNumberText}";
            }
            else
            {
                diceResultR = $"{diceTotal}[{string.Join(", ", diceList)}]×[{string.Join(", ", diceBroken)}]{modifyNumberText}";
            }

            // result_r
            string resultR;
            if (diceTotal <= fumble)
            {
                resultR = "ファンブル!";
            }
            else if (diceTotal >= critical)
            {
                resultR = "クリティカル!";
            }
            else if (hasCmpOp)
            {
                if (total >= targetNumber)
                {
                    resultR = $"{total} 成功";
                }
                else
                {
                    resultR = $"{total} 失敗";
                }
            }
            else
            {
                resultR = total.ToString();
            }

            var sequence = new[] { exprR, diceResultR, resultR };
            return Result.CreateBuilder(string.Join(" ＞ ", sequence)).Build();
        }

        private int NormalizeCritical(int critical, string str)
        {
            if (Regex.IsMatch(str, @"@[+-]"))
            {
                critical = 12 + critical;
            }
            if (critical < 2)
            {
                critical = 2;
            }
            return critical;
        }

        private int NormalizeFumble(int fumble, string str)
        {
            if (Regex.IsMatch(str, @"#[+-]"))
            {
                fumble = 2 + fumble;
            }
            return Clamp(fumble, 0, 12);
        }

        private Result? DamageRoll(string str, IRandomizer randomizer)
        {
            var m = DamageParseRegex.Match(str);
            if (!m.Success)
            {
                return null;
            }

            string commandPart = m.Groups[1].Value; // e.g. "3D6"
            int modifyNumber = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
            int breakCount = Math.Abs(int.Parse(m.Groups[3].Value));

            int times = int.Parse(commandPart.Replace("D6", ""));
            if (times == 0)
            {
                return null;
            }

            var diceList = randomizer.RollBarabara(times, 6).OrderBy(x => x).ToList();

            // pop(breakCount) = 末尾からbreakCount個を取り出す
            List<int> diceBroken;
            if (breakCount > 0 && breakCount < diceList.Count)
            {
                diceBroken = diceList.Skip(diceList.Count - breakCount).ToList();
                diceList = diceList.Take(diceList.Count - breakCount).ToList();
            }
            else if (breakCount >= diceList.Count)
            {
                diceBroken = new List<int>(diceList);
                diceList = new List<int>();
            }
            else
            {
                diceBroken = new List<int>();
            }

            int totalN = diceList.Sum() + modifyNumber;
            if (totalN < 0)
            {
                totalN = 0;
            }

            // expr_damage
            var modifyNumberStr = Format.Modifier(modifyNumber);
            var brakStr = (breakCount == 0) ? "" : $"b[{breakCount}]";
            var exprDamage = $"({times}D6{modifyNumberStr}{brakStr})";

            // result_damage
            int diceTotal = diceList.Sum();
            var modifyNumberText = Format.Modifier(modifyNumber);
            string resultDamage;
            if (diceBroken.Count == 0)
            {
                resultDamage = $"{diceTotal}[{string.Join(", ", diceList)}]{modifyNumberText}";
            }
            else
            {
                resultDamage = $"{diceTotal}[{string.Join(", ", diceList)}]×[{string.Join(", ", diceBroken)}]{modifyNumberText}";
            }

            var sequence = new string[] { exprDamage, resultDamage, totalN.ToString() };
            return Result.CreateBuilder(string.Join(" ＞ ", sequence)).Build();
        }

    }
}
