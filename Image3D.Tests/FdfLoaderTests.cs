using System.Drawing;
using Xunit;

namespace Image3D.Tests;

public class FdfLoaderTests
{
    public static string MapPath(string name) => Path.Combine(AppContext.BaseDirectory, "maps", name);

    [Fact]
    public void ElemIs5x5AndHeightsMatchFile()
    {
        HeightMap map = FdfLoader.Load(MapPath("elem.fdf"));

        Assert.Equal((5, 5), (map.Cols, map.Rows));
        Assert.Equal(5, map.Heights[2, 2]);
        Assert.Equal(2, map.Heights[1, 1]);
        Assert.Equal(0, map.Heights[4, 0]);
    }

    [Theory]
    [InlineData("7", 7, null)]
    [InlineData("-3", -3, null)]
    [InlineData("2,0xFF8800", 2, "#FF8800")]
    [InlineData("1,0xff", 1, "#0000FF")]
    public void ParsesValue(string text, int z, string? color)
    {
        Assert.Equal((z, color), FdfLoader.ParseValue(text));
    }

    [Theory]
    [InlineData("empty.fdf")]
    [InlineData("not_number.fdf")]
    [InlineData("ragged.fdf")]
    [InlineData("bad_color.fdf")]
    [InlineData("extra_comma.fdf")]
    public void BadMapIsFormatError(string name)
    {
        Assert.Throws<MapFormatError>(() => FdfLoader.Load(MapPath("bad/" + name)));
    }

    [Fact]
    public void ErrorHasLineAndColumn()
    {
        var ex = Assert.Throws<MapFormatError>(() => FdfLoader.Load(MapPath("bad/not_number.fdf")));

        Assert.Equal((2, 3), (ex.Line, ex.Column));
        Assert.StartsWith("строка 2, колонка 3: 'x'", ex.Message);
    }

    [Fact]
    public void MissingFileIsNotFormatError()
    {
        Assert.Throws<FileNotFoundException>(() => FdfLoader.Load(MapPath("no_such_map.fdf")));
    }

    [Fact]
    public void TrailingEmptyLinesAreIgnored()
    {
        Assert.Equal(2, FdfLoader.Parse(new[] { "1 2", "3 4", "", "  " }).Rows);
    }

    [Fact]
    public void ColorsFromFileAndPinkByDefault()
    {
        HeightMap map = FdfLoader.Load(MapPath("peak.fdf"));

        Assert.Equal(Color.FromArgb(255, 0, 0), map.Colors[2, 2]);
        Assert.Equal(Color.FromArgb(0, 0, 255), map.Colors[0, 0]);
        Assert.Equal(Color.Pink, map.Colors[1, 1]);
    }
}
