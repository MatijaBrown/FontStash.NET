using FontStash.NET;
using FontStash.NET.GL;
using Silk.NET.GLFW;
using Silk.NET.OpenGL.Legacy;
using System;

namespace Example
{
    unsafe class Program
    {

        private static Glfw glfw;
        private static GL gl;

        private static WindowHandle* window;

        private static bool debug;

        private static void Dash(float dx, float dy)
        {
            gl.Begin(GLEnum.Lines);
            gl.Color4(0, 0, 0, 0.5f);
            gl.Vertex2(dx - 5, dy);
            gl.Vertex2(dx - 10, dy);
            gl.End();
        }

        private static unsafe void Key(WindowHandle* window, Keys key, int scancode, InputAction action, KeyModifiers mods)
        {
            if (key == Keys.Escape && action == InputAction.Press)
                glfw.SetWindowShouldClose(window, true);
            if (key == Keys.Space && action == InputAction.Press)
                debug = !debug;
        }

        static unsafe void Main(string[] args)
        {
            glfw = Glfw.GetApi();

            int fontNormal = Fontstash.INVALID;
            int fontItalic = Fontstash.INVALID;
            int fontBold = Fontstash.INVALID;
            int fontJapanese = Fontstash.INVALID;
            VideoMode* mode;

            Fontstash fs = null;

            if (!glfw.Init())
                Environment.Exit(-1);

            mode = glfw.GetVideoMode(glfw.GetPrimaryMonitor());
            window = glfw.CreateWindow(mode->Width - 40, mode->Height - 80, "Font Stash", null, null);
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

            fontNormal = fs.AddFont("sans", "./fonts/DroidSerif-Regular.ttf", 0);
            if (fontNormal == Fontstash.INVALID)
            {
                Console.Error.WriteLine("Could not add font normal!");
                Environment.Exit(-1);
            }
            fontItalic = fs.AddFont("sans", "./fonts/DroidSerif-Italic.ttf", 0);
            if (fontNormal == Fontstash.INVALID)
            {
                Console.Error.WriteLine("Could not add font italic!");
                Environment.Exit(-1);
            }
            fontBold = fs.AddFont("sans", "./fonts/DroidSerif-Bold.ttf", 0);
            if (fontNormal == Fontstash.INVALID)
            {
                Console.Error.WriteLine("Could not add font bold!");
                Environment.Exit(-1);
            }
            fontJapanese = fs.AddFont("sans", "./fonts/DroidSansJapanese.ttf", 0);
            if (fontNormal == Fontstash.INVALID)
            {
                Console.Error.WriteLine("Could not add font japanese!");
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

                Dash(dx, dy);

                fs.ClearState();

                fs.SetSize(124.0f);
                fs.SetFont(fontNormal);
                fs.VertMetrics(out float _, out float _, out float lh);
                dx = sx;
                dy += lh;
                Dash(dx, dy);

                fs.SetSize(124.0f);
                fs.SetFont(fontNormal);
                fs.SetColour(white);
                dx = fs.DrawText(dx, dy, "The quick ", '\0');

                gl.Enable(EnableCap.DepthTest);

                glfw.SwapBuffers(window);
                glfw.PollEvents();
            }

            glfons.Dispose();
            glfw.Terminate();
        }

    }
}
