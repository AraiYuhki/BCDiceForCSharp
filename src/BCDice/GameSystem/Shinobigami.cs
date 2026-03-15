using System;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// シノビガミ
    /// </summary>
    public sealed class Shinobigami : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Shinobigami Instance = new Shinobigami();

        private static readonly Regex EmotionTableRegex = new Regex(
            @"^S?ET$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex FumbleTableRegex = new Regex(
            @"^S?FT$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "Shinobigami";

        /// <inheritdoc/>
        public override string Name => "シノビガミ";

        /// <inheritdoc/>
        public override string SortKey => "しのひかみ";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
【シノビガミ】

・判定コマンド
　2D6>=5   目標値5で判定
　         1ゾロ：ファンブル（自動失敗）
　         6ゾロ：スペシャル（自動成功）

・感情表（ET）
・ファンブル表（FT）
";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // 感情表
            if (EmotionTableRegex.IsMatch(command))
            {
                return RollEmotionTable(command, randomizer);
            }

            // ファンブル表
            if (FumbleTableRegex.IsMatch(command))
            {
                return RollFumbleTable(command, randomizer);
            }

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        /// <inheritdoc/>
        protected override Result? EvalCommonCommand(string command, IRandomizer randomizer)
        {
            var result = base.EvalCommonCommand(command, randomizer);

            if (result != null && result.Rands != null && result.Rands.Count >= 2)
            {
                if (result.Rands[0].Sides == 6 && result.Rands[1].Sides == 6)
                {
                    int die1 = result.Rands[0].Value;
                    int die2 = result.Rands[1].Value;

                    if (die1 == 1 && die2 == 1)
                    {
                        return Result.CreateBuilder(result.Text + "【ファンブル】")
                            .SetSecret(result.IsSecret)
                            .SetFumble(true)
                            .SetFailure(true)
                            .SetRands(result.Rands)
                            .SetDetailedRands(result.DetailedRands)
                            .Build();
                    }
                    else if (die1 == 6 && die2 == 6)
                    {
                        return Result.CreateBuilder(result.Text + "【スペシャル】")
                            .SetSecret(result.IsSecret)
                            .SetCritical(true)
                            .SetSuccess(true)
                            .SetRands(result.Rands)
                            .SetDetailedRands(result.DetailedRands)
                            .Build();
                    }
                }
            }

            return result;
        }

        private Result RollEmotionTable(string command, IRandomizer randomizer)
        {
            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase);

            int roll = randomizer.RollD66(D66SortType.Ascending);
            string emotion = GetEmotion(roll);

            string text = $"感情表({roll}) ＞ {emotion}";

            return Result.CreateBuilder(text)
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result RollFumbleTable(string command, IRandomizer randomizer)
        {
            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase);

            int roll = randomizer.RollSum(2, 6);
            string effect = GetFumbleEffect(roll);

            string text = $"ファンブル表({roll}) ＞ {effect}";

            return Result.CreateBuilder(text)
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static string GetEmotion(int roll)
        {
            return roll switch
            {
                11 => "共感（プラス）/ 不信（マイナス）",
                12 => "友情（プラス）/ 怒り（マイナス）",
                13 => "愛情（プラス）/ 妬み（マイナス）",
                14 => "忠誠（プラス）/ 侮蔑（マイナス）",
                15 => "憧憬（プラス）/ 劣等感（マイナス）",
                16 => "狂信（プラス）/ 殺意（マイナス）",
                22 => "友情（プラス）/ 怒り（マイナス）",
                23 => "愛情（プラス）/ 妬み（マイナス）",
                24 => "忠誠（プラス）/ 侮蔑（マイナス）",
                25 => "憧憬（プラス）/ 劣等感（マイナス）",
                26 => "狂信（プラス）/ 殺意（マイナス）",
                33 => "愛情（プラス）/ 妬み（マイナス）",
                34 => "忠誠（プラス）/ 侮蔑（マイナス）",
                35 => "憧憬（プラス）/ 劣等感（マイナス）",
                36 => "狂信（プラス）/ 殺意（マイナス）",
                44 => "忠誠（プラス）/ 侮蔑（マイナス）",
                45 => "憧憬（プラス）/ 劣等感（マイナス）",
                46 => "狂信（プラス）/ 殺意（マイナス）",
                55 => "憧憬（プラス）/ 劣等感（マイナス）",
                56 => "狂信（プラス）/ 殺意（マイナス）",
                66 => "狂信（プラス）/ 殺意（マイナス）",
                _ => "共感（プラス）/ 不信（マイナス）"
            };
        }

        private static string GetFumbleEffect(int roll)
        {
            return roll switch
            {
                2 => "自分のプロット全てが敵に見られる",
                3 => "忍具を1つ失う",
                4 => "相手の攻撃が命中した場合、追加で1点ダメージ",
                5 => "次のラウンド、奥義を使えない",
                6 => "何も起こらない",
                7 => "何も起こらない",
                8 => "何も起こらない",
                9 => "次のラウンド、奥義を使えない",
                10 => "相手の攻撃が命中した場合、追加で1点ダメージ",
                11 => "忍具を1つ失う",
                12 => "自分のプロット全てが敵に見られる",
                _ => "何も起こらない"
            };
        }
    }
}
