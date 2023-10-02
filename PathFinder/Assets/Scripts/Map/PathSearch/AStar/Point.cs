using System;

public struct Point : IEquatable<Point>
{
    public static readonly Point Zero = new Point(0, 0);

    public static readonly Point Ones = new Point(1, 1);

    public static readonly Point MiusOnes = new Point(-1, -1);

    public readonly short row, col;
    public Point(short _row, short _col)
    {
        row = _row;
        col = _col;
    }
    public static bool operator ==(Point point_a, Point point_b)
    {
        if (point_a.row != point_b.row) return false;
        if (point_a.col != point_b.col) return false;
        return true;
    }
    public static bool operator !=(Point point_a, Point point_b) => !(point_a == point_b);
    public static Point operator +(Point pt1, Point pt2) => new Point((short)(pt1.row + pt2.row), (short)(pt1.col + pt2.col));
    public static Point operator -(Point pt1, Point pt2) => new Point((short)(pt1.row + pt2.row), (short)(pt1.col + pt2.col));
    public static int Cross(Point p1, Point p2) => p1.row * p2.col - p2.row * p1.col;
    public override bool Equals(object obj)
    {
        if (!(obj is Point)) return false;
        return Equals((Point)obj);
    }
    public bool Equals(Point other)
    {
        if (row != other.row) return false;
        if (col != other.col) return false;
        return true;
    }
    public override string ToString()
    {
        return $"{{\"row\": {row}, \"col\": {col}}}";
    }
    public override int GetHashCode() => ((0x0000ffff & col) << 16) | (0x0000ffff & row);
    public static Point Unhash(int hash) => new Point((short)(hash & 0x0000ffff), (short)((hash >> 16) & 0x0000ffff));
}

public struct PointsPair : IEquatable<PointsPair>
{
    private readonly Point _start;
    private readonly Point _end;
    public static long HashPointsPair(Point p1, Point p2) 
    {
       return (((long)p1.GetHashCode() << 32) | (long)p2.GetHashCode());
    }
    public static void UnhashPointsPair(long hash, ref Point p1, ref Point p2)
    {
        p1 = Point.Unhash((int)((hash >> 32) & 0xffffffff));
        p2 = Point.Unhash((int)(hash & 0xffffffff));
    }

    public Point Start => _start;
    public Point End => _end;
    public PointsPair(Point start, Point end)
    {
        _start = start;
        _end = end;
    }
    public PointsPair Swap() => new PointsPair(End, Start);
    public override bool Equals(object other)
    {
        if (!(other is PointsPair)) return false;
        return Equals((PointsPair)other);
    }
    public bool Equals(PointsPair other)
    {
        if (!Start.Equals(other.Start)) return false;
        if (!End.Equals(other.End)) return false;
        return true;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(_start, _end);
    }
    public override string ToString()
    {
        return $"{{\n\"start\": {Start},\n\"end\"  :{End}\n}}";
    }
}