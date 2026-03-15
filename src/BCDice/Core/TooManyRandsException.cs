using System;

namespace BCDice.Core
{
    /// <summary>
    /// ダイスロール回数が上限を超えた場合にスローされる例外
    /// </summary>
    public class TooManyRandsException : Exception
    {
        /// <summary>
        /// デフォルトのエラーメッセージで例外を初期化します
        /// </summary>
        public TooManyRandsException()
            : base("ダイスロール回数が上限を超えました")
        {
        }

        /// <summary>
        /// 指定されたエラーメッセージで例外を初期化します
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        public TooManyRandsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 指定されたエラーメッセージと内部例外で例外を初期化します
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        /// <param name="innerException">内部例外</param>
        public TooManyRandsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
