using System.Collections.Generic;

namespace BCDice.Core
{
    /// <summary>
    /// ダイスロールの結果を表す不変クラス
    /// </summary>
    /// <remarks>
    /// コマンドの結果の文字列や、成功/失敗/クリティカル/ファンブルの情報を保持する。
    /// 成功/失敗は同時に発生しないこととする。
    /// 成功/失敗のペアとクリティカル、ファンブルの三者は独立した要素とし、
    /// 「クリティカルだが失敗」や「ファンブルだが成功でも失敗でもない」を許容する。
    /// </remarks>
    public sealed class Result
    {
        /// <summary>
        /// 結果テキスト
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// シークレットダイスかどうか
        /// </summary>
        public bool IsSecret { get; }

        /// <summary>
        /// 成功かどうか
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 失敗かどうか
        /// </summary>
        public bool IsFailure { get; }

        /// <summary>
        /// クリティカルかどうか
        /// </summary>
        public bool IsCritical { get; }

        /// <summary>
        /// ファンブルかどうか
        /// </summary>
        public bool IsFumble { get; }

        /// <summary>
        /// ダイスロールの履歴（値と面数のペア）
        /// </summary>
        public IReadOnlyList<(int Value, int Sides)>? Rands { get; }

        /// <summary>
        /// ダイスロールの詳細な履歴
        /// </summary>
        public IReadOnlyList<DetailedRandResult>? DetailedRands { get; }

        private Result(
            string text,
            bool isSecret,
            bool isSuccess,
            bool isFailure,
            bool isCritical,
            bool isFumble,
            IReadOnlyList<(int Value, int Sides)>? rands,
            IReadOnlyList<DetailedRandResult>? detailedRands)
        {
            Text = text;
            IsSecret = isSecret;
            IsSuccess = isSuccess;
            IsFailure = isFailure;
            IsCritical = isCritical;
            IsFumble = isFumble;
            Rands = rands;
            DetailedRands = detailedRands;
        }

        /// <summary>
        /// 成功の結果を作成する
        /// </summary>
        /// <param name="text">結果テキスト</param>
        /// <returns>成功を示すResult</returns>
        public static Result Success(string text)
        {
            return new Result(
                text: text,
                isSecret: false,
                isSuccess: true,
                isFailure: false,
                isCritical: false,
                isFumble: false,
                rands: null,
                detailedRands: null);
        }

        /// <summary>
        /// 失敗の結果を作成する
        /// </summary>
        /// <param name="text">結果テキスト</param>
        /// <returns>失敗を示すResult</returns>
        public static Result Failure(string text)
        {
            return new Result(
                text: text,
                isSecret: false,
                isSuccess: false,
                isFailure: true,
                isCritical: false,
                isFumble: false,
                rands: null,
                detailedRands: null);
        }

        /// <summary>
        /// クリティカル（成功を含む）の結果を作成する
        /// </summary>
        /// <param name="text">結果テキスト</param>
        /// <returns>クリティカルを示すResult</returns>
        public static Result Critical(string text)
        {
            return new Result(
                text: text,
                isSecret: false,
                isSuccess: true,
                isFailure: false,
                isCritical: true,
                isFumble: false,
                rands: null,
                detailedRands: null);
        }

        /// <summary>
        /// ファンブル（失敗を含む）の結果を作成する
        /// </summary>
        /// <param name="text">結果テキスト</param>
        /// <returns>ファンブルを示すResult</returns>
        public static Result Fumble(string text)
        {
            return new Result(
                text: text,
                isSecret: false,
                isSuccess: false,
                isFailure: true,
                isCritical: false,
                isFumble: true,
                rands: null,
                detailedRands: null);
        }

        /// <summary>
        /// 結果のビルダーを作成する
        /// </summary>
        /// <param name="text">結果テキスト</param>
        /// <returns>Resultのビルダー</returns>
        public static Builder CreateBuilder(string text)
        {
            return new Builder(text);
        }

        /// <summary>
        /// Resultを構築するためのビルダークラス
        /// </summary>
        public sealed class Builder
        {
            private readonly string _text;
            private bool _isSecret;
            private bool _isSuccess;
            private bool _isFailure;
            private bool _isCritical;
            private bool _isFumble;
            private IReadOnlyList<(int Value, int Sides)>? _rands;
            private IReadOnlyList<DetailedRandResult>? _detailedRands;

            internal Builder(string text)
            {
                _text = text;
            }

            /// <summary>
            /// シークレットフラグを設定する
            /// </summary>
            public Builder SetSecret(bool isSecret)
            {
                _isSecret = isSecret;
                return this;
            }

            /// <summary>
            /// 成功フラグを設定する
            /// </summary>
            public Builder SetSuccess(bool isSuccess)
            {
                _isSuccess = isSuccess;
                return this;
            }

            /// <summary>
            /// 失敗フラグを設定する
            /// </summary>
            public Builder SetFailure(bool isFailure)
            {
                _isFailure = isFailure;
                return this;
            }

            /// <summary>
            /// クリティカルフラグを設定する
            /// </summary>
            public Builder SetCritical(bool isCritical)
            {
                _isCritical = isCritical;
                return this;
            }

            /// <summary>
            /// ファンブルフラグを設定する
            /// </summary>
            public Builder SetFumble(bool isFumble)
            {
                _isFumble = isFumble;
                return this;
            }

            /// <summary>
            /// 条件に基づいて成功/失敗を設定する
            /// </summary>
            /// <param name="condition">trueなら成功、falseなら失敗</param>
            public Builder SetCondition(bool condition)
            {
                _isSuccess = condition;
                _isFailure = !condition;
                return this;
            }

            /// <summary>
            /// ダイスロール履歴を設定する
            /// </summary>
            public Builder SetRands(IReadOnlyList<(int Value, int Sides)>? rands)
            {
                _rands = rands;
                return this;
            }

            /// <summary>
            /// 詳細なダイスロール履歴を設定する
            /// </summary>
            public Builder SetDetailedRands(IReadOnlyList<DetailedRandResult>? detailedRands)
            {
                _detailedRands = detailedRands;
                return this;
            }

            /// <summary>
            /// Resultを構築する
            /// </summary>
            /// <returns>構築されたResult</returns>
            public Result Build()
            {
                return new Result(
                    _text,
                    _isSecret,
                    _isSuccess,
                    _isFailure,
                    _isCritical,
                    _isFumble,
                    _rands,
                    _detailedRands);
            }
        }
    }
}
