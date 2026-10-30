using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TufControl;

public class CurveCanvas : FrameworkElement
{
	private const double Left = 40.0;

	private const double Right = 12.0;

	private const double Top = 14.0;

	private const double Bottom = 26.0;

	public FanPoint[] Points = Profile.DefaultCurve();

	public Brush Series = Brushes.DeepSkyBlue;

	public double Current = double.NaN;

	public Action<int, int, int>? Edit;

	private int dragging = -1;

	private int hover = -1;

	private double W => Math.Max(1.0, ActualWidth - 40.0 - 12.0);

	private double H => Math.Max(1.0, ActualHeight - 14.0 - 26.0);

	private double X(double t)
	{
		return 40.0 + W * (t - 20.0) / 80.0;
	}

	private double Y(double p)
	{
		return 14.0 + H * (100.0 - p) / 100.0;
	}

	private Point Plot(FanPoint p)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return new Point(X(p.Temperature), Y(p.Percent));
	}

	protected override void OnRender(DrawingContext dc)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_0475: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0496: Unknown result type (might be due to invalid IL or missing references)
		//IL_049b: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0517: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Unknown result type (might be due to invalid IL or missing references)
		//IL_056c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Unknown result type (might be due to invalid IL or missing references)
		//IL_058d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0592: Unknown result type (might be due to invalid IL or missing references)
		//IL_0596: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_068a: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0777: Unknown result type (might be due to invalid IL or missing references)
		//IL_073e: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_090e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0945: Unknown result type (might be due to invalid IL or missing references)
		dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, ActualWidth, ActualHeight));
		Brush zone = Ink.Alpha(Ink.Danger, 18);
		dc.DrawRectangle(zone, null, new Rect(new Point(X(70.0), Y(30.0)), new Point(X(80.0), Y(0.0))));
		dc.DrawRectangle(zone, null, new Rect(new Point(X(80.0), Y(70.0)), new Point(X(90.0), Y(0.0))));
		dc.DrawRectangle(zone, null, new Rect(new Point(X(90.0), Y(100.0)), new Point(X(100.0), Y(0.0))));
		dc.DrawLine(Ink.Dashed(Ink.Alpha(Ink.Danger, 90), 1.0), new Point(X(70.0), 14.0), new Point(X(70.0), 14.0 + H));
		Ink.Draw(dc, this, Lang.T("◀ serbest bölge"), new Point(X(70.0) - 6.0, 14.0 + H - 18.0), 10.5, Ink.Label, 1.0);
		Ink.Draw(dc, this, Lang.T("korumalı ▶"), new Point(X(70.0) + 6.0, 14.0 + H - 18.0), 10.5, Ink.Alpha(Ink.Danger, 190));
		for (int p = 0; p <= 100; p += 25)
		{
			double y = Math.Round(Y(p)) + 0.5;
			dc.DrawLine(Ink.Freeze(new Pen(Ink.Grid, 1.0)), new Point(40.0, y), new Point(40.0 + W, y));
			Ink.Draw(dc, this, p + "%", new Point(30.0, y - 8.0), 10.5, Ink.Label, 1.0);
		}
		for (int t = 20; t <= 100; t += 10)
		{
			double x = Math.Round(X(t)) + 0.5;
			dc.DrawLine(Ink.Dashed(Ink.Grid, 1.0, 2.0), new Point(x, 14.0), new Point(x, 14.0 + H));
			if (t % 20 == 0)
			{
				Ink.Draw(dc, this, t + "°C", new Point(x, 14.0 + H + 8.0), 10.5, Ink.Label, 0.5);
			}
		}
		List<Point> pts = Points.Select(Plot).ToList();
		StreamGeometry area = new StreamGeometry();
		Point val;
		using (StreamGeometryContext ctx = area.Open())
		{
			ctx.BeginFigure(new Point(40.0, Y(0.0)), isFilled: true, isClosed: true);
			val = pts[0];
			ctx.LineTo(new Point(40.0, val.Y), isStroked: false, isSmoothJoin: false);
			foreach (Point p2 in pts)
			{
				ctx.LineTo(p2, isStroked: false, isSmoothJoin: true);
			}
			double num = 40.0 + W;
			val = pts[pts.Count - 1];
			ctx.LineTo(new Point(num, val.Y), isStroked: false, isSmoothJoin: false);
			ctx.LineTo(new Point(40.0 + W, Y(0.0)), isStroked: false, isSmoothJoin: false);
		}
		((Freezable)area).Freeze();
		dc.DrawGeometry(Ink.Fade(Series, 64), null, area);
		StreamGeometry line = new StreamGeometry();
		using (StreamGeometryContext ctx2 = line.Open())
		{
			val = pts[0];
			ctx2.BeginFigure(new Point(40.0, val.Y), isFilled: false, isClosed: false);
			foreach (Point p3 in pts)
			{
				ctx2.LineTo(p3, isStroked: true, isSmoothJoin: true);
			}
			double num2 = 40.0 + W;
			val = pts[pts.Count - 1];
			ctx2.LineTo(new Point(num2, val.Y), isStroked: true, isSmoothJoin: true);
		}
		((Freezable)line).Freeze();
		dc.DrawGeometry(null, Ink.Line(Series, 2.4), line);
		if (double.IsFinite(Current))
		{
			double x2 = X(Math.Clamp(Current, 20.0, 100.0));
			dc.DrawLine(Ink.Dashed(Ink.Alpha(Ink.Text, 70), 1.0, 3.0, 3.0), new Point(x2, 14.0), new Point(x2, 14.0 + H));
			Ink.Draw(dc, this, Lang.F("Şu an {0}°C", Current.ToString("0")), new Point(x2 + 6.0, 14.0), 10.5, Ink.Text);
		}
		for (int i = 0; i < pts.Count; i++)
		{
			bool active = i == dragging || i == hover;
			if (active)
			{
				dc.DrawEllipse(Ink.Alpha(Series, 46), null, pts[i], 13.0, 13.0);
			}
			dc.DrawEllipse(Ink.Surface, Ink.Line(Series, 2.4), pts[i], active ? 6.5 : 5.0, active ? 6.5 : 5.0);
		}
		int shown = ((dragging >= 0) ? dragging : hover);
		if (shown >= 0)
		{
			Point p4 = pts[shown];
			FormattedText label = Ink.Format($"{Points[shown].Temperature} °C  ·  %{Points[shown].Percent}", 11.5, Ink.Text, this, bold: true);
			double bw = label.Width + 18.0;
			double bh = label.Height + 10.0;
			double bx = Math.Clamp(p4.X - bw / 2.0, 40.0, 40.0 + W - bw);
			double by = p4.Y - bh - 14.0;
			if (by < 0.0)
			{
				by = p4.Y + 16.0;
			}
			dc.DrawRoundedRectangle(Ink.Popup, Ink.Freeze(new Pen(Ink.Alpha(Series, 120), 1.0)), new Rect(bx, by, bw, bh), 2.0, 2.0);
			dc.DrawText(label, new Point(bx + 9.0, by + 5.0));
		}
	}

	private int Nearest(Point mouse)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		int best = Enumerable.Range(0, Points.Length).OrderBy((int i) =>
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			Vector val2 = Plot(Points[i]) - mouse;
			return val2.Length;
		}).First();
		Vector val = Plot(Points[best]) - mouse;
		if (!(val.Length <= 18.0))
		{
			return -1;
		}
		return best;
	}

	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (IsEnabled)
		{
			dragging = Nearest(e.GetPosition(this));
			if (dragging >= 0)
			{
				CaptureMouse();
				e.Handled = true;
				InvalidateVisual();
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Point p = e.GetPosition(this);
		if (dragging < 0 || e.LeftButton != MouseButtonState.Pressed)
		{
			int h = Nearest(p);
			if (h != hover)
			{
				hover = h;
				Cursor = ((h >= 0) ? Cursors.Hand : Cursors.Arrow);
				InvalidateVisual();
			}
		}
		else
		{
			int low = ((dragging == 0) ? 20 : (Points[dragging - 1].Temperature + 1));
			int high = ((dragging == 7) ? 100 : (Points[dragging + 1].Temperature - 1));
			int t = Math.Clamp((int)Math.Round(20.0 + (p.X - 40.0) / W * 80.0), low, high);
			int pct = Math.Clamp((int)Math.Round(100.0 - (p.Y - 14.0) / H * 100.0), 0, 100);
			Edit?.Invoke(dragging, t, pct);
		}
	}

	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		dragging = -1;
		ReleaseMouseCapture();
		InvalidateVisual();
	}

	protected override void OnMouseLeave(MouseEventArgs e)
	{
		if (dragging < 0 && hover >= 0)
		{
			hover = -1;
			InvalidateVisual();
		}
	}
}
