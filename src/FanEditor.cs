using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TufControl;

public class FanEditor : UserControl
{
	public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register("Series", typeof(Brush), typeof(FanEditor), (PropertyMetadata)(object)new FrameworkPropertyMetadata((object)Brushes.DeepSkyBlue, (PropertyChangedCallback)((DependencyObject d, DependencyPropertyChangedEventArgs _) =>
	{
		FanEditor fanEditor = (FanEditor)(object)d;
		fanEditor.canvas.Series = fanEditor.Series;
		fanEditor.canvas.InvalidateVisual();
	})));

	private readonly CurveCanvas canvas = new CurveCanvas
	{
		Height = 300.0
	};

	private FanPoint[] points = Profile.DefaultCurve();

	public Brush Series
	{
		get
		{
			return (Brush)((DependencyObject)this).GetValue(SeriesProperty);
		}
		set
		{
			((DependencyObject)this).SetValue(SeriesProperty, (object)value);
		}
	}

	public double Current
	{
		get
		{
			return canvas.Current;
		}
		set
		{
			canvas.Current = value;
			canvas.InvalidateVisual();
		}
	}

	public FanPoint[] Points
	{
		get
		{
			return points.ToArray();
		}
		set
		{
			points = value.ToArray();
			Sync();
		}
	}

	public event Action? Changed;

	public FanEditor()
	{
		canvas.Edit = SetPoint;
		Content = canvas;
		Sync();
	}

	private void SetPoint(int i, int t, int pct)
	{
		points[i] = new FanPoint(t, Math.Max(pct, Floor(t)));
		Sync();
		Changed?.Invoke();
	}

	private void Sync()
	{
		canvas.Points = points;
		canvas.InvalidateVisual();
	}

	public static int Floor(int t)
	{
		if (t < 90)
		{
			if (t < 80)
			{
				if (t < 70)
				{
					return 0;
				}
				return 30;
			}
			return 70;
		}
		return 100;
	}

	static FanEditor()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected Obj, but got Unknown
	}
}
