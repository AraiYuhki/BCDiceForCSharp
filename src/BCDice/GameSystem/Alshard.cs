using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// アルシャード
    /// SRSベースのゲームシステム。ALコマンドが2D6のエイリアスとして使用可能。
    /// </summary>
    public sealed class Alshard : SRS
    {
        public new static readonly Alshard Instance = new Alshard();

        public override string Id => "Alshard";
        public override string Name => "アルシャード";
        public override string SortKey => "あるしやあと";

        protected override string[] Aliases => new[] { "AL" };

        public override string HelpMessage => @"・判定
　・通常判定：2D6+m@c#f>=t または 2D6+m>=t[c,f]
　　修正値m、目標値t、クリティカル値c、ファンブル値fで判定ロールを行います。
　　修正値、クリティカル値、ファンブル値は省略可能です（[]ごと省略可、@c・#fの指定は順不同）。
　　クリティカル値、ファンブル値の既定値は、それぞれ12、2です。
　　自動成功、自動失敗、成功、失敗を自動表示します。

　　例) 2d6>=10　　　　　修正値0、目標値10で判定
　　例) 2d6+2>=10　　　　修正値+2、目標値10で判定
　　例) AL+2>=10　　　　 2d6+2>=10と同じ（ALが2D6のショートカットコマンド）

　・クリティカルおよびファンブルのみの判定：2D6+m@c#f または 2D6+m[c,f]
　　例) AL　　　　　 2d6[]と同じ（ALが2D6のショートカットコマンド）
　　例) AL+2@12#4　　2d6+2@12#4と同じ（ALが2D6のショートカットコマンド）

・D66ダイスあり（入れ替えなし)
";
    }
}