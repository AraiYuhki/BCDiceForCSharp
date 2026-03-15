using System;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// アースドーン
    /// </summary>
    public sealed class EarthDawn : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly EarthDawn Instance = new EarthDawn();

        /// <inheritdoc/>
        public override string Id => "EarthDawn";

        /// <inheritdoc/>
        public override string Name => "アースドーン";

        /// <inheritdoc/>
        public override string SortKey => "ああすとおん";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ステップダイス　(xEn+k)
        ステップx、目標値n(省略可能）、カルマダイスk(D2-D20)でステップダイスをロールします。
        振り足しも自動。
        例）9E　10E8　10E+D12
        ";

        private static readonly Regex StepRegex = new Regex(
            @"(\d+)E(\d+)?(\+)?(\d+)?(D\d+)?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex D20Regex = new Regex(@"D20", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex D12Regex = new Regex(@"D12", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex D10Regex = new Regex(@"D10", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex D8Regex = new Regex(@"D8", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex D6Regex = new Regex(@"D6", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex D4Regex = new Regex(@"D4", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return EdStep(command, randomizer);
        }

        private Result? EdStep(string str, IRandomizer randomizer)
        {
            string? output = GetStepResult(str, randomizer);
            if (output == null)
            {
                return null;
            }

            return Result.CreateBuilder(output).Build();
        }

        private string? GetStepResult(string str, IRandomizer randomizer)
        {
            var m = StepRegex.Match(str);
            if (!m.Success)
            {
                return null;
            }

            int stepTotal = 0;
            bool isFailed = true;

            int step = int.Parse(m.Groups[1].Value); // ステップ
            int targetNumber = 0; // 目標値
            bool hasKarmaDice = false; // カルマダイスの有無
            int karmaDiceCount = 0; // カルマダイスの個数又は修正
            string karmaDiceType = ""; // カルマダイスの種類

            // 空値があった時の為のばんぺいくんRX
            if (step > 40)
            {
                step = 40;
            }

            if (m.Groups[2].Success)
            {
                targetNumber = int.Parse(m.Groups[2].Value);
                if (targetNumber > 43)
                {
                    targetNumber = 42;
                }
            }

            if (m.Groups[3].Success)
            {
                hasKarmaDice = true;
            }

            if (m.Groups[4].Success)
            {
                karmaDiceCount = int.Parse(m.Groups[4].Value);
            }

            if (m.Groups[5].Success)
            {
                karmaDiceType = m.Groups[5].Value;
            }

            if (targetNumber < 0)
            {
                return null;
            }

            int[][] stable = GetStepTable();

            int nmod = stable[0][step - 1];
            int d20step = stable[1][step - 1];
            int d12step = stable[2][step - 1];
            int d10step = stable[3][step - 1];
            int d8step = stable[4][step - 1];
            int d6step = stable[5][step - 1];
            int d4step = stable[6][step - 1];

            if (hasKarmaDice)
            {
                if (D20Regex.IsMatch(karmaDiceType))
                {
                    d20step += karmaDiceCount;
                }
                else if (D12Regex.IsMatch(karmaDiceType))
                {
                    d12step += karmaDiceCount;
                }
                else if (D10Regex.IsMatch(karmaDiceType))
                {
                    d10step += karmaDiceCount;
                }
                else if (D8Regex.IsMatch(karmaDiceType))
                {
                    d8step += karmaDiceCount;
                }
                else if (D6Regex.IsMatch(karmaDiceType))
                {
                    d6step += karmaDiceCount;
                }
                else if (D4Regex.IsMatch(karmaDiceType))
                {
                    d4step += karmaDiceCount;
                }
                else
                {
                    nmod += karmaDiceCount;
                }
            }

            string diceStr = "";

            stepTotal += RollStep(20, d20step, randomizer, ref diceStr, ref isFailed);
            stepTotal += RollStep(12, d12step, randomizer, ref diceStr, ref isFailed);
            stepTotal += RollStep(10, d10step, randomizer, ref diceStr, ref isFailed);
            stepTotal += RollStep(8, d8step, randomizer, ref diceStr, ref isFailed);
            stepTotal += RollStep(6, d6step, randomizer, ref diceStr, ref isFailed);
            stepTotal += RollStep(4, d4step, randomizer, ref diceStr, ref isFailed);

            if (nmod > 0)
            {
                diceStr += "+";
            }

            if (nmod != 0)
            {
                diceStr += nmod.ToString();
                stepTotal += nmod;
            }

            // ステップ判定終了
            diceStr += $" ＞ {stepTotal}";

            string output = $"ステップ{step} ＞ {diceStr}";
            if (targetNumber == 0)
            {
                return output;
            }

            // 結果判定
            diceStr += " ＞ ";

            int excelentSuccessNumber = stable[7][targetNumber - 1];
            int superSuccessNumber = stable[8][targetNumber - 1];
            int goodSuccessNumber = stable[9][targetNumber - 1];
            int failedNumber = stable[11][targetNumber - 1];

            if (isFailed)
            {
                diceStr += "自動失敗";
            }
            else if (stepTotal >= excelentSuccessNumber)
            {
                diceStr += "最良成功";
            }
            else if (stepTotal >= superSuccessNumber)
            {
                diceStr += "優成功";
            }
            else if (stepTotal >= goodSuccessNumber)
            {
                diceStr += "良成功";
            }
            else if (stepTotal >= targetNumber)
            {
                diceStr += "成功";
            }
            else if (stepTotal < failedNumber)
            {
                diceStr += "大失敗";
            }
            else
            {
                diceStr += "失敗";
            }

            output = $"ステップ{step}>={targetNumber} ＞ {diceStr}";

            return output;
        }

        private int[][] GetStepTable()
        {
            // 表      1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36 37 38 39 40 41 42 34x 3x 4x 5x 6x 7x 8x 9x10x11x12x13x
            var mod = new[] { -2, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var d20 = new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var d12 = new[] { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1 };
            var d10 = new[] { 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 2, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 2, 1, 1, 1, 1, 2, 1, 1, 1, 2, 3, 2, 1, 1, 1, 2, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 2, 1 };
            var d8 = new[] { 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 1, 1, 2, 1, 1, 1, 2, 2, 1, 1, 1, 1, 2, 1, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0 };
            var d6 = new[] { 0, 0, 0, 1, 0, 0, 0, 2, 1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 2, 1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 2, 1, 0, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 0, 1, 0, 0, 0, 2, 1, 1, 0, 0, 0 };
            var d4 = new[] { 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var exsuc = new[] { 6, 8, 10, 12, 14, 17, 19, 20, 22, 24, 25, 27, 29, 32, 33, 35, 37, 38, 39, 41, 42, 44, 45, 47, 48, 49, 51, 52, 54, 55, 56, 58, 59, 60, 62, 64, 65, 67, 68, 70, 71, 72 };
            var ssuc = new[] { 4, 6, 8, 10, 11, 13, 15, 16, 18, 19, 21, 22, 24, 26, 27, 29, 30, 32, 33, 34, 35, 37, 38, 40, 41, 42, 43, 45, 46, 47, 48, 49, 51, 52, 53, 55, 56, 58, 59, 60, 61, 62 };
            var gsuc = new[] { 2, 4, 6, 7, 9, 10, 12, 13, 14, 15, 17, 18, 20, 21, 22, 24, 25, 26, 27, 28, 29, 31, 32, 33, 34, 35, 36, 38, 39, 40, 41, 42, 43, 45, 46, 47, 48, 50, 51, 52, 53, 54 };
            var nsuc = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
            var fsuc = new[] { 0, 1, 1, 1, 1, 2, 2, 3, 4, 5, 5, 6, 6, 7, 8, 8, 9, 10, 11, 12, 13, 13, 14, 15, 16, 17, 18, 18, 18, 20, 21, 22, 23, 23, 24, 25, 26, 26, 27, 28, 29, 30 };

            var stable = new int[][] { mod, d20, d12, d10, d8, d6, d4, exsuc, ssuc, gsuc, nsuc, fsuc };
            return stable;
        }

        private int RollStep(int diceType, int diceCount, IRandomizer randomizer, ref string diceStr, ref bool isFailed)
        {
            int stepTotal = 0;
            if (diceCount <= 0)
            {
                return stepTotal;
            }

            // diceぶんのステップ判定
            if (diceStr.Length > 0)
            {
                diceStr += "+";
            }

            diceStr += $"{diceCount}d{diceType}[";

            for (int i = 0; i < diceCount; i++)
            {
                int diceNow = randomizer.RollOnce(diceType);

                if (diceNow != 1)
                {
                    isFailed = false;
                }

                int diceIn = diceNow;

                // 振り足し（最大値が出たら振り足す）
                while (diceNow == diceType)
                {
                    diceNow = randomizer.RollOnce(diceType);
                    diceIn += diceNow;
                }

                stepTotal += diceIn;

                if (i != 0)
                {
                    diceStr += ",";
                }

                diceStr += diceIn.ToString();
            }

            diceStr += "]";

            return stepTotal;
        }
    }
}
