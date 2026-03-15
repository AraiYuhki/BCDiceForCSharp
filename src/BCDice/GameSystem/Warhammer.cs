using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ウォーハンマー
    /// </summary>
    public sealed class Warhammer : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Warhammer Instance = new Warhammer();

        private static readonly Regex AttackRegex = new Regex(
            @"^(WH\d+(@[\dWH]*)?)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex CriticalRegex = new Regex(
            @"^(WH[HABTLW]\d+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex CriticalParseRegex = new Regex(
            @"WH([HABTLW])(\d+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex AttackSplitRegex = new Regex(
            @"(.+)(@.*)",
            RegexOptions.Compiled);

        private static readonly Regex AttackValueRegex = new Regex(
            @"WH(\d+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PosType2WRegex = new Regex(
            @"@(2W|W2)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PosType4WRegex = new Regex(
            @"@(4W|W4)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PosType4HRegex = new Regex(
            @"@(4H|H4)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PosType4Regex = new Regex(
            @"@4",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PosTypeWRegex = new Regex(
            @"@W",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PosType2HRegex = new Regex(
            @"@(2H|H2|2)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "Warhammer";

        /// <inheritdoc/>
        public override string Name => "ウォーハンマー";

        /// <inheritdoc/>
        public override string SortKey => "うおおはんまあ";

        /// <inheritdoc/>
        public override RoundType RoundType => RoundType.Ceiling;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・クリティカル表(whHxx/whAxx/whBxx/whLxx)
        　""WH部位 クリティカル値""の形で指定します。部位は「H(頭部)」「A(腕)」「B(胴体)」「L(足)」の４カ所です。
        　例）whH10 whA5 WHL4
        ・命中判定(WHx@t)
        　""WH(命中値)@(種別)""の形で指定します。
        　種別は脚の数を数字、翼が付いているものは「W」、手が付いているものは「H」で書きます。
        　「2H(二足)」「2W(有翼二足)」「4(四足)」「4H(半人四足)」「4W(有翼四足)」「W(鳥類)」となります。
        　命中判定を行って、当たれば部位も表示します。
        　なお、種別指定を省略すると「二足」、「@」だけにすると全種別の命中部位を表示します。(コマンドを忘れた時の対応です)
        　例）wh60　　wh43@4W　　WH65@
        ";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            string? outputMsg = null;

            var match = AttackRegex.Match(command);
            if (match.Success)
            {
                var attackCommand = match.Groups[1].Value;
                outputMsg = GetAttackResult(attackCommand, randomizer);
            }
            else
            {
                match = CriticalRegex.Match(command);
                if (match.Success)
                {
                    var criticalCommand = match.Groups[1].Value;
                    outputMsg = GetCriticalResult(criticalCommand, randomizer);
                }
            }

            if (outputMsg == null)
            {
                return null;
            }

            return Result.CreateBuilder(outputMsg).Build();
        }

        private Result? Result1d100(int total, int diceTotal, string cmpOp, int target)
        {
            if (cmpOp != "<=")
            {
                return null;
            }

            if (total <= target)
            {
                return Result.CreateBuilder($"成功(成功度{(target - total) / 10})")
                    .SetSuccess(true)
                    .Build();
            }
            else
            {
                return Result.CreateBuilder($"失敗(失敗度{(total - target) / 10})")
                    .SetFailure(true)
                    .Build();
            }
        }

        private string Result1d100Text(int total, int diceTotal, string cmpOp, int target)
        {
            var result = Result1d100(total, diceTotal, cmpOp, target)?.Text;
            if (result == null)
            {
                return "";
            }
            else
            {
                return $" ＞ {result}";
            }
        }

        private string? GetCriticalResult(string input, IRandomizer randomizer)
        {
            // クリティカル効果データ
            var whh = new[]
            {
                "01:打撃で状況が把握出来なくなる。次ターンは1回の半アクションしか行なえない。",
                "02:耳を強打された為、耳鳴りが酷く目眩がする。1Rに渡って一切のアクションを行なえない。",
                "03:打撃が頭皮を酷く傷つけた。【武器技術度】に-10%。治療を受けるまで継続。",
                "04:鎧が損傷し当該部位のAP-1。修理するには(職能:鎧鍛冶)テスト。鎧を着けていないなら1Rの間アクションを行なえない。",
                "05:転んで倒れ、頭がくらくらする。1Rに渡ってあらゆるテストに-30で、立ち上がるには起立アクションが必要。",
                "06:1d10R気絶。",
                "07:1d10分気絶。以後CTはサドンデス。",
                "08:顔がずたずたになって倒れ、以後無防備状態。治療を受けるまで毎Rの被害者のターン開始時に20%で死亡。以後CTはサドンデスを適用。【頑強】テストに失敗すると片方の視力を失う。",
                "09:凄まじい打撃により頭蓋骨が粉砕される。死は瞬時に訪れる。",
                "10:死亡する。いかに盛大に出血し、どのような死に様を見せたのかを説明してもよい。",
            };
            var wha = new[]
            {
                "01:手に握っていたものを落とす。盾はくくりつけられている為、影響なし。",
                "02:打撃で腕が痺れ、1Rの間使えなくなる。",
                "03:手の機能が失われ、治療を受けるまで回復できない。手で握っていたもの(盾を除く)は落ちる。",
                "04:鎧が損傷する。当該部位のAP-1。修理するには(職能:鎧鍛冶)テスト。鎧を着けていないなら腕が痺れ、1Rの間使えなくなる。",
                "05:腕の機能が失われ、治療を受けるまで回復できない。手で握っていたもの(盾を除く)は落ちる。",
                "06:腕が砕かれる。手で握っていたもの(盾を除く)は落ちる。出血がひどく、治療を受けるまで毎Rの被害者のターン開始時に20%で死亡。以後CTはサドンデスを適用。",
                "07:手首から先が血まみれの残骸と化す。手で握っていたもの(盾を除く)は落ちる。出血がひどく、治療を受けるまで毎Rの被害者のターン開始時に20%で死亡。以後CTはサドンデスを適用。【頑健】テストに失敗すると手の機能を失う。",
                "08:腕は血まみれの肉塊がぶら下がっている状態になる。手で握っていたもの(盾を除く)は落ちる。治療を受けるまで毎Rの被害者のターン開始時に20%で死亡。以後CTはサドンデスを適用。【頑健】テストに失敗すると肘から先の機能を失う。",
                "09:大動脈に傷が及んだ。コンマ数秒の内に損傷した肩から血を噴出して倒れる。ショックと失血により、ほぼ即死する。",
                "10:死亡する。いかに盛大に出血し、どのような死に様を見せたのかを説明してもよい。",
            };
            var whb = new[]
            {
                "01:打撃で息が詰まる。1Rの間、キャラクターの全てのテストや攻撃に-20%。",
                "02:股間への一撃。苦痛のあまり、1Rに渡って一切のアクションを行なえない。",
                "03:打撃で肋骨がぐちゃぐちゃになる。以後治療を受けるまでの間、【武器技術度】に-10%。",
                "04:鎧が損傷する。当該部位のAP-1。修理するには(職能:鎧鍛冶)テスト。鎧を着けていないなら股間への一撃、1Rに渡って一切のアクションを行なえない。",
                "05:転んで倒れ、息が詰まって悶絶する。1Rに渡ってあらゆるテストに-30の修正、立ち上がるには起立アクションが必要。",
                "06:1d10R気絶。",
                "07:ひどい内出血が起こり、無防備状態。出血がひどく、治療を受けるまで毎Rの被害者のターン開始時に20%で死亡。",
                "08:脊髄が粉砕されて倒れ、以後治療を受けるまで無防備状態。以後CTはサドンデスを適用。【頑強】テストに失敗すると腰から下が不随になる。",
                "09:凄まじい打撃により複数の臓器が破裂し、死は数秒のうちに訪れる。",
                "10:死亡する。いかに盛大に出血し、どのような死に様を見せたのかを説明してもよい。",
            };
            var whl = new[]
            {
                "01:よろめく。次のターン、1回の半アクションしか行なえない。",
                "02:脚が痺れる。1Rに渡って【移動】は半減し、脚に関連する【敏捷】テストに-20%。回避が出来なくなる。",
                "03:脚の機能が失われ、治療を受けるまで回復しない。【移動】は半減し、脚に関連する【敏捷】テストに-20%。回避が出来なくなる。",
                "04:鎧が損傷する。当該部位のAP-1。修理するには(職能:鎧鍛冶)テスト。鎧を着けていないなら脚が痺れる、1Rに渡って【移動】は半減し、脚に関連する【敏捷】テストに-20%、回避不可になる。",
                "05:転んで倒れ、頭がくらくらする。1Rに渡ってあらゆるテストに-30の修正、立ち上がるには起立アクションが必要。",
                "06:脚が砕かれ、無防備状態。出血がひどく、治療を受けるまで毎Rの被害者のターン開始時に20%で死亡。以後CTはサドンデスを適用。",
                "07:脚は血まみれの残骸と化し、無防備状態になる。治療を受けるまで毎Rの被害者のターン開始時に20%で死亡。以後CTはサドンデスを適用。【頑強】テストに失敗すると足首から先を失う。",
                "08:脚は血まみれの肉塊がぶらさがっている状態。以後無防備状態。治療を受けるまで毎Rの被害者のターン開始時に20%で死亡。以後CTはサドンデスを適用。【頑強】テストに失敗すると膝から下を失う。",
                "09:大動脈に傷が及ぶ。コンマ数秒の内に脚の残骸から血を噴出して倒れ、ショックと出血で死は瞬時に訪れる。",
                "10:死亡する。いかに盛大に出血し、どのような死に様を見せたのかを説明してもよい。",
            };
            var whw = new[]
            {
                "01:軽打。1ラウンドに渡って、あらゆるテストに-10％。",
                "02:かすり傷。+10％の【敏捷】テストを行い、失敗なら直ちに高度を1段階失う。地上にいるクリーチャーは、次のターンには飛び立てない。",
                "03:損傷する。【飛行移動力】が2点低下する。-10％の【敏捷】テストを行い、失敗なら直ちに高度を1段階失う。地上にいるクリーチャーは、次のターンには飛び立てない。",
                "04:酷く損傷する。【飛行移動力】が4点低下する。-30％の【敏捷】テストを行い、失敗なら直ちに高度を1段階失う。地上にいるクリーチャーは、1d10ターンが経過するまで飛び立てない。",
                "05:翼が使えなくなる。【飛行移動力】が0に低下する。飛行中のものは落下し、高度に応じたダメージを受ける。地上にいるクリーチャーは、怪我が癒えるまで飛び立てない。",
                "06:翼の付け根に傷が開く。【飛行移動力】が0に低下する。飛行中のものは落下し、高度に応じたダメージを受ける。地上にいるクリーチャーは、怪我が癒えるまで飛び立てない。治療を受けるまで毎R被害者のターン開始時に20％の確率で死亡。以後CTはサドンデスを適用。",
                "07:翼は血まみれの残骸と化し、無防備状態になる。【飛行移動力】が0に低下する。飛行中のものは落下し、高度に応じたダメージを受ける。地上にいるクリーチャーは、怪我が癒えるまで飛び立てない。治療を受けるまで毎R被害者のターン開始時に20％の確率で死亡。以後CTはサドンデスを適用。【頑強】テストに失敗すると飛行能力を失う。",
                "08:翼が千切れてバラバラになり、無防備状態になる。【飛行移動力】が0に低下する。飛行中のものは落下し、高度に応じたダメージを受ける。地上にいるクリーチャーは、怪我が癒えるまで飛び立てない。治療を受けるまで毎R被害者のターン開始時に20％の確率で死亡。以後CTはサドンデスを適用。飛行能力を失う。",
                "09:大動脈が切断された。コンマ数秒の内に血を噴き上げてくずおれる、ショックと出血で死は瞬時に訪れる。",
                "10:死亡する。いかに盛大に出血し、どのような死に様を見せたのかを説明してもよい。",
            };

            var criticalTable = new[]
            {
                5, 7, 9, 10, 10, 10, 10, 10, 10, 10, // 01-10
                5, 6, 8, 9, 10, 10, 10, 10, 10, 10,  // 11-20
                4, 6, 8, 9, 9, 10, 10, 10, 10, 10,   // 21-30
                4, 5, 7, 8, 9, 9, 10, 10, 10, 10,    // 31-40
                3, 5, 7, 8, 8, 9, 9, 10, 10, 10,     // 41-50
                3, 4, 6, 7, 8, 8, 9, 9, 10, 10,      // 51-60
                2, 4, 6, 7, 7, 8, 8, 9, 9, 10,       // 61-70
                2, 3, 5, 6, 7, 7, 8, 8, 9, 9,        // 71-80
                1, 3, 5, 6, 6, 7, 7, 8, 8, 9,        // 81-90
                1, 2, 4, 5, 6, 6, 7, 7, 8, 8,        // 91-00
            };

            var critMatch = CriticalParseRegex.Match(input);
            if (!critMatch.Success)
            {
                return "1";
            }

            var partsWord = critMatch.Groups[1].Value; // 部位
            var criticalValue = int.Parse(critMatch.Groups[2].Value); // クリティカル値
            if (criticalValue > 10) criticalValue = 10;
            if (criticalValue < 1) criticalValue = 1;

            string whpp = "";
            string[] whppp = Array.Empty<string>();

            switch (partsWord.ToUpperInvariant())
            {
                case "H":
                    whpp = "頭部";
                    whppp = whh;
                    break;
                case "A":
                    whpp = "腕部";
                    whppp = wha;
                    break;
                case "T":
                case "B":
                    whpp = "胴体";
                    whppp = whb;
                    break;
                case "L":
                    whpp = "脚部";
                    whppp = whl;
                    break;
                case "W":
                    whpp = "翼部";
                    whppp = whw;
                    break;
            }

            var diceNow = randomizer.RollOnce(100);

            var critNo = (diceNow - 1) / 10 * 10;
            var critNum = criticalTable[critNo + criticalValue - 1];

            var resultText = whppp[critNum - 1];
            if (critNum >= 5)
            {
                resultText += "サドンデス×";
            }
            else
            {
                resultText += "サドンデス○";
            }

            var output = $"{whpp}CT表({diceNow}+{criticalValue}) ＞ {resultText}";

            return output;
        }

        private string WhAtpos(int posNum, string posType)
        {
            // Ruby arrays are mixed int/string; in C# we use object[] for each position table
            // Format: [name, threshold1, bodyPart1, threshold2, bodyPart2, ...]
            var pos2l = new object[] { "二足", 15, "頭部", 35, "右腕", 55, "左腕", 80, "胴体", 90, "右脚", 100, "左脚" };
            var pos2lw = new object[] { "有翼二足", 15, "頭部", 25, "右腕", 35, "左腕", 45, "右翼", 55, "左翼", 80, "胴体", 90, "右脚", 100, "左脚" };
            var pos4l = new object[] { "四足", 15, "頭部", 60, "胴体", 70, "右前脚", 80, "左前脚", 90, "右後脚", 100, "左後脚" };
            var pos4la = new object[] { "半人四足", 10, "頭部", 20, "右腕", 30, "左腕", 60, "胴体", 70, "右前脚", 80, "左前脚", 90, "右後脚", 100, "左後脚" };
            var pos4lw = new object[] { "有翼四足", 10, "頭部", 20, "右翼", 30, "左翼", 60, "胴体", 70, "右前脚", 80, "左前脚", 90, "右後脚", 100, "左後脚" };
            var posB = new object[] { "鳥", 15, "頭部", 35, "右翼", 55, "左翼", 80, "胴体", 90, "右脚", 100, "左脚" };

            var whPos = new object[][] { pos2l, pos2lw, pos4l, pos4la, pos4lw, posB };

            int posT = 0;
            if (posType != "")
            {
                if (PosType2WRegex.IsMatch(posType))
                {
                    posT = 1;
                }
                else if (PosType4WRegex.IsMatch(posType))
                {
                    posT = 4;
                }
                else if (PosType4HRegex.IsMatch(posType))
                {
                    posT = 3;
                }
                else if (PosType4Regex.IsMatch(posType))
                {
                    posT = 2;
                }
                else if (PosTypeWRegex.IsMatch(posType))
                {
                    posT = 5;
                }
                else
                {
                    if (!PosType2HRegex.IsMatch(posType))
                    {
                        posT = -1;
                    }
                }
            }

            var output = "";

            if (posT < 0)
            {
                foreach (var posI in whPos)
                {
                    output += GetWhAtposMessage(posI, posNum);
                }
            }
            else
            {
                var posI = whPos[posT];
                output += GetWhAtposMessage(posI, posNum);
            }

            return output;
        }

        private string GetWhAtposMessage(object[] posI, int posNum)
        {
            var output = " " + (string)posI[0] + ":";

            // Ruby: 1.step(pos_i.length + 1, 2) -- iterates index 1, 3, 5, ...
            // posI[i] is threshold (int), posI[i+1] is body part name (string)
            for (int i = 1; i < posI.Length; i += 2)
            {
                if (posNum <= (int)posI[i])
                {
                    output += (string)posI[i + 1];
                    break;
                }
            }

            return output;
        }

        private string? GetAttackResult(string input, IRandomizer randomizer)
        {
            var posType = "";

            var splitMatch = AttackSplitRegex.Match(input);
            if (splitMatch.Success)
            {
                input = splitMatch.Groups[1].Value;
                posType = splitMatch.Groups[2].Value;
            }

            var valueMatch = AttackValueRegex.Match(input);
            if (!valueMatch.Success)
            {
                return "1";
            }

            var diff = int.Parse(valueMatch.Groups[1].Value);

            var totalN = randomizer.RollOnce(100);

            var output = $"({input}) ＞ {totalN}";
            output += Result1d100Text(totalN, totalN, "<=", diff);

            var posNum = (totalN % 10) * 10 + totalN / 10;
            if (totalN >= 100)
            {
                posNum = 100;
            }

            if (totalN <= diff)
            {
                output += WhAtpos(posNum, posType);
            }

            return output;
        }
    }
}
