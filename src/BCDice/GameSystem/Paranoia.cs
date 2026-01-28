using System;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// パラノイア
    /// </summary>
    public sealed class Paranoia : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Paranoia Instance = new Paranoia();

        /// <inheritdoc/>
        public override string Id => "Paranoia";

        /// <inheritdoc/>
        public override string Name => "パラノイア";

        /// <inheritdoc/>
        public override string SortKey => "はらのいあ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
※「パラノイア」は完璧なゲームであるため特殊なダイスコマンドを必要としません。
※このダイスボットは部屋のシステム名表示用となります。
";

        private static readonly Regex GetaCommand = new Regex(
            @"^geta$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private Paranoia() { }

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
            int dice = randomizer.RollOnce(2);

            string getaString = dice switch
            {
                1 => "幸福です",
                2 => "幸福ではありません",
                _ => "不明"
            };

            string text = $"{command} ＞ 幸福ですか？ ＞ {getaString}";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }
    }
}
