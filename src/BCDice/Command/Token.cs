using BCDice.Core;

namespace BCDice.Command
{
    /// <summary>
    /// 字句解析で得られるトークン
    /// </summary>
    public readonly struct Token
    {
        /// <summary>
        /// トークンの種類
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// トークンの文字列値
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// 数値トークンの場合の整数値
        /// </summary>
        public int NumberValue { get; }

        /// <summary>
        /// 比較演算子トークンの場合の演算子
        /// </summary>
        public CompareOperator? CompareOperator { get; }

        /// <summary>
        /// 識別子トークンを作成する
        /// </summary>
        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
            NumberValue = 0;
            CompareOperator = null;
        }

        /// <summary>
        /// 数値トークンを作成する
        /// </summary>
        public Token(int number)
        {
            Type = TokenType.Number;
            Value = number.ToString();
            NumberValue = number;
            CompareOperator = null;
        }

        /// <summary>
        /// 比較演算子トークンを作成する
        /// </summary>
        public Token(string value, CompareOperator compareOp)
        {
            Type = TokenType.CompareOp;
            Value = value;
            NumberValue = 0;
            CompareOperator = compareOp;
        }

        /// <summary>
        /// EOFトークン
        /// </summary>
        public static Token Eof => new Token(TokenType.Eof, "$");

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Token({Type}, \"{Value}\")";
        }
    }
}
