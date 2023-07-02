using SwfLib;
using SwfLib.Tags;
using SwfLib.Tags.ControlTags;
using SwfLib.Tags.DisplayListTags;
using SwfLib.Tags.ShapeTags;

namespace SwfShapeExporter;

public class SwfLoader
{
    public SwfLoader(Stream stream, bool initSymbolClass = true)
    {
        ReadFrom(stream, initSymbolClass);
    }

    SwfFile? SWF{get; set;}
    public void ReadFrom(Stream stream, bool initSymbolClass = true)
    {
        SWF = SwfFile.ReadFrom(stream);
        if(initSymbolClass) InitSymbolClass();
    }

    SymbolClassTag? SymbolClass{get; set;}
    public void InitSymbolClass()
    {
        if(SWF is null) throw new ArgumentException("Attempt to init symbol class with a null SWF file. Run ReadFrom first");
        SymbolClass = SWF.Tags.FirstOrDefault(t => t is SymbolClassTag) as SymbolClassTag;
        if(SymbolClass is null)
        {
            throw new ArgumentException("Could not find SymbolClass in the given SWF file");
        }
    }

    public ShapeBaseTag GetShapeTagFromSpriteName(string spriteName)
    {
        var ID = GetSymbolID(spriteName);
        var sprite = GetSpriteTag(ID);
        var place = GetFirstPlaceObjectTag(sprite);
        var shape = GetShapeTagFromPlaceObjectTag(place);
        return shape;
    }

    public ushort GetSymbolID(string symbolName)
    {
        if(SymbolClass is null) throw new ArgumentException("Attempt to find symbol ID with a null SymbolClass. Run InitSymbolClass first");
        ushort? symbolID = SymbolClass.References.FirstOrDefault(r => r.SymbolName == symbolName)?.SymbolID;
        if(symbolID is null) throw new ArgumentException($"Could not find symbol ID of symbol {symbolName}");
        return (ushort)symbolID;
    }

    public DefineSpriteTag GetSpriteTag(ushort spriteID)
    {
        if(SWF is null) throw new ArgumentException("Attempt to find sprite tag with a null SWF file. Run ReadFrom first");
        DefineSpriteTag? sprite = SWF.Tags.FirstOrDefault(t => t is DefineSpriteTag st && st.SpriteID == spriteID) as DefineSpriteTag;
        if(sprite is null) throw new ArgumentException($"Attempt could not find sprite tag with ID {spriteID}");
        return sprite;
    }

    public PlaceObjectBaseTag GetFirstPlaceObjectTag(DefineSpriteTag sprite)
    {
        PlaceObjectBaseTag? place = sprite.Tags.FirstOrDefault(t => t is PlaceObjectBaseTag) as PlaceObjectBaseTag;
        if(place is null) throw new ArgumentException("Given sprite does not have any place object tags");
        return place;
    }

    public ShapeBaseTag GetShapeTagFromPlaceObjectTag(PlaceObjectBaseTag place)
    {
        if(SWF is null) throw new ArgumentException("Attempt to find shape tag with a null SWF file. Run ReadFrom first");
        var characterID = place.CharacterID;
        var shape = SWF.Tags.FirstOrDefault(t => t is ShapeBaseTag st && st.ShapeID == characterID) as ShapeBaseTag;
        if(shape is null) throw new ArgumentException("Could not find the define shape tag for the given place object tag");
        return shape;
    }
}