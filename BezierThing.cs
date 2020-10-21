using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace OpenCLSamples
{
	public class BezierThing
	{
		public static void ProcessCurve()
		{
			(float x, float y, float cx, float cy)[] points = new (float x, float y, float cx, float cy)[]
			{
				(0, 0, 1, 0),
				(1, 1, 0, 1),
				
				(1, 1, 0, 1),
				(1, 0, 0, 1),
			};

			int resolution = 100;

			float step = 1f / resolution;

			(float x, float y)[] result = new (float x, float y)[(points.Length - 1) * resolution + 1];

			for (int j = 0; j < points.Length - 1; j+=2)
			{
				for (int i = 0; i < resolution + 1; i++)
				{
					float t = i * step;
					(float x, float y, float cx, float cy) start = points[j + 1];
					(float x, float y, float cx, float cy) end = points[j + 0];

					float x = MathF.Pow(1 - t, 3) * start.x + 3 * MathF.Pow(1 - t, 2) * t * start.cx +
					          3 * (1 - t) * MathF.Pow(t, 2) * end.cx + MathF.Pow(t, 3) * end.x;

					float y = MathF.Pow(1 - t, 3) * start.y + 3 * MathF.Pow(1 - t, 2) * t * start.cy +
					          3 * (1 - t) * MathF.Pow(t, 2) * end.cy + MathF.Pow(t, 3) * end.y;

					result[j*resolution + i] = (x, y);
				}
			}

			foreach ((float x, float y) tuple in result)
			{
				Console.WriteLine(tuple);
			}
			
			GameWindow window = new GameWindow(GameWindowSettings.Default, NativeWindowSettings.Default);

			int vbo = 0;
			int vao = 0;
			window.Resize+= delegate(ResizeEventArgs args)
			{
				GL.Viewport(0,0,args.Width, args.Height);
			};
			window.Load += delegate
			{
				GL.ClearColor(Color.Black);

				vbo = GL.GenBuffer();
				GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
				GL.BufferData(BufferTarget.ArrayBuffer, result.Length * sizeof(float) * 2, result,
					BufferUsageHint.StaticDraw);

				Shader shader = new Shader("s.v", "s.f");
				shader.Use();
				vao = GL.GenVertexArray();
				GL.BindVertexArray(vao);
				GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
				GL.EnableVertexAttribArray(0);
				GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
			};
			window.RenderFrame += delegate(FrameEventArgs args)
			{
				GL.Clear(ClearBufferMask.ColorBufferBit);
				
				GL.BindVertexArray(vao);
				GL.DrawArrays(PrimitiveType.LineStrip, 0, result.Length);
				
				
				window.SwapBuffers();
			};
			
			window.Run();
		}
		
	}
}