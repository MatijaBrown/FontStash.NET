using Silk.NET.OpenGL.Legacy;
using System;

namespace FontStash.NET.GL.Legacy
{
    public sealed class GLFons : IDisposable
    {

        private readonly Silk.NET.OpenGL.Legacy.GL _gl;

        private Fontstash _fons;
        private uint _tex;
        private int _width, _height;

        public GLFons(Silk.NET.OpenGL.Legacy.GL gl)
        {
            _gl = gl;
        }

        private unsafe bool RenderCreate(int width, int height)
        {
            if (_tex != 0)
            {
                _gl.DeleteTexture(_tex);
                _tex = 0;
            }
            _tex = _gl.GenTexture();
            if (_tex == 0)
                return false;
            _width = width;
            _height = height;
            _gl.BindTexture(TextureTarget.Texture2D, _tex);
            _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)GLEnum.Alpha, (uint)_width, (uint)_height, 0, GLEnum.Alpha, GLEnum.UnsignedByte, (void*)0);
            _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
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
            _gl.PushClientAttrib((uint)ClientAttribMask.ClientPixelStoreBit);
            _gl.BindTexture(TextureTarget.Texture2D, _tex);
            _gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            _gl.PixelStore(PixelStoreParameter.UnpackRowLength, _width);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipPixels, rect[0]);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipRows, rect[1]);
            fixed (byte* d = data)
            {
                _gl.TexSubImage2D(TextureTarget.Texture2D, 0, rect[0], rect[1], (uint)w, (uint)h, GLEnum.Alpha, GLEnum.UnsignedByte, d);
            }
            _gl.PopClientAttrib();
        }

        private unsafe void RenderDraw(float[] verts, float[] tcoords, uint[] colours, int nverts)
        {
            _gl.BindTexture(TextureTarget.Texture2D, _tex);
            _gl.Enable(EnableCap.Texture2D);
            _gl.EnableClientState(EnableCap.VertexArray);
            _gl.EnableClientState(EnableCap.TextureCoordArray);
            _gl.EnableClientState(EnableCap.ColorArray);

            fixed (float* d = verts)
            {
                _gl.VertexPointer(2, GLEnum.Float, sizeof(float) * 2, d);
            }
            fixed (float* tc = tcoords)
            {
                _gl.TexCoordPointer(2, GLEnum.Float, sizeof(float) * 2, tc);
            }
            fixed (uint* col = colours)
            {
                _gl.ColorPointer(4, GLEnum.UnsignedByte, sizeof(uint), col);
            }

            _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)nverts);

            _gl.Disable(EnableCap.Texture2D);
            _gl.DisableClientState(EnableCap.VertexArray);
            _gl.DisableClientState(EnableCap.TextureCoordArray);
            _gl.DisableClientState(EnableCap.ColorArray);
        }

        private void RenderDelete()
        {
            if (_tex != 0)
                _gl.DeleteTexture(_tex);
            _tex = 0;
        }

        public Fontstash Create(int width, int height, int flags)
        {
            FonsParams prams = new()
            {
                Width = width,
                Height = height,
                Flags = (byte)flags,
                RenderCreate = RenderCreate,
                RenderResize = RenderResize,
                RenderUpdate = RenderUpdate,
                RenderDraw = RenderDraw,
                RenderDelete = RenderDelete
            };

            _fons = new Fontstash(prams);
            return _fons;
        }

        public void Dispose()
        {
            _fons.Dispose();
        }

        public static uint Rgba(byte r, byte g, byte b, byte a)
        {
            return (uint)((r) | (g << 8) | (b << 16) | (a << 24));
        }

    }
}
