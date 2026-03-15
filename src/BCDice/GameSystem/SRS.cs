using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// スタンダードRPGシステム
    /// </summary>
    public class SRS : GameSystemBase
    {
        public static readonly SRS Instance = new SRS();

        private static readonly Regex SrsCommandRegex = new Regex(
            @"^(2D6)([+\-]\d+(?:[+\-]\d+)*)?(?:@(\d+))?(?:#(\d+))?(?:>=(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SrsLegacyRegex = new Regex(
            @"^(.+)\[(.*)\]$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex LegacyCFRegex = new Regex(
            @"^(-?\d+)?(?:,(-?\d+))?$",
            RegexOptions.Compiled);

        protected const int DefaultCriticalValue = 12;
        protected const int DefaultFumbleValue = 2;

        public override string Id => "SRS";
        public override string Name => "スタンダードRPGシステム";
        public override string SortKey => "すたんたあとRPGしすてむ";
        public override D66SortType D66SortType => D66SortType.NoSort;

        public override string HelpMessage => @"・判定
　・通常判定：2D6+m@c#f>=t または 2D6+m>=t[c,f]
　　修正値m、目標値t、クリティカル値c、ファンブル値fで判定ロールを行います。
　　修正値、クリティカル値、ファンブル値は省略可能です（[]ごと省略可、@c・#fの指定は順不同）。
　　クリティカル値、ファンブル値の既定値は、それぞれ12、2です。
　　自動成功、自動失敗、成功、失敗を自動表示します。

　　例) 2d6>=10　　　　　修正値0、目標値10で判定
　　例) 2d6+2>=10　　　　修正値+2、目標値10で判定
　　例) 2d6+2>=10[11]　　↑をクリティカル値11で判定
　　例) 2d6+2@11>=10 　　↑をクリティカル値11で判定
　　例) 2d6+2>=10[12,4]　↑をクリティカル値12、ファンブル値4で判定
　　例) 2d6+2@12#4>=10 　↑をクリティカル値12、ファンブル値4で判定

　・クリティカルおよびファンブルのみの判定：2D6+m@c#f または 2D6+m[c,f]
　　目標値を指定せず、修正値m、クリティカル値c、ファンブル値fで判定ロールを行います。
　　修正値、クリティカル値、ファンブル値は省略可能です（[]は省略不可、@c・#fの指定は順不同）。
　　自動成功、自動失敗を自動表示します。

