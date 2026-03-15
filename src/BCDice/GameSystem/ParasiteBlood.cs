using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class ParasiteBlood : GameSystemBase
    {
        public static readonly ParasiteBlood Instance = new ParasiteBlood();

        public override string Id => "ParasiteBlood";
        public override string Name => "パラサイトブラッドRPG";
        public override string SortKey => "はらさいとふらつとRPG";
        public override string HelpMessage => @"・衝動表　(URGEx)
　""URGE衝動レベル""の形で指定します。
　衝動表に従って自動でダイスロールを行い、結果を表示します。
　ダイスロールと同様に、他のプレイヤーに隠れてロールすることも可能です。
　頭に識別文字を追加して、デフォルト以外の衝動表もロールできます。
　・AURGEx　頭に「A」を付けると「誤作動表」。
例）URGE1　　　urge5　　　Aurge2
・D66ダイスあり
";
    }
}