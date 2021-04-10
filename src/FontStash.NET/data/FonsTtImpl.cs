using StbTrueTypeSharp;

namespace FontStash.NET
{
    public class FonsTtImpl
    {

        public StbTrueType.stbtt_fontinfo font;

        public FonsTtImpl()
        {
            font = new StbTrueType.stbtt_fontinfo();
        }

    }
}
