using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 呪印感染
    /// </summary>
    public sealed class JuinKansen : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly JuinKansen Instance = new JuinKansen();

        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly Dictionary<string, object> TABLES = new Dictionary<string, object>();

        /// <summary>コマンドエイリアス</summary>
        private static readonly Dictionary<string, string> ALIAS = new Dictionary<string, string>
        {
            { "DAI", "DAILY" },
            { "PCI", "PLACECITY" },
            { "PCO", "PLACECOUNTRYSIDE" },
            { "PFA", "PLACEFACILITY" },
            { "FL", "FIRSTLOOK" },
            { "AF", "APPRECIATIVEFRIEND" },
            { "FOR", "FORESHADOW" },
            { "EMO", "EMOTION" },
            { "SIT", "SITUATION" },
            { "TAR", "TARGET" },
            { "INS", "INSANITY" },
            { "DEA", "DEATH" },
            { "FEA", "FEAR" },
        };

        /// <inheritdoc/>
        public override string Id => "JuinKansen";

        /// <inheritdoc/>
        public override string Name => "呪印感染";

        /// <inheritdoc/>
        public override string SortKey => "しゆいんかんせん";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ 表
          ・日常表 (DAI, Daily)
          ・場所表
            ・「都市」 (PCI, PlaceCity)
            ・「田舎」 (PCO, PlaceCountryside)
            ・「施設内」 (PFA, PlaceFacility)
          ・初見表 (FL, FirstLook)
          ・知己表 (AF, AppreciativeFriend)
          ・伏線表 (FOR, Foreshadow)
          ・感情表 (EMO, Emotion)
          ・状況表 (SIT, Situation)
          ・対象表 (TAR, Target)
          ・狂気表 (INS, Insanity)
          ・終焉表 (DEA, Death)
          ・恐怖表 (FEA, Fear)
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var resolvedCommand = ALIAS.TryGetValue(command, out var alias) ? alias : command;
            return RollTable(resolvedCommand, TABLES, randomizer);
        }

    }
}
