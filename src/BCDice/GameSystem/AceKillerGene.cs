using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class AceKillerGene : GameSystemBase
    {
        public static readonly AceKillerGene Instance = new AceKillerGene();

        public override string Id => "AceKillerGene";
        public override string Name => "エースキラージーン";
        public override string SortKey => "ええすきらあしいん";
        public override string HelpMessage => @"・基本判定
　AKx/y@z　x：成功率、y：連続攻撃回数（省略可）、z：クリティカル値（省略可）
　（連続攻撃では1回の判定のみが実施されます）
　例）AK55　AK100/2　AK70@10　AK155/3@44
・負傷表
　DCxxy
　xx：属性（切断：SL，銃弾：BL，衝撃：IM，灼熱：BR，冷却：RF，電撃：EL）
　y：ダメージ
　例）DCSL7　DCEL22
";
    }
}