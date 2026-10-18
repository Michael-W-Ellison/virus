using System.Windows;
using System.Windows.Media;
using CoreColor = BiochemSimulator.Core.Primitives.Color;
using CorePoint = BiochemSimulator.Core.Primitives.Point;

namespace BiochemSimulator
{
    /// <summary>
    /// Conversions between the portable core primitives and their WPF
    /// equivalents. This is the single boundary where the Windows UI maps the
    /// framework-neutral types used by the simulation core onto WPF types.
    /// </summary>
    public static class WpfInterop
    {
        public static Color ToMediaColor(this CoreColor color)
            => Color.FromArgb(color.A, color.R, color.G, color.B);

        public static CorePoint ToCorePoint(this Point point)
            => new CorePoint(point.X, point.Y);

        public static Point ToWpfPoint(this CorePoint point)
            => new Point(point.X, point.Y);
    }
}
