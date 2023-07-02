namespace SwfShapeExporter;

public class CurvedEdge : StraightEdge, IEdge
{
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