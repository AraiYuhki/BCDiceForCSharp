using System;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// エモクロアTRPG
    /// </summary>
    public sealed class Emoklore : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Emoklore Instance = new Emoklore();

        private static readonly Regex EmotionTableRegex = new Regex(
            @"^S?EMT$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ResonanceTableRegex = new Regex(
            @"^S?RT$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "Emoklore";

        /// <inheritdoc/>
        public override string Name => "エモクロアTRPG";

        /// <inheritdoc/>
        public override string SortKey => "えもくろあ";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
【エモクロアTRPG】

・判定コマンド
　2D6>=8    目標値8で判定
　2D6+2>=8  修正値+2で判定
　          1ゾロ：ファンブル
　          6ゾロ：スペシャル

・感情表（EMT）
・共鳴表（RT）
";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // 感情表
            if (EmotionTableRegex.IsMatch(command))
            {
                return RollEmotionTable(command, randomizer);
            }

            // 共鳴表
            if (ResonanceTableRegex.IsMatch(command))
            {
                return RollResonanceTable(command, randomizer);
            }

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        /// <inheritdoc/>
        protected override Result? EvalCommonCommand(string command, IRandomizer randomizer)
        {
            var result = base.EvalCommonCommand(command, randomizer);

            // 2D6判定でスペシャル/ファンブル判定を追加
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

        private Result RollResonanceTable(string command, IRandomizer randomizer)
        {
            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase);

            int roll = randomizer.RollSum(2, 6);
            string resonance = GetResonance(roll);

            string text = $"共鳴表({roll}) ＞ {resonance}";

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
                11 => "喜び / 悲しみ",
                12 => "期待 / 不安",
                13 => "信頼 / 嫌悪",
                14 => "驚き / 予感",
                15 => "恐怖 / 勇気",
                16 => "怒り / 穏やか",
                22 => "愛情 / 憎悪",
                23 => "楽観 / 悲観",
                24 => "服従 / 支配",
                25 => "畏怖 / 軽蔑",
                26 => "後悔 / 満足",
                33 => "興奮 / 冷静",
                34 => "羨望 / 優越",
                35 => "罪悪感 / 正義感",
                36 => "孤独 / 連帯",
                44 => "好奇心 / 無関心",
                45 => "感謝 / 恨み",
                46 => "希望 / 絶望",
                55 => "誇り / 恥",
                56 => "郷愁 / 前進",
                66 => "自由 / 束縛",
                _ => "喜び / 悲しみ"
            };
        }

        private static string GetResonance(int roll)
        {
            return roll switch
            {
                2 => "【暴走】感情が制御不能になる",
                3 => "【動揺】次の判定に-2",
                4 => "【揺らぎ】次の判定に-1",
                5 => "【微弱】特に影響なし",
                6 => "【安定】感情が安定する",
                7 => "【調和】感情が調和する",
                8 => "【安定】感情が安定する",
                9 => "【微弱】特に影響なし",
                10 => "【高揚】次の判定に+1",
                11 => "【覚醒】次の判定に+2",
                12 => "【超越】感情が極限に達する",
                _ => "【調和】感情が調和する"
            };
        }
    }
}
