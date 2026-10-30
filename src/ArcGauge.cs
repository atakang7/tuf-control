using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TufControl;

public class ArcGauge : FrameworkElement
{
	public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(ArcGauge), (PropertyMetadata)(object)new FrameworkPropertyMetadata((object)double.NaN, (PropertyChangedCallback)((DependencyObject d, DependencyPropertyChangedEventArgs e) =>
	{
		((ArcGauge)(object)d).Animate((double)e.NewValue);
	})));

	private static readonly DependencyProperty DisplayProperty = DependencyProperty.Register("Display", typeof(double), typeof(ArcGauge), (PropertyMetadata)(object)new FrameworkPropertyMetadata((object)double.NaN, FrameworkPropertyMetadataOptions.AffectsRender));

	private const double Thickness = 12.0;

	private const double Sweep = 270.0;

	private const double Start = 135.0;

	private const double Min = 20.0;

	private const double Max = 100.0;

	public double Value
	{
		get
		{
			return (double)((DependencyObject)this).GetValue(ValueProperty);
		}
		set
		{
			((DependencyObject)this).SetValue(ValueProperty, (object)value);
		}
	}

	private void Animate(double to)
	{
		if (!double.IsFinite(to))
		{
			BeginAnimation(DisplayProperty, null);
			((DependencyObject)this).SetValue(DisplayProperty, (object)double.NaN);
			return;
		}
		double now = (double)((DependencyObject)this).GetValue(DisplayProperty);
		BeginAnimation(DisplayProperty, new DoubleAnimation(double.IsFinite(now) ? now : 20.0, to, TimeSpan.FromMilliseconds(700L))
		{
			EasingFunction = new CubicEase
			{
				EasingMode = EasingMode.EaseOut
			}
		});
	}

	protected override void OnRender(DrawingContext dc)
	{
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		double r = Math.Min(ActualWidth / 2.0 - 12.0 - 6.0, (ActualHeight - 12.0 - 8.0) / 1.7071);
		double cx;
		double cy;
		if (!(r < 20.0))
		{
			cx = ActualWidth / 2.0;
			cy = 10.0 + r;
			dc.DrawGeometry(null, Ink.Line(Ink.Track, 12.0), Arc(135.0, 270.0, r));
			for (int t = 20; t <= 100; t += 5)
			{
				double a = 135.0 + 270.0 * ((double)t - 20.0) / 80.0;
				bool major = t % 20 == 0;
				dc.DrawLine(Ink.Line(major ? Ink.Label : Ink.Grid, major ? 1.6 : 1.2), At(a, r + 6.0 + 4.0), At(a, r + 6.0 + (double)(major ? 10 : 7)));
			}
			double v = (double)((DependencyObject)this).GetValue(DisplayProperty);
			if (double.IsFinite(v))
			{
				double frac = Math.Clamp((v - 20.0) / 80.0, 0.005, 1.0);
				LinearGradientBrush gradient = new LinearGradientBrush
				{
					MappingMode = BrushMappingMode.Absolute,
					StartPoint = new Point(cx - r, cy),
					EndPoint = new Point(cx + r, cy)
				};
				gradient.GradientStops.Add(new GradientStop(Color.FromRgb(61, 214, 140), 0.0));
				gradient.GradientStops.Add(new GradientStop(Color.FromRgb(245, 184, 61), 0.55));
				gradient.GradientStops.Add(new GradientStop(Color.FromRgb(byte.MaxValue, 93, 93), 1.0));
				((Freezable)gradient).Freeze();
				Geometry arc = Arc(135.0, 270.0 * frac, r);
				dc.PushOpacity(0.16);
				dc.DrawGeometry(null, Ink.Line(gradient, 24.0), arc);
				dc.Pop();
				dc.DrawGeometry(null, Ink.Line(gradient, 12.0), arc);
				dc.DrawEllipse(Ink.Text, null, At(135.0 + 270.0 * frac, r), 4.0, 4.0);
			}
		}
		Geometry Arc(double from, double sweep, double rad)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			StreamGeometry g = new StreamGeometry();
			using (StreamGeometryContext ctx = g.Open())
			{
				ctx.BeginFigure(At(from, rad), isFilled: false, isClosed: false);
				ctx.ArcTo(At(from + sweep, rad), new Size(rad, rad), 0.0, sweep > 180.0, SweepDirection.Clockwise, isStroked: true, isSmoothJoin: false);
			}
			((Freezable)g).Freeze();
			return g;
		}
		Point At(double deg, double rad)
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			return new Point(cx + rad * Math.Cos(deg * Math.PI / 180.0), cy + rad * Math.Sin(deg * Math.PI / 180.0));
		}
	}

	static ArcGauge()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected Obj, but got Unknown
	}
}
