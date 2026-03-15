using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// スタリィドール
    /// </summary>
    public sealed class StarryDolls : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly StarryDolls Instance = new StarryDolls();

        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();

        private const string SPECIAL = "スペシャル(【星命力】を1D6点回復)";
        private const string FUMBLE = "ファンブル(ランダムなサインが【蝕】状態へ変化)";

        // nD6±m@s>=t  or  nD6  or  nD6>=t  etc.
        // Pattern: nD6  optionally ±m  optionally @s  optionally >=t
        private static readonly Regex ActionRegex = new Regex(
            @"^(\d+)D6([+-]\d+)?(?:@(\d+))?(>=(\?|\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "StarryDolls";

        /// <inheritdoc/>
        public override string Name => "スタリィドール";

        /// <inheritdoc/>
        public override string SortKey => "すたりいとおる";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・行為判定 nD6±m@s>=t
        　nD6の行為判定を行う。スペシャル／ファンブル／成功／失敗を判定。
        　　n: ダイス数
        　　m: 修正（省略可）
        　　s: スペシャル値（省略時 12）
        　　t: 目標値（?指定可）
        ・各種表
        　・ランダム特技決定表 RTTn (n：分野番号、省略可能)
        　　　1願望 2元素 3星使い 4動作 5召喚 6人間性
        　・ランダム分野表 RCT
        　・ランダム星座表 HOR
        　　　ランダム星座表A HORA／ランダム星座表B HORB
        　・主人関係表 MRT／関係属性表 RAT
        　・従者関係表 SRT
        　・奇跡表 MIR
        　・戦果表 BRT
        　・事件表 TRO
        　・森事件表 TROF
        　・庭園事件表 TROG
        　・城内事件表 TROC
        　・都市事件表 TROT
        　・図書館事件表 TROL
        　・駅事件表 TROS
        　・従者トラブル表 TRS
        　・リアクション表　忠誠 RAL
        　・リアクション表　冷静 RAC
        　・リアクション表　母性 RAM
        　・リアクション表　年長者 RAO
        　・リアクション表　無邪気 RAI
        　・リアクション表　長老 RAE
        　・遭遇表 ENC
        　・致命傷表 FWT
        　・カタストロフ表 CAT
        　・回想表
        　　　〈魔術師の庭〉回想表 JDSRT／〈セブンス・ヘブン〉回想表 SHRT
        　　　／〈祝福の鐘〉回想表 BCRT／〈オメガ探偵社〉回想表 ODRT
        　・出張表
        　　　〈魔術師の庭〉出張表 JDSBT／〈セブンス・ヘブン〉出張表 SHBT
        　　　／〈祝福の鐘〉出張表 BCBT／〈オメガ探偵社〉出張表 ODBT
        　　　／〈天の川商店街〉出張表 ASBT／〈ポラリス星学院〉出張表 PABT
        　　　／〈人形騎士団〉出張表 SCBT／〈人形騎士団〉への出張表 TOSCBT
        ・D66ダイスあり
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return ActionRoll(command, randomizer) ?? RollTables(command, TABLES);
        }

        private Result? ActionRoll(string command, IRandomizer randomizer)
        {
            var m = ActionRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            int times = int.Parse(m.Groups[1].Value);
            if (times < 1)
            {
                return null;
            }

            int modify = m.Groups[2].Value == "" ? 0 : int.Parse(m.Groups[2].Value);
            int special = m.Groups[3].Value == "" ? 12 : int.Parse(m.Groups[3].Value);
            string targetStr = m.Groups[5].Value;
            bool questionTarget = targetStr == "?";
            int? targetNumber = (targetStr == "" || questionTarget) ? (int?)null : int.Parse(targetStr);
            bool hasTarget = m.Groups[4].Success;

            int fumble = 2;

            // スペシャル値がファンブル値以下の場合は調整
            if (special <= fumble)
            {
                special = fumble + 1;
            }

            var diceList = randomizer.RollBarabara(times, 6);
            int diceTotal = diceList.Sum();
            int total = diceTotal + modify;

            Result result;
            if (diceTotal <= fumble)
            {
                result = Result.Fumble(FUMBLE);
            }
            else if (diceTotal >= special)
            {
                result = Result.Critical(SPECIAL);
            }
            else if (questionTarget || !hasTarget)
            {
                result = Result.CreateBuilder("").Build();
            }
            else
            {
                result = total >= targetNumber!.Value
                    ? Result.Success("成功")
                    : Result.Failure("失敗");
            }

            string modifyStr = modify == 0 ? "" : (modify > 0 ? $"+{modify}" : $"{modify}");
            string specialStr = m.Groups[3].Value == "" ? "" : $"@{special}";
            string targetPart = !hasTarget ? "" : (questionTarget ? ">=?" : $">={targetNumber}");
            string cmdStr = $"{times}D6{modifyStr}{specialStr}{targetPart}";

            var sequence = new List<string>
            {
                $"({cmdStr})",
                $"{diceTotal}[{string.Join(",", diceList)}]{modifyStr}",
                total.ToString(),
            };
            if (result.Text != "") sequence.Add(result.Text);

            string text = string.Join(" ＞ ", sequence);
            return Result.CreateBuilder(text)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .SetRands(randomizer.RandResults)
                .Build();
        }
    }
}
