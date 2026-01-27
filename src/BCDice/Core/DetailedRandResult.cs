using System;

namespace BCDice.Core
{
    /// <summary>
    /// ダイスロールの詳細な結果を表す構造体
    /// </summary>
    public readonly struct DetailedRandResult : IEquatable<DetailedRandResult>
    {
        /// <summary>
        /// ダイスロールの種類
        /// </summary>
        public RandResultKind Kind { get; }

        /// <summary>
        /// ダイスの面数
        /// </summary>
        public int Sides { get; }

        /// <summary>
        /// ダイスロールの結果値
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// DetailedRandResultを初期化する
        /// </summary>
        /// <param name="kind">ダイスロールの種類</param>
        /// <param name="sides">ダイスの面数</param>
        /// <param name="value">ダイスロールの結果値</param>
        public DetailedRandResult(RandResultKind kind, int sides, int value)
        {
            Kind = kind;
            Sides = sides;
            Value = value;
        }

        /// <inheritdoc/>
        public bool Equals(DetailedRandResult other)
        {
            return Kind == other.Kind && Sides == other.Sides && Value == other.Value;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is DetailedRandResult other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Kind.GetHashCode();
                hash = hash * 31 + Sides.GetHashCode();
                hash = hash * 31 + Value.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// 等価演算子
        /// </summary>
        public static bool operator ==(DetailedRandResult left, DetailedRandResult right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 非等価演算子
        /// </summary>
        public static bool operator !=(DetailedRandResult left, DetailedRandResult right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"DetailedRandResult {{ Kind = {Kind}, Sides = {Sides}, Value = {Value} }}";
        }
    }
}
