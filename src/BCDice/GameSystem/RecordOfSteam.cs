using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class RecordOfSteam : GameSystemBase
    {
        public static readonly RecordOfSteam Instance = new RecordOfSteam();

        public override string Id => "RecordOfSteam";
        public override string Name => "Record of Steam";
        public override string SortKey => "\u308c\u3053\u304a\u3068\u304a\u3075\u3059\u3061\u3044\u3080";

        private static readonly Regex SReg = new Regex(
            @"^(\d+)[sS](\d+)(@(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = SReg.Match(command);
            if (!m.Success) return null;

            int diceCount = int.Parse(m.Groups[1].Value);
            int targetNumber = int.Parse(m.Groups[2].Value);
            int criticalValue = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 1;

            if (diceCount >= 150)
                return Result.Success("(\u591a\u5206)\u7121\u9650\u500b\u306a\u306e\u3067\u632f\u308c\u307e\u305b\u3093\uff01 \u30e4\u30e1\u30c6\u30af\u30c0\u30b5\u30a4\u3001(\u30d7\u30ed\u30bb\u30b9\u304c)\u6b7b\u3093\u3067\u3057\u307e\u3044\u307e\u3059\u3063");
            if (criticalValue >= 3)
                return Result.Success("(\u591a\u5206)\u7121\u9650\u500b\u306a\u306e\u3067\u632f\u308c\u307e\u305b\u3093\uff01 \u30e4\u30e1\u30c6\u30af\u30c0\u30b5\u30a4\u3001(\u30d7\u30ed\u30bb\u30b9\u304c)\u6b7b\u3093\u3067\u3057\u307e\u3044\u307e\u3059\u3063");

            int specialValue = criticalValue;
            var (rollResult, successCount, roundCount, specialCount, fumbleCount) =
                GetDiceRollResult(diceCount, targetNumber, criticalValue, specialValue, randomizer);

            string output = "(" + command + ") \uff1e " + rollResult;
            string roundCountText = GetRoundCountText(roundCount);
            string successText = GetSuccessText(successCount);
            string specialText = GetSpecialText(specialCount);
            string fumbleText = GetFumbleText(fumbleCount);
            string text = output + roundCountText + specialText + successText + fumbleText;
            return Result.Success(text);
        }

        private static (string rollResult, int successCount, int roundCount, int specialCount, int fumbleCount)
            GetDiceRollResult(int diceCount, int targetNumber, int criticalValue, int specialValue, IRandomizer randomizer)
        {
            int successCount = 0;
            int roundCount = 0;
            string rollResult = "";
            int specialCount = 0;
            bool specialFlag = false;
            int fumbleCount = 0;
            bool fumbleFlag = false;

            while (diceCount > 0)
            {
                var diceList = randomizer.RollBarabara(diceCount, 6);
                string diceListText = string.Join(",", diceList);
                if (rollResult != "") rollResult += ",";
                rollResult += diceListText;

                var distinct = diceList.Distinct().ToList();
                if (distinct.Count == 1 && roundCount == 0)
                {
                    if (distinct[0] <= specialValue)
                        specialFlag = true;
                    else if (distinct[0] == 6)
                        fumbleFlag = true;
                }

                if (specialFlag)
                {
                    specialCount = 1;
                    successCount = diceCount * 3;
                    return (rollResult, successCount, roundCount, specialCount, fumbleCount);
                }
                if (fumbleFlag)
                {
                    fumbleCount = 1;
                    return (rollResult, successCount, roundCount, specialCount, fumbleCount);
                }

                diceCount = 0;
                foreach (int diceValue in diceList)
                {
                    if (diceValue <= criticalValue)
                    {
                        diceCount += 2;
                        roundCount += 1;
                    }
                    if (diceValue <= targetNumber)
                        successCount += 1;
                }
            }

            return (rollResult, successCount, roundCount, specialCount, fumbleCount);
        }

        private static string GetRoundCountText(int roundCount)
        {
            return roundCount <= 0 ? "" : " \uff1e " + roundCount + "\u56de\u8ee2";
        }

        private static string GetSuccessText(int successCount)
        {
            return successCount > 0 ? " \uff1e \u6210\u529f\u6570" + successCount : " \uff1e \u5931\u6557";
        }

        private static string GetSpecialText(int specialCount)
        {
            return specialCount == 1 ? " \uff1e \u30b9\u30da\u30b7\u30e3\u30eb" : "";
        }

        private static string GetFumbleText(int fumbleCount)
        {
            return fumbleCount == 1 ? " \uff1e \u30d5\u30a1\u30f3\u30d6\u30eb" : "";
        }
    }
}
