using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ルーインブレイカーズ
    /// </summary>
    public sealed class RuinBreakers : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly RuinBreakers Instance = new RuinBreakers();


        private static readonly Regex RbRegex = new Regex(
            @"^RB",
            RegexOptions.Compiled);

        private static readonly Regex FpdRegex = new Regex(
            @"^FPD",
            RegexOptions.Compiled);

        private static readonly Regex FprRegex = new Regex(
            @"^FPR",
            RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "RuinBreakers";

        /// <inheritdoc/>
        public override string Name => "ルーインブレイカーズ";

        /// <inheritdoc/>
        public override string SortKey => "るういんふれいかあす";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ 基本判定 (RBx@y#z)
          x：成功率、y：クリティカル値（省略可）、z：ファンブル値（省略可）
          1D100を振って、成功率に応じて成功／失敗／クリティカル／ファンブルの判定を行います。(P.60)
          クリティカル値を省略した場合は成功率の5分の1（切り捨て、最低1）
          ファンブル値を省略した場合は、成功率が99以下の場合は96、100以上の場合は99
          例） RB32, RB(45+20)/2, RB30@10, RB35+20#90, RB40-20+10@10#90

        ■ FPへのダメージ (FPDx)
          x：破滅ポイント
          ルーインブレイクロール失敗時やラウンド終了時に、残っている
          破滅ポイントに応じて発生するダメージのダイスロールを行います。(P.91,92)
          例） FPD23

        ■ FPの回復 (FPRx)
          x：破滅ポイント
          ルーインブレイク成功時に発生する、FPの回復量を決定するダイスロールを行います。(P.93)
          例） FPR29

        ■ 各種表
          ・ポジティブ感情表 (PE)
          ・ネガティブ感情表 (NE)
          ・デウス・エクス・マキナ表 (DXM)
          ・断罪チャート (JC)
          ・破滅のイヤな感じ表 (RDF)
          ・トラブルチャート／トラブル解決チャート (TC)
          ・ドタバタアクション表 (DA)
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            Match match;

            match = RbRegex.Match(command);
            if (match.Success)
            {
                return CheckRoll(command, randomizer);
            }

            match = FpdRegex.Match(command);
            if (match.Success)
            {
                return RollFpDamage(command, randomizer);
            }

            match = FprRegex.Match(command);
            if (match.Success)
            {
                return RollFpRecovery(command, randomizer);
            }


            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result? CheckRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^RB(-?\d+([+\-*/]\d+)*)(@(\d+))?(#(\d+))?$");
            if (!m.Success)
            {
                return null;
            }

            int successRate = ArithmeticEvaluator.Eval(m.Groups[1].Value, RoundType.Floor) ?? 0;
            int criticalBorder = m.Groups[4].Success
                ? int.Parse(m.Groups[4].Value)
                : Math.Max(successRate / 5, 1);
            int fumbleBorder = m.Groups[6].Success
                ? int.Parse(m.Groups[6].Value)
                : (successRate < 100 ? 96 : 99);

            int total = randomizer.RollOnce(100);

            string compareResult;
            bool isSuccess;
            bool isFailure;
            bool isCritical;
            bool isFumble;

            if (total >= fumbleBorder)
            {
                isFumble = true;
                isFailure = true;
                isSuccess = false;
                isCritical = false;
                compareResult = "ファンブル";
            }
            else if (total == 1 || total <= criticalBorder)
            {
                isCritical = true;
                isSuccess = true;
                isFailure = false;
                isFumble = false;
                compareResult = "クリティカル";
            }
            else if (total <= successRate)
            {
                isSuccess = true;
                isFailure = false;
                isCritical = false;
                isFumble = false;
                compareResult = "成功";
            }
            else
            {
                isFailure = true;
                isSuccess = false;
                isCritical = false;
                isFumble = false;
                compareResult = "失敗";
            }

            var sequence = new[] { $"(1D100<={successRate}@{criticalBorder}#{fumbleBorder})", total.ToString(), compareResult };
            string text = string.Join(" ＞ ", sequence);

            return Result.CreateBuilder(text)
                .SetSuccess(isSuccess)
                .SetFailure(isFailure)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result? RollFpDamage(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^FPD(\d+)$");
            if (!m.Success)
            {
                return null;
            }

            int ruinPoint = Convert.ToInt32(m.Groups[1].Value);
            int ruinPointTens = ruinPoint / 10;
            int ruinPointOnes = ruinPoint % 10;

            int[] diceList = randomizer.RollBarabara(1 + ruinPointTens, 10);
            int total = diceList.Sum();
            string diceStr = string.Join(",", diceList);

            var sequence = new[]
            {
                $"((1+{ruinPointTens})D10+{ruinPointOnes})",
                $"{total}[{diceStr}]+{ruinPointOnes}",
                $"{total + ruinPointOnes}ダメージ"
            };

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result? RollFpRecovery(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^FPR(\d+)$");
            if (!m.Success)
            {
                return null;
            }

            int ruinPoint = Convert.ToInt32(m.Groups[1].Value);
            int diceCount = (int)Math.Ceiling(ruinPoint / 10.0);

            int[] diceList = randomizer.RollBarabara(diceCount, 10);
            int total = diceList.Sum();
            string diceStr = string.Join(",", diceList);

            var sequence = new[]
            {
                $"({diceCount}D10)",
                $"{total}[{diceStr}]",
                $"{total}回復"
            };

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

    }
}
