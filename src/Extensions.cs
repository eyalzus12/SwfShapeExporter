using SwfLib.Data;

namespace SwfShapeExporter;

public static class Extensions
{
    public static Color ToImageSharpColor(this SwfRGB color) => Color.FromRgba(color.Red, color.Green, color.Blue, 255);
    public static Color ToImageSharpColor(this SwfRGBA color) => Color.FromRgba(color.Red, color.Green, color.Blue, color.Alpha);
}
