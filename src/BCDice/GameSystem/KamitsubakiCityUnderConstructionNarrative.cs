using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 神椿市建設中。NARRATIVE
    /// </summary>
    public sealed class KamitsubakiCityUnderConstructionNarrative : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly KamitsubakiCityUnderConstructionNarrative Instance = new KamitsubakiCityUnderConstructionNarrative();

        /// <inheritdoc/>
        public override string Id => "KamitsubakiCityUnderConstructionNarrative";

        /// <inheritdoc/>
        public override string Name => "神椿市建設中。NARRATIVE";

        /// <inheritdoc/>
        public override string SortKey => "かみつはきしけんせつちゆうならていふ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・可組（KA）
        　KA6 行動判定
        　KA8 技能ロール
        　KA10 特技ロール
        　KA12 Aロール

        ・裏組（RI）
        　RI6 行動判定
        　RI8 技能ロール
        　RI10 特技ロール
        　RI12 Aロール

        ・羽組（HA）
        　HA6 行動判定
        　HA8 技能ロール
        　HA10 特技ロール
        　HA12 Aロール

        ・星組（SE）
        　SE6 行動判定
        　SE8 技能ロール
        　SE10 特技ロール
        　SE12 Aロール

        ・狐組（CO）
        　CO6 行動判定
        　CO8 技能ロール
        　CO10 特技ロール
        　CO12 Aロール

        ・GM用
        　GM6 （成否判定なし）
        　GM8 技能ロール
        　GM10 特技ロール
        　Q12 Qロール

        ・存在証明 EXI<=x
        　存在証明の判定を行う
        　x: 存在値
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollKumi(command, randomizer) ?? RollExistence(command, randomizer);
        }

        private static readonly Dictionary<string, object> TABLES = new Dictionary<string, object>
        {
            { "KA6", new KumiD6("可") },
            { "KA8", new KumiDice(new[] { "Q", "", "", "", "可", "可", "可", "M" }) },
            { "KA10", new KumiDice(new[] { "Q", "", "", "可", "可", "可", "可", "可", "M", "M" }) },
            { "KA12", new KumiDice(new[] { "Q", "", "", "可", "可", "可", "可", "可", "可", "可", "M", "M" }) },

            { "RI6", new KumiD6("裏") },
            { "RI8", new KumiDice(new[] { "Q", "", "", "", "裏", "裏", "裏", "M" }) },
            { "RI10", new KumiDice(new[] { "Q", "", "", "裏", "裏", "裏", "裏", "裏", "M", "M" }) },
            { "RI12", new KumiDice(new[] { "M", "M", "裏", "裏", "裏", "裏", "裏", "裏", "裏", "", "", "Q" }) },

            { "HA6", new KumiD6("羽") },
            { "HA8", new KumiDice(new[] { "Q", "", "", "", "羽", "羽", "羽", "M" }) },
            { "HA10", new KumiDice(new[] { "Q", "", "", "羽", "羽", "羽", "羽", "羽", "M", "M" }) },
            { "HA12", new KumiDice(new[] { "Q", "Q", "羽", "羽", "羽", "", "", "", "M", "M", "M", "M" }) },

            { "SE6", new KumiD6("星") },
            { "SE8", new KumiDice(new[] { "Q", "", "", "", "星", "星", "星", "M" }) },
            { "SE10", new KumiDice(new[] { "Q", "", "", "星", "星", "星", "星", "星", "M", "M" }) },
            { "SE12", new KumiDice(new[] { "星", "", "星", "星", "M", "Q", "M", "星", "星", "星", "", "星" }) },

            { "CO6", new KumiD6("狐") },
            { "CO8", new KumiDice(new[] { "Q", "", "", "", "狐", "狐", "狐", "M" }) },
            { "CO10", new KumiDice(new[] { "Q", "", "", "狐", "狐", "狐", "狐", "狐", "M", "M" }) },
            { "CO12", new KumiDice(new[] { "Q", "", "", "狐狐狐", "狐狐", "狐", "狐狐狐", "狐狐", "狐", "狐", "M", "M" }) },

            { "GM6", new KumiD6(null) },
            { "GM8", new KumiDice(new[] { "Q", "", "", "", "W", "W", "W", "M" }) },
            { "GM10", new KumiDice(new[] { "Q", "", "", "W", "W", "W", "W", "W", "M", "M" }) },
            { "Q12", new QDice(new[] { "", "", "", "Q", "Q", "Q", "Q", "Q", "Q", "Q", "M", "M" }) },
        };

        private Result? RollKumi(string command, IRandomizer randomizer)
        {
            if (!TABLES.TryGetValue(command, out var table))
            {
                return null;
            }

            if (table is KumiDice kumiDice)
            {
                return kumiDice.Roll(randomizer);
            }
            else if (table is KumiD6 kumiD6)
            {
                return kumiD6.Roll(randomizer);
            }
            else if (table is QDice qDice)
            {
                return qDice.Roll(randomizer);
            }
            return null;
        }

        private Result? RollExistence(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^EXI<=(\d+)$");
            if (!m.Success)
            {
                return null;
            }

            var target = int.Parse(m.Groups[1].Value);
            var dice = randomizer.RollOnce(20);

            var isCritical = dice == 1;
            var isFumble = dice == 20;
            var isSuccess = (dice <= target && !isFumble) || isCritical;

            string result_tail;
            if (isCritical)
            {
                result_tail = "M ＞ マジック";
            }
            else if (isFumble)
            {
                result_tail = "Q ＞ ファンブル";
            }
            else if (isSuccess)
            {
                result_tail = "成功";
            }
            else
            {
                result_tail = "失敗";
            }

            var text = string.Join(" ＞ ", new[] { $"(D20<={target})", dice.ToString(), result_tail });

            return Result.CreateBuilder(text)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .SetCondition(isSuccess)
                .Build();
        }

        private class KumiDice
        {
            public const string CRITICAL = "M";
            public const string FUMBLE = "Q";

            private readonly string[] _items;

            public KumiDice(string[] items)
            {
                _items = items;
            }

            public Result Roll(IRandomizer randomizer)
            {
                var dice = randomizer.RollOnce(_items.Length);
                var chosen = _items[dice - 1];

                var fumble = chosen == FUMBLE;
                var critical = chosen == CRITICAL;

                string result_tail;
                if (fumble)
                {
                    result_tail = "ファンブル";
                }
                else if (critical)
                {
                    result_tail = "マジック";
                }
                else if (chosen.Length > 0)
                {
                    result_tail = "成功";
                }
                else
                {
                    result_tail = "失敗";
                }

                var parts = new List<string>();
                parts.Add($"(D{_items.Length})");
                parts.Add(dice.ToString());
                if (chosen.Length > 0)
                {
                    parts.Add(chosen);
                }
                parts.Add(result_tail);

                var text = string.Join(" ＞ ", parts);

                return Result.CreateBuilder(text)
                    .SetCritical(critical)
                    .SetFumble(fumble)
                    .SetCondition(chosen.Length > 0 && !fumble)
                    .Build();
            }
        }

        private class KumiD6
        {
            private static readonly string[] TABLE = new[] { "裏", "羽", "星", "狐", "可", "Q" };

            private readonly string? _successSymbol;

            public KumiD6(string? successSymbol)
            {
                _successSymbol = successSymbol;
            }

            public Result Roll(IRandomizer randomizer)
            {
                var dice = randomizer.RollOnce(6);
                var chosen = TABLE[dice - 1];

                var builder = Result.CreateBuilder("");

                bool isFumble = false;
                bool isSuccess = false;
                bool isFailure = false;

                if (_successSymbol != null)
                {
                    isFumble = chosen == "Q";
                    isSuccess = chosen == _successSymbol;
                    isFailure = !isSuccess && !isFumble;
                }

                string? result_tail = null;
                if (isFumble)
                {
                    result_tail = "ファンブル";
                }
                else if (isSuccess)
                {
                    result_tail = "成功";
                }
                else if (isFailure)
                {
                    result_tail = "失敗";
                }

                var parts = new List<string>();
                parts.Add("(D6)");
                parts.Add(dice.ToString());
                parts.Add(chosen);
                if (result_tail != null)
                {
                    parts.Add(result_tail);
                }

                var text = string.Join(" ＞ ", parts);

                builder = Result.CreateBuilder(text);
                if (_successSymbol != null)
                {
                    builder.SetFumble(isFumble);
                    builder.SetCondition(isSuccess);
                }
                return builder.Build();
            }
        }

        private class QDice
        {
            public const string CRITICAL = "M";

            private readonly string[] _items;

            public QDice(string[] items)
            {
                _items = items;
            }

            public Result Roll(IRandomizer randomizer)
            {
                var dice = randomizer.RollOnce(_items.Length);
                var chosen = _items[dice - 1];

                var critical = chosen == CRITICAL;

                string result_tail;
                if (critical)
                {
                    result_tail = "マジック";
                }
                else if (chosen.Length > 0)
                {
                    result_tail = "成功";
                }
                else
                {
                    result_tail = "失敗";
                }

                var parts = new List<string>();
                parts.Add($"(D{_items.Length})");
                parts.Add(dice.ToString());
                if (chosen.Length > 0)
                {
                    parts.Add(chosen);
                }
                parts.Add(result_tail);

                var text = string.Join(" ＞ ", parts);

                return Result.CreateBuilder(text)
                    .SetCritical(critical)
                    .SetCondition(chosen.Length > 0)
                    .Build();
            }
        }

    }
}
