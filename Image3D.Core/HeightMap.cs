using System.Collections;
using System.Drawing;

namespace Image3D;

public class HeightMap(int cols, int rows) : IEnumerable<(int I, int J)>
{
    public int Cols { get; } = cols;
    public int Rows { get; } = rows;
    public int Count => Cols * Rows;
    public double[,] Heights { get; } = new double[cols, rows];
    public Color[,] Colors { get; } = new Color[cols, rows];

    public IEnumerable<(int I, int J)> Neighbors(int i, int j)
    {
        if (i + 1 < Cols) yield return (i + 1, j);
        if (j + 1 < Rows) yield return (i, j + 1);
    }

    public IEnumerator<(int I, int J)> GetEnumerator()
    {
        for (int j = 0; j < Rows; j++)
            for (int i = 0; i < Cols; i++)
                yield return (i, j);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
