using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 片道勇者TRPG
    /// </summary>
    public sealed class OneWayHeroics : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly OneWayHeroics Instance = new OneWayHeroics();


        private static readonly Regex RetDRegex = new Regex(
            @"^RET(\d+)$",
            RegexOptions.Compiled);

        private static readonly Regex RetpDRegex = new Regex(
            @"^RETP(\d+)$",
            RegexOptions.Compiled);

        private static readonly Regex DngnDRegex = new Regex(
            @"^DNGN(\d+)$",
            RegexOptions.Compiled);

        private static readonly Regex DngnpDRegex = new Regex(
            @"^DNGNP(\d+)$",
            RegexOptions.Compiled);

        private static readonly Regex DJdRegex = new Regex(
            @"^\d*JD",
            RegexOptions.Compiled);

        private static readonly Regex JdCommandRegex = new Regex(
            @"^(\d*)JD(\d*)(\+(\d*))?(,(\d+))?$",
            RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "OneWayHeroics";

        /// <inheritdoc/>
        public override string Name => "片道勇者TRPG";

        /// <inheritdoc/>
        public override string SortKey => "かたみちゆうしやTRPG";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定　aJDx+y,z
        　a:ダイス数（省略時2個)、x:能力値、
        　y:修正値（省略可。「＋」のみなら＋１）、z:目標値（省略可）
        　例１）JD2+1,8 or JD2+,8　：能力値２、修正＋１、目標値８
        　例２）JD3,10 能力値３、修正なし、目標値10
        　例３）3JD4+ ダイス3個から2個選択、能力値４、修正なし、目標値なし
        ・ファンブル表 FT／魔王追撃表   DC／進行ルート表 PR／会話テーマ表 TT
        逃走判定表   EC／ランダムNPC特徴表 RNPC／偵察表 SCT
        施設表　FCLT／施設表プラス　FCLTP／希少動物表 RANI／王特徴表プラス KNGFTP
        野外遭遇表 OUTENC／野外遭遇表プラス OUTENCP
        モンスター特徴表 MONFT／モンスター特徴表プラス MONFTP
        ドロップアイテム表 DROP／ドロップアイテム表プラス DROPP
        武器ドロップ表 DROPWP／武器ドロップ表2 DROPWP2
        防具ドロップ表 DROPAR／防具ドロップ表2 DROPAR2
        聖武具ドロップ表 DROPHW／聖武具ドロップ表プラス DROPHWP
        食品ドロップ表 DROPFD／食品ドロップ表2 DROPFD2
        巻物ドロップ表 DROPSC／巻物ドロップ表2 DROPSC2
        その他ドロップ表 DROPOT／その他 ドロップ表2 DROPOT2
        薬品ドロップ表プラス DROPDRP／珍しい箱ドロップ表2 DROPRAREBOX2
        ・ランダムイベント表 RETx（x：現在の日数）、ランダムイベント表プラス RETPx
        　例）RET3、RETP4
        ・ダンジョン表 DNGNx（x：現在の日数）、ダンジョン表プラス DNGNPx
        　例）DNGN3、DNGNP4
        ";

        private OneWayHeroics() { }

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            Match match;

            match = RetDRegex.Match(command);
            if (match.Success)
            {
                // RANDOM_EVENT_TABLE is not yet ported from Ruby
                return null;
            }

            match = RetpDRegex.Match(command);
            if (match.Success)
            {
                // RANDOM_EVENT_TABLE_PLUS is not yet ported from Ruby
                return null;
            }

            match = DngnDRegex.Match(command);
            if (match.Success)
            {
                // DUNGEON_TABLE is not yet ported from Ruby
                return null;
            }

            match = DngnpDRegex.Match(command);
            if (match.Success)
            {
                // DUNGEON_TABLE_PLUS is not yet ported from Ruby
                return null;
            }

            match = DJdRegex.Match(command);
            if (match.Success)
            {
                var resultText = GetRollDiceCommandResult(command, randomizer);
                if (resultText == null)
                {
                    return null;
                }
                return Result.CreateBuilder(resultText)
                    .SetRands(randomizer.RandResults)
                    .Build();
            }

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private string? GetRollDiceCommandResult(string command, IRandomizer randomizer)
        {
            var match = JdCommandRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            var diceCountStr = match.Groups[1].Value;
            int diceCount;
            if (string.IsNullOrEmpty(diceCountStr))
            {
                diceCount = 2;
            }
            else
            {
                diceCount = int.Parse(diceCountStr);
            }
            if (diceCount < 2)
            {
                return null;
            }

            var ability = int.Parse(string.IsNullOrEmpty(match.Groups[2].Value) ? "0" : match.Groups[2].Value);
            int? target = null;
            if (match.Groups[6].Success && !string.IsNullOrEmpty(match.Groups[6].Value))
            {
                target = int.Parse(match.Groups[6].Value);
            }

            var modifyText = match.Groups[3].Success ? match.Groups[3].Value : "";
            if (modifyText == "+")
            {
                modifyText = "+1";
            }
            var modifyValue = string.IsNullOrEmpty(modifyText) ? 0 : int.Parse(modifyText);

            var (dice, diceText) = RollJudgeDice(diceCount, randomizer);
            var total = dice + ability + modifyValue;

            var text = command;
            text += $" ＞ {diceCount}D6[{diceText}]+{ability}{modifyText}";
            text += $" ＞ {total}";

            var result = GetJudgeReusltText(dice, total, target);
            if (!string.IsNullOrEmpty(result))
            {
                text += $" ＞ {result}";
            }

            return text;
        }

        private (int dice, string diceText) RollJudgeDice(int diceCount, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(diceCount, 6).ToList();
            var dice = diceList.Sum();
            var diceText = string.Join(",", diceList);

            if (diceCount == 2)
            {
                return (dice, diceText);
            }

            diceList.Sort();
            diceList.Reverse();

            var total = diceList[0] + diceList[1];
            var text = $"{diceText}→{diceList[0]},{diceList[1]}";

            return (total, text);
        }

        private string GetJudgeReusltText(int dice, int total, int? target)
        {
            if (dice == 2)
            {
                return "ファンブル";
            }
            if (dice == 12)
            {
                return "スペシャル";
            }

            if (target == null)
            {
                return "";
            }

            if (total >= target.Value)
            {
                return "成功";
            }

            return "失敗";
        }
    }
}
