using System;
using System.IO;

namespace FontStash.NET
{

    public delegate void HandleError(FonsErrorCode error, int val);

    public sealed partial class Fontstash : IDisposable
    {

        public const int INVALID = -1;

        internal const int SCRATCH_BUF_SIZE = 96000;
        internal const int HASH_LUT_SIZE = 256;
        internal const int INIT_FONTS = 4;
        internal const int INIT_GLYPHS = 256;
        internal const int INIT_ATLAS_NODES = 256;
        internal const int VERTEX_COUNT = 1024;
        internal const int MAX_STATES = 20;
        internal const int MAX_FALLBACKS = 20;

        // Meta
        private readonly FonsParams _params;
        private float _itw, _ith;

        // Texture
        private readonly int[] _dirtyRect = new int[4];
        private byte[] _texData;

        // Fonts
        private FonsFont[] _fonts;
        private FonsAtlas _atlas;
        private int _cfonts;
        private int _nfonts;

        // Rendering
        private readonly float[] _verts = new float[VERTEX_COUNT * 2];
        private readonly float[] _tcoords = new float[VERTEX_COUNT * 2];
        private readonly uint[] _colours = new uint[VERTEX_COUNT * 2];
        private int _nverts;

        // Scratch Buffer
        private byte[] _scratch;
        private int _nscratch;

        // States
        private readonly FonsState[] _states = new FonsState[MAX_STATES];
        private int _nstates;

        // Error Handling
        private HandleError _handleError;

        #region Constructor and Destructor
        public Fontstash(FonsParams @params)
        {
            _params = @params;

            _scratch = new byte[SCRATCH_BUF_SIZE];

            if (_params.RenderCreate != null)
            {
                if (!_params.RenderCreate.Invoke(_params.Width, _params.Height))
                {
                    Dispose();
                    throw new Exception("Failed to create fontstash!");
                }
            }

            _atlas = new FonsAtlas(_params.Width, _params.Height, INIT_ATLAS_NODES);

            _fonts = new FonsFont[INIT_FONTS];
            _cfonts = INIT_FONTS;
            _nfonts = 0;

            _itw = 1.0f / _params.Width;
            _ith = 1.0f / _params.Height;
            _texData = new byte[_params.Width * _params.Height];

            _dirtyRect[0] = _params.Width;
            _dirtyRect[1] = _params.Height;
            _dirtyRect[2] = 0;
            _dirtyRect[3] = 0;

            AddWhiteRect(2, 2);

            PushState();
            ClearState();
        }

        public void Dispose()
        {

        }
        #endregion

        #region Error
        public void SetErrorCallback()
        {

        }
        #endregion

        #region Metrics
        public void GetAtlasSize()
        {

        }

        public bool ExpandAtlas()
        {
            return true;
        }

        public bool ResetAtlas()
        {
            return true;
        }
        #endregion

        #region Add Fonts
        public int AddFont(string name, string path, int fontIndex)
        {
            if (!File.Exists(path))
                return INVALID;

            byte[] data = File.ReadAllBytes(path);
            return AddFontMem(name, data, 1, fontIndex);
        }

        public int AddFontMem(string name, byte[] data, int freeData, int fontIndex)
        {
            int idx = AllocFont();
            if (idx == INVALID)
                return INVALID;

            FonsFont font = _fonts[idx];

            font.name = name;

            for ( int i = 0; i < HASH_LUT_SIZE; i++)
            {
                font.lut[i] = -1;
            }

            font.dataSize = data.Length;
            font.data = data;
            font.freeData = (byte)freeData;

            _nscratch = 0;
            if (FonsTt.LoadFont(font.font, data, fontIndex) == 0)
            {
                _nfonts--;
                return INVALID;
            }

            FonsTt.GetFontVMetrics(font.font, out int ascent, out int descent, out int lineGap);
            ascent += lineGap;
            int fh = ascent - descent;
            font.ascender = (float)ascent / (float)fh;
            font.descender = (float)descent / (float)fh;
            font.lineh = font.ascender - font.descender;

            return idx;
        }

        public int GetFontByName()
        {
            return 0;
        }

        public int AddFallbackFont()
        {
            return 0;
        }
        #endregion

        #region State Handling
        public void PushState()
        {
            if (_nstates >= MAX_STATES)
            {
                _handleError?.Invoke(FonsErrorCode.StatesOverflow, 0);
                return;
            }

            if (_nstates > 0)
            {
                _states[_nstates - 1] = _states[_nstates].Copy();
            }
            _nstates++;
        }

