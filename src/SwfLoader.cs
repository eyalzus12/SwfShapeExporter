using SwfLib;
using SwfLib.Tags;
using SwfLib.Tags.ControlTags;
using SwfLib.Tags.DisplayListTags;
using SwfLib.Tags.ShapeTags;

namespace SwfShapeExporter;

/// <summary>
/// A wrapper over SwfLib operations.
/// </summary>
public class SwfLoader
{
    /// <summary>
    /// Create an swf loader from a data stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="initSymbolClass">Whether to automatically initialize the symbol class.</param>
    public SwfLoader(Stream stream, bool initSymbolClass = true)
    {
        ReadFrom(stream, initSymbolClass);
    }

    SwfFile? SWF{get; set;}
    /// <summary>
    /// Creates an SwfFile from a stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="initSymbolClass">Whether to automatically initialize the symbol class.</param>
    public void ReadFrom(Stream stream, bool initSymbolClass = true)
    {
        SWF = SwfFile.ReadFrom(stream);
        if(initSymbolClass) InitSymbolClass();
    }

    SymbolClassTag? SymbolClass{get; set;}
    /// <summary>
    /// Initialize the symbol class. Must be done before any symbol ID lookups.
    /// </summary>
    public void InitSymbolClass()
    {
        if(SWF is null) throw new ArgumentException("Attempt to init symbol class with a null SWF file. Run ReadFrom first");
        SymbolClass = SWF.Tags.FirstOrDefault(t => t is SymbolClassTag) as SymbolClassTag;
        if(SymbolClass is null)
        {
            throw new ArgumentException("Could not find SymbolClass in the given SWF file");
        }
    }

    /// <summary>
    /// Searches the symbol class to find the symbol ID matching the symbol name.
    /// </summary>
    /// <param name="symbolName">The symbol name to search.</param>
    /// <returns>The symbol ID.</returns>
    public ushort GetSymbolID(string symbolName)
    {
        if(SymbolClass is null) throw new ArgumentException("Attempt to find symbol ID with a null SymbolClass. Run InitSymbolClass first");
        ushort? symbolID = SymbolClass.References.FirstOrDefault(r => r.SymbolName == symbolName)?.SymbolID;
        if(symbolID is null) throw new ArgumentException($"Could not find symbol ID of symbol {symbolName}");
        return (ushort)symbolID;
    }

    /// <summary>
    /// Finds the sprite tag for the sprite ID.
    /// </summary>
    /// <param name="spriteID">The sprite ID.</param>
    /// <returns>The sprite tag.</returns>
    public DefineSpriteTag GetSpriteTag(ushort spriteID)
    {
        if(SWF is null) throw new ArgumentException("Attempt to find sprite tag with a null SWF file. Run ReadFrom first");
        DefineSpriteTag? sprite = SWF.Tags.FirstOrDefault(t => t is DefineSpriteTag st && st.SpriteID == spriteID) as DefineSpriteTag;
        if(sprite is null) throw new ArgumentException($"Attempt could not find sprite tag with ID {spriteID}");
        return sprite;
    }

    /// <summary>
    /// Finds the sprite tag for a sprite name.
    /// </summary>
    /// <param name="symbolName">The sprite name.</param>
    /// <returns>The sprite tag.</returns>
    public DefineSpriteTag SpriteTagFromSymbolName(string symbolName) => GetSpriteTag(GetSymbolID(symbolName));

    /// <summary>
    /// Returns the list of place object tags for the given sprite tag.
    /// </summary>
    /// <param name="sprite">The sprite tag.</param>
    /// <returns>The list of place object tags.</returns>
    public List<PlaceObjectBaseTag> GetPlaceObjectTags(DefineSpriteTag sprite) =>
        sprite.Tags.OfType<PlaceObjectBaseTag>().ToList();

    /// <summary>
    /// Finds the shape tag for the given place object tag.
    /// </summary>
    /// <param name="place">The place object tag.</param>
    /// <returns>The shape tag.</returns>
    public ShapeBaseTag GetShapeTagFromPlaceObjectTag(PlaceObjectBaseTag place)
    {
        if(SWF is null) throw new ArgumentException("Attempt to find shape tag with a null SWF file. Run ReadFrom first");
        var characterID = place.CharacterID;
        var shape = SWF.Tags.FirstOrDefault(t => t is ShapeBaseTag st && st.ShapeID == characterID) as ShapeBaseTag;
        if(shape is null) throw new ArgumentException("Could not find the define shape tag for the given place object tag");
        return shape;
    }
}