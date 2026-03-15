using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class SajinsenkiAGuS2E : GameSystemBase
    {
        public static readonly SajinsenkiAGuS2E Instance = new SajinsenkiAGuS2E();

        public override string Id => "SajinsenkiAGuS2E";
        public override string Name => "砂塵戦機アーガス2ndEdition";
        public override string SortKey => "さしんせんきああかす2";
        public override string HelpMessage => @"・一般判定Lv（チャンス出目0→判定0） nAG+x
　　　nは習得レベル、Lv0の場合nの省略可能。xは判定値修正（数式による修正可）、省略した場合はレベル修正0
　　　例）AG:習得レベル0の一般技能、1AG+1:習得レベル1・判定値修正+1の技能、AG+2-1：習得レベル0・判定値修正2-1の技能、(1-1)AG：習得レベル1・レベル修正-1の技能

・適正距離での命中判定（チャンス出目0→判定0、HR算出）OM+y@z
　　　yは命中補正値（数式可）、zはクリティカル値。クリティカル値省略時は0
　　　HRの算出時には、HRが大きくなる場合に出目0を10に読み替えます。
　　　例）OM+18-6@2:命中補正値+18-6でクリティカル値2、適正距離の判定

・非適正距離での命中判定（チャンス出目0→判定0、HR算出）NM+y@z
　　　yは命中補正値（数式可）、zはクリティカル値。クリティカル値省略時は0
　　　HRの算出時には、HRが大きくなる場合に出目0を10に読み替えます。
　　　例）NM+4-3:命中補正値+4-3で非適正距離の判定


・『西風旅徨』で導入されたファンブル・ルールを用いた判定
　判定時にダイスがすべて8以上ならファンブル(自動失敗)です。
　それぞれのコマンドにWを付けると『西風旅徨』モードになります。
　　　・一般判定                nAGW+x
　　　・適正距離での命中判定    OMW+y@z
　　　・非適正距離での命中判定  NMW+y@z



・クリティカル表　　 CR
・鹵獲結果表　　　　 CAP
・幕間クエスト表　　 INT
・サルベージ表　　　 SAL
・赤字ペナルティー表 DEF
・特殊戦況表　　　　 SPE

※通常の1D10などの10面ダイスにおいて出目10の読み替えはしません。コマンドのみです。
　ページ参照は、何もない場合は「ルールブック」、wは「西風旅徨」を示します。
";
    }
}