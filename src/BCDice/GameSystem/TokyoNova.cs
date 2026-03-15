using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// トーキョーN◎VA
    /// </summary>
    public sealed class TokyoNova : GameSystemBase
    {
        public static readonly TokyoNova Instance = new TokyoNova();

        public override string Id => "TokyoNova";
        public override string Name => "トーキョーN◎VA";
        public override string SortKey => "とおきよおのは";

        public override string HelpMessage => @"※このダイスボットは部屋のシステム名表示用となります。
";
    }
}