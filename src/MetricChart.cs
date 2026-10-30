using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TufControl;

public class MetricChart : FrameworkElement
{
	public Metric? Metric;

	public TimeSpan Window = TimeSpan.FromMinutes(5L);

	public bool Compact;

	private double hoverX = double.NaN;

	private const double Right = 8.0;

	private const double Top = 8.0;

	private double Left => Compact ? 30 : 40;

	private double Bottom => Compact ? 16 : 20;

	protected override void OnMouseMove(MouseEventArgs e)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Point position = e.GetPosition(this);
		hoverX = position.X;
		InvalidateVisual();
	}

	protected override void OnMouseLeave(MouseEventArgs e)
	{
		hoverX = double.NaN;
		InvalidateVisual();
	}

	protected override void OnRender(DrawingContext dc)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_066c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ace: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b0b: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c4f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c66: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c98: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dc4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e00: Unknown result type (might be due to invalid IL or missing references)
		//IL_076c: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0873: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ae: Unknown result type (might be due to invalid IL or missing references)
		dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, ActualWidth, ActualHeight));
		Metric m = Metric;
		if (m == null)
		{
			return;
		}
		double w = Math.Max(1.0, ActualWidth - Left - 8.0);
		double h = Math.Max(1.0, ActualHeight - 8.0 - Bottom);
		double lo = m.Floor;
		double hi = Math.Max(m.Ceiling, lo + 1.0);
		DateTime now = DateTime.Now;
		DateTime from = now - Window;
		double fs = (Compact ? 10.0 : 10.5);
		for (int k = 0; k <= 4; k++)
		{
			double v = lo + (hi - lo) * (double)k / 4.0;
			double y = Math.Round(Y(v)) + 0.5;
			dc.DrawLine((k == 0) ? Ink.Freeze(new Pen(Ink.Grid, 1.0)) : Ink.Dashed(Ink.Grid, 1.0), new Point(Left, y), new Point(Left + w, y));
			if (!Compact || k % 2 == 0)
			{
				Ink.Draw(dc, this, v.ToString("0"), new Point(Left - 6.0, y - 7.0), fs, Ink.Label, 1.0);
			}
		}
		string span = ((Window.TotalMinutes >= 1.0) ? Lang.F("{0} dk önce", Window.TotalMinutes.ToString("0")) : Lang.F("{0} sn önce", Window.TotalSeconds.ToString("0")));
		Ink.Draw(dc, this, span, new Point(Left, 8.0 + h + 3.0), fs, Ink.Label);
		Ink.Draw(dc, this, Lang.T("şimdi"), new Point(Left + w, 8.0 + h + 3.0), fs, Ink.Label, 1.0);
		List<Sample> visible = m.Samples.Where((Sample sample) => sample.Time >= from).ToList();
		if (visible.Count < 2)
		{
			Ink.Draw(dc, this, Lang.T("Veri toplanıyor…"), new Point(Left + w / 2.0, 8.0 + h / 2.0 - 7.0), 11.5, Ink.Label, 0.5);
			return;
		}
		int buckets = Math.Max(2, (int)(w / 2.0));
		double[] avg = new double[buckets];
		double[] bmin = new double[buckets];
		double[] bmax = new double[buckets];
		int[] cnt = new int[buckets];
		foreach (Sample s in visible)
		{
			int b = Math.Clamp((int)((s.Time - from).TotalMilliseconds / Window.TotalMilliseconds * (double)buckets), 0, buckets - 1);
			if (cnt[b] == 0)
			{
				bmin[b] = s.Value;
				bmax[b] = s.Value;
			}
			bmin[b] = Math.Min(bmin[b], s.Value);
			bmax[b] = Math.Max(bmax[b], s.Value);
			avg[b] += s.Value;
			cnt[b]++;
		}
		int maxGap = Math.Max(1, (int)Math.Ceiling(15.0 / Window.TotalSeconds * (double)buckets));
		List<List<int>> runs = new List<List<int>>();
		List<int> run = null;
		int last = -1000;
		for (int b2 = 0; b2 < buckets; b2++)
		{
			if (cnt[b2] != 0)
			{
				avg[b2] /= cnt[b2];
				if (run == null || b2 - last > maxGap)
				{
					run = new List<int>();
					runs.Add(run);
				}
				run.Add(b2);
				last = b2;
			}
		}
		MetricStats stats = m.Stats(Window, now);
		dc.PushClip(new RectangleGeometry(new Rect(Left, 0.0, w + 2.0, 8.0 + h + 1.0)));
		foreach (List<int> r in runs.Where((List<int> list) => list.Count > 1))
		{
			StreamGeometry area = new StreamGeometry();
			using (StreamGeometryContext ctx = area.Open())
			{
				ctx.BeginFigure(new Point(Bx(r[0]), 8.0 + h), isFilled: true, isClosed: true);
				foreach (int b3 in r)
				{
					ctx.LineTo(new Point(Bx(b3), Y(avg[b3])), isStroked: false, isSmoothJoin: true);
				}
				ctx.LineTo(new Point(Bx(r[r.Count - 1]), 8.0 + h), isStroked: false, isSmoothJoin: false);
			}
			((Freezable)area).Freeze();
			dc.DrawGeometry(Ink.Fade(m.Brush, (byte)(Compact ? 50u : 60u)), null, area);
			StreamGeometry band = new StreamGeometry();
			using (StreamGeometryContext ctx2 = band.Open())
			{
				ctx2.BeginFigure(new Point(Bx(r[0]), Y(bmax[r[0]])), isFilled: true, isClosed: true);
				foreach (int b4 in r)
				{
					ctx2.LineTo(new Point(Bx(b4), Y(bmax[b4])), isStroked: false, isSmoothJoin: true);
				}
				for (int i = r.Count - 1; i >= 0; i--)
				{
					ctx2.LineTo(new Point(Bx(r[i]), Y(bmin[r[i]])), isStroked: false, isSmoothJoin: true);
				}
			}
			((Freezable)band).Freeze();
			dc.DrawGeometry(Ink.Alpha(m.Brush, 45), null, band);
			StreamGeometry line = new StreamGeometry();
			using (StreamGeometryContext ctx3 = line.Open())
			{
				ctx3.BeginFigure(new Point(Bx(r[0]), Y(avg[r[0]])), isFilled: false, isClosed: false);
				foreach (int b5 in r.Skip(1))
				{
					ctx3.LineTo(new Point(Bx(b5), Y(avg[b5])), isStroked: true, isSmoothJoin: true);
				}
			}
			((Freezable)line).Freeze();
			dc.DrawGeometry(null, Ink.Line(m.Brush, Compact ? 1.6 : 2.0), line);
		}
		dc.Pop();
		if (!Compact)
		{
			double? max = stats.Max;
			if (max.HasValue)
			{
				double mx = max.GetValueOrDefault();
				max = stats.Min;
				if (max.HasValue)
				{
					double mn = max.GetValueOrDefault();
					max = stats.Avg;
					if (max.HasValue)
					{
						double av = max.GetValueOrDefault();
						Guide(mx, "maks", 90, 4.0, 3.0);
						if (Y(mn) - Y(mx) > 28.0)
						{
							Guide(mn, "min", 70, 4.0, 3.0);
						}
						if (Y(av) - Y(mx) > 18.0 && Y(mn) - Y(av) > 18.0)
						{
							Guide(av, "ort", 60, 1.0, 3.0);
						}
					}
				}
			}
		}
		Sample tail = visible[visible.Count - 1];
		if ((now - tail.Time).TotalSeconds < 15.0)
		{
			Point p = new Point(X(tail.Time), Y(tail.Value));
			dc.DrawEllipse(Ink.Alpha(m.Brush, 50), null, p, 6.0, 6.0);
			dc.DrawEllipse(m.Brush, Ink.Line(Ink.Surface, 1.5), p, 3.0, 3.0);
		}
		if (double.IsFinite(hoverX) && hoverX >= Left && hoverX <= Left + w)
		{
			DateTime t = from + TimeSpan.FromMilliseconds((hoverX - Left) / w * Window.TotalMilliseconds);
			Sample near = visible.MinBy((Sample sample) => Math.Abs((sample.Time - t).TotalMilliseconds));
			if (Math.Abs((near.Time - t).TotalSeconds) < Math.Max(10.0, Window.TotalSeconds / w * 8.0))
			{
				double x = X(near.Time);
				double y2 = Y(near.Value);
				dc.DrawLine(Ink.Freeze(new Pen(Ink.Alpha(Ink.Text, 80), 1.0)), new Point(x, 8.0), new Point(x, 8.0 + h));
				dc.DrawEllipse(m.Brush, Ink.Line(Ink.Text, 1.5), new Point(x, y2), 4.0, 4.0);
				FormattedText f = Ink.Format($"{near.Time:HH:mm:ss}  ·  {m.Show(near.Value)}", 11.0, Ink.Text, this, bold: true);
				double bw = f.Width + 16.0;
				double bh = f.Height + 8.0;
				double bxp = Math.Clamp(x - bw / 2.0, Left, Left + w - bw);
				double byp = Math.Max(0.0, y2 - bh - 10.0);
				dc.DrawRoundedRectangle(Ink.Popup, Ink.Freeze(new Pen(Ink.Alpha(m.Brush, 140), 1.0)), new Rect(bxp, byp, bw, bh), 2.0, 2.0);
				dc.DrawText(f, new Point(bxp + 8.0, byp + 4.0));
			}
		}
		double Bx(int num)
		{
			return Left + w * ((double)num + 0.5) / (double)buckets;
		}
		void Guide(double num, string label, byte alpha, double on, double off)
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			double y3 = Math.Round(Y(num)) + 0.5;
			dc.DrawLine(Ink.Dashed(Ink.Alpha(Ink.Text, alpha), 1.0, on, off), new Point(Left, y3), new Point(Left + w, y3));
			FormattedText f2 = Ink.Format(Lang.T(label) + " " + m.Show(num), 10.0, Ink.Text, this, bold: true);
			double ty = Math.Clamp(y3 - f2.Height - 1.0, 0.0, 8.0 + h - f2.Height);
			dc.DrawRoundedRectangle(Ink.Popup, null, new Rect(Left + 4.0, ty, f2.Width + 8.0, f2.Height), 2.0, 2.0);
			dc.DrawText(f2, new Point(Left + 8.0, ty));
		}
		double X(DateTime dateTime)
		{
			return Left + w * (dateTime - from).TotalMilliseconds / Window.TotalMilliseconds;
		}
		double Y(double value)
		{
			return 8.0 + h * (1.0 - (Math.Clamp(value, lo, hi) - lo) / (hi - lo));
		}
	}
}
