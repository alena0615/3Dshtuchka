using System.Drawing.Drawing2D;

namespace Image3D;

public static class ImageLoader
{
    public static HeightMap MakeGrid(Bitmap img, int cols, bool invert)
    {
        int rows = Math.Max(2, cols * img.Height / img.Width);
        using Bitmap small = Shrink(img, cols, rows, true);
        double zMax = 0.25 * Math.Max(cols, rows);
        var map = new HeightMap(cols, rows);
        foreach (var (i, j) in map)
        {
            Color c = map.Colors[i, j] = small.GetPixel(i, j);
            map.Heights[i, j] = (invert ? 255 - Brightness(c) : Brightness(c)) / 255 * zMax;
        }
        return map;
    }

    public static bool IsLight(Bitmap img)
    {
        using Bitmap small = Shrink(img, 32, 32, false);
        return Enumerable.Range(0, 32 * 32).Sum(k => Brightness(small.GetPixel(k / 32, k % 32))) / (32 * 32) > 127;
    }

    static double Brightness(Color c) => 0.299 * c.R + 0.587 * c.G + 0.114 * c.B;

    static Bitmap Shrink(Bitmap img, int w, int h, bool smooth)
    {
        var small = new Bitmap(w, h);
        using Graphics g = Graphics.FromImage(small);
        if (smooth)
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        }
        g.DrawImage(img, 0, 0, w, h);
        return small;
    }
}
