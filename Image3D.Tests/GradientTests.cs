using System.Drawing;
using Xunit;

namespace Image3D.Tests;

public class GradientTests
{
    [Theory]
    [InlineData(0.0, 0, 0, 0)]
    [InlineData(1.0, 255, 136, 0)]
    [InlineData(0.5, 128, 68, 0)]
    public void LerpColorGoesByChannels(double t, int r, int g, int b)
    {
        Assert.Equal(Color.FromArgb(r, g, b), Camera.LerpColor(Color.FromArgb(0, 0, 0), Color.FromArgb(255, 136, 0), t));
    }
}
