# FontStash.NET
A port of [memononen/fontstash](https://github.com/memononen/fontstash) to C# for [SilkyNvg](https://github.com/MatijaBrown/SilkyNvg).

## Usage / Examples
> Examples can be found in the 'samples' directory.

### Creating a new context
Information about the desired font atlas and the callbacks used for rendering
are stored in the ```FonsParams``` struct.
```cs
FonsParams prams;
// the atlas's initial size
prams.Width = 512;
prams.Height = 512;
// where the (0|0) on the atlas will be
prams.flags = (byte)FonsFlags.ZeroTopLeft;
// Callbacks for creating, resisizing, updating, drawing and deleting the atlas
prams.RenderCreate = Create;
prams.RenderResize = Resize;
prams.RenderUpdate = Update;
prams.RenderDraw = Draw;
prams.RenderDelete = Delete;
```
Examples for implementations of these methods can be found in either the
``src/FontStash.NET.GL.Legacy`` (for people not used to OpenGL) or
``src/FontStash.NET.GL`` (for how to render in modern OpenGL) projects.

To finally create the Fontstash context and instance, do the following:
```cs
var fons = new Fontstash(prams);
```

Now, one can use the Fontstash instance the same as [memononen/fontstash](https://github.com/memononen/fontstash),
apart from not having to parse a context with every call, as the context information is stored per api-instance.
```cs
// Create a new font
// Set last parameter to 0 always, incase using StbTrueType font indices.
int fornNormal = fons.CreateFont("testFont", "./fonts/verdana.ttf", 0);

// Rendering method here
// Colours are stored as 4 bytes (rgba) next to each other in a uint.
private uint GetColour(byte r, byte g, byte b, byte a) => return (uint)((r) | (g << 8) | (b << 16) | (a << 24));
uint fontColourRed = GetColour(255, 0, 0, 255);

// Render "I am the walrus!"
fons.SetFont(fontNormal);
fons.SetSize(72.0f);
fons.SetColour(fontColourRed);
// FontStash.DrawText(float, float string) returns the end X-Position of the rendered string on the window.
float endX = fons.DrawText(20, 100, "I am the walrus!");

// render the font atlas in it's current state
fons.DrawDebug(800, 200);
```

#### OpenGL utillities
When using OpenGL FontStash.NET, the same as fontstash, provids utillity classes
to aid rendering. They are located in either ``src/FontStash.NET.GL.Legacy`` for legacy OpenGL
and ``src/FontStash.NET.GL`` for modern OpenGL respectively. These use [Silk.NET](https://github.com/dotnet/Silk.NET)
for rendering, so a compatible OpenGL object must be parsed.
```cs
GLFons glFons = new GLFons(gl);
Fontstash fs = glFons.Create(512, 512, (int)FonsFlags.ZeroTopleft);
```
The example seen above has the same effect as the "manual" example.

### Examples
Two example projects using OpenGL can be found in 'samples' directory.<br/><br/>
*Without debug atlas displayed*<br/><br/>
![Example without Debug](/docs/images/example_nodebug.PNG)
*With debug atlas displayed*<br/><br/>
![Example without Debug](/docs/images/example_debug.PNG)

## Credits
Obviously [mnemononen/fontstash](https://github.com/memononen/fontstash)<br/><br/>
[StbTrueTypeSharp](https://github.com/StbSharp/StbTrueTypeSharp) for the StbTrueType implementation<br/><br/>
[Silk.NET](https://github.com/dotnet/Silk.NET) for the OpenGL implementation in the helper classes.<br/><br/>

## License
> FontStash.NET uses the MIT-License<br/><br/>
> fontstash uses the ZLib-License<br/><br/>