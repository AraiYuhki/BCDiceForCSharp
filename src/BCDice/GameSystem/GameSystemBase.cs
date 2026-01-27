using System.Collections.Generic;
using BCDice.CommonCommand;
using BCDice.CommonCommand.AddDice;
using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ゲームシステムの基底クラス
    /// </summary>
    public abstract class GameSystemBase : IGameSystem, IGameSystemContext
    {
        /// <inheritdoc/>
        public abstract string Id { get; }

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public virtual string SortKey => Name;

        /// <inheritdoc/>
        public virtual string HelpMessage => "";

        /// <inheritdoc/>
        public virtual RoundType RoundType => RoundType.Floor;

        /// <inheritdoc/>
        public virtual D66SortType D66SortType => D66SortType.NoSort;

        /// <summary>
        /// 暗黙のダイス面数（デフォルト: 6）
        /// </summary>
        public virtual int SidesImplicitD => 6;

        /// <summary>
        /// 共通コマンドのリスト
        /// </summary>
        protected virtual IReadOnlyList<ICommonCommand> CommonCommands { get; } = new ICommonCommand[]
        {
            CalcCommand.Instance,
            ChoiceCommand.Instance,
            D66Command.Instance,
            RepeatCommand.Instance,
            UpperDiceCommand.Instance,
            LowerDiceCommand.Instance,
            CountSuccessCommand.Instance,
            AddDiceCommand.Instance,
        };

        /// <inheritdoc/>
        public Result? Eval(string command)
        {
            return Eval(command, new Randomizer());
        }

        /// <inheritdoc/>
        public Result? Eval(string command, IRandomizer randomizer)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return null;
            }

            // コマンドを前処理
            string normalizedCommand = NormalizeCommand(command);

            // ゲームシステム固有コマンドを先に試す
            var result = EvalGameSystemSpecificCommand(normalizedCommand, randomizer);
            if (result != null)
            {
                return result;
            }

            // 共通コマンドを試す
            return EvalCommonCommand(normalizedCommand, randomizer);
        }

        /// <summary>
        /// ゲームシステム固有のコマンドを評価する
        /// </summary>
        /// <param name="command">正規化されたコマンド</param>
        /// <param name="randomizer">乱数生成器</param>
        /// <returns>評価結果、認識されない場合はnull</returns>
        protected virtual Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return null;
        }

        /// <summary>
        /// 共通コマンドを評価する
        /// </summary>
        /// <param name="command">正規化されたコマンド</param>
        /// <param name="randomizer">乱数生成器</param>
        /// <returns>評価結果、認識されない場合はnull</returns>
        protected virtual Result? EvalCommonCommand(string command, IRandomizer randomizer)
        {
            foreach (var commonCommand in CommonCommands)
            {
                var result = commonCommand.Eval(command, this, randomizer);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// コマンドを正規化する
        /// </summary>
        /// <param name="command">入力コマンド</param>
        /// <returns>正規化されたコマンド</returns>
        protected virtual string NormalizeCommand(string command)
        {
            // 先頭・末尾の空白を除去し、大文字に変換
            string normalized = command.Trim().ToUpperInvariant();

            // ゲームシステム固有のテキスト変換を適用
            normalized = ChangeText(normalized);

            return normalized;
        }

        /// <inheritdoc/>
        public virtual string ChangeText(string text)
        {
            return text;
        }
    }
}
