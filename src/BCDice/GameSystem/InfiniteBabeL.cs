using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 少年のための少女展爛会異聞 Infinite BabeL
    /// </summary>
    public sealed class InfiniteBabeL : GameSystemBase
    {
        public static readonly InfiniteBabeL Instance = new InfiniteBabeL();

        public override string Id => "InfiniteBabeL";
        public override string Name => "少年のための少女展爛会異聞 Infinite BabeL";
        public override string SortKey => "しようねんのためのしようしよてんらんかいいふんいんふいにつとはへる";

        public override string HelpMessage => "■ 一日の出来事表\n" +
                "          HOT  暑い日\n" +
                "          COLD  寒い日\n" +
                "          MORNING  朝\n" +
                "          NOON  昼\n" +
                "          AFTERNOON  昼下がり\n" +
                "          TWILIGHT  黄昏\n" +
                "          NIGHT  夜中\n" +
                "          MIDNIGHT  夜更け\n" +
                "\n" +
                "        ■ 場所の出来事表 -アバウト\n" +
                "          LARGE  広い場所\n" +
                "          ALLEY  路地\n" +
                "          STAIRS  階段\n" +
                "          COSY  居心地のいい場所\n" +
                "          SKY  空が見える場所\n" +
                "          BATH  浴室\n" +
                "          BOTANY  植物がある場所\n" +
                "          WAHER  水がある場所\n" +
                "          DIM  薄暗い場所\n" +
                "          FIRE  火がある場所\n" +
                "          LABO  ラボトラリー\n" +
                "          BED  寝室\n" +
                "\n" +
                "        ■ 場所の出来事表 -表立った場所\n" +
                "          CHIMNEY  煙突街\n" +
                "          LIBRARY  ライブラリー\n" +
                "          SLUM  スラム\n" +
                "          JUNKYARD  ジャンクヤード\n" +
                "          IROMACHI  イロマチ\n" +
                "          MONASTERY  修道院\n" +
                "          ROSE  ローズガーデン";

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // TODO: Implement game-specific commands
            return null;
        }
    }
}