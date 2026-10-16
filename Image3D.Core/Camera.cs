using System.Drawing;

namespace Image3D;

public class Camera
{
    static readonly double K = Math.Sqrt(2) * Math.Cos(Math.PI / 6);
    double fitScale;

    public double Rotation { get; set; }
    public double Elevation { get; set; } = Math.Asin(Math.Tan(Math.PI / 6));
    public bool TopView { get; set; }
    public double ZScale { get; set; } = 1;
    public double Scale { get; set; } = 1;
    public double OffsetX { get; set; }
    public double OffsetY { get; set; }
    public double CenterX { get; set; }
    public double CenterY { get; set; }

    public (double X, double Y) Project(double x, double y, double z)
    {
        var (px, py) = View(x, y, z, TopView);
        return (px * Scale + OffsetX, py * Scale + OffsetY);
    }

    public void Fit(HeightMap map, double width, double height, double top = 0, bool keepZoom = false)
    {
        if (width <= 0 || height <= 0) return;
        bool keep = keepZoom && fitScale != 0;
        double zoom = keep ? Scale / fitScale : 1, x = OffsetX, y = OffsetY;
        CenterX = (map.Cols - 1) / 2.0;
        CenterY = (map.Rows - 1) / 2.0;

        var pts = map.Select(p => View(p.I, p.J, map.Heights[p.I, p.J], false)).ToList();
        double minX = pts.Min(p => p.X), maxX = pts.Max(p => p.X);
        double minY = pts.Min(p => p.Y), maxY = pts.Max(p => p.Y);
        double s = Math.Min(maxX > minX ? width / (maxX - minX) : double.PositiveInfinity,
                            maxY > minY ? height / (maxY - minY) : double.PositiveInfinity);

        fitScale = Scale = double.IsInfinity(s) ? 1 : s * 0.9;
        OffsetX = width / 2 - (minX + maxX) / 2 * Scale;
        OffsetY = top + height / 2 - (minY + maxY) / 2 * Scale;
        if (keep) (Scale, OffsetX, OffsetY) = (Scale * zoom, x, y);
    }

    public void Zoom(double factor, double x, double y)
    {
        OffsetX = x - (x - OffsetX) * factor;
        OffsetY = y - (y - OffsetY) * factor;
        Scale *= factor;
    }

    public void Move(double dx, double dy) => (OffsetX, OffsetY) = (OffsetX + dx, OffsetY + dy);

    public static Color LerpColor(Color a, Color b, double t)
    {
        t = Math.Max(0, Math.Min(1, t));
        int Lerp(int p, int q) => (int)Math.Round(p + (q - p) * t);
        return Color.FromArgb(Lerp(a.R, b.R), Lerp(a.G, b.G), Lerp(a.B, b.B));
    }

    (double X, double Y) View(double x, double y, double z, bool top)
    {
        double dx = x - CenterX, dy = y - CenterY;
        if (top) return (dx * Math.Cos(Rotation) - dy * Math.Sin(Rotation), dx * Math.Sin(Rotation) + dy * Math.Cos(Rotation));
        double t = Math.PI / 4 + Rotation;
        double u = dx * Math.Cos(t) - dy * Math.Sin(t);
        double v = dx * Math.Sin(t) + dy * Math.Cos(t);
        return (K * u, K * (v * Math.Sin(Elevation) - z * ZScale * Math.Cos(Elevation)));
    }
}
