namespace SwfShapeExporter;

public class StraightEdge : IEdge
{
    public Point From{get; set;}
    public Point To{get; set;}
    public int FillStyleIdx{get; set;}
    public int LineStyleIdx{get; set;}
    public bool IsReverseEdge{get; set;} = false;
    public virtual IEdge ReverseWithStyle(int newFillStyleIdx) => new StraightEdge
    {
        From = this.To,
        To = this.From,
        FillStyleIdx = newFillStyleIdx,
        LineStyleIdx = this.LineStyleIdx,
        IsReverseEdge = !this.IsReverseEdge
    };
}