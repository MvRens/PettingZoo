using System;

namespace PettingZoo.UI
{
    public readonly struct TextPosition : IEquatable<TextPosition>
    {
        public int Row { get; }
        public int Column { get; }


        public TextPosition(int row, int column)
        {
            Row = row;
            Column = column;
        }


        public bool Equals(TextPosition other)
        {
            return Row == other.Row && Column == other.Column;
        }

        public override bool Equals(object? obj)
        {
            return obj is TextPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column);
        }

        public static bool operator ==(TextPosition left, TextPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextPosition left, TextPosition right)
        {
            return !(left == right);
        }
    }
}
