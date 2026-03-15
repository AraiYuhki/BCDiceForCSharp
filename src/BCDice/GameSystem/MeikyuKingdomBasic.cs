using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class MeikyuKingdomBasic : GameSystemBase
    {
        public static readonly MeikyuKingdomBasic Instance = new MeikyuKingdomBasic();

        public override string Id => "MeikyuKingdomBasic";
        public override string Name => "迷宮キングダム 基本ルールブック";
        public override string SortKey => "めいきゆうきんくたむきほんるうるふつく";
        public override D66SortType D66SortType => D66SortType.Ascending;
        public override RoundType RoundType => RoundType.Ceiling;
        public override string HelpMessage => @"・判定　(nMK+m)
　n個のD6を振って大きい物二つだけみて達成値を算出します。修正mも可能です。
　絶対成功と絶対失敗も自動判定します。
・各種表
　・休憩表：才覚 TBT／魅力 CBT／探索 SBT／武勇 VBT
             お祭り FBT／空振り EBT／全体 WBT／カップル LBT
             お食事 FDBT
　・ハプニング表：才覚 THT／魅力 CHT／探索 SHT／武勇 VHT
　・視察表 RT／情報収集表 IG／ランダムマップ選択表 RMS
　・痛打表 CAT／致命傷表 FWT／戦闘ファンブル表 CFT
　・道中表 TT／交渉表 NT／相場表 MPT／王国災厄表 KDT／王国変動表 KCT
　・感情表 ET／好意表 FET／敵意表 HET
　・お宝表１／２／３／４／５ T1T／T2T／T3T／T4T／T5T
　・特殊遭遇表 SE
　　　上級：人工 ARN／水域 WEN／自然 NEN／洞窟 CEN／天空 SEN／異界 OEN
　・他国との関係表 ORT
・D66ダイスあり
";
    }
}