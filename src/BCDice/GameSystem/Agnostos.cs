using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// アグノストス
    /// </summary>
    public sealed class Agnostos : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Agnostos Instance = new Agnostos();

        /// <inheritdoc/>
        public override string Id => "Agnostos";

        /// <inheritdoc/>
        public override string Name => "アグノストス";

        /// <inheritdoc/>
        public override string SortKey => "あくのすとす";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ 行為判定
          CDx>=t
          x: コンディションレベル（A~E もしくは 5~1)
          t: 目標値
          の成否とコンディションの変動量を判定します。

        ■ 心拍のコンディションチェック
          HCDx
          x: コンディションレベル（C+, B+, A, B-, C-, もしくは 5~1）
          酸素の消費量、コンディションの変動量、気絶したかを判定します。

        ■ 必殺技
          xSPy
          x: メインコンディション（A~E もしくは 5~1)
          y: サブコンディション（A~E もしくは 5~1)
          必殺技のダメージ量を判定します。
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return ConditionRoll(command, randomizer) ?? HeartConditionRoll(command, randomizer) ?? SpecialRoll(command, randomizer);
        }

        private Result? ConditionRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^CD([A-E1-5])>=(\d+)$");
            if (!m.Success)
            {
                return null;
            }

            var conditionLevel = ToConditionLevel(m.Groups[1].Value);
            var sides = ToSides(conditionLevel);
            var targetNumber = int.Parse(m.Groups[2].Value);
            var value = randomizer.RollOnce(sides);

            var isCritical = IsCritical(sides, value);
            var isFumble = IsFumble(sides, value);
            var isSuccess = value >= targetNumber;

            var text = string.Join(" ＞ ", new[]
            {
                $"(CD{conditionLevel}>={targetNumber})",
                $"(1D{sides}>={targetNumber})",
                value.ToString(),
                isSuccess ? "成功" : "失敗",
                ConditionChange(sides, value),
            });

            return Result.CreateBuilder(text)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .SetCondition(isSuccess)
                .Build();
        }

        private string ToConditionLevel(string ch)
        {
            return ch switch
            {
                "5" => "A",
                "4" => "B",
                "3" => "C",
                "2" => "D",
                "1" => "E",
                _ => ch,
            };
        }

        private int ToSides(string condition)
        {
            return condition switch
            {
                "A" => 12,
                "B" => 10,
                "C" => 8,
                "D" => 6,
                _ => 4, // "E"
            };
        }

        private string ConditionChange(int sides, int value)
        {
            if (IsCritical(sides, value))
            {
                return "コンディション：2段階上昇（クリティカル）";
            }
            else if (IsFumble(sides, value))
            {
                return "コンディション：2段階下降（ファンブル）";
            }
            else if (sides != 12 && sides - value <= 1)
            {
                return "コンディション：1段階上昇";
            }
            else if (sides == 12 && value <= 6)
            {
                return "コンディション：1段階下降";
            }
            else if (sides == 10 && value <= 3)
            {
                return "コンディション：1段階下降";
            }
            else if (sides == 8 && value <= 2)
            {
                return "コンディション：1段階下降";
            }
            else
            {
                return "コンディション：変動なし";
            }
        }

        private bool IsCritical(int sides, int value)
        {
            return sides != 12 && sides == value;
        }

        private bool IsFumble(int sides, int value)
        {
            return sides != 4 && value == 1;
        }

        private Result? HeartConditionRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^HCD([A1-5]|[BC][+-])$");
            if (!m.Success)
            {
                return null;
            }

            var suffix = m.Groups[1].Value;
            var conditionLevel = ToHeartConditionLevel(suffix);
            var sides = ToHeartSides(conditionLevel);
            var value = randomizer.RollOnce(sides);

            var isCritical = IsCritical(sides, value);
            var isFumble = IsFumble(sides, value);

            var text = string.Join(" ＞ ", new[]
            {
                $"(HCD{conditionLevel})",
                $"(1D{sides})",
                value.ToString(),
                HeartConditionChange(sides, value),
            });

            return Result.CreateBuilder(text)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .Build();
        }

        private string ToHeartConditionLevel(string ch)
        {
            return ch switch
            {
                "5" => "C+",
                "4" => "B+",
                "3" => "A",
                "2" => "B-",
                "1" => "C-",
                _ => ch,
            };
        }

        private int ToHeartSides(string condition)
        {
            return condition switch
            {
                "C+" => 12,
                "B+" => 10,
                "A" => 8,
                "B-" => 6,
                _ => 4, // "C-"
            };
        }

        private bool IsFainted(int sides, int value)
        {
            return sides switch
            {
                12 => value >= 7,
                10 => value >= 10,
                8 => false,
                6 => value <= 1,
                _ => value <= 2, // 4
            };
        }

        private string HeartConditionChange(int sides, int value)
        {
            if (IsFainted(sides, value))
            {
                return "気絶";
            }
            else
            {
                return ConditionChange(sides, value);
            }
        }

        private Result? SpecialRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^([A-E1-5])SP([A-E1-5])$");
            if (!m.Success)
            {
                return null;
            }

            var timesConditionLevel = ToConditionLevel(m.Groups[1].Value);
            var sidesConditionLevel = ToConditionLevel(m.Groups[2].Value);

            var times = ToTimes(timesConditionLevel);
            var sides = ToSides(sidesConditionLevel);

            var diceList = randomizer.RollBarabara(times, sides);
            var value = diceList.Sum();

            var text = string.Join(" ＞ ", new[]
            {
                $"({timesConditionLevel}SP{sidesConditionLevel})",
                $"({times}D{sides})",
                $"{value}[{string.Join(",", diceList)}]",
                value.ToString(),
            });

            return Result.CreateBuilder(text).Build();
        }

        private int ToTimes(string condition)
        {
            return condition switch
            {
                "A" => 5,
                "B" => 4,
                "C" => 3,
                "D" => 2,
                _ => 1, // "E"
            };
        }

    }
}
