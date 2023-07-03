using SwfLib.Tags.ShapeTags;
using SwfLib.Data;

namespace SwfShapeExporter;

/// <summary>
/// A wrapper for exporting a shape tag into an Image.
/// </summary>
public static class ShapeToImageConverter
{
    /// <summary>
    /// Turns the given shape tag into an Image.
    /// </summary>
    /// <param name="t">The shape tag.</param>
    /// <returns>The image created through the given shape tag.</returns>
    public static Image<Rgba32> RenderShape(ShapeBaseTag t) => RenderSwfShape(t.ShapeBounds, ShapeTagToSwfShape(t));

    /// <summary>
    /// Convert a shape tag into its matching swf shape builder.
    /// </summary>
    /// <param name="t">The shape tag.</param>
    /// <returns>The swf shape builder from that shape tag.</returns>
    public static ISwfShape ShapeTagToSwfShape(ShapeBaseTag t) => t switch
    {
        DefineShapeTag t1 => new SwfShapeRGB(t1),
        DefineShape2Tag t2 => new SwfShapeRGB(t2),
        DefineShape3Tag t3 => new SwfShapeRGBA(t3),
        DefineShape4Tag t4 => throw new NotImplementedException("DefineShape4 exporting is not supported"),
        _ => throw new ArgumentException($"Attempt to render a shape of an unknown type {t}")
    };

    /// <summary>
    /// Takes a generic swf shape and the shape bounds, and renders the shape into an image.
    /// </summary>
    /// <param name="bounds">The bounds of the shape.</param>
    /// <param name="shape">The shape to render.</param>
    /// <typeparam name="T">The type of the shape.</typeparam>
    /// <returns>The rendered image.</returns>
    public static Image<Rgba32> RenderSwfShape<T>(SwfRect bounds, T shape) where T : ISwfShape
    {
        var width = bounds.BottomRight.X - bounds.TopLeft.X;
		var height = bounds.BottomRight.Y - bounds.TopLeft.Y;
		Image<Rgba32> image = new(width,height,Color.Transparent.ToPixel<Rgba32>());
		ShapeImageHandler exporter = new ShapeImageHandler()
        {
            Canvas = image,
            Offset = new PointF(-bounds.TopLeft.X, -bounds.TopLeft.Y)
        };
		shape.Export(exporter);
        return image;
    }
}