・D66ダイスあり（入れ替えなし)
";

        /// <summary>
        /// エイリアスコマンド（サブクラスでオーバーライド可能）
        /// </summary>
        protected virtual string[] Aliases => Array.Empty<string>();

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // レガシー形式 (command[c,f]) のチェック
            var legacyMatch = SrsLegacyRegex.Match(command);
            SrsRollNode? node;

            if (legacyMatch.Success)
            {
                node = ParseLegacy(legacyMatch.Groups[1].Value, legacyMatch.Groups[2].Value);
            }
            else
            {
                node = Parse(command);
            }

            if (node != null)
            {
                return ExecuteSrsRoll(node, randomizer);
            }

            return null;
        }

        private SrsRollNode? Parse(string command)
        {
            // エイリアスコマンドの置換
            string replaced = command;
            foreach (var alias in Aliases)
            {
                if (command.StartsWith(alias, StringComparison.OrdinalIgnoreCase))
                {
                    replaced = "2D6" + command.Substring(alias.Length);
                    break;
                }
            }

            var match = SrsCommandRegex.Match(replaced);
            if (!match.Success)
            {
                return null;
            }

            int modifier = 0;
            if (match.Groups[2].Success)
            {
                modifier = EvalModifier(match.Groups[2].Value);
            }

            int? critical = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : (int?)null;
            int? fumble = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : (int?)null;
            int? target = match.Groups[5].Success ? int.Parse(match.Groups[5].Value) : (int?)null;

            // 2D6で始まりクリティカル・ファンブル・ターゲットいずれも指定されていない場合は通常のダイスに任せる
            if (command.StartsWith("2D6", StringComparison.OrdinalIgnoreCase) &&
                critical == null && fumble == null && target == null)
            {
                return null;
            }

            return new SrsRollNode(
                modifier,
                critical ?? DefaultCriticalValue,
                fumble ?? DefaultFumbleValue,
                target
            );
        }

        private SrsRollNode? ParseLegacy(string command, string cf)
        {
            var cfMatch = LegacyCFRegex.Match(cf);
            if (!cfMatch.Success)
            {
                return null;
            }

            int critical = cfMatch.Groups[1].Success ? int.Parse(cfMatch.Groups[1].Value) : DefaultCriticalValue;
            int fumble = cfMatch.Groups[2].Success ? int.Parse(cfMatch.Groups[2].Value) : DefaultFumbleValue;

            // エイリアスコマンドの置換
            string replaced = command;
            foreach (var alias in Aliases)
            {
                if (command.StartsWith(alias, StringComparison.OrdinalIgnoreCase))
                {
                    replaced = "2D6" + command.Substring(alias.Length);
                    break;
                }
            }

            var match = Regex.Match(replaced, @"^2D6([+\-]\d+(?:[+\-]\d+)*)?(?:>=(\d+))?$", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return null;
            }

            int modifier = 0;
            if (match.Groups[1].Success)
            {
                modifier = EvalModifier(match.Groups[1].Value);
            }

            int? target = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : (int?)null;

            return new SrsRollNode(modifier, critical, fumble, target);
        }

        private Result ExecuteSrsRoll(SrsRollNode node, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(2, 6);
            Array.Sort(diceList);

            int sum = diceList.Sum();
            string diceStr = string.Join(",", diceList);
            int modifiedSum = sum + node.Modifier;

            var result = CompareResult(node, sum, modifiedSum);

            string modifierStr = FormatModifier(node.Modifier);
            var parts = new List<string>
            {
                $"({node})",
                $"{sum}[{diceStr}]{modifierStr}",
                modifiedSum.ToString()
            };

            if (!string.IsNullOrEmpty(result.Text))
            {
                parts.Add(result.Text);
            }

            string text = string.Join(" ＞ ", parts);

            return Result.CreateBuilder(text)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static Result CompareResult(SrsRollNode node, int sum, int modifiedSum)
        {
            if (sum >= node.CriticalValue)
            {
                return Result.CreateBuilder("自動成功")
                    .SetCritical(true).SetSuccess(true).Build();
            }
            if (sum <= node.FumbleValue)
            {
                return Result.CreateBuilder("自動失敗")
                    .SetFumble(true).SetFailure(true).Build();
            }
            if (node.TargetValue == null)
            {
                return Result.CreateBuilder("").Build();
            }
            if (modifiedSum >= node.TargetValue.Value)
            {
                return Result.CreateBuilder("成功")
                    .SetSuccess(true).Build();
            }
            return Result.CreateBuilder("失敗")
                .SetFailure(true).Build();
        }

        private static int EvalModifier(string expr)
        {
            if (string.IsNullOrEmpty(expr)) return 0;

            int result = 0;
            var matches = Regex.Matches(expr, @"[+\-]\d+");
            foreach (Match m in matches)
            {
                result += int.Parse(m.Value);
            }
            return result;
        }

        protected static string FormatModifier(int value)
        {
            if (value == 0) return "";
            return value > 0 ? $"+{value}" : value.ToString();
        }

        protected class SrsRollNode
        {
            public int Modifier { get; }
            public int CriticalValue { get; }
            public int FumbleValue { get; }
            public int? TargetValue { get; }

            public SrsRollNode(int modifier, int criticalValue, int fumbleValue, int? targetValue)
            {
                Modifier = modifier;
                CriticalValue = criticalValue;
                FumbleValue = fumbleValue;
                TargetValue = targetValue;
            }

            public override string ToString()
            {
                string lhs = $"2D6{FormatModifier(Modifier)}";
                string expression = TargetValue.HasValue ? $"{lhs}>={TargetValue}" : lhs;
                return $"{expression}[{CriticalValue},{FumbleValue}]";
            }

            private static string FormatModifier(int value)
            {
                if (value == 0) return "";
                return value > 0 ? $"+{value}" : value.ToString();
            }
        }
    }
}