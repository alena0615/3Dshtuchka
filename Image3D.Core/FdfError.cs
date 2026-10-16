namespace Image3D;

public class FdfError(string message) : Exception(message);

public class MapFormatError : FdfError
{
    public int Line { get; }
    public int Column { get; }

    public MapFormatError(string message) : base(message) { }

    public MapFormatError(int line, int column, string message)
        : base($"строка {line}, колонка {column}: {message}") => (Line, Column) = (line, column);
}
