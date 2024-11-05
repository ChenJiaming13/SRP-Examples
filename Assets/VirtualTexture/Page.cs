using System;

namespace VirtualTexture
{
    public class Page
    {
        public readonly int Mip;
        public readonly int Row;
        public readonly int Col;

        public Page(int mip, int row, int col)
        {
            Mip = mip;
            Row = row;
            Col = col;
        }

        public override bool Equals(object obj)
        {
            if (obj is Page other)
            {
                return Mip == other.Mip
                    && Row == other.Row
                    && Col == other.Col;
            }
            return false;
        }

        public bool Equals(Page other)
        {
            return Mip == other.Mip && Row == other.Row && Col == other.Col;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Mip, Row, Col);
        }

        public override string ToString()
        {
            return $"({Mip}-{Row}-{Col})";
        }
    }
}