using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace TufControl;

public class SeriesChart : FrameworkElement
{
	public const int Capacity = 200;

	private const double Gutter = 34.0;

	private const double Bottom = 22.0;

	private readonly List<(Brush Brush, List<double?> Data)> series = new List<(Brush, List<double?>)>();

	private int window = 120;

	public string Span = "6 dk";

	public int Window
	{
		get
		{
			return window;
		}
		set
		{
			window = Math.Clamp(value, 10, 200);
			InvalidateVisual();
		}
	}

	public void Define(params Brush[] brushes)
	{
		series.Clear();
		foreach (Brush b in brushes)
		{
			series.Add((b, new List<double?>()));
		}
	}

	public void Push(params double?[] values)
	{
		for (int i = 0; i < series.Count; i++)
		{
			List<double?> d = series[i].Data;
			d.Add((i < values.Length) ? values[i] : ((double?)null));
			if (d.Count > 200)
			{
				d.RemoveAt(0);
			}
		}
		InvalidateVisual();
	}

	protected override void OnRender(DrawingContext dc)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		double w = Math.Max(1.0, ActualWidth - 34.0);
		double h = Math.Max(1.0, ActualHeight - 22.0);
		dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, ActualWidth, ActualHeight));
		for (int t = 0; t <= 100; t += 25)
		{
			double y = Math.Round(Y(t)) + 0.5;
			dc.DrawLine(Ink.Dashed(Ink.Grid, 1.0), new Point(34.0, y), new Point(34.0 + w, y));
			Ink.Draw(dc, this, t.ToString(), new Point(24.0, y - 8.0), 11.0, Ink.Label, 1.0);
		}
		for (int k = 0; k <= 4; k++)
		{
			double x = 34.0 + w * (double)k / 4.0;
			if (k > 0 && k < 4)
			{
				dc.DrawLine(Ink.Dashed(Ink.Grid, 1.0, 2.0), new Point(x, 4.0), new Point(x, h + 4.0));
			}
		}
		Ink.Draw(dc, this, Lang.F("{0} önce", Lang.T(Span)), new Point(34.0, h + 8.0), 11.0, Ink.Label);
		Ink.Draw(dc, this, Lang.T("şimdi"), new Point(34.0 + w, h + 8.0), 11.0, Ink.Label, 1.0);
		int count = ((series.Count > 0) ? series[0].Data.Count : 0);
		if (count < 2)
		{
			Ink.Draw(dc, this, Lang.T("Canlı örnekler toplanıyor…"), new Point(34.0 + w / 2.0, h / 2.0 - 6.0), 12.5, Ink.Label, 0.5);
			return;
		}
		int start = Math.Max(0, count - window);
		dc.PushClip(new RectangleGeometry(new Rect(34.0, 0.0, w + 6.0, h + 8.0)));
		for (int s = series.Count - 1; s >= 0; s--)
		{
			Draw(dc, series[s].Data, series[s].Brush, start, X, Y, h);
		}
		dc.Pop();
		double X(int i)
		{
			return 34.0 + w * (double)(i - start + window - (count - start)) / (double)(window - 1);
		}
		double Y(double v)
		{
			return h * (100.0 - Math.Clamp(v, 0.0, 100.0)) / 100.0 + 4.0;
		}
	}

	private static void Draw(DrawingContext dc, List<double?> data, Brush brush, int start, Func<int, double> X, Func<double, double> Y, double h)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		List<List<Point>> runs = new List<List<Point>>();
		List<Point> run = null;
		for (int i = start; i < data.Count; i++)
		{
			double? num = data[i];
			if (num.HasValue)
			{
				double v = num.GetValueOrDefault();
				if (run == null)
				{
					run = new List<Point>();
				}
				run.Add(new Point(X(i), Y(v)));
			}
			else if (run != null)
			{
				runs.Add(run);
				run = null;
			}
		}
		if (run != null)
		{
			runs.Add(run);
		}
		Brush fill = Ink.Fade(brush, 48);
		Pen pen = Ink.Line(brush, 2.0);
		foreach (List<Point> pts in runs.Where((List<Point> r) => r.Count > 1))
		{
			StreamGeometry area = new StreamGeometry();
			using (StreamGeometryContext ctx = area.Open())
			{
				Point val = pts[0];
				ctx.BeginFigure(new Point(val.X, h + 4.0), isFilled: true, isClosed: true);
				ctx.LineTo(pts[0], isStroked: false, isSmoothJoin: false);
				Ink.Smooth(ctx, pts);
				val = pts[pts.Count - 1];
				ctx.LineTo(new Point(val.X, h + 4.0), isStroked: false, isSmoothJoin: false);
			}
			((Freezable)area).Freeze();
			dc.DrawGeometry(fill, null, area);
			StreamGeometry line = new StreamGeometry();
			using (StreamGeometryContext ctx2 = line.Open())
			{
				ctx2.BeginFigure(pts[0], isFilled: false, isClosed: false);
				Ink.Smooth(ctx2, pts);
			}
			((Freezable)line).Freeze();
			dc.DrawGeometry(null, pen, line);
		}
		if (runs.Count > 0)
		{
			if (data[data.Count - 1].HasValue)
			{
				List<Point> list = runs[runs.Count - 1];
				Point last = list[list.Count - 1];
				dc.DrawEllipse(Ink.Alpha(brush, 50), null, last, 7.0, 7.0);
				dc.DrawEllipse(brush, Ink.Line(Ink.Surface, 2.0), last, 3.5, 3.5);
			}
		}
	}
}
