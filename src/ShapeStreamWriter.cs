using SwfLib.Tags.ShapeTags;
using SwfLib.Shapes.Records;
using SwfLib.Shapes.FillStyles;
using SwfLib.Shapes.LineStyles;
using SwfLib.Data;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Gif;

namespace SwfShapeExporter;

public class ShapeStreamWriter
{
    public static void ShapeToStreamBMP(ShapeBaseTag t, Stream stream) => ShapeToStream(t, stream, new BmpEncoder());
    public static void ShapeToStreamPNG(ShapeBaseTag t, Stream stream) => ShapeToStream(t, stream, new PngEncoder());
    public static void ShapeToStreamGIF(ShapeBaseTag t, Stream stream) => ShapeToStream(t, stream, new GifEncoder());
    public static void ShapeToStreamJPEG(ShapeBaseTag t, Stream stream) => ShapeToStream(t, stream, new JpegEncoder());

    public static void ShapeToStream(ShapeBaseTag t, Stream stream, IImageEncoder encoder)
    {
        if(t is DefineShapeTag t1) DefineShapeToStream(t1, stream, encoder);
        else if(t is DefineShape2Tag t2) DefineShape2ToStream(t2, stream, encoder);
        else if(t is DefineShape3Tag t3) DefineShape3ToStream(t3, stream, encoder);
        else if(t is DefineShape4Tag t4) DefineShape4ToStream(t4, stream, encoder);
        else throw new ArgumentException($"Invalid shape tag {t}");
    }

    public static void DefineShapeToStream(DefineShapeTag t, Stream stream, IImageEncoder encoder)
    {
        var fillStyles = t.FillStyles;
        var lineStyles = t.LineStyles;
        var records = t.ShapeRecords;
        var bounds = t.ShapeBounds;
        RecordsToStreamRGB(bounds, records, fillStyles, lineStyles, stream, encoder);
    }

    public static void DefineShape2ToStream(DefineShape2Tag t, Stream stream, IImageEncoder encoder)
    {
        var fillStyles = t.FillStyles;
        var lineStyles = t.LineStyles;
        var records = t.ShapeRecords;
        var bounds = t.ShapeBounds;
        RecordsToStreamRGB(bounds, records, fillStyles, lineStyles, stream, encoder);
    }

    public static void DefineShape3ToStream(DefineShape3Tag t, Stream stream, IImageEncoder encoder)
    {
        var fillStyles = t.FillStyles;
        var lineStyles = t.LineStyles;
        var records = t.ShapeRecords;
        var bounds = t.ShapeBounds;
        RecordsToStreamRGBA(bounds, records, fillStyles, lineStyles, stream, encoder);
    }

    public static void DefineShape4ToStream(DefineShape4Tag t, Stream stream, IImageEncoder encoder)
    {
        throw new NotImplementedException("DefineShape4 exporting is unsupported");
    }

    public static void RecordsToStreamRGB(SwfRect bounds,
        IList<IShapeRecordRGB> records, IList<FillStyleRGB> fillStyles, IList<LineStyleRGB> lineStyles,
        Stream stream, IImageEncoder encoder
    )
    {
        var width = bounds.BottomRight.X - bounds.TopLeft.X;
		var height = bounds.BottomRight.Y - bounds.TopLeft.Y;
		SwfShapeRGB exportShape = new();
		exportShape.Records = records.ToList();
		exportShape.FillStyles = fillStyles.ToList();
		exportShape.LineStyles = lineStyles.ToList();
		ShapeExporter exporter = new();
		Image<Rgba32> image = new(width,height,Color.Transparent.ToPixel<Rgba32>());
		exporter.Canvas = image;
		exporter.Offset = new PointF(-bounds.TopLeft.X, -bounds.TopLeft.Y);
		exportShape.Export(exporter);
        image.Save(stream, encoder);
    }

    public static void RecordsToStreamRGBA(SwfRect bounds,
        IList<IShapeRecordRGBA> records, IList<FillStyleRGBA> fillStyles, IList<LineStyleRGBA> lineStyles,
        Stream stream, IImageEncoder encoder
    )
    {
        var width = bounds.BottomRight.X - bounds.TopLeft.X;
		var height = bounds.BottomRight.Y - bounds.TopLeft.Y;
		SwfShapeRGBA exportShape = new();
		exportShape.Records = records.ToList();
		exportShape.FillStyles = fillStyles.ToList();
		exportShape.LineStyles = lineStyles.ToList();
		ShapeExporter exporter = new();
		Image<Rgba32> image = new(width,height,Color.Transparent.ToPixel<Rgba32>());
		exporter.Canvas = image;
		exporter.Offset = new PointF(-bounds.TopLeft.X, -bounds.TopLeft.Y);
		exportShape.Export(exporter);
        image.Save(stream, encoder);
    }
}