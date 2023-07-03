namespace SwfShapeExporter;

/// <summary>
/// Represents a quadratic bezier curve.
/// </summary>
public class CurvedEdge : StraightEdge, IEdge
{
    /// <summary>
    /// The middle paramater point in the curve.
    /// </summary>
    /// <value></value>
    public PointF Control{get; set;}
    public override IEdge ReverseWithStyle(int newFillStyleIdx) => new CurvedEdge
    {
        From = this.To,
        To = this.From,
        FillStyleIdx = newFillStyleIdx,
        LineStyleIdx = this.LineStyleIdx,
        IsReverseEdge = !this.IsReverseEdge,
        Control = this.Control
    };
}