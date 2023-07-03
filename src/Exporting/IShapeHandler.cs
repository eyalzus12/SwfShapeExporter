
namespace SwfShapeExporter;

/// <summary>
/// Represents a wrapper over path operations to be used for creating the image.
/// </summary>
public interface IShapeHandler
{
    /// <summary>
    /// Called when the shape starts building.
    /// </summary>
    void BeginShape();
    /// <summary>
    /// Called when the shape finishes building.
    /// </summary>
    void EndShape();

    /// <summary>
    /// Called when a fill path export begins.
    /// </summary>
    void BeginFills();
    /// <summary>
    /// Called when a fill path export ends.
    /// </summary>
    void EndFills();

    /// <summary>
    /// Called when a line path export begins.
    /// </summary>
    void BeginLines();
    /// <summary>
    /// Called when a line path export ends.
    /// </summary>
    /// <param name="close">Whether the current figure should be closed.</param>
    void EndLines(bool close);

    /// <summary>
    /// Called when a new fill begins.
    /// </summary>
    /// <param name="color">The fill color.</param>
    void BeginFill(Color color);
    /// <summary>
    /// Called when a fill ends.
    /// </summary>
    void EndFill();

    /// <summary>
    /// Called when a line style changes.
    /// </summary>
    /// <param name="thickness">The new thickness.</param>
    /// <param name="color">The new color.</param>
    void LineStyle(float thickness = float.NaN, Color color = default);

    /// <summary>
    /// Move the current drawing position.
    /// </summary>
    /// <param name="pos">The new position.</param>
    void MoveTo(PointF pos);

    /// <summary>
    /// Draw a line to a new position, and set current position to that one.
    /// </summary>
    /// <param name="pos">The new position.</param>
    void LineTo(PointF pos);

    /// <summary>
    /// Draw a quadratic bezier curve to a new position, and set current position to that one.
    /// </summary>
    /// <param name="control">The middle paramater point of the curve.</param>
    /// <param name="anchor">The target position.</param>
    void CurveTo(PointF control, PointF anchor);

    /// <summary>
    /// Draw the current path, and reset.
    /// </summary>
    void FinalizePath();
}