using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace TufControl;

internal static class Brand
{
	private static readonly (float X, float Y)[][] Mark = new (float, float)[2][]
	{
		new (float, float)[4]
		{
			(3f, 7f),
			(29f, 7f),
			(26f, 12f),
			(6f, 12f)
		},
		new (float, float)[5]
		{
			(13f, 14f),
			(19f, 14f),
			(19f, 24f),
			(16f, 28f),
			(13f, 24f)
		}
	};

	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref int value, int size);

	[DllImport("user32.dll")]
	private static extern bool DestroyIcon(nint handle);

	public static void StyleFrame(nint hwnd)
	{
		int dark = 1;
		int round = 2;
		int border = 2892317;
		DwmSetWindowAttribute(hwnd, 20, ref dark, 4);
		DwmSetWindowAttribute(hwnd, 33, ref round, 4);
		DwmSetWindowAttribute(hwnd, 34, ref border, 4);
	}

	public static ImageSource WindowIcon()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		DrawingVisual visual = new DrawingVisual();
		using (DrawingContext dc = visual.RenderOpen())
		{
			dc.DrawRectangle(System.Windows.Media.Brushes.Black, null, new Rect(0.0, 0.0, 64.0, 64.0));
			StreamGeometry g = new StreamGeometry();
			using (StreamGeometryContext ctx = g.Open())
			{
				(float, float)[][] mark = Mark;
				foreach ((float, float)[] poly in mark)
				{
					ctx.BeginFigure(new System.Windows.Point((double)(poly[0].Item1 * 2f), (double)(poly[0].Item2 * 2f + 2f)), isFilled: true, isClosed: true);
					foreach (var (x, y) in poly.Skip(1))
					{
						ctx.LineTo(new System.Windows.Point((double)(x * 2f), (double)(y * 2f + 2f)), isStroked: true, isSmoothJoin: false);
					}
				}
			}
			((Freezable)g).Freeze();
			dc.DrawGeometry(System.Windows.Media.Brushes.White, null, g);
		}
		RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(64, 64, 96.0, 96.0, PixelFormats.Pbgra32);
		renderTargetBitmap.Render(visual);
		((Freezable)renderTargetBitmap).Freeze();
		return renderTargetBitmap;
	}

	public static Icon TrayIcon()
	{
		object value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "SystemUsesLightTheme", 0);
		bool light = value is int && (int)value == 1;
		using Bitmap bmp = new Bitmap(32, 32);
		using (Graphics g = Graphics.FromImage(bmp))
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			using SolidBrush ink = new SolidBrush(light ? System.Drawing.Color.Black : System.Drawing.Color.White);
			(float, float)[][] mark = Mark;
			foreach ((float, float)[] poly in mark)
			{
				g.FillPolygon(ink, poly.Select(((float X, float Y) p) => new PointF(p.X, p.Y + 1f)).ToArray());
			}
		}
		nint hicon = bmp.GetHicon();
		Icon icon = (Icon)Icon.FromHandle(hicon).Clone();
		DestroyIcon(hicon);
		return icon;
	}
}
