using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class Torg1_5 : GameSystemBase
    {
        public static readonly Torg1_5 Instance = new Torg1_5();

        public override string Id => "Torg1.5";
        public override string Name => "トーグ1.5版";
        public override string SortKey => "とおく1.5";
        public override string HelpMessage => @"・判定　(TGm)
　TORG専用の判定コマンドです。
　""TG(技能基本値)""でロールします。Rコマンドに読替されます。
　振り足しを自動で行い、20の出目が出たときには技能無し値も並記します。
・各種表　""(表コマンド)(数値)""で振ります。
　・一般結果表 成功度出力「RTx or RESULTx」
　・威圧/威嚇 対人行為結果表「ITx or INTIMIDATEx or TESTx」
　・挑発/トリック 対人行為結果表「TTx or TAUNTx or TRICKx or CTx」
　・間合い 対人行為結果表「MTx or MANEUVERx」
　・オーズ（一般人）ダメージ　「ODTx or ORDSx or ODAMAGEx」
　・ポシビリティー能力者ダメージ「DTx or DAMAGEx」
　・ボーナス表「BTx+y or BONUSx+y or TOTALx+y」 xは数値, yは技能基本値
";
    }
}