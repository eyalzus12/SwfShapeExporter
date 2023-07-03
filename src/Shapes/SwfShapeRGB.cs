using SwfLib.Tags.ShapeTags;
using SwfLib.Shapes.Records;
using SwfLib.Shapes.FillStyles;
using SwfLib.Shapes.LineStyles;

using EdgeMap = System.Collections.Generic.Dictionary<int,System.Collections.Generic.List<SwfShapeExporter.IEdge>>;
using CoordMap = System.Collections.Generic.Dictionary<SixLabors.ImageSharp.Point,System.Collections.Generic.List<SwfShapeExporter.IEdge>>;
using Path = System.Collections.Generic.List<SwfShapeExporter.IEdge>;

namespace SwfShapeExporter;

/// <summary>
/// Builds and exports an RGB shape.
/// See also: SwfShapeRGBA.
/// They are seperated since SwfLib does not define generic classes for fill styles and line styles.
/// </summary>
public class SwfShapeRGB : ISwfShape
{
    public List<IShapeRecordRGB> Records{get; set;} = new();
    public List<FillStyleRGB> FillStyles{get; set;} = new();
    public List<LineStyleRGB> LineStyles{get; set;} = new();

    public List<EdgeMap> FillEdgeMaps{get; set;} = new();
    public EdgeMap CurrentFillEdgeMap{get; set;} = new();
    public List<EdgeMap> LineEdgeMaps{get; set;} = new();
    public EdgeMap CurrentLineEdgeMap{get; set;} = new();
    public int NumGroups{get; set;} = 0;
    public CoordMap CoordMap{get; set;} = new();
    public CoordMap ReverseCoordMap{get; set;} = new();
    public bool EdgeMapsCreated{get; private set;} = false;

    public SwfShapeRGB(){}
    public SwfShapeRGB(IList<IShapeRecordRGB> records, IList<FillStyleRGB> fillStyles, IList<LineStyleRGB> lineStyles)
    {
        Records = records.ToList();
        FillStyles = fillStyles.ToList();
        LineStyles = lineStyles.ToList();
    }
    public SwfShapeRGB(DefineShapeTag shape) : this(shape.ShapeRecords, shape.FillStyles, shape.LineStyles){}
    public SwfShapeRGB(DefineShape2Tag shape) : this(shape.ShapeRecords, shape.FillStyles, shape.LineStyles){}

    public void Export(IShapeHandler handler)
    {
        CreateEdgeMaps();
        handler.BeginShape();
        for(int i = 0; i < NumGroups; ++i)
        {
            ExportFillPath(handler, i);
            ExportLinePath(handler, i);
        }
        handler.EndShape();
    }

    public void ExportFillPath(IShapeHandler handler, int i)
    {
        var path = PathFromEdgeMap(FillEdgeMaps[i]);
        if(path.Count == 0) return;
        var pos = new Point(int.MaxValue, int.MaxValue);
        var fillStyleIdx = int.MaxValue;
        handler.BeginFills();
        foreach(var edge in path)
        {
            if(fillStyleIdx != edge.FillStyleIdx)
            {
                if(fillStyleIdx != int.MaxValue)handler.EndFill();

                fillStyleIdx = edge.FillStyleIdx;
                pos = new Point(int.MaxValue, int.MaxValue);
                var fillStyle = (fillStyleIdx == 0)?null:FillStyles[fillStyleIdx-1];

                if(fillStyle is null) handler.BeginFill(Color.Black);
                else switch(fillStyle.Type)
                {
                    case FillStyleType.SolidColor:
                        var solidFillStyle = (SolidFillStyleRGB)fillStyle;
                        handler.BeginFill(solidFillStyle.Color.ToImageSharpColor());
                        break;
                    case FillStyleType.LinearGradient:
                    case FillStyleType.RadialGradient:
                    case FillStyleType.FocalGradient:
                    case FillStyleType.RepeatingBitmap:
                    case FillStyleType.ClippedBitmap:
                    case FillStyleType.NonSmoothedRepeatingBitmap:
                    case FillStyleType.NonSmoothedClippedBitmap:
                        throw new NotImplementedException($"Unsupported fill style type {fillStyle.Type}");
                    default:
                        throw new ArgumentException($"Invalid fill style type {fillStyle.Type}");
                }
            }

            if(pos != edge.From)
                handler.MoveTo(edge.From);
                
            if(edge is CurvedEdge cedge)
                handler.CurveTo(cedge.Control, cedge.To);
            else
                handler.LineTo(edge.To);
            
            pos = edge.To;
        }

        if(fillStyleIdx != int.MaxValue) handler.EndFill();
        handler.EndFills();
    }

