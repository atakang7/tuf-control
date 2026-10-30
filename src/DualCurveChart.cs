using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace TufControl;

public class DualCurveChart : FrameworkElement
{
	public FanPoint[] Cpu = Profile.DefaultCurve();

	public FanPoint[] Gpu = Profile.DefaultCurve();

	public bool Custom;

	public Brush CpuBrush = Brushes.DeepSkyBlue;

	public Brush GpuBrush = Brushes.MediumPurple;

	private const double Left = 34.0;

	private const double Right = 8.0;

	private const double Top = 8.0;

	private const double Bottom = 22.0;

	protected override void OnRender(DrawingContext dc)
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		double w = Math.Max(1.0, ActualWidth - 34.0 - 8.0);
		double h = Math.Max(1.0, ActualHeight - 8.0 - 22.0);
		for (int p = 0; p <= 100; p += 25)
		{
			double y = Math.Round(8.0 + h * (double)(100 - p) / 100.0) + 0.5;
			dc.DrawLine(Ink.Freeze(new Pen(Ink.Grid, 1.0)), new Point(34.0, y), new Point(34.0 + w, y));
			Ink.Draw(dc, this, p.ToString(), new Point(26.0, y - 8.0), 10.5, Ink.Label, 1.0);
		}
		for (int t = 20; t <= 100; t += 20)
		{
			Ink.Draw(dc, this, t + "°", new Point(34.0 + w * (double)(t - 20) / 80.0, 8.0 + h + 6.0), 10.5, Ink.Label, 0.5);
		}
		dc.PushOpacity(Custom ? 1.0 : 0.4);
		(FanPoint[], Brush)[] array = new (FanPoint[], Brush)[2]
		{
			(Gpu, GpuBrush),
			(Cpu, CpuBrush)
		};
		for (int i = 0; i < array.Length; i++)
		{
			(FanPoint[], Brush) tuple = array[i];
			FanPoint[] curve = tuple.Item1;
			Brush brush = tuple.Item2;
			List<Point> pts = curve.Select(P).ToList();
			StreamGeometry area = new StreamGeometry();
			using (StreamGeometryContext ctx = area.Open())
			{
				Point val = pts[0];
				ctx.BeginFigure(new Point(val.X, 8.0 + h), isFilled: true, isClosed: true);
				foreach (Point p2 in pts)
				{
					ctx.LineTo(p2, isStroked: false, isSmoothJoin: true);
				}
				val = pts[pts.Count - 1];
				ctx.LineTo(new Point(val.X, 8.0 + h), isStroked: false, isSmoothJoin: false);
			}
			((Freezable)area).Freeze();
			dc.DrawGeometry(Ink.Fade(brush, 40), null, area);
			StreamGeometry line = new StreamGeometry();
			using (StreamGeometryContext ctx2 = line.Open())
			{
				ctx2.BeginFigure(pts[0], isFilled: false, isClosed: false);
				foreach (Point p3 in pts.Skip(1))
				{
					ctx2.LineTo(p3, isStroked: true, isSmoothJoin: true);
				}
			}
			((Freezable)line).Freeze();
			dc.DrawGeometry(null, Ink.Line(brush, 2.0), line);
			foreach (Point p4 in pts)
			{
				dc.DrawEllipse(brush, null, p4, 3.0, 3.0);
			}
		}
		dc.Pop();
		if (!Custom)
		{
			Ink.Draw(dc, this, Lang.T("Özel eğri kapalı · firmware yönetiyor"), new Point(42.0, 10.0), 11.5, Ink.Label);
		}
		Point P(FanPoint fanPoint)
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			return new Point(34.0 + w * (double)(fanPoint.Temperature - 20) / 80.0, 8.0 + h * (double)(100 - fanPoint.Percent) / 100.0);
		}
	}
}