        public void PopState()
        {
            if (_nstates <= 1)
            {
                _handleError.Invoke(FonsErrorCode.StatesUnderflow, 0);
                return;
            }
            _nstates--;
        }

        public void ClearState()
        {
            FonsState state = GetState();
            state.size = 12.0f;
            state.colour = 0xffffffff;
            state.font = 0;
            state.blur = 0;
            state.spacing = 0;
            state.align = (int)FonsAlign.Left | (int)FonsAlign.Baseline;
        }
        #endregion

        #region State Settings
        public void SetSize(float size)
        {
            GetState().size = size;
        }

        public void SetColour(uint colour)
        {
            GetState().colour = colour;
        }

        public void SetSpacing(float spacing)
        {
            GetState().spacing = spacing;
        }

        public void SetBlur(float blur)
        {
            GetState().blur = blur;
        }

        public void SetAlign(int align)
        {
            GetState().align = align;
        }

        public void SetFont(int font)
        {
            GetState().font = font;
        }
        #endregion

        #region Draw Text
        public float DrawText(float x, float y, string str, char end)
        {
            FonsState state = GetState();
            uint codepoint = 0;
            uint utf8state = 0;
            FonsGlyph glyph = null;
            int prevGlyphIndex = INVALID;
            short isize = (short)(state.size * 10.0f);
            short iblur = (short)state.blur;

            if (state.font < 0 || state.font >= _nfonts)
                return x;
            FonsFont font = _fonts[state.font];
            if (font.data == null)
                return x;

            float scale = FonsTt.GetPixelHeightScale(font.font, (float)isize / 10.0f);

            // Horizontal alignment
            if ((state.align & (int)FonsAlign.Left) != 0)
            {
                // empty
            } else if ((state.align & (int)FonsAlign.Right) != 0)
            {
                // TODO
            } else if ((state.align & (int)FonsAlign.Center) != 0)
            {
                // TODO
            }

            y += GetVertAlign(font, state.align, isize);

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (c == end)
                    break;

                if (Utf8.DecUtf8(ref utf8state, ref codepoint, c) != 0)
                    continue;
                glyph = GetGlyph(font, codepoint, isize, iblur, FonsGlyphBitmap.Requiered);
                if (glyph != null)
                {
                    FonsQuad q = GetQuad(font, prevGlyphIndex, glyph, scale, state.spacing, ref x, ref y);

                    if (_nverts + 6 > VERTEX_COUNT)
                        Flush();

                    Vertex(q.x0, q.y0, q.s0, q.t0, state.colour);
                    Vertex(q.x1, q.y1, q.s1, q.t1, state.colour);
                    Vertex(q.x1, q.y0, q.s1, q.t0, state.colour);

                    Vertex(q.x0, q.y0, q.s0, q.t0, state.colour);
                    Vertex(q.x0, q.y1, q.s0, q.t1, state.colour);
                    Vertex(q.x1, q.y1, q.s1, q.t1, state.colour);
                }

                prevGlyphIndex = glyph != null ? glyph.index : -1;
            }

            Flush();

            return 0.0f;
        }
        #endregion

        #region Measure Text
        public float TextBounds(float x, float y, string str, string end, ref float[] bounds)
        {
            return 0.0f;
        }

        public void LineBounds()
        {

        }

        public void VertMetrics(out float ascender, out float descender, out float lineh)
        {
            FonsState state = GetState();
            ascender = descender = lineh = INVALID;

            if (state.font < 0 || state.font >= _nfonts)
                return;
            FonsFont font = _fonts[state.font];
            short isize = (short)(state.size * 10.0f);
            if (font.data == null)
                return;

            ascender = font.ascender * isize / 10.0f;
            descender = font.descender * isize / 10.0f;
            lineh = font.lineh * isize / 10.0f;
        }
        #endregion

        #region Text iterator
        public bool TextIterInit()
        {
            return true;
        }

        public bool TextIterNext()
        {
            return true;
        }
        #endregion

        #region Pull Texture Changes
        public string GetTextureData()
        {
            return null;
        }

        public bool ValidateTexture()
        {
            return true;
        }
        #endregion

        #region Debug
        public void DrawDebug()
        {

        }
        #endregion

    }
}
