using Xunit;

namespace Image3D.Tests;

public class HeightMapTests
{
    readonly HeightMap map = FdfLoader.Load(FdfLoaderTests.MapPath("elem.fdf"));

    [Fact]
    public void CountAndForeach()
    {
        var points = map.ToList();

        Assert.Equal(25, map.Count);
        Assert.Equal(25, points.Count);
        Assert.Equal(new[] { (0, 0), (1, 0) }, points.Take(2));
        Assert.Equal((4, 4), points[24]);
    }

    [Fact]
    public void CornerNeighbors()
    {
        Assert.Equal(new[] { (1, 0), (0, 1) }, map.Neighbors(0, 0));
        Assert.Empty(map.Neighbors(4, 4));
    }

    [Fact]
    public void EachEdgeIsCountedOnce()
    {
        Assert.Equal(2 * 5 * 4, map.Sum(p => map.Neighbors(p.I, p.J).Count()));
    }
}
