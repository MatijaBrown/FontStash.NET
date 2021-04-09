namespace FontStash.NET
{

    public delegate bool RenderCreate(int width, int height);
    public delegate bool RenderResize(int width, int height);
    public delegate void RenderUpdate(int[] rect, byte[] data);
    public delegate void RenderDraw(float[] verts, float[] tcoords, uint[] colours, int nverts);
    public delegate void RenderDelete();

    public struct FonsParams
    {

        public int Width { internal get; set; }
        public int Height { internal get; set; }

        public byte Flags { internal get; set; }

        public RenderCreate RenderCreate { internal get; set; }
        public RenderResize RenderResize { internal get; set; }
        public RenderUpdate RenderUpdate { internal get; set; }
        public RenderDraw RenderDraw { internal get; set; }
        public RenderDelete RenderDelete { internal get; set; }

    }
}
