using System;

namespace BiochemSimulator.Core.Primitives
{
    /// <summary>
    /// A framework-neutral 2D point. Mirrors the small surface of
    /// System.Windows.Point (double X/Y) that the simulation relies on, so the
    /// core carries no dependency on WPF. UI layers convert at their boundary.
    /// </summary>
    public struct Point : IEquatable<Point>
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Point other) => X == other.X && Y == other.Y;
        public override bool Equals(object? obj) => obj is Point other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public static bool operator ==(Point left, Point right) => left.Equals(right);
        public static bool operator !=(Point left, Point right) => !left.Equals(right);

        public override string ToString() => $"({X}, {Y})";
    }
}
