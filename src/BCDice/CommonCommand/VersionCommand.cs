using System;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.CommonCommand
{
    /// <summary>
    /// バージョン表示コマンド
    /// 例: BCDiceVersion
    /// </summary>
    public class VersionCommand : ICommonCommand
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly VersionCommand Instance = new VersionCommand();

        private static readonly Regex CommandRegex = new Regex(
            @"^BCDiceVersion$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public string PrefixPattern => "BCDiceVersion";

        /// <inheritdoc/>
        public Result? Eval(string command, IGameSystemContext gameSystem, IRandomizer randomizer)
        {
            // コマンドの最初の部分のみを取得（スペース以降を無視）
            string cmd = command.Split(' ')[0];

            var match = CommandRegex.Match(cmd);
            if (!match.Success)
            {
                return null;
            }

            return Result.CreateBuilder($"BCDice {BCDiceVersion.Version}")
                .Build();
        }
    }
}
