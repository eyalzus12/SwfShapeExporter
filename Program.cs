using SwfShapeExporter;

const string SHAPE_NAME = "a_DemonAnimation_IdleHeavyFrame14";
const string READ_PATH = @"C:/Program Files (x86)/Steam/steamapps/common/Brawlhalla/bones/Bones_GameModes.swf";
const string WRITE_PATH = @"C:/Program Files (x86)/Steam/steamapps/common/Brawlhalla/SwfTest.png";

FileStream read = new(READ_PATH, FileMode.Open, FileAccess.Read);
SwfLoader loader = new(read);
var shape = loader.GetShapeTagFromSpriteName(SHAPE_NAME);

FileStream write = new(WRITE_PATH, FileMode.Create, FileAccess.Write);
ShapeStreamWriter.ShapeToStreamPNG(shape, write);

