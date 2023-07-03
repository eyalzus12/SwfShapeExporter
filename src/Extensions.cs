using SwfLib.Data;

using ImageSharpColor = SixLabors.ImageSharp.Color;

namespace SwfShapeExporter;

/// <summary>
/// Provides extension methods.
/// </summary>
public static class Extensions
{
    public static ImageSharpColor ToImageSharpColor(this SwfRGB color) => ImageSharpColor.FromRgba(color.Red, color.Green, color.Blue, 255);
    public static ImageSharpColor ToImageSharpColor(this SwfRGBA color) => ImageSharpColor.FromRgba(color.Red, color.Green, color.Blue, color.Alpha);
}
