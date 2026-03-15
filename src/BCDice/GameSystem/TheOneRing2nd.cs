using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 一つの指輪：指輪物語TRPG2版
    /// </summary>
    public sealed class TheOneRing2nd : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly TheOneRing2nd Instance = new TheOneRing2nd();

        private static readonly Regex DRgRegex = new Regex(
            @"^\d+RG",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex FdRegex = new Regex(
            @"^FD",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // 定数
        private const int SAURONS_EYE_NUMBER = 11;
        private const int GANDALF_RUNE_NUMBER = 12;
        private const string CHOICE_DIE_MARK = "◎";

        // RGコマンドパラメータインデックス
        private const int RG_DIFFICULTY_INDEX = 1;
        private const int RG_SUCCESS_DICE_INDEX = 3;
        private const int RG_PIERCING_BLOWS_INDEX = 5;
        private const int RG_ADJUST_NUMBER_INDEX = 7;
        private const int RG_OPTION_START_INDEX = 8;

        // FDコマンドパラメータインデックス
        private const int FD_ADJUST_NUMBER_INDEX = 2;
        private const int FD_OPTION_START_INDEX = 3;

        /// <inheritdoc/>
        public override string Id => "TheOneRing2nd";

        /// <inheritdoc/>
        public override string Name => "一つの指輪：指輪物語TRPG2版";

        /// <inheritdoc/>
        public override string SortKey => "ひとつのゆひわゆひわものかたりTRPG2";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定コマンド(nRG[x][@y][Az][f[0|1]][i[0|1]][w[0|1]][m[0|1]])
         判定用に難易度nを指定して判定ダイスを振る。技量ダイスx、痛打判定値y、修正値zを指定可能。
         技量ダイス、痛打判定値、修正値は0、または未指定（0と同じ）にできる。
         痛打判定値の0、未指定は痛打判定を行わない。
         修正値は判定合計値に加算され、「ガンダルフ・ルーン」や「サウロンの目」はその影響を受けない。
         例1: 13RG     (難易度13 技量ダイス0個)
         例2: 13RG3    (難易度13 技量ダイス3個)
         例3: 13RG3@10A1  (難易度13 技量ダイス3個、痛打判定10、結果に1を加算)

        ・表用コマンド(FD[x][f[0|1]][i[0|1]])
         表用に判定ダイスを振る。修正値xが指定可能。修正値は0、あるいは未指定(0と同じ)にできる。
         「ガンダルフ・ルーン」や「サウロンの目」は修正値の影響を受けず、値が10を越えることもない。
         例1: FD      (1d12で判定)
         例2: FD1     (1d12で判定し、ダイス目に+1修正)

        ・コマンドオプション
        オプションは、判定コマンドなら4個まで、表用コマンドなら2個まで、順不同で指定可能（重複可）。
          f: 有利(favoured)オプション。不利と同時指定時は相殺。選択された値に◎。
          i: 不利(ill-favoured)オプション。有利と同時指定時は相殺。選択された値に◎。
         例1: 13RG3f   (難易度13 技量ダイス3個、有利)
         例2: FD1f     (1修正、有利)
         例3: 13RG3if   (難易度13 技量ダイス3個、不利、有利)
              ※有利/不利は相殺。

         判定コマンドでは更に下記のオプションを同じ条件で指定可能。
          w: 疲労(weary)状態オプション。
          m: 絶望(miserable)状態オプション。
         例1: 13RG3wf   (難易度13 技量ダイス3個、疲労状態、有利)
         例2: 13RG3fiwm (難易度13 技量ダイス3個、有利、不利、疲労状態、絶望状態)
              ※有利/不利は相殺。最大オプション数である4つを指定。

        ・オプションスイッチ
         指定したオプションのon/offを1/0で指定可能。1はon、0はoffを表す。
         複数の同じオプションへのスイッチ指定は、最後のスイッチが有効となる。
         例1: 13RG3if0  (難易度13 技量ダイス3個、不利はon、有利はoff)
              ※ 有利指定がoffのため、相殺されず不利となる。
         例2: 13RG3f1f0 (難易度13 技量ダイス3個、有利は最終的にoff)
        ";

        // 有利/不利状態のenum
        private enum FavouredState
        {
            Normal,
            Favoured,
            IllFavoured
        }

        // オプションデータクラス
        private class OptionData
        {
            public FavouredState FavouredStateValue { get; }
            public bool Weary { get; }
            public bool Miserable { get; }

            public OptionData(FavouredState favouredState = FavouredState.Normal, bool weary = false, bool miserable = false)
            {
                FavouredStateValue = favouredState;
                Weary = weary;
                Miserable = miserable;
            }
        }

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            if (DRgRegex.IsMatch(command))
            {
                return RgCommandExec(command, randomizer);
            }

            if (FdRegex.IsMatch(command))
            {
                return FdCommandExec(command, randomizer);
            }

            return null;
        }

        // ================ RG/FDコマンド共有メソッド等 ================

        private string GetAdjustNumberText(int adjustNumber)
        {
            if (adjustNumber > 0)
            {
                return $"+{adjustNumber}";
            }
            else if (adjustNumber < 0)
            {
                return adjustNumber.ToString();
            }
            return "";
        }

        private string GetConditionText(OptionData opts)
        {
            if (opts.FavouredStateValue == FavouredState.Normal && !opts.Weary && !opts.Miserable)
            {
                return "";
            }

            var conditionText = "\n状態：";
            if (opts.FavouredStateValue == FavouredState.Favoured)
            {
                conditionText += "有利 ";
            }
            else if (opts.FavouredStateValue == FavouredState.IllFavoured)
            {
                conditionText += "不利 ";
            }
            if (opts.Weary)
            {
                conditionText += "疲労 ";
            }
            if (opts.Miserable)
            {
                conditionText += "絶望 ";
            }

            return conditionText.TrimEnd();
        }

        private OptionData GetOptions(IEnumerable<string> optParams)
        {
            var favouredStateValue = FavouredState.Normal;
            var miserableCondition = false;
            var wearyCondition = false;

            foreach (var x in optParams)
            {
                var optMatch = Regex.Match(x, @"[WFIM]");
                if (!optMatch.Success) continue;

                switch (optMatch.Value)
                {
                    case "W":
                        wearyCondition = OnOptionSwitch(x);
                        break;
                    case "F":
                        favouredStateValue = GetFavouredState(OnOptionSwitch(x), favouredStateValue, FavouredState.Favoured);
                        break;
                    case "I":
                        favouredStateValue = GetFavouredState(OnOptionSwitch(x), favouredStateValue, FavouredState.IllFavoured);
                        break;
                    case "M":
                        miserableCondition = OnOptionSwitch(x);
                        break;
                }
            }

            return new OptionData(favouredStateValue, wearyCondition, miserableCondition);
        }

        private FavouredState GetFavouredState(bool optionSwitch, FavouredState beforeFavouredStateValue, FavouredState targetStateType)
        {
            if (optionSwitch)
            {
                if (beforeFavouredStateValue == targetStateType || beforeFavouredStateValue == FavouredState.Normal)
                {
                    return targetStateType;
                }
                return FavouredState.Normal;
            }
            else
            {
                if (beforeFavouredStateValue == targetStateType)
                {
                    return FavouredState.Normal;
                }
            }
            return beforeFavouredStateValue;
        }

        private bool OnOptionSwitch(string optValue)
        {
            if (optValue.Length == 1)
            {
                return true;
            }
            int switchValue;
            if (int.TryParse(optValue.Substring(1), out switchValue))
            {
                return switchValue > 0;
            }
            return true;
        }

        /// <summary>
        /// 技量ダイスロールを行う
        /// </summary>
        private (string resultText, int totalNumber, int successCount) MakeSuccessDiceRoll(int successDiceCount, bool wearyCondition, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(successDiceCount, 6);
            var successCount = diceList.Count(d => d == 6);
            int successTotalNumber;
            if (wearyCondition)
            {
                successTotalNumber = diceList.Where(i => i > 3).Sum();
            }
            else
            {
                successTotalNumber = diceList.Sum();
            }
            return ($"[{string.Join(",", diceList)}]", successTotalNumber, successCount);
        }

        /// <summary>
        /// 判定ダイスロールを行う
        /// </summary>
        private (string resultText, int dieNumber, int diceCount) MakeFeatDiceRoll(FavouredState favouredStateValue, IRandomizer randomizer)
        {
            var featDiceCount = favouredStateValue == FavouredState.Normal ? 1 : 2;
            var diceList = randomizer.RollBarabara(featDiceCount, 12);

            var choiceDieNumber = DieChoice(diceList, favouredStateValue);
            string featResultText;

            if (featDiceCount > 1)
            {
                var choiceIndex = Array.IndexOf(diceList, choiceDieNumber);

                var firstDie = $"{(choiceIndex == 0 ? CHOICE_DIE_MARK : "")}{GetSpecialDieStr(diceList[0])}";
                var secondDie = $"{(choiceIndex == 1 ? CHOICE_DIE_MARK : "")}{GetSpecialDieStr(diceList[1])}";

                featResultText = $"[{firstDie}, {secondDie}]";
            }
            else
            {
                featResultText = $"[{GetSpecialDieStr(choiceDieNumber)}]";
            }

            return (featResultText, choiceDieNumber, featDiceCount);
        }

        private int DieChoice(int[] diceList, FavouredState favouredStateValue)
        {
            if (favouredStateValue == FavouredState.IllFavoured)
            {
                if (diceList.Count(d => d == SAURONS_EYE_NUMBER) >= 1)
                {
                    return SAURONS_EYE_NUMBER;
                }
                else
                {
                    return diceList.Min();
                }
            }
            else if (favouredStateValue == FavouredState.Favoured)
            {
                if (diceList.Count(d => d == GANDALF_RUNE_NUMBER) >= 1)
                {
                    return GANDALF_RUNE_NUMBER;
                }
                else
                {
                    // ガンダルフ・ルーンが無ければサウロンの目を除いた最大値を選ぶ
                    // ※ どちらもサウロンの目ならサウロンの目を返す
                    if (diceList.Count(d => d == SAURONS_EYE_NUMBER) == 2)
                    {
                        return SAURONS_EYE_NUMBER;
                    }
                    else
                    {
                        return diceList.Where(x => x != SAURONS_EYE_NUMBER).Max();
                    }
                }
            }
            return diceList[0];
        }

        private string GetSpecialDieStr(int dieNumber)
        {
            if (dieNumber == GANDALF_RUNE_NUMBER)
            {
                return "ガンダルフ・ルーン";
            }
            else if (dieNumber == SAURONS_EYE_NUMBER)
            {
                return "サウロンの目";
            }
            return dieNumber.ToString();
        }

        // ================ FDコマンド ================

        private Result? FdCommandExec(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(FD)(-?\d*)?([FI]-?\d*)?([FI]-?\d*)?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            // コマンドパラメータ取得
            var (adjustNumber, opts) = GetFdParams(m);

            // 判定ダイス処理
            var (featResultText, featDieNumber, featDiceCount) = MakeFeatDiceRoll(opts.FavouredStateValue, randomizer);
            var resultHeaderText = $"({featDiceCount}D12";

            return GetFdRollResult(resultHeaderText, featResultText, featDieNumber, featDiceCount, adjustNumber);
        }

        private Result? GetFdRollResult(string resultHeaderText, string featResultText, int featDieNumber, int featDiceCount, int adjustNumber)
        {
            var (resultDieNumber, adjustNumberText) = GetFdAdjust(featDieNumber, adjustNumber);
            resultHeaderText = $"{resultHeaderText}{adjustNumberText}) ＞ {featResultText}{adjustNumberText}";

            if (adjustNumber != 0 || featDiceCount != 1)
            {
                var text = $"{resultHeaderText} ＞ [{GetSpecialDieStr(resultDieNumber)}]";
                return Result.CreateBuilder(text).Build();
            }
            return Result.CreateBuilder(resultHeaderText).Build();
        }

        private (int resultDieNumber, string adjustNumberText) GetFdAdjust(int featDieNumber, int adjustNumber)
        {
            if (new[] { SAURONS_EYE_NUMBER, GANDALF_RUNE_NUMBER }.Contains(featDieNumber))
            {
                return (featDieNumber, GetAdjustNumberText(adjustNumber));
            }

            var resTotalNum = featDieNumber + adjustNumber;
            if (resTotalNum > 10)
            {
                resTotalNum = 10;
            }
            else if (resTotalNum < 1)
            {
                resTotalNum = 1;
            }
            return (resTotalNum, GetAdjustNumberText(adjustNumber));
        }

        private (int adjustNumber, OptionData opts) GetFdParams(Match m)
        {
            var adjustNumberStr = m.Groups[FD_ADJUST_NUMBER_INDEX].Value;
            var adjustNumber = string.IsNullOrEmpty(adjustNumberStr) ? 0 : int.Parse(adjustNumberStr);

            var optParams = new List<string>();
            for (int i = FD_OPTION_START_INDEX; i < m.Groups.Count; i++)
            {
                if (m.Groups[i].Success)
                {
                    optParams.Add(m.Groups[i].Value);
                }
            }

            return (adjustNumber, GetOptions(optParams));
        }

        // ================ RGコマンド ================

        private Result? RgCommandExec(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)(RG)(\d*)(@(\d{0,2}))?(A(-?\d*))?([WFIM]-?\d*)?([WFIM]-?\d*)?([WFIM]-?\d*)?([WFIM]-?\d*)?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var successCount = 0;

            // コマンドパラメータ取得
            var (difficulty, successDiceCount, piercingBlowsNumber, adjustNumber, opts) = GetRgParams(m);

            // 判定ダイス処理
            var (featResultText, featDieNumber, featDiceCount) = MakeFeatDiceRoll(opts.FavouredStateValue, randomizer);
            var totalDiceNumber = (featDieNumber != SAURONS_EYE_NUMBER ? featDieNumber : 0);

            var resultHeaderText = $"({featDiceCount}D12";
            var resultDiceText = featResultText;

            // 技量ダイスが指定されているならその処理
            if (successDiceCount > 0)
            {
                var (successResultText, successTotalNumber, sc) = MakeSuccessDiceRoll(successDiceCount, opts.Weary, randomizer);
                successCount = sc;
                totalDiceNumber += successTotalNumber;

                resultHeaderText = $"{resultHeaderText}+{successDiceCount}D6";
                resultDiceText = $"{resultDiceText}+{successResultText}";
            }

            // 修正値処理
            string adjustNumberText;
            (totalDiceNumber, adjustNumberText) = GetRgAdjust(totalDiceNumber, adjustNumber);

            resultHeaderText = $"{resultHeaderText}{adjustNumberText}) ＞ {resultDiceText}{adjustNumberText}";

            return GetRgRollResult(resultHeaderText, difficulty, featDieNumber, piercingBlowsNumber, totalDiceNumber, successCount, opts);
        }

        private (int totalDiceNumber, string adjustNumberText) GetRgAdjust(int totalDiceNumber, int adjustNumber)
        {
            totalDiceNumber += adjustNumber;
            if (totalDiceNumber < 0)
            {
                totalDiceNumber = 0;
            }
            return (totalDiceNumber, GetAdjustNumberText(adjustNumber));
        }

        private Result? GetRgRollResult(string resultHeaderText, int difficulty, int featDieNumber, int piercingBlowsNumber, int totalDiceNumber, int successCount, OptionData opts)
        {
            // 状態表示取得
            var conditionText = GetConditionText(opts);

            var successCountText = $"成功度 {successCount}";

            // 痛打判定
            Func<int, int, string, string, string> piercingBlows = (featDieNum, piercingBlowsNum, resText, condText) =>
            {
                if (piercingBlowsNum > 0 && featDieNum >= piercingBlowsNum && featDieNum != SAURONS_EYE_NUMBER)
                {
                    resText = $"{resText} 痛打発生！";
                }
                return $"{resText}{condText}";
            };

            if (featDieNumber == GANDALF_RUNE_NUMBER)
            {
                var gandalfRuneText = $"{resultHeaderText}：自動成功[{successCountText}]";
                gandalfRuneText = piercingBlows(featDieNumber, piercingBlowsNumber, gandalfRuneText, conditionText);
                if (successCount >= 2)
                {
                    return Result.CreateBuilder(gandalfRuneText).SetSuccess(true).SetCritical(true).Build();
                }
                return Result.CreateBuilder(gandalfRuneText).SetSuccess(true).Build();
            }
            else if (opts.Miserable && featDieNumber == SAURONS_EYE_NUMBER)
            {
                return Result.CreateBuilder($"{resultHeaderText}：自動失敗{conditionText}").SetFailure(true).Build();
            }

            var resultDetailText = $"難易度 {difficulty} 達成値 {totalDiceNumber}";
            if (difficulty > totalDiceNumber)
            {
                return Result.CreateBuilder($"{resultHeaderText} {resultDetailText}：失敗{conditionText}").SetFailure(true).Build();
            }

            var successText = $"{resultHeaderText} {resultDetailText}：成功[{successCountText}]";
            successText = piercingBlows(featDieNumber, piercingBlowsNumber, successText, conditionText);
            if (successCount >= 2)
            {
                return Result.CreateBuilder(successText).SetSuccess(true).SetCritical(true).Build();
            }
            return Result.CreateBuilder(successText).SetSuccess(true).Build();
        }

        private (int difficulty, int successDiceCount, int piercingBlowsNumber, int adjustNumber, OptionData opts) GetRgParams(Match m)
        {
            var difficulty = int.Parse(m.Groups[RG_DIFFICULTY_INDEX].Value);
            var successDiceCountStr = m.Groups[RG_SUCCESS_DICE_INDEX].Value;
            var successDiceCount = string.IsNullOrEmpty(successDiceCountStr) ? 0 : int.Parse(successDiceCountStr);
            var adjustNumberStr = m.Groups[RG_ADJUST_NUMBER_INDEX].Value;
            var adjustNumber = string.IsNullOrEmpty(adjustNumberStr) ? 0 : int.Parse(adjustNumberStr);
            var piercingBlowsNumber = m.Groups[RG_PIERCING_BLOWS_INDEX].Success && !string.IsNullOrEmpty(m.Groups[RG_PIERCING_BLOWS_INDEX].Value)
                ? int.Parse(m.Groups[RG_PIERCING_BLOWS_INDEX].Value)
                : -1;

            var optParams = new List<string>();
            for (int i = RG_OPTION_START_INDEX; i < m.Groups.Count; i++)
            {
                if (m.Groups[i].Success)
                {
                    optParams.Add(m.Groups[i].Value);
                }
            }

            return (difficulty, successDiceCount, piercingBlowsNumber, adjustNumber, GetOptions(optParams));
        }
    }
}
