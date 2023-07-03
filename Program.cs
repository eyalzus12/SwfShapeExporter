using SwfShapeExporter;

if(args.Length < 3)
{
    Console.WriteLine("Too little arguments given. Format: SOURCE_PATH SPRITE_NAME TARGET_PATH");
    return;
}

if(args.Length > 3)
{
    Console.WriteLine("Too many arguments given. Format: SOURCE_PATH SPRITE_NAME TARGET_PATH");
    return;
}

string read_path = args[0];
string sprite_name = args[1];
string target_path = args[2];

SwfLoader loader;
using(FileStream read = new(read_path, FileMode.Open, FileAccess.Read))
{
    loader = new(read);
}

var spriteTag = loader.SpriteTagFromSymbolName(sprite_name);
var placeObjects = loader.GetPlaceObjectTags(spriteTag);
var shape = loader.GetShapeTagFromPlaceObjectTag(placeObjects.First());

var image = ShapeToImageConverter.RenderShape(shape);

using(FileStream write = new(target_path, FileMode.Create, FileAccess.Write))
{
    image.SaveAsPng(write);
}

Console.WriteLine("Done");
