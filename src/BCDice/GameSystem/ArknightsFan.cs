using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Arithmetic;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// アークナイツTRPG by daaaper
    /// </summary>
    public sealed class ArknightsFan : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly ArknightsFan Instance = new ArknightsFan();

        /// <inheritdoc/>
        public override string Id => "ArknightsFan";

        /// <inheritdoc/>
        public override string Name => "アークナイツTRPG by daaaper";

        /// <inheritdoc/>
        public override string SortKey => "ああくないつTRPGはいてえはあ";

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override int SidesImplicitD => 100;

        /// <inheritdoc/>
        public override string HelpMessage => @"
■ 能力値判定 (nADm<=x)
  nDmのダイスロールをして、出目が x 以下であれば成功。
  出目が91以上でエラー。
  出目が10以下でクリティカル。

■ 攻撃/防御判定 (nABm<=x)
  nBmのダイスロールをして、
    出目が x 以下であれば成功数+1。
    出目が91以上でエラー。成功数-1。
    出目が10以下でクリティカル。成功数+1。
  上記による成功数をカウント。

■ 役職効果付き攻撃判定 (nABm<=x--役職名h)
  h: 健康状態(0: 健康、1: 中等症、2: 重症)
  nBmのダイスロールをして、
    出目が x 以下であれば成功数+1。
    出目が91以上でエラー。成功数-1。
    出目が10以下でクリティカル。成功数+1。
  上記による成功数をカウントした上で、以下の役職名による成功数増加効果を適応。
    狙撃（SNI）: 健康(h=0)かつ成功数1以上のとき、成功数+1。
  健康状態hを省略した場合、健康(h=0)として扱われる。

■ 鉱石病判定 (ORPx@y+Dd+Tt)
  x: 生理的耐性、y: 上昇後侵食度、d: ダイス補正、t: 判定値補正
  生理的耐性xのOPが侵食度yに上昇した際の鉱石病判定を、ダイス数補正d、判定値補正tで行う。
  ダイス数補正と判定値補正は省略可能。例えば ORP60@25 は ORP60@25+D0+T0 と同義。
  また、ダイス数補正と判定値補正は逆順でも可。例えば ORP60@25+T10+D2 も可。

■ 増悪判定（--WORSENING）
  症状を「末梢神経障害」「内臓機能不全」「精神症状」からランダムに選択。
  継続ラウンド数を1d6+1で判定。

■ 中毒判定（--ADDICTION）
  症状を「脳神経障害」「多臓器不全」「急性精神反応」からランダムに選択。

■ 判定の省略表記
  nADm、nABm、nABmにおいて、
    n（ダイス個数）を省略した場合、1として扱われる。
    m（ダイス種類）を省略した場合、100として扱われる。
  例えば、AD<=90は1AD100<=90として解釈される。
