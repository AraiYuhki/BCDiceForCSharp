using System;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ゆうやけこやけ
    /// </summary>
    public sealed class GoldenSkyStories : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly GoldenSkyStories Instance = new GoldenSkyStories();

        /// <inheritdoc/>
        public override string Id => "GoldenSkyStories";

        /// <inheritdoc/>
        public override string Name => "ゆうやけこやけ";

        /// <inheritdoc/>
        public override string SortKey => "ゆうやけこやけ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
※「ゆうやけこやけ」はダイスロールを使用しないシステムです。
※このダイスボットは部屋のシステム名表示用となります。

・下駄占い (GETA)
  あーしたてんきになーれ
";

        private static readonly Regex GetaCommand = new Regex(
            @"^GETA$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private GoldenSkyStories() { }

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            if (GetaCommand.IsMatch(command))
            {
                return RollGeta(command, randomizer);
            }

            return null;
        }

        private Result RollGeta(string command, IRandomizer randomizer)
        {
            int dice = randomizer.RollOnce(7);

            string getaString = dice switch
            {
                1 => "裏：あめ",
                2 => "表：はれ",
                3 => "裏：あめ",
                4 => "表：はれ",
                5 => "裏：あめ",
                6 => "表：はれ",
                7 => "横：くもり",
                _ => "不明"
            };

            string text = $"{command} ＞ 下駄占い ＞ {getaString}";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }
    }
}
