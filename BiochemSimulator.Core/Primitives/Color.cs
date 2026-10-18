using System;

namespace BiochemSimulator.Core.Primitives
{
    /// <summary>
    /// A framework-neutral ARGB color. Mirrors the small surface of
    /// System.Windows.Media.Color used by the simulation (byte channels,
    /// FromRgb/FromArgb) so the core needs no WPF reference. UI layers convert
    /// to their own color type at the boundary.
    /// </summary>
    public struct Color : IEquatable<Color>
    {
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public static Color FromArgb(byte a, byte r, byte g, byte b)
            => new Color { A = a, R = r, G = g, B = b };

        public static Color FromRgb(byte r, byte g, byte b)
            => new Color { A = 255, R = r, G = g, B = b };

        public bool Equals(Color other)
            => A == other.A && R == other.R && G == other.G && B == other.B;
        public override bool Equals(object? obj) => obj is Color other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(A, R, G, B);
        public static bool operator ==(Color left, Color right) => left.Equals(right);
        public static bool operator !=(Color left, Color right) => !left.Equals(right);

        public override string ToString() => $"#{A:X2}{R:X2}{G:X2}{B:X2}";
    }

    /// <summary>
    /// Named colors matching the WPF/standard ARGB values the simulation used,
    /// so colors are visually identical after the core was made portable.
    /// </summary>
    public static class Colors
    {
        public static Color Beige => Color.FromRgb(0xF5, 0xF5, 0xDC);
        public static Color Black => Color.FromRgb(0x00, 0x00, 0x00);
        public static Color Blue => Color.FromRgb(0x00, 0x00, 0xFF);
        public static Color Brown => Color.FromRgb(0xA5, 0x2A, 0x2A);
        public static Color Cyan => Color.FromRgb(0x00, 0xFF, 0xFF);
        public static Color Gray => Color.FromRgb(0x80, 0x80, 0x80);
        public static Color Green => Color.FromRgb(0x00, 0x80, 0x00);
        public static Color LightBlue => Color.FromRgb(0xAD, 0xD8, 0xE6);
        public static Color LightCyan => Color.FromRgb(0xE0, 0xFF, 0xFF);
        public static Color LightGoldenrodYellow => Color.FromRgb(0xFA, 0xFA, 0xD2);
        public static Color LightGray => Color.FromRgb(0xD3, 0xD3, 0xD3);
        public static Color LightGreen => Color.FromRgb(0x90, 0xEE, 0x90);
        public static Color LightPink => Color.FromRgb(0xFF, 0xB6, 0xC1);
        public static Color LightYellow => Color.FromRgb(0xFF, 0xFF, 0xE0);
        public static Color Orange => Color.FromRgb(0xFF, 0xA5, 0x00);
        public static Color PaleGoldenrod => Color.FromRgb(0xEE, 0xE8, 0xAA);
        public static Color PaleTurquoise => Color.FromRgb(0xAF, 0xEE, 0xEE);
        public static Color Pink => Color.FromRgb(0xFF, 0xC0, 0xCB);
        public static Color Purple => Color.FromRgb(0x80, 0x00, 0x80);
        public static Color Red => Color.FromRgb(0xFF, 0x00, 0x00);
        public static Color Silver => Color.FromRgb(0xC0, 0xC0, 0xC0);
        public static Color Transparent => Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF);
        public static Color Violet => Color.FromRgb(0xEE, 0x82, 0xEE);
        public static Color White => Color.FromRgb(0xFF, 0xFF, 0xFF);
        public static Color Yellow => Color.FromRgb(0xFF, 0xFF, 0x00);
        public static Color YellowGreen => Color.FromRgb(0x9A, 0xCD, 0x32);
    }
}