    public void ExportLinePath(IShapeHandler handler, int i)
    {
        var path = PathFromEdgeMap(LineEdgeMaps[i]);
        if(path.Count == 0) return;
        var pos = new Point(int.MaxValue, int.MaxValue);
        var lastMove = pos;
        var lineStyleIdx = int.MaxValue;
        
        handler.BeginLines();
        foreach(var edge in path)
        {
            if(lineStyleIdx != edge.LineStyleIdx)
            {
                lineStyleIdx = edge.LineStyleIdx;
                pos = new Point(int.MaxValue, int.MaxValue);
                
                if(lineStyleIdx == 0) handler.LineStyle(0, Color.Black);
                else
                {
                    var lineStyle = LineStyles[lineStyleIdx-1];
                    handler.LineStyle(lineStyle.Width, lineStyle.Color.ToImageSharpColor());
                }
            }

            if(pos != edge.From)
            {
                handler.MoveTo(edge.From);
                lastMove = edge.From;
            }

            if(edge is CurvedEdge cedge)
                handler.CurveTo(cedge.Control, cedge.To);
            else
                handler.LineTo(edge.To);
            
            pos = edge.To;
        }
        handler.EndLines(pos == lastMove);
    }

    public void CreateEdgeMaps()
    {
        if(EdgeMapsCreated) return;
        Point pos = Point.Empty;
        Point from = Point.Empty;
        Point to = Point.Empty;
        Point control = Point.Empty;
        int fillStyleIdxOffset = 0;
        int lineStyleIdxOffset = 0;
        int currentFillStyleIdx0 = 0;
        int currentFillStyleIdx1 = 0;
        int currentLineStyleIdx = 0;
        Path subPath = new();
        NumGroups = 0;
        FillEdgeMaps.Clear();
        LineEdgeMaps.Clear();
        CurrentFillEdgeMap.Clear();
        CurrentLineEdgeMap.Clear();

        for(int i = 0; i < Records.Count; ++i)
        {
            var shapeRecord = Records[i];
            switch(shapeRecord.Type)
            {
                case ShapeRecordType.StyleChangeRecord:
                    var styleChangeRecord = (StyleChangeShapeRecordRGB)shapeRecord;
                    if(styleChangeRecord.LineStyle is not null || styleChangeRecord.FillStyle0 is not null || styleChangeRecord.FillStyle1 is not null)
                    {
                        ProcessSubPath(subPath, currentLineStyleIdx, currentFillStyleIdx0, currentFillStyleIdx1);
                        subPath.Clear();
                    }

                    if(styleChangeRecord.StateNewStyles)
                    {
                        fillStyleIdxOffset = FillStyles.Count;
                        lineStyleIdxOffset = LineStyles.Count;
                        FillStyles.AddRange(styleChangeRecord.FillStyles);
                        LineStyles.AddRange(styleChangeRecord.LineStyles);
                    }

                    if(styleChangeRecord.LineStyle is not null && styleChangeRecord.LineStyle == 0 && 
                        styleChangeRecord.FillStyle0 is not null && styleChangeRecord.FillStyle0 == 0 &&
                        styleChangeRecord.FillStyle1 is not null && styleChangeRecord.FillStyle1 == 0)
                    {
                        CleanEdgeMap(CurrentFillEdgeMap);
                        CleanEdgeMap(CurrentLineEdgeMap);
                        FillEdgeMaps.Add(CurrentFillEdgeMap);
                        LineEdgeMaps.Add(CurrentLineEdgeMap);
                        //we must create new instead of Clear because the edge map lists hold a reference
                        CurrentFillEdgeMap = new EdgeMap();
                        CurrentLineEdgeMap = new EdgeMap();
                        currentLineStyleIdx = 0;
                        currentFillStyleIdx0 = 0;
                        currentFillStyleIdx1 = 0;
                        NumGroups++;
                    }
                    else
                    {
                        if(styleChangeRecord.LineStyle is not null)
                        {
                            currentLineStyleIdx = (int)styleChangeRecord.LineStyle;
                            if(currentLineStyleIdx > 0)currentLineStyleIdx += lineStyleIdxOffset;
                        }
                        if(styleChangeRecord.FillStyle0 is not null)
                        {
                            currentFillStyleIdx0 = (int)styleChangeRecord.FillStyle0;
                            if(currentFillStyleIdx0 > 0)currentFillStyleIdx0 += fillStyleIdxOffset;
                        }
                        if(styleChangeRecord.FillStyle1 is not null)
                        {
                            currentFillStyleIdx1 = (int)styleChangeRecord.FillStyle1;
                            if(currentFillStyleIdx1 > 0)currentFillStyleIdx1 += fillStyleIdxOffset;
                        }
                    }

                    if(styleChangeRecord.StateMoveTo)
                    {
                        pos = new Point(styleChangeRecord.MoveDeltaX, styleChangeRecord.MoveDeltaY);
                    }
                    break;
                case ShapeRecordType.StraightEdge:
                    var straightEdgeRecord = (StraightEdgeShapeRecord)shapeRecord;
                    from = new Point(pos.X, pos.Y);
                    Size delta = new(straightEdgeRecord.DeltaX, straightEdgeRecord.DeltaY);
                    pos += delta;
                    to = new Point(pos.X, pos.Y);
                    subPath.Add(new StraightEdge{From = from, To = to, LineStyleIdx = currentLineStyleIdx, FillStyleIdx = currentFillStyleIdx1});
                    break;
                case ShapeRecordType.CurvedEdgeRecord:
                    var curvedEdgeRecord = (CurvedEdgeShapeRecord)shapeRecord;
                    from = new Point(pos.X, pos.Y);
                    Size controlDelta = new(curvedEdgeRecord.ControlDeltaX, curvedEdgeRecord.ControlDeltaY);
                    control = pos + controlDelta;
                    Size anchorDelta = new(curvedEdgeRecord.AnchorDeltaX, curvedEdgeRecord.AnchorDeltaY);
                    pos = control + anchorDelta;
                    to = new Point(pos.X, pos.Y);
                    subPath.Add(new CurvedEdge{From = from, Control = control, To = to, LineStyleIdx = currentLineStyleIdx, FillStyleIdx = currentFillStyleIdx1});
                    break;
                case ShapeRecordType.EndRecord:
                    ProcessSubPath(subPath, currentLineStyleIdx, currentFillStyleIdx0, currentFillStyleIdx1);
                    CleanEdgeMap(CurrentFillEdgeMap);
                    CleanEdgeMap(CurrentLineEdgeMap);
                    FillEdgeMaps.Add(CurrentFillEdgeMap);
                    LineEdgeMaps.Add(CurrentLineEdgeMap);
                    NumGroups++;
                    break;
                default:
                    throw new ArgumentException($"Invalid record type {shapeRecord.Type}");
            }
        }

        EdgeMapsCreated = true;
    }

