using System;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// インセイン
    /// </summary>
    public sealed class Insane : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Insane Instance = new Insane();

        private static readonly Regex JudgeRegex = new Regex(
            @"^S?(?:ST|FT|ET)?(\d+)?(?:\+(\d+))?(?:\-(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SpecialTableRegex = new Regex(
            @"^S?ST$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex FumbleTableRegex = new Regex(
            @"^S?FT$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "Insane";

        /// <inheritdoc/>
        public override string Name => "インセイン";

        /// <inheritdoc/>
        public override string SortKey => "いんせいん";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
【インセイン】

・判定コマンド
　2D6>=5   目標値5で判定
　         1ゾロ：ファンブル
　         6ゾロ：スペシャル

・スペシャル表（ST）
・ファンブル表（FT）
";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // スペシャル表
            if (SpecialTableRegex.IsMatch(command))
            {
                return RollSpecialTable(command, randomizer);
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
            // 2D6判定を拡張してスペシャル/ファンブル判定を追加
            var result = base.EvalCommonCommand(command, randomizer);

            if (result != null && result.Rands != null && result.Rands.Count >= 2)
            {
                // 最初の2つのダイスがD6かチェック
                if (result.Rands[0].Sides == 6 && result.Rands[1].Sides == 6)
                {
                    int die1 = result.Rands[0].Value;
                    int die2 = result.Rands[1].Value;

                    // ゾロ目判定
                    if (die1 == 1 && die2 == 1)
                    {
                        // ファンブル
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
                        // スペシャル
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

        private Result RollSpecialTable(string command, IRandomizer randomizer)
        {
            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase);

            int roll = randomizer.RollSum(2, 6);
            string effect = GetSpecialEffect(roll);

            string text = $"スペシャル表({roll}) ＞ {effect}";

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

        private static string GetSpecialEffect(int roll)
        {
            return roll switch
            {
                2 => "【絶好調】このシーン中、判定にボーナス+2",
                3 => "【閃き】好きな情報を1つ得る",
                4 => "【回復】生命力を1点回復",
                5 => "【幸運】次の判定に+1ボーナス",
                6 => "【覚醒】このシーン中、任意の能力値+1",
                7 => "【逆転】失敗を成功にできる（1回）",
                8 => "【覚醒】このシーン中、任意の能力値+1",
                9 => "【幸運】次の判定に+1ボーナス",
                10 => "【回復】生命力を1点回復",
                11 => "【閃き】好きな情報を1つ得る",
                12 => "【絶好調】このシーン中、判定にボーナス+2",
                _ => "効果なし"
            };
        }

        private static string GetFumbleEffect(int roll)
        {
            return roll switch
            {
                2 => "【大惨事】生命力2点ダメージ、狂気カード1枚獲得",
                3 => "【恐怖】狂気カードを1枚獲得",
                4 => "【負傷】生命力1点ダメージ",
                5 => "【動揺】次の判定に-1ペナルティ",
                6 => "【混乱】このシーン中、任意の能力値-1",
                7 => "【空振り】特に何も起こらない",
                8 => "【混乱】このシーン中、任意の能力値-1",
                9 => "【動揺】次の判定に-1ペナルティ",
                10 => "【負傷】生命力1点ダメージ",
                11 => "【恐怖】狂気カードを1枚獲得",
                12 => "【大惨事】生命力2点ダメージ、狂気カード1枚獲得",
                _ => "効果なし"
            };
        }
    }
}
