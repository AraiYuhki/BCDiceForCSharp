using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// スチームパンカーズ
    /// </summary>
    public sealed class SteamPunkers : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly SteamPunkers Instance = new SteamPunkers();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "SteamPunkers";

        /// <inheritdoc/>
        public override string Name => "スチームパンカーズ";

        /// <inheritdoc/>
        public override string SortKey => "すちいむはんかあす";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定コマンド (SPn)
          SP(判定ダイス数)>=(目標値)
          SP4>=3のように入力し、5が出たらヒット数1，6が出たらヒット数2として成功数を数えます。
          ≪スチームパンク！≫による振り直しのため、出力には失敗ダイス数を表示します。
          目標値は省略可能です。
          例：(SP4>=3) ＞ [3,4,1,6] ＞ 成功数:2, 失敗数:3 ＞ 失敗
          例：(SP4) ＞ [3,4,1,6] ＞ 成功数:2, 失敗数:3
        ・表
          ・プロフ(Profile)：年齢表 PAT, 性別表 PST, 国籍表 PCT
          ・名前表(Name)：イギリス NIT, アメリカ NAT, フランス NFT, ドイツ NGT, ソビエト NST, 日本表 NJT
          ・過去表(Past)：トゥルース PTT, ガーディアン PGT, ノーブル PNT, デヴォート PDT, エセティック PET, チャレンジ PACT
          ・経緯表(Background)：ギガントアームズ BGT, アーマードリム BAT, エッジモービル BET, クロノウェポン BCT, スパイテック BST, スチームウェア BWT
          ・特徴表(Feature)：特徴表S FST, 特徴表P FPT
          ・関係性表(Relationship)：関係性表S RST, 関係性表P RPT
          ・感情表(Emotion)：感情表S EST, 感情表P EPT
          ・その他(Other)；災厄表 ODT, 場面表 OST, 交流表 OIT, 激憤表 OFT
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var result = RollTable(command, TABLES, randomizer);
            if (result != null)
            {
                return result;
            }
            return RollSp(command, randomizer);
        }

        private Result? RollSp(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^SP(\d+)(?:>=(\d+))?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var diceCount = Convert.ToInt32(m.Groups[1].Value);
            int? targetNumber = m.Groups[2].Success ? (int?)int.Parse(m.Groups[2].Value) : null;
            var diceList = randomizer.RollBarabara(diceCount, 6);
            var diceListText = string.Join(",", diceList);
            var successes = diceList.Count(v => v == 6) * 2 + diceList.Count(v => v == 5);
            var failures = diceList.Count(x => x <= 4);

            Result result;
            if (diceList.All(x => x == 1))
            {
                result = Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
            }
            else if (targetNumber != null)
            {
                result = successes >= targetNumber.Value
                    ? Result.CreateBuilder("成功").SetSuccess(true).Build()
                    : Result.CreateBuilder("失敗").SetFailure(true).Build();
            }
            else
            {
                result = Result.CreateBuilder("").Build();
            }

            var resultText = result.Text;
            var parts = new List<string> { $"({command})", $"[{diceListText}]", $"成功数:{successes}, 失敗数:{failures}" };
            if (!string.IsNullOrEmpty(resultText))
            {
                parts.Add(resultText);
            }
            var fullText = string.Join(" ＞ ", parts);

            bool hasSix = diceList.Contains(6);
            result = Result.CreateBuilder(fullText)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetFumble(result.IsFumble)
                .SetCritical(hasSix)
                .SetRands(randomizer.RandResults)
                .Build();

            return result;
        }

    }
}
