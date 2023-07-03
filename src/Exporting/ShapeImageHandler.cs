using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace SwfShapeExporter;

/// <summary>
/// An implementation of the IShapeHandler interface for a standard Image
/// </summary>
public class ShapeImageHandler : IShapeHandler
{
    public SolidBrush? Fill{get; set;}
    public Pen? Line{get; set;}
    public Image<Rgba32>? Canvas{get; set;}
    public PathBuilder Builder{get; set;} = new();
    public PointF Offset{get; set;} = PointF.Empty;

    public void BeginShape()
    {
        Builder.MoveTo(Offset);
    }

    public void EndShape(){}

    public void BeginFills(){}
    public void EndFills(){}

    public void BeginLines(){}
    public void EndLines(bool close)
    {
        if(close) Builder.CloseFigure();
        FinalizePath();
    }

    public void BeginFill(Color color)
    {
        FinalizePath();
        Fill = new SolidBrush(color);
    }
    public void EndFill()
    {
        FinalizePath();
    }

    public void LineStyle(float thickness = float.NaN, Color color = default)
    {
        FinalizePath();
        Line = new Pen(color, float.IsNaN(thickness)?1:thickness);
    }

    public void MoveTo(PointF pos)
    {
        Builder.MoveTo(PointF.Add(Offset, pos));
    }

    public void LineTo(PointF pos)
    {
        Builder.LineTo(PointF.Add(Offset, pos));
    }

    public void CurveTo(PointF control, PointF anchor)
    {
        Builder.QuadraticBezierTo(
            PointF.Add(Offset, control),
            PointF.Add(Offset, anchor)
        );
    }

    public void FinalizePath()
    {
        var path = Builder.Build();

        Canvas?.Mutate(x =>
        {
            if(Fill is not null)x.Fill(Fill,path);
            if(Line is not null)x.Draw(Line,path);
        });

        Builder.Clear(); Builder.MoveTo(Offset);

        Fill = null;
        Line = null;
    }
}