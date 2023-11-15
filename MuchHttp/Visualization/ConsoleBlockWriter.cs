﻿namespace MuchHttp.Visualization;

public class ConsoleBlockWriter
{
    public int MaxPropertyWidth { get; init; }

    public void WriteSpacing()
    {
        Console.WriteLine();
    }

    public void WriteHeading(string line)
    {
        Console.WriteLine(line);
    }

    public void WriteProperty(string property, object value)
    {
        Console.WriteLine($" {property.PadRight(MaxPropertyWidth, '.')}: {value}");
    }
}