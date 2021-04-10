using FontStash.NET;
using FontStash.NET.GL.Legacy;
using Silk.NET.GLFW;
using Silk.NET.OpenGL.Legacy;
using System;

namespace Error
{
    unsafe class Program
    {

        private static Glfw glfw;
        private static GL gl;

        private static Fontstash fs = null;
        private static int size = 90;

        private static void Dash(float dx, float dy)
        {
            gl.Begin(GLEnum.Lines);
            gl.Color4(0, 0, 0, 0.5f);
            gl.Vertex2(dx - 5, dy);
            gl.Vertex2(dx - 10, dy);
            gl.End();
        }

        private static void Line(float sx, float sy, float ex, float ey)
        {
            gl.Begin(GLEnum.Lines);
            gl.Color4(0, 0, 0, 0.5f);
            gl.Vertex2(sx, sy);
            gl.Vertex2(ex, ey);
            gl.End();
        }

        private static void ExpandAtlas()
        {
            fs.GetAtlasSize(out int w, out int h);
            if (w < h)
                w *= 2;
            else
                h *= 2;
            fs.ExpandAtlas(w, h);
            Console.WriteLine("Expanded atlas to " + w + " X " + h);
        }

        private static void ResetAtlas()
        {
            fs.ResetAtlas(256, 256);
            Console.WriteLine("Reset atlas to 256 X 256");
        }

        private static void Key(WindowHandle* window, Keys key, int scancode, InputAction action, KeyModifiers mods)
        {
            if (key == Keys.Escape && action == InputAction.Press)
            {
                glfw.SetWindowShouldClose(window, true);
            }
            if (key == Keys.E && action == InputAction.Press)
            {
                ExpandAtlas();
            }
            if (key == Keys.R && action == InputAction.Press)
            {
                ResetAtlas();
            }
            if (key == Keys.Up && action == InputAction.Press)
            {
                size += 10;
            }
            if (key == Keys.Down && action == InputAction.Press)
            {
                size -= 10;
                if (size < 20)
                    size = 20;
            }
        }

        private static void StashError(FonsErrorCode error, int val)
        {
            switch (error)
            {
                case FonsErrorCode.AtlasFull:
                    Console.WriteLine("Atlas full!");
                    ExpandAtlas();
                    break;
                case FonsErrorCode.ScratchFull:
                    Console.WriteLine("Scratch full, tried to allocate " + val + " has " + Fontstash.SCRATCH_BUF_SIZE);
                    break;
                case FonsErrorCode.StatesOverflow:
                    Console.WriteLine("States overflow!");
                    break;
                case FonsErrorCode.StatesUnderflow:
                    Console.WriteLine("States underflow");
                    break;
            }
        }

        static void Main(string[] args)
        {
            glfw = Glfw.GetApi();

            int fontNormal = Fontstash.INVALID;
            int fontItalic = Fontstash.INVALID;
            int fontBold = Fontstash.INVALID;
            WindowHandle* window;
            VideoMode* mode;

            if (!glfw.Init())
                Environment.Exit(-1);

            mode = glfw.GetVideoMode(glfw.GetPrimaryMonitor());
            window = glfw.CreateWindow(mode->Width - 40, mode->Height - 80, "FontStash.NET", null, null);
            if (window == null)
            {
                glfw.Terminate();
                Environment.Exit(-1);
            }

            glfw.SetKeyCallback(window, Key);
            glfw.MakeContextCurrent(window);

            gl = GL.GetApi(new GlfwContext(glfw, window));

            GLFons glfons = new(gl);
            fs = glfons.Create(512, 512, (int)FonsFlags.ZeroTopleft);

            fs.SetErrorCallback(StashError);

            fontNormal = fs.AddFont("sans", "./fonts/DroidSerif-Regular.ttf", 0);
            if (fontNormal == Fontstash.INVALID)
            {
                Console.Error.WriteLine("Could not add font normal!");
                Environment.Exit(-1);
            }
            fontItalic = fs.AddFont("sans-italic", "./fonts/DroidSerif-Italic.ttf", 0);
            if (fontNormal == Fontstash.INVALID)
            {
                Console.Error.WriteLine("Could not add font italic!");
                Environment.Exit(-1);
            }
            fontBold = fs.AddFont("sans-bold", "./fonts/DroidSerif-Bold.ttf", 0);
            if (fontNormal == Fontstash.INVALID)
            {
                Console.Error.WriteLine("Could not add font bold!");
                Environment.Exit(-1);
            }

            while (!glfw.WindowShouldClose(window))
            {
                glfw.GetFramebufferSize(window, out int width, out int height);

                gl.Viewport(0, 0, (uint)width, (uint)height);
                gl.ClearColor(0.3f, 0.3f, 0.32f, 1.0f);
                gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.DepthBufferBit);
                gl.Enable(EnableCap.Blend);
                gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                gl.Disable(EnableCap.Texture2D);
                gl.MatrixMode(MatrixMode.Projection);
                gl.LoadIdentity();
                gl.Ortho(0, width, height, 0, -1, 1);

                gl.MatrixMode(MatrixMode.Modelview);
                gl.LoadIdentity();
                gl.Disable(EnableCap.DepthTest);
                gl.Color4(255, 255, 255, 255);
                gl.Enable(EnableCap.Blend);
                gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                gl.Enable(EnableCap.CullFace);

                uint white = GLFons.Rgba(255, 255, 255, 255);
                uint brown = GLFons.Rgba(192, 128, 0, 128);
                uint blue = GLFons.Rgba(0, 192, 255, 255);
                uint black = GLFons.Rgba(0, 0, 0, 255);

                float sx = 50, sy = 50;
                float dx = sx, dy = sy;

                float asc, desc, lh;

                Dash(dx, dy);

                fs.ClearState();

                fs.SetSize(size);
                fs.SetFont(fontNormal);
                fs.VertMetrics(out asc, out desc, out lh);
                dx = sx;
                dy += lh;
                Dash(dx, dy);

                fs.SetSize(size);
                fs.SetFont(fontNormal);
                fs.SetColour(white);
                dx = fs.DrawText(dx, dy, "The quick ");

                fs.SetSize(size / 2);
                fs.SetFont(fontItalic);
                fs.SetColour(brown);
                dx = fs.DrawText(dx, dy, "brown ");

                fs.SetSize(size / 3);
                fs.SetFont(fontNormal);
                fs.SetColour(white);
                dx = fs.DrawText(dx, dy, "fox ");

                fs.SetSize(14);
                fs.SetFont(fontNormal);
                fs.SetColour(white);
                fs.DrawText(20, height - 20, "Press UP / DOWN keys to change font size and to trigger atlas full callback, R to reset atlas, E to expand atlas.");

                fs.GetAtlasSize(out int atlasw, out int atlash);
                string msg = "Atlas: " + atlasw + " X " + atlash;
                fs.DrawText(20, height - 50, msg);

                fs.DrawDebug(width - atlasw - 20, 20.0f);

                gl.Enable(EnableCap.DepthTest);

                glfw.SwapBuffers(window);
                glfw.PollEvents();
            }

            glfons.Dispose();

            glfw.Terminate();
            Environment.Exit(0);
        }

    }
}
