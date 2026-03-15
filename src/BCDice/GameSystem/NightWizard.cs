using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Arithmetic;
using BCDice.Command;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ナイトウィザード The 2nd Edition
    /// </summary>
    public sealed class NightWizard : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly NightWizard Instance = new NightWizard();

        /// <inheritdoc/>
        public override string Id => "NightWizard";

        /// <inheritdoc/>
        public override string Name => "ナイトウィザード The 2nd Edition";

        /// <inheritdoc/>
        public override string SortKey => "ないとういさあと2";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定用コマンド　(aNW+b@x#y$z+c)
        　　a : 基本値
        　　b : 常時に準じる特技による補正
        　　c : 常時以外の特技、および支援効果による補正（ファンブル時には適用されない）
        　　x : クリティカル値のカンマ区切り（省略時 10）
        　　y : ファンブル値のカンマ区切り（省略時 5）
        　　z : プラーナによる達成値補正のプラーナ消費数（ファンブル時には適用されない）
        　クリティカル値、ファンブル値が無い場合は1や13などのあり得ない数値を入れてください。
        　例）12NW-5@7#2$3 1NW 50nw+5@7,10#2,5 50nw-5+10@7,10#2,5+15+25
        ";

        private readonly string _nwCommand = "NW";

        // Instance state for roll_nw loop
        private int[] _criticalNumbers = Array.Empty<int>();
        private int[] _fumbleNumbers = Array.Empty<int>();
        private int _total;
        private string _interimExpr = "";
        private string? _status;

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var cmd = ParseNw(command) ?? Parse2r6(command);
            if (cmd == null)
            {
                return null;
            }

            var (total, interimExpr, status) = RollNw(cmd, randomizer);

            string? result = null;
            if (cmd.CmpOp != null)
            {
                result = CompareOperator(total, cmd.CmpOp.Value, cmd.TargetNumber ?? 0) ? "成功" : "失敗";
            }

            var sequence = new List<string?> { $"({cmd})", interimExpr, status, total.ToString(), result }
                .Where(x => x != null)
                .ToList();
            return Result.CreateBuilder(string.Join(" ＞ ", sequence)).Build();
        }

        private static bool CompareOperator(int left, CompareOperator op, int right) => op switch
        {
            Core.CompareOperator.Equal => left == right,
            Core.CompareOperator.NotEqual => left != right,
            Core.CompareOperator.LessThan => left < right,
            Core.CompareOperator.LessThanOrEqual => left <= right,
            Core.CompareOperator.GreaterThan => left > right,
            Core.CompareOperator.GreaterThanOrEqual => left >= right,
            _ => false,
        };

        private Parsed? ParseNw(string input)
        {
            var pattern = $@"^([-+]?\d+)?{_nwCommand}((?:[-+]\d+)+)?(?:@(\d+(?:,\d+)*))?(?:#(\d+(?:,\d+)*))?(?:\$(\d+))?((?:[-+]\d+)+)?(?:([>=]+)(\d+))?$";
            var m = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var command = new ParsedNW(_nwCommand);
            command.Base = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
            command.ModifyNumber = ArithmeticEvaluator.Eval(m.Groups[2].Value, RoundType) ?? 0;
            command.CriticalNumbers = m.Groups[3].Success
                ? m.Groups[3].Value.Split(',').Select(s => int.Parse(s)).ToArray()
                : new[] { 10 };
            command.FumbleNumbers = m.Groups[4].Success
                ? m.Groups[4].Value.Split(',').Select(s => int.Parse(s)).ToArray()
                : new[] { 5 };
            command.Prana = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : (int?)null;
            command.ActiveModifyNumber = ArithmeticEvaluator.Eval(m.Groups[6].Value, RoundType) ?? 0;
            command.CmpOp = Normalize.ComparisonOperator(m.Groups[7].Value);
            command.TargetNumber = m.Groups[8].Success ? int.Parse(m.Groups[8].Value) : (int?)null;
            return command;
        }

        private Parsed? Parse2r6(string input)
        {
            var m = Regex.Match(input, @"^2R6m\[([-+]?\d+(?:[-+]\d+)*)(?:,([-+]?\d+(?:[-+]\d+)*))?\](?:c\[(\d+(?:,\d+)*)\])?(?:f\[(\d+(?:,\d+)*)\])?(?:([>=]+)(\d+))?", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var command = new Parsed2R6();
            command.PassiveModifyNumber = ArithmeticEvaluator.Eval(m.Groups[1].Value, RoundType) ?? 0;
            command.ActiveModifyNumber = ArithmeticEvaluator.Eval(m.Groups[2].Value, RoundType) ?? 0;
            command.CriticalNumbers = m.Groups[3].Success
                ? m.Groups[3].Value.Split(',').Select(s => int.Parse(s)).ToArray()
                : new[] { 10 };
            command.FumbleNumbers = m.Groups[4].Success
                ? m.Groups[4].Value.Split(',').Select(s => int.Parse(s)).ToArray()
                : new[] { 5 };
            command.CmpOp = Normalize.ComparisonOperator(m.Groups[5].Value);
            command.TargetNumber = m.Groups[6].Success ? int.Parse(m.Groups[6].Value) : (int?)null;
            return command;
        }

        private (int total, string interimExpr, string? status) RollNw(Parsed parsed, IRandomizer randomizer)
        {
            _criticalNumbers = parsed.CriticalNumbers;
            _fumbleNumbers = parsed.FumbleNumbers;
            _total = 0;
            _interimExpr = "";
            _status = null;

            var rollStatus = RollOnce(true, randomizer);
            while (rollStatus == "critical")
            {
                rollStatus = RollOnce(false, randomizer);
            }

            if (rollStatus != "fumble" && parsed.Prana.HasValue)
            {
                var diceList = randomizer.RollBarabara(parsed.Prana.Value, 6);
                var pranaBonus = diceList.Sum();
                var pranaList = string.Join(",", diceList);
                _total += pranaBonus;
                _interimExpr += $"+{pranaBonus}[{pranaList}]";
            }

            int baseValue;
            if (rollStatus == "fumble")
            {
                baseValue = FumbleBaseNumber(parsed);
            }
            else
            {
                baseValue = parsed.GetPassiveModifyNumber() + parsed.ActiveModifyNumber;
            }

            _total += baseValue;
            _interimExpr = baseValue.ToString() + _interimExpr;

            return (_total, _interimExpr, _status);
        }

        private string? RollOnce(bool fumbleable, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(2, 6);
            var diceValue = diceList.Sum();
            var diceStr = string.Join(",", diceList);

            if (fumbleable && _fumbleNumbers.Contains(diceValue))
            {
                _total -= 10;
                _interimExpr += $"-10[{diceStr}]";
                _status = "ファンブル";
                return "fumble";
            }
            else if (_criticalNumbers.Contains(diceValue))
            {
                _total += 10;
                _interimExpr += $"+10[{diceStr}]";
                _status = "クリティカル";
                return "critical";
            }
            else
            {
                _total += diceValue;
                _interimExpr += $"+{diceValue}[{diceStr}]";
                return null;
            }
        }

        private int FumbleBaseNumber(Parsed parsed)
        {
            return parsed.GetPassiveModifyNumber();
        }

        /// <summary>
        /// 判定結果の基底クラス
        /// </summary>
        private abstract class Parsed
        {
            public int[] CriticalNumbers { get; set; } = new[] { 10 };
            public int[] FumbleNumbers { get; set; } = new[] { 5 };
            public int? Prana { get; set; }
            public int ActiveModifyNumber { get; set; }
            public CompareOperator? CmpOp { get; set; }
            public int? TargetNumber { get; set; }

            public abstract int GetPassiveModifyNumber();
            public abstract override string ToString();
        }

        private sealed class ParsedNW : Parsed
        {
            private readonly string _command;

            public int Base { get; set; }
            public int ModifyNumber { get; set; }

            public ParsedNW(string command)
            {
                _command = command;
            }

            public override int GetPassiveModifyNumber()
            {
                return Base + ModifyNumber;
            }

            public override string ToString()
            {
                var baseStr = Base == 0 ? null : Base.ToString();
                var modifyNumber = FormatModifier(ModifyNumber);
                var activeModifyNumber = FormatModifier(ActiveModifyNumber);
                var dollar = Prana.HasValue ? $"${Prana.Value}" : null;
                return $"{baseStr}{_command}{modifyNumber}@{string.Join(",", CriticalNumbers)}#{string.Join(",", FumbleNumbers)}{dollar}{activeModifyNumber}{CmpOpToString()}{TargetNumber}";
            }

            private string CmpOpToString()
            {
                return CmpOp switch
                {
                    Core.CompareOperator.GreaterThanOrEqual => ">=",
                    Core.CompareOperator.GreaterThan => ">",
                    Core.CompareOperator.Equal => "=",
                    Core.CompareOperator.LessThanOrEqual => "<=",
                    Core.CompareOperator.LessThan => "<",
                    Core.CompareOperator.NotEqual => "<>",
                    _ => "",
                };
            }

            private static string FormatModifier(int value)
            {
                if (value == 0)
                {
                    return "";
                }
                return value > 0 ? $"+{value}" : value.ToString();
            }
        }

        private sealed class Parsed2R6 : Parsed
        {
            public int PassiveModifyNumber { get; set; }

            public override int GetPassiveModifyNumber()
            {
                return PassiveModifyNumber;
            }

            public override string ToString()
            {
                return $"2R6M[{PassiveModifyNumber},{ActiveModifyNumber}]C[{string.Join(",", CriticalNumbers)}]F[{string.Join(",", FumbleNumbers)}]{CmpOpToString()}{TargetNumber}";
            }

            private string CmpOpToString()
            {
                return CmpOp switch
                {
                    Core.CompareOperator.GreaterThanOrEqual => ">=",
                    Core.CompareOperator.GreaterThan => ">",
                    Core.CompareOperator.Equal => "=",
                    Core.CompareOperator.LessThanOrEqual => "<=",
                    Core.CompareOperator.LessThan => "<",
                    Core.CompareOperator.NotEqual => "<>",
                    _ => "",
                };
            }
        }
    }
}
