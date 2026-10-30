using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace TufControl;

internal static class Ink
{
	public static readonly Brush Grid = Freeze(new SolidColorBrush(Color.FromRgb(25, 25, 30)));

	public static readonly Brush Track = Freeze(new SolidColorBrush(Color.FromRgb(28, 28, 33)));

	public static readonly Brush Label = Freeze(new SolidColorBrush(Color.FromRgb(102, 110, 128)));

	public static readonly Brush Text = Freeze(new SolidColorBrush(Color.FromRgb(237, 240, 245)));

	public static readonly Brush Surface = Freeze(new SolidColorBrush(Color.FromRgb(11, 11, 13)));

	public static readonly Brush Popup = Freeze(new SolidColorBrush(Color.FromRgb(28, 28, 33)));

	public static readonly Brush Danger = Freeze(new SolidColorBrush(Color.FromRgb(byte.MaxValue, 93, 93)));

	private static readonly FontFamily Family = new FontFamily("Segoe UI Variable Text, Segoe UI");

	public static T Freeze<T>(T f) where T : Freezable
	{
		((Freezable)f).Freeze();
		return f;
	}

	public static Color Of(Brush b)
	{
		if (!(b is SolidColorBrush s))
		{
			return Colors.White;
		}
		return s.Color;
	}

	public static Brush Alpha(Brush b, byte a)
	{
		Color c = Of(b);
		return Freeze(new SolidColorBrush(Color.FromArgb(a, c.R, c.G, c.B)));
	}

	public static FormattedText Format(string text, double size, Brush brush, Visual v, bool bold = false)
	{
		return new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(Family, FontStyles.Normal, bold ? FontWeights.SemiBold : FontWeights.Normal, FontStretches.Normal), size, brush, VisualTreeHelper.GetDpi(v).PixelsPerDip);
	}

	public static void Draw(DrawingContext dc, Visual v, string text, Point p, double size, Brush brush, double align = 0.0, bool bold = false)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		FormattedText f = Format(text, size, brush, v, bold);
		dc.DrawText(f, new Point(p.X - f.Width * align, p.Y));
	}

	public static Pen Line(Brush b, double w)
	{
		return Freeze(new Pen(b, w)
		{
			StartLineCap = PenLineCap.Round,
			EndLineCap = PenLineCap.Round,
			LineJoin = PenLineJoin.Round
		});
	}

	public static Pen Dashed(Brush b, double w, double on = 3.0, double off = 4.0)
	{
		Pen pen = new Pen(b, w);
		pen.DashStyle = new DashStyle(new _003C_003Ez__ReadOnlyArray<double>(new double[2] { on, off }), 0.0);
		return Freeze(pen);
	}

	public static Brush Fade(Brush b, byte top)
	{
		Color c = Of(b);
		return Freeze(new LinearGradientBrush(Color.FromArgb(top, c.R, c.G, c.B), Color.FromArgb(0, c.R, c.G, c.B), 90.0));
	}

	public static void Smooth(StreamGeometryContext ctx, IReadOnlyList<Point> pts)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < pts.Count - 1; i++)
		{
			Point p0 = pts[Math.Max(0, i - 1)];
			Point p1 = pts[i];
			Point p2 = pts[i + 1];
			Point p3 = pts[Math.Min(pts.Count - 1, i + 2)];
			ctx.BezierTo(new Point(p1.X + (p2.X - p0.X) / 6.0, p1.Y + (p2.Y - p0.Y) / 6.0), new Point(p2.X - (p3.X - p1.X) / 6.0, p2.Y - (p3.Y - p1.Y) / 6.0), p2, isStroked: true, isSmoothJoin: true);
		}
	}
}
