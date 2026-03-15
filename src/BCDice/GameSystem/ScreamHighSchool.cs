using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class ScreamHighSchool : GameSystemBase
    {
        public static readonly ScreamHighSchool Instance = new ScreamHighSchool();

        public override string Id => "ScreamHighSchool";
        public override string Name => "スクリームハイスクール";
        public override string SortKey => "すくりいむはいすくうる";
        public override string HelpMessage => @"・基本判定
　SHx/y@z　x：成功率、y：連続攻撃回数（省略可）、z：クリティカル値（省略可）
　（連続攻撃では1回の判定のみが実施されます）
　例）SH55　SH(40-20) SH100/2　SH70@10　SH155/3@44
・感情判定
　EMx@z　x：成功率、z：クリティカル値（省略可）
　例）EM50　EM50@15
・性格傾向判定
　TRx@z　x：成功率、z：クリティカル値（省略可）
　例）TR60　TR60@15
・恐怖判定
　FEx@z　x：成功率、z：クリティカル値（省略可）
　例）FE70　FE70@15
・負傷表
　DCxxy
　xx：属性（切断：SL，銃弾：BL，衝撃：IM，灼熱：BR，冷却：RF，電撃：EL）
　y：ダメージ
　例）DCSL7　DCEL22
";
    }
}