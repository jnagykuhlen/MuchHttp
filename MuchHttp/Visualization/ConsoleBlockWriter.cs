namespace MuchHttp.Visualization;

public class ConsoleBlockWriter
{
    public int MaxPropertyWidth { get; init; }

    public void WriteSpacing()
    {
        Console.WriteLine();
    }

    public void WriteHeading(string heading)
    {
        Console.WriteLine(heading);
    }

    public void WriteProperty(string property, object value)
    {
        if (property.Length > MaxPropertyWidth - 3)
            property = property.Substring(0, MaxPropertyWidth - 3);
        
        Console.WriteLine($" {property.PadRight(MaxPropertyWidth, '.')}: {value}");
    }

    public void WriteException(Exception exception)
    {
        WriteLineColored($"{exception.GetType().Name}: {exception.Message}", ConsoleColor.Red);
    }

    private void WriteLineColored(string line, ConsoleColor color)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(line);
        Console.ForegroundColor = previousColor;
    }
}