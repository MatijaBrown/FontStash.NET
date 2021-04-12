using Silk.NET.OpenGL;
using System;

namespace FontStash.NET.GL
{
    public sealed class GLFons : IDisposable
    {

        private const int VERTEX_ATTRIB = 0;
        private const int TCOORD_ATTRIB = 1;
        private const int COLOUR_ATTRIB = 2;

        private uint _tex;
        private int _width, _height;
        private uint _vertexArray;
        private uint _vertexBuffer;
        private uint _tcoordBuffer;
        private uint _colourBuffer;
        private Fontstash _fs;

        private readonly Silk.NET.OpenGL.GL _gl;

        public GLFons(Silk.NET.OpenGL.GL gl)
        {
            _gl = gl;
        }

        private unsafe bool RenderCreate(int width, int height)
        {
            if (_tex == 0)
            {
                _gl.DeleteTexture(_tex);
                _tex = 0;
            }
            _tex = _gl.GenTexture();
            if (_tex == 0)
                return false;

            if (_vertexArray == 0)
                _vertexArray = _gl.GenVertexArray();
            if (_vertexArray == 0)
                return false;

            _gl.BindVertexArray(_vertexArray);

            if (_vertexBuffer == 0)
                _vertexBuffer = _gl.GenBuffer();
            if (_vertexBuffer == 0)
                return false;
            if (_tcoordBuffer == 0)
                _tcoordBuffer = _gl.GenBuffer();
            if (_tcoordBuffer == 0)
                return false;
            if (_colourBuffer == 0)
                _colourBuffer = _gl.GenBuffer();
            if (_colourBuffer == 0)
                return false;

            _width = width;
            _height = height;
            _gl.BindTexture(TextureTarget.Texture2D, _tex);
            _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Red, (uint)_width, (uint)_height, 0, GLEnum.Red, GLEnum.UnsignedByte, null);
            _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            int[] swizzleRgbaParams = { (int)GLEnum.One, (int)GLEnum.One, (int)GLEnum.One, (int)GLEnum.Red };
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleRgba, swizzleRgbaParams);

            return true;
        }

        private bool RenderResize(int width, int height)
        {
            return RenderCreate(width, height);
        }

        private unsafe void RenderUpdate(int[] rect, byte[] data)
        {
            int w = rect[2] - rect[0];
            int h = rect[3] - rect[1];

            if (_tex == 0)
                return;

            int alignement, rowLength, skipPixels, skipRows;
            alignement = _gl.GetInteger(GLEnum.UnpackAlignment);
            rowLength = _gl.GetInteger(GLEnum.UnpackRowLength);
            skipPixels = _gl.GetInteger(GLEnum.UnpackSkipPixels);
            skipRows = _gl.GetInteger(GLEnum.UnpackSkipRows);

            _gl.BindTexture(TextureTarget.Texture2D, _tex);

            _gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            _gl.PixelStore(PixelStoreParameter.UnpackRowLength, _width);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipPixels, rect[0]);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipRows, rect[1]);

            fixed (byte* d = data)
            {
                _gl.TexSubImage2D(GLEnum.Texture2D, 0, rect[0], rect[1], (uint)w, (uint)h, GLEnum.Red, GLEnum.UnsignedByte, d);
            }

            _gl.PixelStore(PixelStoreParameter.UnpackAlignment, alignement);
            _gl.PixelStore(PixelStoreParameter.UnpackRowLength, rowLength);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipPixels, skipPixels);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipRows, skipRows);
        }

        private unsafe void RenderDraw(float[] verts, float[] tcoords, uint[] colours, int nverts)
        {
            if (_tex == 0 || _vertexArray == 0)
                return;

            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, _tex);

            _gl.BindVertexArray(_vertexArray);

            _gl.EnableVertexAttribArray(VERTEX_ATTRIB);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBuffer);
            fixed (float* d = verts)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(nverts * 2 * sizeof(float)), d, BufferUsageARB.DynamicDraw);
            }
            _gl.VertexAttribPointer(VERTEX_ATTRIB, 2, GLEnum.Float, false, 0, null);

            _gl.EnableVertexAttribArray(TCOORD_ATTRIB);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _tcoordBuffer);
            fixed (float* d = tcoords)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(nverts * 2 * sizeof(float)), d, BufferUsageARB.DynamicDraw);
            }
            _gl.VertexAttribPointer(TCOORD_ATTRIB, 2, GLEnum.Float, false, 0, null);

            _gl.EnableVertexAttribArray(COLOUR_ATTRIB);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _colourBuffer);
            fixed (uint* d = colours)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(nverts * sizeof(uint)), d, BufferUsageARB.DynamicDraw);
            }
            _gl.VertexAttribPointer(COLOUR_ATTRIB, 4, GLEnum.UnsignedByte, false, 0, null);

            _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)nverts);

            _gl.DisableVertexAttribArray(VERTEX_ATTRIB);
            _gl.DisableVertexAttribArray(TCOORD_ATTRIB);
            _gl.DisableVertexAttribArray(COLOUR_ATTRIB);

            _gl.BindVertexArray(0);
        }

        private void RenderDelete()
        {
            if (_tex != 0)
            {
                _gl.DeleteTexture(_tex);
                _tex = 0;
            }

            _gl.BindVertexArray(0);

            if (_vertexBuffer != 0)
            {
                _gl.DeleteBuffer(_vertexBuffer);
                _vertexBuffer = 0;
            }
            if (-_tcoordBuffer != 0)
            {
                _gl.DeleteBuffer(_tcoordBuffer);
                _tcoordBuffer = 0;
            }
            if (_colourBuffer != 0)
            {
                _gl.DeleteBuffer(_colourBuffer);
                _colourBuffer = 0;
            }
            if (_vertexArray != 0)
            {
                _gl.DeleteVertexArray(_vertexArray);
                _vertexArray = 0;
            }
        }

        public Fontstash Create(int width, int height, int flags)
        {
            FonsParams prams = default;
            prams.width = width;
            prams.height = height;
            prams.flags = (byte)flags;
            prams.renderCreate = RenderCreate;
            prams.renderResize = RenderResize;
            prams.renderUpdate = RenderUpdate;
            prams.renderDraw = RenderDraw;
            prams.renderDelete = RenderDelete;

            _fs = new Fontstash(prams);
            return _fs;
        }

        public void Dispose()
        {
            _fs.Dispose();
        }

        public static uint Rgba(byte r, byte g, byte b, byte a)
        {
            return (uint)((r) | (g << 8) | (b << 16) | (a << 24));
        }

    }
}
