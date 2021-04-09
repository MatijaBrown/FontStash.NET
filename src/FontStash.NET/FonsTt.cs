using StbTrueTypeSharp;

namespace FontStash.NET
{
    internal static class FonsTt
    {

        public static unsafe int LoadFont(FonsTtImpl font, byte[] data, int fontIndex)
        {
            int offset, stbError;
            font.font = new StbTrueType.stbtt_fontinfo();
            fixed (byte* d = data)
            {
                offset = StbTrueType.stbtt_GetFontOffsetForIndex(d, fontIndex);
                if (offset == Fontstash.INVALID)
                {
                    stbError = 0;
                }
                else
                {
                    stbError = StbTrueType.stbtt_InitFont(font.font, d, offset);
                }
            }
            return stbError;
        }

        public static unsafe void GetFontVMetrics(FonsTtImpl font, out int ascent, out int descent, out int linegap)
        {
            int x, y, z;
            int* asc = &x, desc = &y, lg = &z;
            StbTrueType.stbtt_GetFontVMetrics(font.font, asc, desc, lg);
            ascent = *asc;
            descent = *desc;
            linegap = *lg;
        }

        public static float GetPixelHeightScale(FonsTtImpl font, float size)
        {
            return StbTrueType.stbtt_ScaleForMappingEmToPixels(font.font, size);
        }

        public static int GetGlyphIndex(FonsTtImpl font, int codepoint)
        {
            return StbTrueType.stbtt_FindGlyphIndex(font.font, codepoint);
        }

        public static unsafe bool BuildGlyphBitmap(FonsTtImpl font, int glyph, float scale, out int advance, out int lsb, out int x0, out int y0, out int x1, out int y1)
        {
            int x, y, z, u, v, s;
            int* get_adv = &x, get_lsb = &y, get_x0 = &z, get_y0 = &u, get_x1 = &v, get_y1 = &s;
            StbTrueType.stbtt_GetGlyphHMetrics(font.font, glyph, get_adv, get_lsb);
            StbTrueType.stbtt_GetGlyphBitmapBox(font.font, glyph, scale, scale, get_x0, get_y0, get_x1, get_y1);
            advance = *get_adv;
            lsb = *get_lsb;
            x0 = *get_x0;
            y0 = *get_y0;
            x1 = *get_x1;
            y1 = *get_y1;
            return true;
        }

        public static unsafe void RenderGlyphBitmap(FonsTtImpl font, byte[] texData, int startIndex, int outWidth, int outHeight, int outStride, float scaleX, float scaleY, int glyph)
        {
            fixed (byte* outp = &texData[startIndex])
            {
                StbTrueType.stbtt_MakeGlyphBitmap(font.font, outp, outWidth, outHeight, outStride, scaleX, scaleY, glyph);
            }
        }

        public static int GetGlyphKernAdvance(FonsTtImpl font, int glyph1, int glyph2)
        {
            return StbTrueType.stbtt_GetGlyphKernAdvance(font.font, glyph1, glyph2);
        }

    }
}
