using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// リューチューバーとちいさな奇跡
    /// </summary>
    public sealed class RyuTuber : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly RyuTuber Instance = new RyuTuber();

        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly Dictionary<string, object> TABLES = new Dictionary<string, object>();

        /// <summary>テキスト表示コマンド</summary>
        private static readonly Dictionary<string, string> TEXTS = new Dictionary<string, string>
        {
            ["RTB"] = "判定ルール表示\n①枠主が判定内容を宣言、判定参加者が行動宣言\n②サイコロは竜の巫女なら6個、技能レベルか指定魅力の値個、奇跡の演目を1つ以上クリアで+6個、スパの消費数個\n③振ったサイコロの「1の目」の数が目標値以上なら華麗に成功、目標値未満ならちょっと残念な結果",
            ["MPW"] = "幸運の風が吹いている\n奇跡　以降ゲーム終了まで、サイコロ+1\n①健気に頑張る姿を見せる。\n②報われることはなく、さらに最悪の展開に。\n③それでも健気なところを見せる。",
            ["MPT"] = "困った時はお互い様\n奇跡　そのプレイヤーの判定サイコロを1回振り直しできる\n①けちな様子を見せる。\n②困っている人に施しをする姿を見られる。\n③窮地に陥る。",
            ["MPF"] = "悪い予感は的中する\n奇跡　1判定だけ、サイコロ+3\n①犠牲者が悪い噂を耳にする。\n②犠牲者が悪い冗談を言う。\n③犠牲者が悪い予感に心さざめき、誰かに悪い予感を話す。",
            ["MPL"] = "ついていい嘘もある\n奇跡　ついた（ささやかな）嘘が本当になる　枠主判断でいつか発動する。\n①嘘を言う。\n②嘘によって窮地に立つ。\n③嘘を嘘にしないためにあがく。",
            ["MPS"] = "私には星が見えている\n奇跡　指定したキャラクターの次の行動がわかる\n①少し先のことを言い当てる。\n②気味が悪いと噂になる。\n③言い当てる力を人間観察に用いる。",
            ["MPD"] = "心は竜と共にあり\n奇跡　起こりうる不幸を阻止する\n①心清いひとに助けられる。\n②自分の性根悪さを悲しむ。\n③自分なりのやり方で心清い行いをする。",
            ["MPH"] = "人は石垣、人は城\n奇跡　感化された周りの人が手伝うようになる\n①人々の不幸を見て、親切にしてしまう。\n②けなげに頑張る姿を見られる。\n③見ていた人々が集まってくる。",
        };

        /// <inheritdoc/>
        public override string Id => "RyuTuber";

        /// <inheritdoc/>
        public override string Name => "リューチューバーとちいさな奇跡";

        /// <inheritdoc/>
        public override string SortKey => "りゆうちゆうはあとちいさなきせき";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ◆判定
        　・判定　nB6<=1
        　　※　n:サイコロの数　例）12B6<=1　サイコロの数12個の場合
        　・判定ルールを表示する　RTB
        ◆職業　（カッコ内は使えそうな技能）
        　・職業表　JT
        　・学生表　JST
        　・技術・専門職表　JTPT
        　・事務・サービス職表　JOST
        　・エンタメ職表　JET
        ◆趣味　（カッコ内は使えそうな技能）
        　・趣味表　HT
        　・多人数でできる趣味表　HGT
        　・一人でできるインドア趣味表A　HIAT
        　・一人でできるインドア趣味表B　HIBT
        　・一人でできるアウトドア趣味表A　HOAT
        　・一人でできるアウトドア趣味表B　HOBT
        ◆奇跡の演目を表示する
        　・幸運の風が吹いている MPW
        　・困った時はお互い様 MPT
        　・悪い予感は的中する MPF
        　・ついていい嘘もある MPL
        　・私には星が見えている MPS
        　・心は竜と共にあり MPD
        　・人は石垣、人は城 MPH
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var ret = RollTable(command, TABLES, randomizer);
            if (ret != null)
            {
                return ret;
            }
            if (TEXTS.TryGetValue(command, out var text))
            {
                return Result.CreateBuilder(text.TrimEnd('\n', '\r')).Build();
            }
            return null;
        }

    }
}
