using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ストラトシャウト
    /// </summary>
    public sealed class StratoShout : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly StratoShout Instance = new StratoShout();

        /// <inheritdoc/>
        public override string Id => "StratoShout";

        /// <inheritdoc/>
        public override string Name => "ストラトシャウト";

        /// <inheritdoc/>
        public override string SortKey => "すとらとしやうと";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"

        VOT, GUT, BAT, KEYT, DRT: (ボーカル、ギター、ベース、キーボード、ドラム)トラブル表
        EMO: 感情表
        ATn, RTTn: 特技表(n＝分野。空:ランダム 1:主義 2:身体 3:モチーフ 4:情緒 5:行動 6:逆境)
        RCT: 分野ランダム表
        SCENE, MACHI, GAKKO, BAND: (汎用、街角、学校、バンド)シーン表 接近シーンで使用
        TENKAI: シーン展開表 奔走シーン 練習シーンで使用

        D66入れ替えあり
        ";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        /// <summary>
        /// 2D6の判定結果を返す
        /// </summary>
        private Result? EvalResult2D6(int total, int diceTotal, IReadOnlyList<int> diceList, string cmpOp, int target, IRandomizer randomizer)
        {
            if (cmpOp != ">=")
            {
                return null;
            }

            if (diceTotal <= 2)
            {
                return Result.CreateBuilder(Translate("StratoShout.fumble")).SetFumble(true).SetFailure(true).Build();
            }
            else if (diceTotal >= 12)
            {
                return Result.CreateBuilder(Translate("StratoShout.critical")).SetCritical(true).SetSuccess(true).Build();
            }

            return null;
        }
    }
}
