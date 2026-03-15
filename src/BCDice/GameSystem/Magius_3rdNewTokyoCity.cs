using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// MAGIUS:新世紀エヴァンゲリオンRPG 決戦！第3新東京市
    /// </summary>
    public sealed class Magius_3rdNewTokyoCity : GameSystemBase
    {
        public static readonly Magius_3rdNewTokyoCity Instance = new Magius_3rdNewTokyoCity();

        public override string Id => "Magius_3rdNewTokyoCity";
        public override string Name => "MAGIUS:新世紀エヴァンゲリオンRPG 決戦！第3新東京市";
        public override string SortKey => "まきうすしんせいきえうあんけりおんRPGけつせんたい3しんとうきようし";

        public override string HelpMessage => "■能力値判定　MA+x>=t        x:修正値 t:目標値\n" +
                "        例)MA>=7: ダイスを2個振って、その結果(成功,失敗,絶対成功,絶対失敗)を表示\n" +
                "\n" +
                "        ■技能値判定　MS+x>=t        x:修正値 t:目標値\n" +
                "        例)MS>=7: ダイスを3個振って、そのうち上位2つを採用し、結果(成功,失敗,絶対成功,絶対失敗)を表示";

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // TODO: Implement game-specific commands
            return null;
        }
    }
}