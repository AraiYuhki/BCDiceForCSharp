using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// YearZeroEngine
    /// </summary>
    public sealed class YearZeroEngine : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly YearZeroEngine Instance = new YearZeroEngine();

        /// <inheritdoc/>
        public override string Id => "YearZeroEngine";

        /// <inheritdoc/>
        public override string Name => "YearZeroEngine";

        /// <inheritdoc/>
        public override string SortKey => "いやあせろえんしん";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・ダイスプール判定コマンド(nYZEx+x+x+m)
          (難易度)YZE(能力ダイス数)+(技能ダイス数)+(アイテムダイス数)+(修正値)  # (6のみを数える)
          (難易度)YZE(能力ダイス数)+(技能ダイス数)+(アイテムダイス数)-(修正値)  # (6のみを数える)

        ・ダイスプール判定コマンド(nMYZx+x+x)
          (難易度)MYZ(能力ダイス数)+(技能ダイス数)+(アイテムダイス数)  # (1と6を数え、プッシュ可能数を表示)
          (難易度)MYZ(能力ダイス数)-(技能ダイス数)+(アイテムダイス数)  # (1と6を数え、プッシュ可能数を表示、技能のマイナス指定)

          ※ 難易度と技能、アイテムダイス数は省略可能

        ・ステップダイス判定コマンド(nYZSx+x+m+f)
          (難易度)YZS(能力ダイス面数)+(技能ダイス面数)+(修正値)   # (1,6を数え、プッシュ可能数を表示)
          (難易度)YZS(能力ダイス面数)+(技能ダイス面数)-(修正値)   # (1,6を数え、プッシュ可能数を表示)
          (難易度)YZS(能力ダイス面数)+(技能ダイス面数)+(修正値)A  # (1,6を数え、プッシュ可能数を表示、有利)
          (難易度)YZS(能力ダイス面数)+(技能ダイス面数)-(修正値)A  # (1,6を数え、プッシュ可能数を表示、有利)
          (難易度)YZS(能力ダイス面数)+(技能ダイス面数)+(修正値)D  # (1,6を数え、プッシュ可能数を表示、不利)
          (難易度)YZS(能力ダイス面数)+(技能ダイス面数)-(修正値)D  # (1,6を数え、プッシュ可能数を表示、不利)
        ";

        // インデックス定数
        private const int DIFFICULTY_INDEX = 1;
        private const int COMMAND_TYPE_INDEX = 2;
        private const int ABILITY_INDEX = 3;
        private const int SKILL_SIGNED_INDEX = 5;
        private const int SKILL_INDEX = 6;
        private const int GEAR_INDEX = 8;
        private const int MODIFIER_SIGNED_INDEX = 10;
        private const int MODIFIER_INDEX = 11;

        // インスタンス変数
        private int _totalSuccessDice;
        private int _totalBotchDice;
        private int _baseBotchDice;
        private int _skillBotchDice;
        private int _gearBotchDice;
        private int _pushDice;
        private int _difficulty;

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return ResoluteAction(command, randomizer)
                ?? ResolutePushAction(command, randomizer)
                ?? ResoluteStepAction(command, randomizer);
        }

        private void DiceInfoInit()
        {
            _totalSuccessDice = 0;
            _totalBotchDice = 0;
            _baseBotchDice = 0;
            _skillBotchDice = 0;
            _gearBotchDice = 0;
            _pushDice = 0;
            _difficulty = 0;
        }

        private Result? ResoluteAction(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"\A(\d+)?(YZE)(\d+)((\+)(\d+))?(\+(\d+))?((\+|-)(\d+))?");
            if (!m.Success)
            {
                return null;
            }

            DiceInfoInit();

            _difficulty = ToInt(m.Groups[DIFFICULTY_INDEX].Value);
            var attribute = ToInt(m.Groups[ABILITY_INDEX].Value);
            var skill = ToInt(m.Groups[SKILL_INDEX].Value);
            var gear = ToInt(m.Groups[GEAR_INDEX].Value);
            var modifier = ToInt(m.Groups[MODIFIER_INDEX].Value);

            if (m.Groups[MODIFIER_SIGNED_INDEX].Value == "-")
            {
                if (skill >= modifier)
                {
                    skill -= modifier;
                }
                else
                {
                    modifier -= skill;
                    skill = 0;
                    if (gear >= modifier)
                    {
                        gear -= modifier;
                    }
                    else
                    {
                        modifier -= gear;
                        gear = 0;
                        if (attribute >= modifier)
                        {
                            attribute -= modifier;
                        }
                        else
                        {
                            attribute = 0;
                        }
                    }
                }
            }
            else
            {
                skill += modifier;
            }

            _totalSuccessDice = 0;

            var dicePool = attribute;
            var (abilityDiceText, successDice, botchDice) = MakeDiceRoll(dicePool, randomizer);

            _totalSuccessDice += successDice;
            _totalBotchDice += botchDice;
            _baseBotchDice += botchDice;
            _pushDice += (dicePool - (successDice + botchDice));

            var diceCountText = $"({dicePool}D6)";
            var diceText = abilityDiceText;

            if (!string.IsNullOrEmpty(m.Groups[SKILL_INDEX].Value))
            {
                dicePool = skill;
                string skillDiceText;
                (skillDiceText, successDice, botchDice) = MakeDiceRoll(dicePool, randomizer);

                _totalSuccessDice += successDice;
                _totalBotchDice += botchDice;
                _skillBotchDice += botchDice;
                _pushDice += (dicePool - successDice);

                diceCountText += $"+({dicePool}D6)";
                diceText += $"+{skillDiceText}";
            }

            if (!string.IsNullOrEmpty(m.Groups[GEAR_INDEX].Value))
            {
                dicePool = gear;
                string gearDiceText;
                (gearDiceText, successDice, botchDice) = MakeDiceRoll(dicePool, randomizer);

                _totalSuccessDice += successDice;
                _totalBotchDice += botchDice;
                _gearBotchDice += botchDice;
                _pushDice += (dicePool - (successDice + botchDice));

                diceCountText += $"+({dicePool}D6)";
                diceText += $"+{gearDiceText}";
            }

            return MakeResultWithYze(diceCountText, diceText);
        }

        private Result? ResolutePushAction(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"\A(\d+)?(MYZ)(\d+)((\+|-)(\d+))?(\+(\d+))?");
            if (!m.Success)
            {
                return null;
            }

            DiceInfoInit();

            _difficulty = ToInt(m.Groups[DIFFICULTY_INDEX].Value);
            _totalSuccessDice = 0;

            var dicePool = ToInt(m.Groups[ABILITY_INDEX].Value);
            var (abilityDiceText, successDice, botchDice) = MakeDiceRoll(dicePool, randomizer);

            _totalSuccessDice += successDice;
            _totalBotchDice += botchDice;
            _baseBotchDice += botchDice;
            _pushDice += (dicePool - (successDice + botchDice));

            var diceCountText = $"({dicePool}D6)";
            var diceText = abilityDiceText;

            if (!string.IsNullOrEmpty(m.Groups[SKILL_INDEX].Value))
            {
                dicePool = ToInt(m.Groups[SKILL_INDEX].Value);
                string skillDiceText;
                (skillDiceText, successDice, botchDice) = MakeDiceRoll(dicePool, randomizer);

                var skillUnsigned = m.Groups[SKILL_SIGNED_INDEX].Value;
                if (skillUnsigned == "-")
                {
                    _totalSuccessDice -= successDice;
                }
                else
                {
                    _totalSuccessDice += successDice;
                }

                _totalBotchDice += botchDice;
                _skillBotchDice += botchDice;
                _pushDice += (dicePool - successDice);

                diceCountText += $"{skillUnsigned}({dicePool}D6)";
                diceText += $"{skillUnsigned}{skillDiceText}";
            }

            if (!string.IsNullOrEmpty(m.Groups[GEAR_INDEX].Value))
            {
                dicePool = ToInt(m.Groups[GEAR_INDEX].Value);
                string gearDiceText;
                (gearDiceText, successDice, botchDice) = MakeDiceRoll(dicePool, randomizer);

                _totalSuccessDice += successDice;
                _totalBotchDice += botchDice;
                _gearBotchDice += botchDice;
                _pushDice += (dicePool - (successDice + botchDice));

                diceCountText += $"+({dicePool}D6)";
                diceText += $"+{gearDiceText}";
            }

            return MakeResultWithMyz(diceCountText, diceText);
        }

        private Result MakeResultWithYze(string diceCountText, string diceText)
        {
            var resultText = $"{diceCountText} ＞ {diceText} 成功数:{_totalSuccessDice}";
            if (_difficulty > 0)
            {
                if (_totalSuccessDice >= _difficulty)
                {
                    return Result.CreateBuilder($"{resultText} 難易度={_difficulty}:判定成功！").SetSuccess(true).Build();
                }
                else
                {
                    return Result.CreateBuilder($"{resultText} 難易度={_difficulty}:判定失敗！").SetFailure(true).Build();
                }
            }
            return Result.CreateBuilder(resultText).Build();
        }

        private Result MakeResultWithMyz(string diceCountText, string diceText)
        {
            var resultText = $"{diceCountText} ＞ {diceText} 成功数:{_totalSuccessDice}";
            var atterText = $"\n出目1：[能力：{_baseBotchDice},技能：{_skillBotchDice},アイテム：{_gearBotchDice}) プッシュ可能={_pushDice}ダイス";
            if (_difficulty > 0)
            {
                if (_totalSuccessDice >= _difficulty)
                {
                    return Result.CreateBuilder($"{resultText} 難易度={_difficulty}:判定成功！{atterText}").SetSuccess(true).Build();
                }
                else
                {
                    return Result.CreateBuilder($"{resultText} 難易度={_difficulty}:判定失敗！{atterText}").SetFailure(true).Build();
                }
            }
            return Result.CreateBuilder($"{resultText}{atterText}").Build();
        }

        private (string diceText, int successDice, int botchDice) MakeDiceRoll(int dicePool, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(dicePool, 6);
            var successDice = diceList.Count(d => d == 6);
            var botchDice = diceList.Count(d => d == 1);
            return ($"[{string.Join(",", diceList)}]", successDice, botchDice);
        }

        private (string diceText, int botchDice) MakeDiceARoll(int count, int type, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(count, type);
            var botchDice = diceList.Count(d => d == 1);
            var successDice = diceList.Count(val => val >= 6);
            var successLevel = successDice + diceList.Count(val => val >= 10);

            _totalSuccessDice += successLevel;
            _totalBotchDice += botchDice;
            _pushDice += (count - (successDice + botchDice));

            return ($"[{string.Join(",", diceList)}]", botchDice);
        }

        private (int diceType1, int diceType2) GetRollingDice(int diceType1, int diceType2, int diceUpgrade)
        {
            if (diceType1 < 4) diceType1 = 4;
            if (diceType2 < 4) diceType2 = 4;

            while (diceUpgrade > 0)
            {
                if (diceType1 >= diceType2)
                {
                    if (diceType2 < 12) diceType2 += 2;
                }
                else
                {
                    if (diceType1 < 12) diceType1 += 2;
                }
                diceUpgrade -= 1;
            }

            while (diceUpgrade < 0)
            {
                if (diceType1 <= diceType2)
                {
                    if (diceType2 > 4) diceType2 -= 2;
                }
                else
                {
                    if (diceType1 > 4) diceType1 -= 2;
                }
                diceUpgrade += 1;
            }

            if (diceType1 == 4 && diceType2 == 4)
            {
                diceType1 = 6;
            }

            return (diceType1, diceType2);
        }

        private Result? ResoluteStepAction(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"\A(\d+)?(YZS)(\d+)((\+)(\d+))?((\+|-)(\d+))?(A|D)?");
            if (!m.Success)
            {
                return null;
            }

            DiceInfoInit();

            _difficulty = ToInt(m.Groups[DIFFICULTY_INDEX].Value);
            var attribute = ToInt(m.Groups[ABILITY_INDEX].Value);
            var skill = ToInt(m.Groups[SKILL_INDEX].Value);
            var modifier = ToInt(m.Groups[7].Value);
            var advantage = m.Groups[10].Value;

            var diceCountText = "";
            var diceText = "";

            var (diceType1, diceType2) = GetRollingDice(attribute, skill, modifier);

            if (diceType1 <= diceType2)
            {
                if (!string.IsNullOrEmpty(advantage))
                {
                    if (advantage == "A" && diceType1 > 4)
                    {
                        var (abilityDiceText, botchDice) = MakeDiceARoll(2, diceType1, randomizer);
                        _baseBotchDice += botchDice;
                        diceCountText = $"(2D{diceType1})";
                        diceText = abilityDiceText;
                    }
                }
                else
                {
                    if (diceType1 > 4)
                    {
                        var (abilityDiceText, botchDice) = MakeDiceARoll(1, diceType1, randomizer);
                        _baseBotchDice += botchDice;
                        diceCountText = $"(1D{diceType1})";
                        diceText = abilityDiceText;
                    }
                }

                if (diceType2 > 4)
                {
                    var (skillDiceText, botchDice) = MakeDiceARoll(1, diceType2, randomizer);
                    _skillBotchDice += botchDice;
                    if (diceCountText != "") diceCountText += "+";
                    if (diceText != "") diceText += "+";
                    diceCountText += $"(1D{diceType2})";
                    diceText += skillDiceText;
                }
            }
            else
            {
                if (diceType1 > 4)
                {
                    var (abilityDiceText, botchDice) = MakeDiceARoll(1, diceType1, randomizer);
                    _baseBotchDice += botchDice;
                    diceCountText = $"(1D{diceType1})";
                    diceText = abilityDiceText;
                }

                if (!string.IsNullOrEmpty(advantage))
                {
                    if (advantage == "A" && diceType2 > 4)
                    {
                        var (skillDiceText, botchDice) = MakeDiceARoll(2, diceType2, randomizer);
                        _skillBotchDice += botchDice;
                        diceCountText += $"+(2D{diceType2})";
                        diceText += $"+{skillDiceText}";
                    }
                }
                else
                {
                    if (diceType2 > 4)
                    {
                        var (skillDiceText, botchDice) = MakeDiceARoll(1, diceType2, randomizer);
                        _skillBotchDice += botchDice;
                        diceCountText += $"+(1D{diceType2})";
                        diceText += $"+{skillDiceText}";
                    }
                }
            }

            return MakeResultWithMyz(diceCountText, diceText);
        }

        private static int ToInt(string? value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            return int.TryParse(value, out var result) ? result : 0;
        }
    }
}
