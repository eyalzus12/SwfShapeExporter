namespace SwfShapeExporter;

public interface IEdge
{
    public Point From{get; set;}
    public Point To{get; set;}
    public int FillStyleIdx{get; set;}
    public int LineStyleIdx{get; set;}
    public bool IsReverseEdge{get; set;}
    public IEdge ReverseWithStyle(int newFillStyleIdx);
}