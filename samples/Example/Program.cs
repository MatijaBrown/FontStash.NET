using FontStash.NET;
using FontStash.NET.GL.Legacy;
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

        private static bool debug = false;
        static bool blowup = false;

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

        private static unsafe void Key(WindowHandle* window, Keys key, int scancode, InputAction action, KeyModifiers mods)
        {
            if (key == Keys.Escape && action == InputAction.Press)
                glfw.SetWindowShouldClose(window, true);
            if (key == Keys.Space && action == InputAction.Press)
                debug = !debug;
            if (key == Keys.B && action == InputAction.Press)
                blowup = !blowup;
        }

        static unsafe void Main(string[] args)
        {
            glfw = Glfw.GetApi();

            int fontNormal = Fontstash.INVALID;
            int fontItalic = Fontstash.INVALID;
            int fontBold = Fontstash.INVALID;
            VideoMode* mode;

            Fontstash fs = null;

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

            int ft = fs.AddFont("icons", "./fonts/entypo.ttf", 0);

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

                Dash(dx, dy);

                fs.ClearState();

                fs.SetSize(124.0f);
                fs.SetFont(fontNormal);
                fs.VertMetrics(out float asc, out float desc, out float lh);
                dx = sx;
                dy += lh;
                Dash(dx, dy);

                fs.SetSize(124.0f);
                fs.SetFont(fontNormal);
                fs.SetColour(white);
                dx = fs.DrawText(dx, dy, "The quick", null);

                fs.SetSize(48.0f);
                fs.SetFont(fontItalic);
                fs.SetColour(brown);
                dx = fs.DrawText(dx, dy, "brown ", null);

                fs.SetSize(24.0f);
                fs.SetFont(fontNormal);
                fs.SetColour(white);
                dx = fs.DrawText(dx, dy, "fox ", null);

                fs.VertMetrics(out asc, out desc, out lh);
                dx = sx;
                dy += lh * 1.2f;
                Dash(dx, dy);
                fs.SetFont(fontItalic);
                dx = fs.DrawText(dx, dy, "jumps over ", null);
                fs.SetFont(fontBold);
                dx = fs.DrawText(dx, dy, "the lazy ", null);
                fs.SetFont(fontNormal);
                dx = fs.DrawText(dx, dy, "dog.", null);

                dx = sx;
                dy += lh * 1.2f;
                Dash(dx, dy);
                fs.SetSize(12.0f);
                fs.SetFont(fontNormal);
                fs.SetColour(blue);
                _ = fs.DrawText(dx, dy, "Now is the time for all good men to come to the aid of the party.");

                fs.SetSize(18.0f);
                fs.SetFont(fontNormal);
                fs.SetColour(white);

                dx = 50;
                dy = 350;
                Line(dx - 10, dy, dx + 250, dy);
                fs.SetAlign((int)FonsAlign.Left | (int)FonsAlign.Top);
                dx = fs.DrawText(dx, dy, "Top");
                dx += 10;
                fs.SetAlign((int)FonsAlign.Left | (int)FonsAlign.Middle);
                dx = fs.DrawText(dx, dy, "Middle");
                dx += 10;
                fs.SetAlign((int)FonsAlign.Left | (int)FonsAlign.Baseline);
                dx = fs.DrawText(dx, dy, "Baseline");
                dx += 10;
                fs.SetAlign((int)FonsAlign.Left | (int)FonsAlign.Bottom);
                dx = fs.DrawText(dx, dy, "Bottom");

                dx = 150;
                dy = 400;
                Line(dx, dy - 30, dx, dy + 80.0f);
                fs.SetAlign((int)FonsAlign.Left | (int)FonsAlign.Baseline);
                fs.DrawText(dx, dy, "Left");
                dy += 30;
                fs.SetAlign((int)FonsAlign.Center | (int)FonsAlign.Baseline);
                fs.DrawText(dx, dy, "Center");
                dy += 30;
                fs.SetAlign((int)FonsAlign.Right | (int)FonsAlign.Baseline);
                fs.DrawText(dx, dy, "Right");

                dx = 500;
                dy = 350;
                fs.SetAlign((int)FonsAlign.Left | (int)FonsAlign.Baseline);

                fs.SetSize(60.0f);
                fs.SetFont(fontItalic);
                fs.SetColour(white);
                fs.SetSpacing(5.0f);
                fs.SetBlur(10.0f);
                fs.DrawText(dx, dy, "Blurry...");

                dy += 50.0f;

                fs.SetSize(18.0f);
                fs.SetFont(fontBold);
                fs.SetColour(black);
                fs.SetSpacing(0.0f);
                fs.SetBlur(3.0f);
                fs.DrawText(dx, dy + 2, "DROP THAT SHADOW");

                fs.SetColour(white);
                fs.SetBlur(0.0f);
                fs.DrawText(dx, dy, "DROP THAT SHADOW");

                if (debug)
                    fs.DrawDebug(800.0f, 50.0f);

                gl.Enable(EnableCap.DepthTest);

                glfw.SwapBuffers(window);
                glfw.PollEvents();
            }

            glfons.Dispose();
            glfw.Terminate();
        }

    }
}
