using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class PulpCthulhu : GameSystemBase
    {
        public static readonly PulpCthulhu Instance = new PulpCthulhu();

        public override string Id => "PulpCthulhu";
        public override string Name => "パルプ・クトゥルフ";
        public override string SortKey => "はるふくとうるふ";
        public override string HelpMessage => @"※私家翻訳のため、用語・ルールの詳細については原本を参照願います。
※コマンドは入力内容の前方一致で検出しています。
・判定　CC(x)<=（目標値）
　x：ボーナス・ペナルティダイス (2～－2)。省略可。
　目標値が無くても1D100は表示される。
　ファンブル／失敗／
　成功／ハード成功／イクストリーム成功／クリティカル を自動判定。
例）CC<=30　CC(2)<=50　CC(-1)<=75 CC-1<=50 CC1<=65 CC

・組み合わせ判定　(CBR(x,y))
　目標値 x と y で％ロールを行い、成否を判定。
　例）CBR(50,20)

・自動火器の射撃判定　FAR(w,x,y,z,d)
　w：弾丸の数(1～100）、x：技能値（1～100）、y：故障ナンバー、
　z：ボーナス・ペナルティダイス(-2～2)。省略可。
　d：指定難易度で連射を終える（レギュラー：r,ハード：h,イクストリーム：e）。省略可。
　命中数と貫通数、残弾数のみ算出。ダメージ算出はありません。
例）FAR(25,70,98)　FAR(50,80,98,-1)　far(30,70,99,1,R)
　　far(25,88,96,2,h)　FaR(40,77,100,,e)

・各種表
　【狂気関連】
　・狂気の発作（リアルタイム）（Bouts of Madness Real Time）　BMR
　・狂気の発作（サマリー）（Bouts of Madness Summary）　BMS
　・恐怖症（Sample Phobias）表　PH／マニア（Sample Manias）表　MA
　・狂気のタレント（Insane Talents）表　IT
　【魔術関連】
　・プッシュ時のキャスティング・ロールの失敗（Failed Casting Effects）表　FCE
";
    }
}