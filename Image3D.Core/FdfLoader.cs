using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Image3D;

public static class FdfLoader
{
    static readonly Regex ColorPattern = new("^0[xX][0-9a-fA-F]{1,6}$");

    public static HeightMap Load(string fileName) => Parse(File.ReadAllLines(fileName));

    public static HeightMap Parse(IReadOnlyList<string> lines)
    {
        int count = lines.Count;
        while (count > 0 && lines[count - 1].Trim() == "") count--;
        if (count == 0) throw new MapFormatError("файл пуст");

        var rows = lines.Take(count).Select(l => l.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)).ToList();
        int width = rows[0].Length;
        var map = new HeightMap(Math.Max(width, 1), count);
        for (int j = 0; j < count; j++)
        {
            if (rows[j].Length == 0) throw new MapFormatError(j + 1, 1, "пустая строка");
            if (rows[j].Length != width)
                throw new MapFormatError(j + 1, Math.Min(rows[j].Length, width) + 1, $"ожидалось значений: {width}, найдено: {rows[j].Length}");
            for (int i = 0; i < width; i++)
            {
                var (z, color) = ParseValue(rows[j][i], j + 1, i + 1);
                int rgb = color == null ? 0 : Convert.ToInt32(color.Substring(1), 16);
                map.Heights[i, j] = z;
                map.Colors[i, j] = color == null ? Color.Pink : Color.FromArgb(rgb >> 16, (rgb >> 8) & 255, rgb & 255);
            }
        }
        return map;
    }

    public static (int Z, string? Color) ParseValue(string text, int line = 1, int column = 1)
    {
        string[] part = text.Split(',');
        if (part.Length > 2) throw new MapFormatError(line, column, $"'{text}' — лишняя запятая");
        if (!int.TryParse(part[0], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int z))
            throw new MapFormatError(line, column, $"'{part[0]}' — не целое число");
        if (part.Length == 1) return (z, null);
        if (!ColorPattern.IsMatch(part[1])) throw new MapFormatError(line, column, $"'{part[1]}' — некорректный цвет");
        return (z, $"#{Convert.ToInt32(part[1].Substring(2), 16):X6}");
    }
}
