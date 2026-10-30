using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace TufControl;

public class PowerChart : FrameworkElement
{
	public double Spl = 35.0;

	public double Slow = 45.0;

	public double Fast = 54.0;

	public bool SplOn;

	public bool BoostOn;

	public Brush Accent = Brushes.DeepSkyBlue;

	private const double Left = 38.0;

	private const double Right = 10.0;

	private const double Top = 14.0;

	private const double Bottom = 24.0;

	private const double MaxW = 70.0;

	protected override void OnRender(DrawingContext dc)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_045f: Unknown result type (might be due to invalid IL or missing references)
		//IL_056f: Unknown result type (might be due to invalid IL or missing references)
		double w = Math.Max(1.0, ActualWidth - 38.0 - 10.0);
		double h = Math.Max(1.0, ActualHeight - 14.0 - 24.0);
		dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, ActualWidth, ActualHeight));
		for (int wv = 0; wv <= 60; wv += 20)
		{
			double y = Math.Round(Y(wv)) + 0.5;
			dc.DrawLine(Ink.Freeze(new Pen(Ink.Grid, 1.0)), new Point(38.0, y), new Point(38.0 + w, y));
			Ink.Draw(dc, this, wv + " W", new Point(30.0, y - 8.0), 10.5, Ink.Label, 1.0);
		}
		double cap = Math.Round(Y(65.0)) + 0.5;
		dc.DrawLine(Ink.Dashed(Ink.Alpha(Ink.Danger, 170), 1.2), new Point(38.0, cap), new Point(38.0 + w, cap));
		Ink.Draw(dc, this, Lang.F("Güvenli tavan {0} W", 65), new Point(38.0 + w, cap - 17.0), 10.5, Ink.Alpha(Ink.Danger, 200), 1.0);
		double x1 = 38.0 + w * 0.14;
		double x2 = 38.0 + w * 0.42;
		Point[] pts = new Point[6]
		{
			new Point(38.0, Y(Fast)),
			new Point(x1, Y(Fast)),
			new Point(x1, Y(Slow)),
			new Point(x2, Y(Slow)),
			new Point(x2, Y(Spl)),
			new Point(38.0 + w, Y(Spl))
		};
		StreamGeometry area = new StreamGeometry();
		using (StreamGeometryContext ctx = area.Open())
		{
			ctx.BeginFigure(new Point(38.0, Y(0.0)), isFilled: true, isClosed: true);
			Point[] array = pts;
			foreach (Point p in array)
			{
				ctx.LineTo(p, isStroked: false, isSmoothJoin: true);
			}
			ctx.LineTo(new Point(38.0 + w, Y(0.0)), isStroked: false, isSmoothJoin: false);
		}
		((Freezable)area).Freeze();
		bool any = SplOn || BoostOn;
		dc.PushOpacity(any ? 1.0 : 0.45);
		dc.DrawGeometry(Ink.Fade(Accent, 70), null, area);
		StreamGeometry line = new StreamGeometry();
		using (StreamGeometryContext ctx2 = line.Open())
		{
			ctx2.BeginFigure(pts[0], isFilled: false, isClosed: false);
			foreach (Point p2 in pts.Skip(1))
			{
				ctx2.LineTo(p2, isStroked: true, isSmoothJoin: true);
			}
		}
		((Freezable)line).Freeze();
		dc.DrawGeometry(null, Ink.Line(Accent, 2.4), line);
		dc.Pop();
		Tag(Lang.T("fPPT · anlık"), Fast, 38.0, x1);
		Tag(Lang.T("sPPT · kısa süre"), Slow, x1, x2);
		Tag(Lang.T("SPL · sürekli"), Spl, x2, 38.0 + w);
		if (!any)
		{
			Ink.Draw(dc, this, Lang.T("Firmware varsayılanı"), new Point(38.0 + w / 2.0, 14.0 + h / 2.0 + 10.0), 11.5, Ink.Label, 0.5);
		}
		void Tag(string label, double watts, double num, double num2)
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			Ink.Draw(dc, this, $"{watts:0} W", new Point((num + num2) / 2.0, Y(watts) - 20.0), 12.0, Ink.Text, 0.5, bold: true);
			Ink.Draw(dc, this, label, new Point((num + num2) / 2.0, 14.0 + h + 6.0), 10.5, Ink.Label, 0.5);
		}
		double Y(double watts)
		{
			return 14.0 + h * (1.0 - watts / 70.0);
		}
	}
}
