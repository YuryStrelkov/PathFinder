using UnityEngine;

public static class Heuristics
{
    public static float ManhattanDistance2D(Point p1, Point p2) => Mathf.Abs(p1.row - p2.row) + Mathf.Abs(p1.col - p2.col);
    public static float ManhattanDistanceMin2D(Point p1, Point p2) => Mathf.Min(Mathf.Abs(p1.row - p2.row), Mathf.Abs(p1.col - p2.col));
    public static float ManhattanDistanceMax2D(Point p1, Point p2) => Mathf.Max(Mathf.Abs(p1.row - p2.row), Mathf.Abs(p1.col - p2.col));
    public static float DiagonalDistance2D(Point p1, Point p2)
    {
        float dx = Mathf.Abs(p1.row - p2.row);
        float dy = Mathf.Abs(p1.col - p2.col);
        return (dx + dy) - 0.414f * Mathf.Min(dx, dy);
    }
    public static float EuclideanDistanceSqr2D(Point p1, Point p2) => (p1.row - p2.row) * (p1.row - p2.row) + (p1.col - p2.col) * (p1.col - p2.col);
    public static float EuclideanDistance2D(Point p1, Point p2) => Mathf.Sqrt(EuclideanDistanceSqr2D(p1, p2));
}
