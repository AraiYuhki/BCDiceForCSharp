using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// エンドブレイカー！
    /// </summary>
    public sealed class EndBreaker : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly EndBreaker Instance = new EndBreaker();


        /// <inheritdoc/>
        public override string Id => "EndBreaker";

        /// <inheritdoc/>
        public override string Name => "エンドブレイカー！";

        /// <inheritdoc/>
        public override string SortKey => "えんとふれいかあ";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.NoSort;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定 (nEB)
          n個のD6を振る判定。ダブルトリガー発動で自動振り足し。
        ・各種表
          ・生死不明表 (LDUT)
        ";

        private static readonly Regex EbRegex = new Regex(
            @"^(\d+)EB$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var match = EbRegex.Match(command);
            if (match.Success)
            {
                int diceCount = int.Parse(match.Groups[1].Value);
                string rollResult = CheckRoll(diceCount, randomizer);
                return Result.CreateBuilder(rollResult).Build();
            }

            if (command == "LDUT")
            {
                string tableName = "生死不明表";
                var (text, indexText) = GetLifeAndDeathUnknownResult();
                string result = $"{tableName}({indexText}):{text}";
                return Result.CreateBuilder(result).Build();
            }

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private string CheckRoll(int diceCount, IRandomizer randomizer)
        {
            Debug("EndBreaker diceCount", diceCount);
            int rollCount = diceCount;
            string result = "";
            var diceFullList = new List<int>();
            while (rollCount != 0)
            {
                int[] diceArray = randomizer.RollBarabara(rollCount, 6);
                var diceList = diceArray.OrderBy(x => x).ToList();
                diceFullList.AddRange(diceList);
                rollCount = diceList.Count(i => i == 1) * 2;
                result += $"[{string.Join("", diceList)}]";
                if (rollCount > 0)
                {
                    result += " ダブルトリガー! ";
                }
            }
            result += " ＞";
            foreach (var num in Enumerable.Range(2, 5))
            {
                int count = diceFullList.Count(i => i == num);
                if (count != 0)
                {
                    result += $" [{num}:{count}個]";
                }
            }
            return result;
        }

        private (string text, string indexText) GetLifeAndDeathUnknownResult()
        {
            var table = new[] { " 1日：生還！", " 1日：生還！", " 1日：生還！", " 1日：生還！", " 1日：生還！", " 1日：生還！", " 1日：生還！", " 5日：敵に捕らわれ、ひどい暴行と拷問を受けた。", " 2日：謎の人物に命を救われた。", "10日：奴隷として売り飛ばされた。", " 8日：おぞましい儀式の生贄として連れ去られた。", " 9日：幽閉・投獄された。", " 1日：生還！", " 7日：モンスター蠢く地下迷宮に滑落した。", "12日強力なマスカレイドにとらわれ、実験台にされた。", " 8日：放浪中に遭遇した事件を、颯爽と解決していた。", " 5日：飢餓状態に追い込まれた。", "15日：記憶を失い放浪した。", " 1日：生還！", "10日：異性に命を救われて、手厚い看病を受けた。", " 3日：負傷からくる熱病で、生死の境を彷徨った。", "11日：闘奴にされたが、戦いと友情の末に自由を獲得した。", " 6日：負傷したまま川に落ち、遥か下流まで流された。", " 9日：敵に連れ去られ、執拗な拷問を受け続けた。", " 1日：生還！", " 4日：繰り返す「死の悪夢」に苛まれた。", " 3日：巨獣の巣に連れ去られた。", "10日：謎の集団に救われて、手厚い看病を受けた。", " 3日：チッタニアンの集落に迷い込み、もてなしを受けた。", " 7日：ピュアリィの群れにとらわれ、弄ばれた。", " 1日：生還！", " 6日：楽園のような場所を発見し、しばらく逗留した。", " 9日：盗賊団に救われ、恩返しとして少し用心棒をした。", "10日：熱病の見せる官能的な幻影にとらわれ、彷徨った。", " 5日：謎の賞金首に狙われ、傷めつけられていた。", " - ：「五分五分」の一般判定。失敗すると死亡。" };
            return GetTableByD66(table);
        }

    }
}
