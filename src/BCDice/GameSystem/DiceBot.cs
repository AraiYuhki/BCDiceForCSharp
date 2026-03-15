namespace BCDice.GameSystem
{
    /// <summary>
    /// 汎用ダイスボット
    /// 特殊なルールを持たない基本的なダイスロール機能を提供する
    /// </summary>
    public sealed class DiceBot : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly DiceBot Instance = new DiceBot();

        /// <inheritdoc/>
        public override string Id => "DiceBot";

        /// <inheritdoc/>
        public override string Name => "ダイスボット（指定なし）";

        /// <inheritdoc/>
        public override string SortKey => "*DiceBot";

        /// <inheritdoc/>
        public override string HelpMessage => @"
【ダイスボット】

・加算ダイス（xDn）
　例）2D6, 3D10+5, 2D6+3>=10

・上方ダイス / ボーナスダイス（xBn）
　複数のダイスを振り、最大値を採用
　例）2B6

・下方ダイス / ペナルティダイス（xRn）
　複数のダイスを振り、最小値を採用
　例）2R6

・D66ダイス
　例）D66, D66S（昇順ソート）

・選択コマンド
　例）choice[A,B,C]

・繰り返しコマンド
　例）x3 2D6, 3#2D6

・計算コマンド
　例）C(10+5*2)
";
    }
}