    public void ProcessSubPath(Path subPath, int lineStyleIdx, int fillStyleIdx0, int fillStyleIdx1)
    {
        if(fillStyleIdx0 != 0)
        {
            if(!CurrentFillEdgeMap.ContainsKey(fillStyleIdx0))CurrentFillEdgeMap[fillStyleIdx0] = new();
            CurrentFillEdgeMap[fillStyleIdx0].AddRange(
                subPath.Select(e => e.ReverseWithStyle(fillStyleIdx0)).Reverse()
            );
        }

        if(fillStyleIdx1 != 0)
        {
            if(!CurrentFillEdgeMap.ContainsKey(fillStyleIdx1))CurrentFillEdgeMap[fillStyleIdx1] = new();
            
            CurrentFillEdgeMap[fillStyleIdx1].AddRange(subPath);
        }

        if(lineStyleIdx != 0)
        {
            if(!CurrentLineEdgeMap.ContainsKey(lineStyleIdx))CurrentLineEdgeMap[lineStyleIdx] = new();
            
            CurrentLineEdgeMap[lineStyleIdx].AddRange(subPath);
        }
    }

    public void CleanEdgeMap(EdgeMap edgeMap)
    {
        foreach(var (styleIdx,subPath) in edgeMap)
        {
            if(subPath.Count == 0) continue;
            IEdge? prevEdge = null;
            IEdge? edge = null;
            Path tmpPath = new();
            CreateCoordMap(subPath);
            CreateReverseCoordMap(subPath);
            while(subPath.Count > 0)
            {
                var idx = 0;
                while(idx < subPath.Count)
                {
                    if(prevEdge is not null)
                    {
                        if(prevEdge.To != subPath[idx].From)
                        {
                            edge = FindNextEdgeInCoordMap(prevEdge);
                            if(edge is not null)
                            {
                                idx = subPath.IndexOf(edge);
                            }
                            else
                            {
                                var revEdge = FindNextEdgeInReverseCoordMap(prevEdge);
                                if(revEdge is not null)
                                {
                                    idx = subPath.IndexOf(revEdge);
                                    var r = revEdge.ReverseWithStyle(revEdge.FillStyleIdx);
                                    UpdateEdgeInCoordMap(revEdge, r);
                                    UpdateEdgeInReverseCoordMap(revEdge, r);
                                    subPath[idx] = r;
                                }
                                else
                                {
                                    idx = 0;
                                    prevEdge = null;
                                }
                            }
                            
                            continue;
                        }
                    }

                    edge = subPath[idx];
                    subPath.RemoveAt(idx);
                    tmpPath.Add(edge);
                    RemoveEdgeFromCoordMap(edge);
                    RemoveEdgeFromReverseCoordMap(edge);
                    prevEdge = edge;
                }
            }
            edgeMap[styleIdx] = tmpPath;
        }
    }

