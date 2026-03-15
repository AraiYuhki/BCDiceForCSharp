using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// インビジブルライアー
    /// </summary>
    public sealed class InvisibleLiar : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly InvisibleLiar Instance = new InvisibleLiar();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "InvisibleLiar";

        /// <inheritdoc/>
        public override string Name => "インビジブルライアー";

        /// <inheritdoc/>
        public override string SortKey => "いんひしふるらいああ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ 採取表
        WOODSn 森
        PRAIRIEn 草原
        LAKEn 湖
        RIVERn 川辺
        SWAMPn 沼地
        CAVEn 洞窟
        ROCKYn 岩場
        MOUNTAINn 山岳
          n: 採取時間（1〜5）
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            RollTable(command, TABLES, randomizer);

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

    }
}