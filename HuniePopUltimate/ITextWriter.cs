using System;

public interface ITextWriter
{
    /// <summary>
    /// Writes a line of text
    /// </summary>
    /// <param name="value">The text to be written.</param>
    public void WriteLine(string value);

    /// <summary>
    /// Changes the color
    /// </summary>
    public void SetColor(ConsoleColor color);

    /// <summary>
    /// Resets the color to default
    /// </summary>
    public void ResetColor();
}