    public IEdge? FindNextEdgeInCoordMap(IEdge edge)
    {
        var key = edge.To;
        Path? path = null;
        if(!CoordMap.TryGetValue(key, out path))
            return null;
        if(path.Count == 0)
            return null;
        return path[0];
    }

    public IEdge? FindNextEdgeInReverseCoordMap(IEdge edge)
    {
        var key = edge.To;
        Path? path = null;
        if(!ReverseCoordMap.TryGetValue(key, out path))
            return null;
        if(path.Count == 0)
            return null;
        return path[0];
    }

    public void RemoveEdgeFromCoordMap(IEdge edge)
    {
        var key = edge.From;
        if(CoordMap.ContainsKey(key))
        {
            if(CoordMap[key].Count == 1)CoordMap.Remove(key);
            else CoordMap[key].Remove(edge);
        }
    }

    public void RemoveEdgeFromReverseCoordMap(IEdge edge)
    {
        var key = edge.To;
        if(ReverseCoordMap.ContainsKey(key))
        {
            if(ReverseCoordMap[key].Count == 1)ReverseCoordMap.Remove(key);
            else ReverseCoordMap[key].Remove(edge);
        }
    }

    public void CreateCoordMap(Path path)
    {
        CoordMap.Clear();
        for(int i = 0; i < path.Count; ++i)
        {
            var key = path[i].From;
            if(!CoordMap.ContainsKey(key))CoordMap[key] = new();
            CoordMap[key].Add(path[i]);
        }
    }

    public void CreateReverseCoordMap(Path path)
    {
        ReverseCoordMap.Clear();
        for(int i = 0; i < path.Count; ++i)
        {
            var key = path[i].To;
            if(!ReverseCoordMap.ContainsKey(key))ReverseCoordMap[key] = new();
            ReverseCoordMap[key].Add(path[i]);
        }
    }

    public void UpdateEdgeInCoordMap(IEdge edge, IEdge newEdge)
    {
        var key1 = edge.From;
        CoordMap[key1].Remove(edge);
        var key2 = newEdge.From;
        if(!CoordMap.ContainsKey(key2))CoordMap[key2] = new();
        CoordMap[key2].Add(newEdge);
    }

    public void UpdateEdgeInReverseCoordMap(IEdge edge, IEdge newEdge)
    {
        var key1 = edge.To;
        ReverseCoordMap[key1].Remove(edge);
        var key2 = newEdge.To;
        if(!ReverseCoordMap.ContainsKey(key2))ReverseCoordMap[key2] = new();
        ReverseCoordMap[key2].Add(newEdge);
    }

    public Path PathFromEdgeMap(EdgeMap edgeMap) => 
        edgeMap.Keys.OrderBy(i=>i).SelectMany(i=>edgeMap[i]).ToList();
}