";

        private static class Status
        {
            public const int CRITICAL = 1;
            public const int SUCCESS = 2;
            public const int FAILURE = 3;
            public const int ERROR = 4;
        }

        private static readonly Dictionary<int, string> STATUS_NAME = new Dictionary<int, string>
        {
            { Status.CRITICAL, "クリティカル！" },
            { Status.SUCCESS, "成功" },
            { Status.FAILURE, "失敗" },
            { Status.ERROR, "エラー" },
        };

        private static readonly int[] ENDURANCE_LEVEL_TABLE = { 20, 40, 70, 90, int.MaxValue };
        private static readonly int[] ORP_TIMES_TABLE = { 1, 2, 2, 3, 4 };

        private static readonly string[] WORSENING_TABLE =
        {
            "末梢神経障害",
            "内臓機能不全",
            "精神症状",
        };

        private static readonly string[] ADDICTION_TABLE =
        {
            "脳神経障害",
            "多臓器不全",
            "急性精神症状",
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return EvalAd(command, randomizer)
                ?? EvalAb(command, randomizer)
                ?? EvalOrp(command, randomizer)
                ?? EvalWorsening(command, randomizer)
                ?? EvalAddiction(command, randomizer);
        }

        /// <summary>
        /// クリティカル、エラー、成功失敗周りの閾値や優先関係が複雑かつルールが変動する可能性があるため、
        /// 明示的にルール管理するための関数。
        /// </summary>
        private int CheckRoll(int rollResult, int target)
        {
            bool success = rollResult <= target;
            string crierror;
            if (rollResult <= 10)
            {
                crierror = "Critical";
            }
            else if (rollResult >= 91)
            {
                crierror = "Error";
            }
            else
            {
                crierror = "Neutral";
            }

            if (success && crierror == "Critical")
            {
                return Status.CRITICAL;
            }
            else if (success && crierror == "Neutral")
            {
                return Status.SUCCESS;
            }
            else if (success && crierror == "Error")
            {
                return Status.SUCCESS;
            }
            else if (!success && crierror == "Critical")
            {
                return Status.FAILURE;
            }
            else if (!success && crierror == "Neutral")
            {
                return Status.FAILURE;
            }
            else // !success && crierror == "Error"
            {
                return Status.ERROR;
            }
        }

        private Result? EvalAd(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^([-+*/\d]*)AD(\d*)<=([-+*/\d]+)$");
            if (!m.Success)
            {
                return null;
            }

            string timesStr = m.Groups[1].Value;
            string sidesStr = m.Groups[2].Value;
            int? target = ArithmeticEvaluator.Eval(m.Groups[3].Value, RoundType);
            if (target == null) return null;

            int times = !string.IsNullOrEmpty(timesStr) ? (ArithmeticEvaluator.Eval(timesStr, RoundType) ?? 1) : 1;
            int sides = !string.IsNullOrEmpty(sidesStr) ? int.Parse(sidesStr) : 100;

            return RollAd(command, times, sides, target.Value, randomizer);
        }

        private Result? EvalAb(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^([-+*/\d]*)AB(\d*)<=([-+*/\d]+)(?:--([^\d\s]+)([0-2])?)?$");
            if (!m.Success)
            {
                return null;
            }

            string timesStr = m.Groups[1].Value;
            string sidesStr = m.Groups[2].Value;
            int? target = ArithmeticEvaluator.Eval(m.Groups[3].Value, RoundType);
            if (target == null) return null;

            string? type = m.Groups[4].Success ? m.Groups[4].Value : null;
            string? typeStatusStr = m.Groups[5].Success ? m.Groups[5].Value : null;

            int times = !string.IsNullOrEmpty(timesStr) ? (ArithmeticEvaluator.Eval(timesStr, RoundType) ?? 1) : 1;
            int sides = !string.IsNullOrEmpty(sidesStr) ? int.Parse(sidesStr) : 100;

            int typeStatus;
            if (typeStatusStr != null)
            {
                typeStatus = int.Parse(typeStatusStr);
            }
            else if (type == "SNIPER")
            {
                // スプレッドシート版キャラシの後方互換性のために必要
                typeStatus = 1;
            }
            else
            {
                typeStatus = 0;
            }

            if (type == null)
            {
                return RollAb(command, times, sides, target.Value, randomizer);
            }
            else
            {
                return RollAbWithtype(command, times, sides, target.Value, type, typeStatus, randomizer);
            }
        }

        private Result? EvalOrp(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^ORP(?'END'[-+*/\d]+)@(?'ORP'[-+*/\d]+)(?:\+D(?'DICE'[-+*/\d]+))?(?:\+T(?'TGT'[-+*/\d]+))?$");
            if (!m.Success)
            {
                // D補正とT補正が逆順でも対応する
                m = Regex.Match(command, @"^ORP(?'END'[-+*/\d]+)@(?'ORP'[-+*/\d]+)(?:\+T(?'TGT'[-+*/\d]+))?(?:\+D(?'DICE'[-+*/\d]+))?$");
            }
            if (!m.Success)
            {
                return null;
            }

            int? endurance = ArithmeticEvaluator.Eval(m.Groups["END"].Value, RoundType);
            int? oripathy = ArithmeticEvaluator.Eval(m.Groups["ORP"].Value, RoundType);
            if (endurance == null || oripathy == null) return null;

            int timesMod = m.Groups["DICE"].Success ? (ArithmeticEvaluator.Eval(m.Groups["DICE"].Value, RoundType) ?? 0) : 0;
            int targetMod = m.Groups["TGT"].Success ? (ArithmeticEvaluator.Eval(m.Groups["TGT"].Value, RoundType) ?? 0) : 0;

            return RollOrp(command, endurance.Value, oripathy.Value, timesMod, targetMod, randomizer);
        }

        private Result? RollAd(string command, int times, int sides, int target, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(times, sides).OrderBy(x => x).ToList();
            int total = diceList.Sum();

            int result = CheckRoll(total, target);

            string resultText;
            if (times == 1)
            {
                resultText = $"({command}) ＞ {string.Join(",", diceList)} ＞ {STATUS_NAME[result]}";
            }
            else
            {
                resultText = $"({command}) ＞ {total}[{string.Join(",", diceList)}] ＞ {STATUS_NAME[result]}";
            }

            switch (result)
            {
                case Status.CRITICAL:
                    return Result.CreateBuilder(resultText).SetSuccess(true).SetCritical(true).Build();
                case Status.SUCCESS:
                    return Result.CreateBuilder(resultText).SetSuccess(true).Build();
                case Status.FAILURE:
                    return Result.CreateBuilder(resultText).SetFailure(true).Build();
                case Status.ERROR:
                    return Result.CreateBuilder(resultText).SetFumble(true).SetFailure(true).Build();
                default:
                    return null;
            }
        }

        private Result? RollAb(string command, int times, int sides, int target, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(times, sides).OrderBy(x => x).ToList();

            var (successCount, criticalCount, errorCount) = ProcessAb(diceList, target);
            int resultCount = successCount + criticalCount - errorCount;

            string resultText = $"({command}) ＞ [{string.Join(",", diceList)}] ＞ {successCount}+{criticalCount}C-{errorCount}E ＞ 成功数{resultCount}";

            return Result.CreateBuilder(resultText)
                .SetCondition(resultCount > 0)
                .SetCritical(criticalCount > 0)
                .SetFumble(errorCount > 0)
                .Build();
        }

        private Result? RollAbWithtype(string command, int times, int sides, int target, string type, int typeStatus, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(times, sides).OrderBy(x => x).ToList();

            var (successCount, criticalCount, errorCount) = ProcessAb(diceList, target);
            int resultCount = successCount + criticalCount - errorCount;

            int? resultMod = null;
            switch (type)
            {
                case "SNI":
                    resultMod = (typeStatus == 0 && resultCount > 0) ? 1 : 0;
                    break;
                case "SNIPER":
                    // スプレッドシート版キャラシの後方互換性のため残している
                    resultMod = (typeStatus != 0 && resultCount > 0) ? 1 : 0;
                    break;
            }

            string resultText;
            if (resultMod != null)
            {
                resultCount += resultMod.Value;
                resultText = $"({command}) ＞ [{string.Join(",", diceList)}] ＞ {successCount}+{criticalCount}C-{errorCount}E+{resultMod}({type}) ＞ 成功数{resultCount}";
            }
            else
            {
                resultText = $"({command}) ＞ [{string.Join(",", diceList)}] ＞ {successCount}+{criticalCount}C-{errorCount}E ＞ 成功数{resultCount}";
            }

            return Result.CreateBuilder(resultText)
                .SetCondition(resultCount > 0)
                .SetCritical(criticalCount > 0)
                .SetFumble(errorCount > 0)
                .Build();
        }

        private Result? RollOrp(string command, int endurance, int oripathy, int timesMod, int targetMod, IRandomizer randomizer)
        {
            int sides = 100;

            int enduranceLevel = Array.FindIndex(ENDURANCE_LEVEL_TABLE, n => endurance <= n);
            int originalTimes = ORP_TIMES_TABLE[enduranceLevel];
            int times = originalTimes + timesMod;

            if (oripathy <= 20)
            {
                return Result.CreateBuilder($"({command}).Build() ＞ 鉱石病判定が発生しない侵食度です。侵食度は21以上を指定してください。").Build();
            }

            int oripathyStage = (int)Math.Ceiling(oripathy / 20.0) - 1;
            int originalTarget = (80 - oripathyStage * 20) - (oripathy - oripathyStage * 20) * 5;
            int target = originalTarget + targetMod;

            string diceAndTargetText = $"ダイス数{originalTimes}"
                + (timesMod > 0 ? $"+{timesMod}" : "")
                + $"、判定値{originalTarget}"
                + (targetMod > 0 ? $"+{targetMod}" : "");

            var resultTexts = new List<string> { $"({command})", diceAndTargetText, $"{times}B100<={target}" };

            if (target <= 0)
            {
                resultTexts.Add("自動失敗！");
                return Result.CreateBuilder(string.Join(" ＞ ", resultTexts)).SetFailure(true).Build();
            }

            var diceList = randomizer.RollBarabara(times, sides).OrderBy(x => x).ToList();
            int successCount = diceList.Count(n => n <= target);

            if (successCount > 0)
            {
                resultTexts.Add($"[{string.Join(",", diceList)}]");
                resultTexts.Add($"成功数{successCount}");
                resultTexts.Add("成功");
                return Result.CreateBuilder(string.Join(" ＞ ", resultTexts)).SetSuccess(true).Build();
            }
            else
            {
                resultTexts.Add($"[{string.Join(",", diceList)}]");
                resultTexts.Add($"成功数{successCount}");
                resultTexts.Add("失敗");
                return Result.CreateBuilder(string.Join(" ＞ ", resultTexts)).SetFailure(true).Build();
            }
        }

        private (int successCount, int criticalCount, int errorCount) ProcessAb(List<int> diceList, int target)
        {
            int successCount = 0;
            int criticalCount = 0;
            int errorCount = 0;

            foreach (int value in diceList)
            {
                switch (CheckRoll(value, target))
                {
                    case Status.CRITICAL:
                        criticalCount += 1;
                        successCount += 1;
                        break;
                    case Status.SUCCESS:
                        successCount += 1;
                        break;
                    case Status.FAILURE:
                        // Nothing to do
                        break;
                    case Status.ERROR:
                        errorCount += 1;
                        break;
                }
            }

            return (successCount, criticalCount, errorCount);
        }

        private Result? EvalWorsening(string command, IRandomizer randomizer)
        {
            if (command != "--WORSENING")
            {
                return null;
            }

            int value = randomizer.RollOnce(3);
            string chosen = WORSENING_TABLE[value - 1];
            int elapse = randomizer.RollOnce(6) + 1;

            return Result.CreateBuilder($"--WORSENING ＞ {chosen}: {elapse} rounds").Build();
        }

        private Result? EvalAddiction(string command, IRandomizer randomizer)
        {
            if (command != "--ADDICTION")
            {
                return null;
            }

            int value = randomizer.RollOnce(3);
            string chosen = ADDICTION_TABLE[value - 1];

            return Result.CreateBuilder($"--ADDICTION ＞ {chosen}").Build();
        }
    }
}
