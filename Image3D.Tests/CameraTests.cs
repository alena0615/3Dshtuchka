using Xunit;

namespace Image3D.Tests;

public class CameraTests
{
    [Fact]
    public void OriginGoesToZeroAndHeightGoesUp()
    {
        var camera = new Camera();
        var (x, y) = camera.Project(0, 0, 0);

        Assert.Equal(0, x, 9);
        Assert.Equal(0, y, 9);
        Assert.True(camera.Project(0, 0, 1).Y < 0);
    }

    [Fact]
    public void DefaultViewIsIsometricFormula()
    {
        var (x, y) = new Camera().Project(3, 1, 2);

        Assert.Equal((3 - 1) * Math.Cos(Math.PI / 6), x, 9);
        Assert.Equal((3 + 1) * Math.Sin(Math.PI / 6) - 2, y, 9);
    }

    [Theory]
    [InlineData("elem.fdf")]
    [InlineData("42.fdf")]
    [InlineData("pyramid.fdf")]
    public void AfterFitAllPointsAreInsideWindow(string name)
    {
        HeightMap map = FdfLoader.Load(FdfLoaderTests.MapPath(name));
        var camera = new Camera();
        camera.Fit(map, 800, 600);

        foreach (var (i, j) in map)
        {
            var (x, y) = camera.Project(i, j, map.Heights[i, j]);
            Assert.InRange(x, 0, 800);
            Assert.InRange(y, 0, 600);
        }
    }

    [Fact]
    public void FlatMapIsSymmetricRhombus()
    {
        HeightMap map = FdfLoader.Load(FdfLoaderTests.MapPath("flat.fdf"));
        var camera = new Camera();
        camera.Fit(map, 800, 600);

        double left = camera.Project(0, map.Rows - 1, 0).X;
        double right = camera.Project(map.Cols - 1, 0, 0).X;

        Assert.Equal(400 - left, right - 400, 6);
        Assert.Equal(400, camera.Project(0, 0, 0).X, 6);
    }

    [Fact]
    public void SinglePointMapIsCentered()
    {
        var camera = new Camera();
        camera.Fit(FdfLoader.Parse(new[] { "7" }), 800, 600);
        var (x, y) = camera.Project(0, 0, 7);

        Assert.Equal(400, x, 6);
        Assert.Equal(300, y, 6);
    }

    [Fact]
    public void TopViewIsFlatMap()
    {
        Assert.Equal((3.0, 1.0), new Camera { TopView = true }.Project(3, 1, 50));
    }

    [Fact]
    public void TopViewDoesNotChangeCenterAndScale()
    {
        var camera = new Camera();
        camera.Fit(FdfLoader.Load(FdfLoaderTests.MapPath("42.fdf")), 800, 600);
        double scale = camera.Scale;
        var center = camera.Project(camera.CenterX, camera.CenterY, 0);

        camera.TopView = true;

        Assert.Equal(scale, camera.Scale);
        Assert.Equal(center, camera.Project(camera.CenterX, camera.CenterY, 0));
    }

    [Fact]
    public void RotationKeepsMapCentered()
    {
        var camera = new Camera();
        camera.Fit(FdfLoader.Load(FdfLoaderTests.MapPath("flat.fdf")), 800, 600);
        var before = camera.Project(camera.CenterX, camera.CenterY, 0);

        camera.Rotation = 1.2;
        var after = camera.Project(camera.CenterX, camera.CenterY, 0);

        Assert.Equal(before.X, after.X, 6);
        Assert.Equal(before.Y, after.Y, 6);
    }

    [Fact]
    public void FitWithKeepZoomKeepsUserZoomAndShift()
    {
        HeightMap map = FdfLoader.Load(FdfLoaderTests.MapPath("42.fdf"));
        var camera = new Camera();
        camera.Fit(map, 800, 600);
        camera.Zoom(0.5, 300, 200);
        camera.Move(30, -10);
        var before = (camera.Scale, camera.OffsetX, camera.OffsetY);

        camera.Fit(map, 800, 600, 0, true);

        Assert.Equal(before.Scale, camera.Scale, 9);
        Assert.Equal(before.OffsetX, camera.OffsetX, 9);
        Assert.Equal(before.OffsetY, camera.OffsetY, 9);
    }

    [Fact]
    public void FitWithKeepZoomOnDenserGridKeepsCenterAndSize()
    {
        HeightMap small = FdfLoader.Parse(Enumerable.Repeat("0 0 0 0 0", 5).ToList());
        HeightMap dense = FdfLoader.Parse(Enumerable.Repeat("0 0 0 0 0 0 0 0 0 0", 10).ToList());
        var camera = new Camera();
        camera.Fit(small, 800, 600);
        camera.Zoom(0.5, 300, 200);
        double width = camera.Project(4, 0, 0).X - camera.Project(0, 4, 0).X;
        var center = (camera.OffsetX, camera.OffsetY);

        camera.Fit(dense, 800, 600, 0, true);

        Assert.Equal(center, (camera.OffsetX, camera.OffsetY));
        Assert.Equal(width, camera.Project(9, 0, 0).X - camera.Project(0, 9, 0).X, 6);
    }

    [Fact]
    public void ZoomKeepsPointUnderCursor()
    {
        var camera = new Camera { Scale = 10, OffsetX = 100, OffsetY = 50 };
        var (x, y) = camera.Project(3, 1, 2);

        camera.Zoom(1.5, x, y);

        Assert.Equal(x, camera.Project(3, 1, 2).X, 6);
        Assert.Equal(y, camera.Project(3, 1, 2).Y, 6);
    }
}
