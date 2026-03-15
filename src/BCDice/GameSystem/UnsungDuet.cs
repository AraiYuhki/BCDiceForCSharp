using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.CommonCommand.AddDice;

namespace BCDice.GameSystem
{
    /// <summary>
    /// アンサング・デュエット
    /// </summary>
    public sealed class UnsungDuet : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly UnsungDuet Instance = new UnsungDuet();

        private static readonly Regex ShifterAliasReg = new Regex(
            @"^(?:shifter|UDS)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex BinderAliasReg = new Regex(
            @"^(?:binder|UDB)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Dictionary<string, string> Alias = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "HINJURY", "HIN" },
            { "HPHYSICAL", "HPH" },
            { "HFEAR", "HFE" },
            { "HFANTASY", "HFA" },
            { "HMIND", "HMI" },
            { "HOTHER", "HOT" },
        };

        private static readonly Dictionary<string, object> Tables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public override string Id => "UnsungDuet";

        /// <inheritdoc/>
        public override string Name => "アンサング・デュエット";

        /// <inheritdoc/>
        public override string SortKey => "あんさんくてゆえつと";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ シフター用判定 (shifter, UDS)
          1D10をダイスロールして判定を行います。
          例） shifter, UDS, shifter>=5, shifter+1>=6

        ■ バインダー用判定 (binder, UDB)
          2D6をダイスロールして判定を行います。
          例） binder, UDB, binder>=5, binder+1>=6

        ■ 変異表
          ・外傷 (HIN, HInjury)
          ・体調の変化 (HPH, HPhysical)
          ・恐怖 (HFE, HFear)
          ・幻想化 (HFA, HFantasy)
          ・精神 (HMI, HMind)
          ・そのほか (HOT, HOther)
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            string resolvedCommand = Alias.TryGetValue(command, out string? aliasValue) ? aliasValue : command;

            return RollReplacedCommandIfMatch(resolvedCommand, ShifterAliasReg, "1D10", randomizer)
                ?? RollReplacedCommandIfMatch(resolvedCommand, BinderAliasReg, "2D6", randomizer)
                ?? RollTables(resolvedCommand, Tables);
        }

        private Result? RollReplacedCommandIfMatch(string command, Regex regexp, string dist, IRandomizer randomizer)
        {
            if (regexp.IsMatch(command))
            {
                string replacedCommand = regexp.Replace(command, dist, 1);
                return AddDiceCommand.Instance.Eval(replacedCommand, this, randomizer);
            }
            return null;
        }

    }
}
