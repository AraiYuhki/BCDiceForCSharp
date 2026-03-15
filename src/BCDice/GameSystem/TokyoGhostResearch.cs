using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 東京ゴーストリサーチ
    /// </summary>
    public sealed class TokyoGhostResearch : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly TokyoGhostResearch Instance = new TokyoGhostResearch();


        private static readonly Regex TkRegex = new Regex(
            @"TK\?<=(\d+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "TokyoGhostResearch";

        /// <inheritdoc/>
        public override string Name => "東京ゴーストリサーチ";

        /// <inheritdoc/>
        public override string SortKey => "とうきようこおすとりさあち";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        判定
        ・タスク処理は目標値以上の値で成功となります。
          1d10>={目標値}
          例：目標値「5」の場合、5～0で成功
        各種表
          ・導入表  OP
          ・一般トラブル表  TB
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var match = TkRegex.Match(command);
            if (match.Success)
            {
                return GetCheckResult(command, match, randomizer);
            }

            if (command == "OP")
            {
                return TgrOpeningTable(randomizer);
            }

            if (command == "TB")
            {
                return TgrCommonTroubleTable(randomizer);
            }


            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result? GetCheckResult(string command, Match match, IRandomizer randomizer)
        {
            var diff = Convert.ToInt32(match.Groups[1].Value);

            if (diff <= 0)
            {
                return null;
            }

            var total_n = randomizer.RollOnce(10);
            var resultText = GetCheckResultText(total_n, diff);
            var text = $"(1D10<={diff}) ＞ {total_n} ＞ {resultText}";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private string GetCheckResultText(int total_n, int diff)
        {
            if (total_n >= diff)
            {
                return "成功";
            }
            else
            {
                return "失敗";
            }
        }

        private Result? TgrOpeningTable(IRandomizer randomizer)
        {
            var name = "導入表";
            var table = new[]
            {
                "【病休中断】体調不良または怪我で療養中だったが強制召喚された。",
                "【忙殺中】別の業務で忙殺中であった。",
                "【出張帰り】遠方での仕事から戻ったばかり。",
                "【休暇取り消し】休暇中だったが呼び戻された。",
                "【平常運転】いつもどおりの仕事中だった。",
                "【休暇明け】十分に休養をとったあとで、心身ともに充実している。",
                "【人生の岐路】人生の岐路にまさに差し掛かったところであった。",
                "【同窓会】かつての同級生に会い、差を実感したばかりだった。",
                "【転職活動中】転職を考えて求人サイトを見ているところだった。",
                "【サボリ中】仕事をサボっているところに呼び出しがあった。",
            };
            return Get1d10TableResult(name, table, randomizer);
        }

        private Result? TgrCommonTroubleTable(IRandomizer randomizer)
        {
            var name = "一般トラブル表";
            var table = new[]
            {
                "トラブルが生じたが、間一髪、危機を脱した。【ダメージなし】",
                "どうにかタスクを処理したが、非常に疲労してしまった。【肉体ダメージ1点】",
                "タスク処理の過程で負傷してしまった。【肉体ダメージ1点】",
                "恐怖や混乱、ストレスなどで精神の均衡を崩してしまった。【精神ダメージ1点】",
                "過去のトラウマなどを思い出し、気分が沈んでしまった。【精神ダメージ1点】",
                "自身の信用をキズつけたり、汚名を背負ってしまった。【環境ダメージ1点】",
                "会社や上司の不興を買ってしまった。【環境ダメージ1点】",
                "疲労困憊で動くこともままならない。【肉体ダメージ1点＋精神ダメージ1点】",
                "負傷したうえ、会社に損害を与えてしまった。【肉体ダメージ1点＋環境ダメージ1点】",
                "上司から厳しく叱責され、まずい立場になった。【精神ダメージ1点＋環境ダメージ1点】",
            };
            return Get1d10TableResult(name, table, randomizer);
        }

        private Result? Get1d10TableResult(string name, string[] table, IRandomizer randomizer)
        {
            var dice = randomizer.RollOnce(10);
            var output = table[dice - 1];
            var text = $"{name}({dice}) ＞ {output}";
            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

    }
}
