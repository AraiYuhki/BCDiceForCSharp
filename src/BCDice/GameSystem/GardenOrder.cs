using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class GardenOrder : GameSystemBase
    {
        public static readonly GardenOrder Instance = new GardenOrder();

        public override string Id => "GardenOrder";
        public override string Name => "ガーデンオーダー";
        public override string SortKey => "かあてんおおたあ";

        private static readonly Regex GoReg = new Regex(
            @"GO(-?\d+)(/(?:\d+))?(@(?:\d+))?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex DcSeReg = new Regex(
            @"^(DC|SE)(SL|BL|IM|BR|RF|EL)(\d+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m1 = GoReg.Match(command);
            if (m1.Success)
            {
                int successRate = int.Parse(m1.Groups[1].Value);
                int repeatCount = m1.Groups[2].Success ? int.Parse(m1.Groups[2].Value.TrimStart('/')) : 1;
                int? critBorderRaw = m1.Groups[3].Success ? int.Parse(m1.Groups[3].Value.TrimStart('@')) : (int?)null;
                int critBorder = GetCriticalBorder(critBorderRaw, successRate);
                return CheckRollRepeatAttack(successRate, repeatCount, critBorder, randomizer);
            }

            var m2 = DcSeReg.Match(command);
            if (m2.Success)
            {
                string chartType = m2.Groups[1].Value.ToUpper();
                string dmgType = m2.Groups[2].Value.ToUpper();
                int damageValue = int.Parse(m2.Groups[3].Value);
                if (chartType == "DC") return LookUpDamageChart(dmgType, damageValue);
                if (chartType == "SE") return LookUpDamageSeChart(dmgType, damageValue);
            }

            return null;
        }

        private static int GetCriticalBorder(int? cb, int sr)
        {
            if (cb.HasValue) return cb.Value;
            return Math.Max(sr / 5, 1);
        }

        private Result? CheckRollRepeatAttack(int sr, int rc, int cb, IRandomizer rng)
        {
            int srp = sr / rc;
            if (rc > 1 && srp < 50)
                return Result.Failure("D100<="+srp+"@"+cb+" ＞ 連続攻撃は成功率が50％以上必要です");
            return CheckRoll(srp, cb, rng);
        }

        private static Result CheckRoll(int sr, int cb, IRandomizer rng)
        {
            if (sr < 0) sr = 0;
            int fb = sr < 100 ? 96 : 99;
            int dv = rng.RollOnce(100);
            var inner = GetCheckResult(dv, sr, cb, fb);
            string t = "D100<="+sr+"@"+cb+" ＞ "+dv+" ＞ "+inner.Text;
            return Result.CreateBuilder(t).SetCondition(inner.IsSuccess).SetCritical(inner.IsCritical).SetFumble(inner.IsFumble).Build();
        }

        private static Result GetCheckResult(int dv, int sr, int cb, int fb)
        {
            if (dv >= fb) return Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
            if (dv <= cb) return Result.CreateBuilder("クリティカル").SetCritical(true).SetSuccess(true).Build();
            if (dv <= sr) return Result.CreateBuilder("成功").SetSuccess(true).Build();
            return Result.CreateBuilder("失敗").SetFailure(true).Build();
        }

        private Result? LookUpDamageChart(string t, int dv)
        {
            if (!DamageTable.TryGetValue(t, out var entry)) return null;
            var row = GetRowByNumber(dv, entry.Table);
            if (row == null) return null;
            string txt = "負儇表："+entry.Name+"["+dv+"] ＞ "+row.Damage+" ｜ "+row.Name+" … "+row.Text;
            return Result.Success(txt);
        }

        private Result? LookUpDamageSeChart(string t, int dv)
        {
            if (!DamageTableSE.TryGetValue(t, out var entry)) return null;
            var row = GetRowByNumber(dv, entry.Table);
            if (row == null) return null;
            string txt = "負儇表(SE)："+entry.Name+"["+dv+"] ＞ "+row.Damage+" ｜ "+row.Name+" … "+row.Text;
            return Result.Success(txt);
        }

        private static DamageRow? GetRowByNumber(int num, List<(int Max, DamageRow Row)> table)
        {
            foreach (var (max, row) in table)
                if (num <= max) return row;
            return null;
        }

        private sealed class DamageRow
        {
            public string Name { get; }
            public string Text { get; }
            public string Damage { get; }
            public DamageRow(string n, string t, string d) { Name=n; Text=t; Damage=d; }
        }

        private sealed class DamageTableEntry
        {
            public string Name { get; }
            public List<(int Max, DamageRow Row)> Table { get; }
            public DamageTableEntry(string n, List<(int Max, DamageRow Row)> t) { Name=n; Table=t; }
        }

        private static readonly Dictionary<string, DamageTableEntry> DamageTable = new Dictionary<string, DamageTableEntry>
        {
            {"SL", new DamageTableEntry("切断", new List<(int Max, DamageRow Row)>
            {
                (5, new DamageRow("切り傷", "皮膚が切り裂かれる。", "軽傷1")),
                (10, new DamageRow("脚部負傷", "足が切り裂かれ、思わずひざまずく。", "軽傷２／マヒ")),
                (13, new DamageRow("出血", "斬り裂かれた傷から出血が続く。", "軽傷３／ＤＯＴ：軽傷1")),
                (16, new DamageRow("胴部負傷", "胴部に大きな傷を受ける。", "軽傷４")),
                (19, new DamageRow("腕部負傷", "腕に大きな傷を受ける。", "重傷1／ＤＯＴ：軽傷1")),
                (22, new DamageRow("腹部負傷", "腹部を深く切り裂かれる。", "重傷２")),
                (25, new DamageRow("大量出血", "傷は深く、そこから大量に出血する。", "重傷２／ＤＯＴ：軽傷２")),
                (28, new DamageRow("裂傷", "治りにくい傷をつけられる。", "重傷３")),
                (31, new DamageRow("視界不良", "頭部に受けた傷から血が流れ、視界がふさがれる。", "重傷３／スタン")),
                (34, new DamageRow("胸部負傷", "胸から腰にかけて大きく切り裂かれる。", "致命傷1")),
                (37, new DamageRow("動脈切断", "動脈が切り裂かれ、噴き出るように出血する。", "致命傷1／ＤＯＴ：軽傷３")),
                (39, new DamageRow("胸部切断", "傷が肺にまで達し、喀血する。", "致命傷２")),
                (9999, new DamageRow("脊髄損傷", "脊髄が損傷する。", "致命傷２／放心、スタン、マヒ")),
            }) },
            {"BL", new DamageTableEntry("銃弾", new List<(int Max, DamageRow Row)>
            {
                (5, new DamageRow("腕部損傷", "銃弾が腕をかすめた。", "軽傷２")),
                (10, new DamageRow("腕部貫通", "銃弾が腕を貫く。痛みはあるが動作に支障はない。", "軽傷３")),
                (13, new DamageRow("胴部負傷", "胴部に銃弾をくらう。痛みで動きが鈍くなる。", "軽傷４／スロウ：－３")),
                (16, new DamageRow("肩負傷", "肩を貫かれる。骨が砕けたようだ。", "重傷1")),
                (19, new DamageRow("腹部負傷", "腹部が貫かれる。かろうじて内臓にダメージはないようだ。", "重傷２")),
                (22, new DamageRow("脚部貫通", "脚を銃弾に貫かれ、その場でひざまずく。", "重傷２／マヒ")),
                (25, new DamageRow("消化器系損傷", "胃などの消化器官にダメージを受ける。", "重傷３")),
                (28, new DamageRow("盲管銃弾", "身体に弾丸が深々と刺さる。激痛が走る。", "重傷３／スロウ：－5")),
                (31, new DamageRow("内臓損傷", "いくつかの内臓にダメージを受ける。", "致命傷1／スタン")),
                (34, new DamageRow("胴部貫通", "腹部への攻撃が貫通し、出血する。", "致命傷1／ＤＯＴ：軽傷1")),
                (37, new DamageRow("胸部負傷", "銃弾で肺を貫かれる。", "致命傷２")),
                (39, new DamageRow("致命的な一撃", "銃弾が頭部に命中。ショックで意識を飛ばされる。", "致命傷２／放心")),
                (9999, new DamageRow("必殺の一撃", "銃弾が心臓の近くを貫く。動脈にダメージを受けたようだ。", "致命傷２／ＤＯＴ：重傷1")),
            }) },
            {"IM", new DamageTableEntry("衝撃", new List<(int Max, DamageRow Row)>
            {
                (5, new DamageRow("打撲", "攻撃を受けた箇所がどす黒く腫れ上がる。", "軽傷1")),
                (10, new DamageRow("転倒", "衝撃で転倒する。", "軽傷1／マヒ")),
                (13, new DamageRow("平衡感覚喪失", "衝撃で三半規管にダメージを受ける。", "軽傷２、疲労２")),
                (16, new DamageRow("ボディーブロー", "腹部に直撃。痛みが継続し、体力を奪う。", "軽傷３／ＤＯＴ：疲労３")),
                (19, new DamageRow("痛打", "胴部や脚部などに打撃を受ける。", "軽傷４／スタン")),
                (22, new DamageRow("頭部痛打", "頭部にクリーンヒット。意識がもうろうとする。", "軽傷5／放心")),
                (25, new DamageRow("脚部骨折", "攻撃が足に命中し、骨折する。", "重傷1／スロウ：－5")),
                (28, new DamageRow("大転倒", "激しい衝撃によって、負傷すると共に大きく体勢を崩す。", "重傷1／マヒ、スタン")),
                (31, new DamageRow("脳震盪", "脳が大きく揺さぶられ、意識が飛びそうになる。", "重傷２／放心")),
                (34, new DamageRow("複雑骨折", "攻撃を受けた部分が大きくひしゃげ、複雑骨折したようだ。", "重傷３／放心、スタン")),
                (37, new DamageRow("頭部裂傷", "頭部に命中。皮膚が大きく裂ける。", "致命傷1、疲労３")),
                (39, new DamageRow("肋骨負傷", "折れた肋骨が肺に突き刺さり、まともに呼吸を行なうことができない。", "致命傷1／放心、スタン")),
                (9999, new DamageRow("内臓損傷", "衝撃が身体の芯まで届き、内臓がいくつか傷ついたようだ。", "致命傷２／ＤＯＴ：重傷1")),
            }) },
            {"BR", new DamageTableEntry("灼熱", new List<(int Max, DamageRow Row)>
            {
                (5, new DamageRow("火傷", "皮膚に小さな火傷を負う。", "軽傷1")),
                (10, new DamageRow("温度上昇", "熱によって、怪我だけではなく体力も奪われる。", "軽傷２、疲労1")),
                (13, new DamageRow("恐怖", "燃え上がる炎に恐怖を感じ、身体がすくんで動きが止まる。", "軽傷３／放心")),
                (16, new DamageRow("発火", "衣服や身体の一部に火が燃え移る。", "軽傷３／ＤＯＴ：軽傷1")),
                (19, new DamageRow("爆発", "爆発により吹き飛ばされ、転倒する。", "重傷1／マヒ")),
                (22, new DamageRow("大火傷", "痕が残るほどの大きな火傷を負う。", "重傷２")),
                (25, new DamageRow("熱波", "火傷と強力な熱により意識がもうろうとする。", "重傷２／スタン")),
                (28, new DamageRow("大爆発", "激しい爆発で吹き飛ばされ、ダメージと共に転倒する。", "重傷３／マヒ")),
                (31, new DamageRow("大発火", "広範囲に火が燃え移る。", "重傷３／ＤＯＴ：軽傷1")),
                (34, new DamageRow("炭化", "高熱のあまり、焼けた部分が炭化してしまう。", "致命傷1")),
                (37, new DamageRow("内臓火傷", "高温の空気を吸い込む、気道にも火傷を負ってしまう。", "致命傷1／ＤＯＴ：軽傷1")),
                (39, new DamageRow("全身火傷", "身体の各所に深い火傷を負う。", "致命傷２")),
                (9999, new DamageRow("致命的火傷", "身体の大部分に焼けどを負う。", "致命傷２／スタン")),
            }) },
            {"RF", new DamageTableEntry("冷却", new List<(int Max, DamageRow Row)>
            {
                (5, new DamageRow("冷気", "軽い凍傷を受ける。", "軽傷1")),
                (10, new DamageRow("霜の衣", "身体が薄い氷で覆われ、動きが鈍る。", "軽傷1／疲労1")),
                (13, new DamageRow("凍傷", "凍傷により身体が傷つけられる。", "軽傷3")),
                (16, new DamageRow("体温低下", "冷気によって体温を奪われる。", "軽傷３／ＤＯＴ：疲労1")),
                (19, new DamageRow("氷の枷", "肘や膝などが氷で覆われ、動きが取りにくくなる。", "重傷1／マヒ")),
                (22, new DamageRow("大凍傷", "身体の各所に凍傷を受ける。", "重傷1／ＤＯＴ：疲労２")),
                (25, new DamageRow("氷の束縛", "下半身が凍りつき、動くことができない。", "重傷２／マヒ")),
                (28, new DamageRow("視界不良", "頭部にも氷が張り、視界がふさがれる。", "重傷２／スタン")),
                (31, new DamageRow("腕部凍結", "腕が凍りづけになり、動かすことができない。", "重傷３／放心")),
                (34, new DamageRow("重度凍傷", "さらに体温が低下し、深刻な凍傷を受ける。", "致命傷1")),
                (37, new DamageRow("全身凍結", "全身が凍りづけになる。", "致命傷1／ＤＯＴ：疲労２")),
                (39, new DamageRow("致命的凍傷", "身体全身に凍傷を受ける。", "致命傷２")),
                (9999, new DamageRow("氷の棺", "完全に氷に閉じ込められる。", "致命傷２／スタン、マヒ")),
            }) },
            {"EL", new DamageTableEntry("電撃", new List<(int Max, DamageRow Row)>
            {
                (5, new DamageRow("静電気", "全身の毛が逆立つ。", "疲労３")),
                (10, new DamageRow("電熱傷", "電流によって傷つく。", "疲労1、軽傷1")),
                (13, new DamageRow("感電", "電流で傷つくと共に、身体が軽くしびれる。", "疲労２、軽傷２")),
                (16, new DamageRow("閃光", "激しい電光により、一時的に視界がふさがれる。", "軽傷３／スタン")),
                (19, new DamageRow("脚部感電", "電流により脚がしびれ、動けなくなる。", "重傷1／マヒ")),
                (22, new DamageRow("大電熱傷", "身体の各所が電流によって傷つく。", "疲労２、重傷２")),
                (25, new DamageRow("腕部負傷", "電流で腕がしびれ、動けなくなる。", "軽傷1、重傷２／放心")),
                (28, new DamageRow("大感電", "電流によって身体中がしびれ、動けなくなる。", "重傷２／スタン、マヒ")),
                (31, new DamageRow("一時心停止", "強力な電撃のショックにより、心臓がほんの一瞬だけ止まる。", "疲労３、重傷３")),
                (34, new DamageRow("大電流", "全身に電流が駆け巡る。", "重傷３／放心、マヒ")),
                (37, new DamageRow("致命電熱傷", "全身が電流によって傷つく。", "重傷1、致命傷1")),
                (39, new DamageRow("心停止", "強力な電撃のショックにより、心臓が一時的に止まる。死の淵が見える。", "疲労３、重傷1、致命傷1")),
                (9999, new DamageRow("組織炭化", "全身が電流で焼かれ、あちこちの組織が炭化する。", "致命傷２／スタン")),
            }) },
        };

        private static readonly Dictionary<string, DamageTableEntry> DamageTableSE = new Dictionary<string, DamageTableEntry>
        {
            {"SL", new DamageTableEntry("切断", new List<(int Max, DamageRow Row)>
            {
                (5, new DamageRow("軽い衝撃", "たいした傷ではないが、衝撃を受ける。", "スタン")),
                (10, new DamageRow("小さな傷", "外装に傷がつく。", "軽傷1")),
                (13, new DamageRow("大きな傷", "外装に大きな傷がつく。", "軽傷2")),
                (16, new DamageRow("とても大きな傷", "外装にさらに大きな傷がつき、内部もダメージを受ける。", "軽傷3/DOT:軽傷1")),
                (19, new DamageRow("外観破損", "外装の一部が欠ける。", "軽傷4")),
                (22, new DamageRow("内部破損", "外装の一部が破損し、内部もダメージを受ける。", "重傷1/DOT:軽傷1")),
                (25, new DamageRow("内部大破損", "内部の一部が大きく破損する。", "重傷2")),
                (28, new DamageRow("一時不全", "外装の一部が壊れ、内部も大きなダメージを受ける。", "重傷2/DOT:軽傷2")),
                (31, new DamageRow("裂傷", "傷をつけられ、内部が顔をのぞかせる。", "重傷3")),
                (34, new DamageRow("視界不良", "取りつけられているカメラに不都合が生じる", "重傷3/スタン")),
                (37, new DamageRow("大裂傷", "大きく切り裂かれ、内部が露わになる。", "致命傷1")),
                (39, new DamageRow("機能不全", "重要な部品が壊れ、このままで機能に大きな障害が出る。", "致命傷1/DOT:軽傷3")),
                (9999, new DamageRow("致命的損傷", "機能が停止しかねないほどの大きな損傷を受ける。", "致命傷2")),
            }) },
            {"BL", new DamageTableEntry("銃弾", new List<(int Max, DamageRow Row)>
            {
                (5, new DamageRow("軽い衝撃", "銃弾ははじいたが、衝撃を受ける。", "スタン")),
                (10, new DamageRow("小さな銃創", "銃弾ははじいたが、外装に凹みができた。", "軽傷2")),
                (13, new DamageRow("大きな銃創", "かろうじて銃弾ははじいたが、外装に大きな凹みができた。", "軽傷3")),
                (16, new DamageRow("機能低下", "銃弾の衝撃で、内部の機能に不都合が講じた。", "軽傷4/スロウ:-3")),
                (19, new DamageRow("とても大きな銃創", "銃弾をはじけず、外装に食い込んだ。", "重傷1")),
                (22, new DamageRow("銃弾停止", "銃弾が貫通した。かろうじて内部にダメージはないようだ。", "重傷2")),
                (25, new DamageRow("内部損傷", "銃弾が貫通した時、内部の機能に衝撃を受ける。", "重傷2/放心")),
                (28, new DamageRow("内部破壊", "銃弾が貫通した時、内部にも大きなダメージを受ける。", "重傷3")),
                (31, new DamageRow("内部大破壊", "銃弾が貫通した時、その衝撃で内部に一時的な損傷を受ける。", "重傷3/スロウ:-5")),
                (34, new DamageRow("機能一部停止", "銃弾によって内部の機能が一時的に役に立たなくなっている。", "致命傷1/スタン")),
                (37, new DamageRow("破損", "銃弾によって外装が吹き飛び、内部の一部が破壊される。", "致命傷1/DOT:軽傷1")),
                (39, new DamageRow("大破損", "銃弾によって外装ごと内部の一部が吹き飛ぶ。", "致命傷2")),
                (9999, new DamageRow("致命的な一撃", "銃弾が重要な部位に命中。衝撃で機能の大部分が一時的に停止する。", "致命傷2/放心")),
            }) },
            {"IM", new DamageTableEntry("衝撃", new List<(int Max, DamageRow Row)>
            {
                (5, new DamageRow("軽い衝撃", "傷も凹みもないが、衝撃を受ける。", "スタン")),
                (10, new DamageRow("衝撃", "衝撃を受けて外装が凹む。", "軽傷1")),
                (13, new DamageRow("大きな衝撃", "衝撃を受けて外装が大きく凹む。", "軽傷2")),
                (16, new DamageRow("とても大きな衝撃", "衝撃を受けて外装が大きく凹み、機能が瞬間的に停止する。", "軽傷2/放心")),
                (19, new DamageRow("内部圧迫", "外装が凹み、内部を圧迫している。", "軽傷3/DOT:疲労3")),
                (22, new DamageRow("痛打", "当たり所が悪く、カメラ機能が一時的に停止する。", "軽傷4/スタン")),
                (25, new DamageRow("内部衝撃", "当たり所が悪く、内部に大きなダメージを与える。機能が一時的に停止する。", "軽傷5/放心")),
                (28, new DamageRow("機能障害", "衝撃によって機能の動作に障害が発生する。", "重傷1/スロウ:-5")),
                (31, new DamageRow("外裝損傷", "外装が損傷し、その破片が内部に突き刺さる。", "重傷1/放心、スタン")),
                (34, new DamageRow("外装大損傷", "外装が大きく損傷し、内部もいくつか破壊される。", "重傷2/放心")),
                (37, new DamageRow("外装破壊", "外装の一部が吹き飛び、内部も破壊される。", "重傷3/放心、スタン")),
                (39, new DamageRow("外装大破壊", "外装の一部が吹き飛び、内部も一緒に吹き飛ばされる。", "致命傷1、疲労3")),
                (9999, new DamageRow("致命的破壊", "外装のほとんどが破壊され、内部もひしゃげ、潰される。", "致命傷1/放心、スタン")),
            }) },
            {"BR", new DamageTableEntry("灼熱", new List<(int Max, DamageRow Row)>
            {
                (5, new DamageRow("軽い溶解", "外装が少しだけ溶け、機能が低下する。", "スタン")),
                (10, new DamageRow("溶解", "外装が溶ける。", "軽傷1")),
                (13, new DamageRow("温度上昇", "熱によって、外装だけでなく内部も少しだけ溶解する。", "軽傷2、疲労1")),
                (16, new DamageRow("温度大上昇", "熱によって、外装だけでなく内部が溶解する。", "軽傷3/放心")),
                (19, new DamageRow("発火", "外装に火が燃え移る。", "軽傷3/DOT:軽傷1")),
                (22, new DamageRow("爆発", "爆発により外装の一部が吹き飛ばされる。", "重傷1/スタン")),
                (25, new DamageRow("大溶解", "痕が残るほどの大きく外装が溶解する。", "重傷2")),
                (28, new DamageRow("熱波", "強力な熱により内部機能が低下する。", "重傷2/スタン")),
                (31, new DamageRow("大爆発", "激しい爆発で外装が大きく吹き飛ばされ、内部にもダメージを受ける。", "重傷3/放心")),
                (34, new DamageRow("大発火", "広範囲に火が燃え移る。", "重傷3/DOT:軽傷1")),
                (37, new DamageRow("炭化", "高熱のあまり、焼けた部分が炭化してしまう。", "致命傷1")),
                (39, new DamageRow("内部溶解", "内部に熱や炎が回り、大きく溶解する。", "致命傷1/DOT:軽傷1")),
                (9999, new DamageRow("致命的溶解", "外装や内部の大部分が溶解する。", "致命傷2")),
            }) },
            {"RF", new DamageTableEntry("冷却", new List<(int Max, DamageRow Row)>
            {
                (5, new DamageRow("軽い冷却", "冷気で機能が低下する。", "スタン")),
                (10, new DamageRow("冷気", "外装が薄い氷で覆われる。", "軽傷1")),
                (13, new DamageRow("霜の衣", "外装が薄い氷で覆われ、動作が鈍る。", "軽傷2/疲労1")),
                (16, new DamageRow("軽い凍結", "凍結によって外装の一部が痛む。", "軽傷3")),
                (19, new DamageRow("温度低下", "冷気によって機能が低下する。", "軽傷3/DOT:疲労1")),
                (22, new DamageRow("氷の枷", "可動部などが氷で覆われ、動きが取りにくくなる。", "重傷1/スタン")),
                (25, new DamageRow("大凍結", "外装の大部分が凍結する。", "重傷1/DOT:疲労2")),
                (28, new DamageRow("氷の束縛", "可動部などが完全に凍りつき、動くことができない。", "重傷2/放心")),
                (31, new DamageRow("視界不良", "カメラのレンズにも氷が張り、視界がふさがれる。", "重傷2/スタン")),
                (34, new DamageRow("動作不良", "凍結によって一部動作に不都合が生じている。", "重傷3/放心")),
                (37, new DamageRow("重度凍結", "さらに温度が低下し、内部にも深刻なダメージを受ける。", "致命傷1")),
                (39, new DamageRow(" 全身凍結", "全体が凍りづけになる。", "致命傷1/DOT:疲労2")),
                (9999, new DamageRow("致命的凍結", "外装だけでなく、内部も致命的なダメージを受ける。", "致命傷2")),
            }) },
            {"EL", new DamageTableEntry("電撃", new List<(int Max, DamageRow Row)>
            {
                (5, new DamageRow("軽い電撃", "電撃で機能が低下する。", "スタン")),
                (10, new DamageRow("帯電", "帯電により軽いダメージを受ける。", "疲労3")),
                (13, new DamageRow("電熱傷", "電流によって外装が傷つく。", "疲労1、軽傷1")),
                (16, new DamageRow("軽い感電", "電流で傷つくと共に、内部に軽いダメージを受ける。", "疲労2、軽傷2")),
                (19, new DamageRow("閃光", "激しい閃光により、一時的にカメラ機能がマヒする。", "軽傷3/スタン")),
                (22, new DamageRow("感電", "電流により内部がダメージを受け、一時的に動作がマヒする。", "重傷1/放心")),
                (25, new DamageRow("大電熱傷", "外装の各所が電流によって傷つく。", "疲労2、重傷2")),
                (28, new DamageRow("感電による負傷", "外装だけでなく、内部も大きなダメージを受け、機能がマヒする。", "軽傷1、重傷2/放心")),
                (31, new DamageRow("大感電", "電流によって機能のほとんどがマヒして、動作しなくなる。", "重傷2/放心、スタン")),
                (34, new DamageRow("大電流", "強力な電撃のショックにより、内部にも多大なダメージを受ける。", "疲労3、重傷3")),
                (37, new DamageRow("一時停止", "全身に電流が駆け巡り、機能の大部分が動作不良を起こす。", "重傷3/放心、スタン")),
                (39, new DamageRow("致命電熱傷", "全体が電流によって傷つく。", "重傷1、致命傷")),
                (9999, new DamageRow("機能停止", "強力な電撃のショックにより、故障寸前のダメージを受ける。", "疲労3、重傷1、致命傷1")),
            }) },
        };
    }
}
