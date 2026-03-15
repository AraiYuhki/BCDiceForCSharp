using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// カローン・サンクションズ
    /// </summary>
    public sealed class CharonSanctions : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly CharonSanctions Instance = new CharonSanctions();

        /// <inheritdoc/>
        public override string Id => "CharonSanctions";

        /// <inheritdoc/>
        public override string Name => "カローン・サンクションズ";

        /// <inheritdoc/>
        public override string SortKey => "かろおんさんくしおんす";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ 判定
          nCSm>=t ［判定］を行う。成功／不完全成功を判定。（p.111）
          n: ダイス数
          m: ［難易度］（省略時 4）
          t: ［必要成功数］（省略時 1）

        ■ 各種表
          ET 感情ワード（キャラクター）表（p.120）
          RT 襲撃表（p.146）
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return ActionRoll(command, randomizer) ?? RollTables(command, TABLES);
        }

        private Result? ActionRoll(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)CS(\d+)?(>=(\d+))?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var times = int.Parse(m.Groups[1].Value);
            if (times < 1)
            {
                return null;
            }

            var required = ((m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 4) < 2 ? 2 : (m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 4) > 6 ? 6 : (m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 4));
            var target = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 1;

            var diceList = randomizer.RollBarabara(times, 6);
            var count = diceList.Count(i => i >= required);

            Result result;
            if (count >= target)
            {
                result = Result.CreateBuilder("成功").SetSuccess(true).Build();
            }
            else
            {
                result = Result.CreateBuilder("不完全成功").SetFailure(true).Build();
            }

            var sequence = new List<string>
            {
                $"({command})",
                $"({times}B6>={required}[>={target}])",
                $"[{string.Join(",", diceList)}]",
                $"成功数:{count}",
                result.Text
            };

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .Build();
        }

        // TABLES は Ruby 版のテーブル定義に対応（省略 - 別途テーブル実装が必要）
        private static readonly Dictionary<string, object> TABLES = new Dictionary<string, object>();

    }
}
