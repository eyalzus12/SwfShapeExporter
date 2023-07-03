namespace SwfShapeExporter;

/// <summary>
/// Represents a generic type of edge
/// </summary>
public interface IEdge
{
    /// <summary>
    /// The source point of the edge.
    /// </summary>
    /// <value></value>
    public Point From{get; set;}
    /// <summary>
    /// The target point of the edge.
    /// </summary>
    /// <value></value>
    public Point To{get; set;}
    /// <summary>
    /// The fill style index of the edge.
    /// </summary>
    /// <value></value>
    public int FillStyleIdx{get; set;}
    /// <summary>
    /// The line style index of the edge.
    /// </summary>
    /// <value></value>
    public int LineStyleIdx{get; set;}
    /// <summary>
    /// Whether the edge is a reverse edge.
    /// </summary>
    /// <value></value>
    public bool IsReverseEdge{get; set;}
    /// <summary>
    /// Return a new edge with a reverse direction and a new fill style.
    /// </summary>
    /// <param name="newFillStyleIdx">The new fill style.</param>
    /// <returns>The reversed edge.</returns>
    public IEdge ReverseWithStyle(int newFillStyleIdx);
}