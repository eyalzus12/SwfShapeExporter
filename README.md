A simple C# library to export SWF vector shapes

**This library is a WIP**

# Currently not implemented:
* DefineShape4
* Fill styles other than Solid
* SVG exporting

# Dependencies
* SwfLib - Used for reading SWF data
* SixLabors.ImageSharp - Cross platform alternative for System.Drawing.Common
* SixLabors.ImageSharp.Drawing - Extension for SixLabors.ImageSharp

# Usage
The library is written for general usage, but a simple CLI program is also available.

CLI argument format: SOURCE_PATH SPRITE_NAME TARGET_PATH

The program will read the swf file from SOURCE_PATH, find the shape of the first place object of the SPRITE_NAME sprite, render it into an image, and output it as a png into TARGET_PATH.

