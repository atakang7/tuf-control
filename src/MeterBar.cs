using System;
using System.Windows;
using System.Windows.Media;

namespace TufControl;

public class MeterBar : FrameworkElement
{
	public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(MeterBar), (PropertyMetadata)(object)new FrameworkPropertyMetadata((object)0.0, FrameworkPropertyMetadataOptions.AffectsRender));

	public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(MeterBar), (PropertyMetadata)(object)new FrameworkPropertyMetadata((object)Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

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

	public Brush Fill
	{
		get
		{
			return (Brush)((DependencyObject)this).GetValue(FillProperty);
		}
		set
		{
			((DependencyObject)this).SetValue(FillProperty, (object)value);
		}
	}

	protected override void OnRender(DrawingContext dc)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		double h = ActualHeight;
		double w = ActualWidth;
		dc.DrawRectangle(Ink.Track, null, new Rect(0.0, 0.0, w, h));
		double f = Math.Clamp(Value, 0.0, 1.0) * w;
		if (f > 0.0)
		{
			dc.DrawRectangle(Fill, null, new Rect(0.0, 0.0, Math.Max(2.0, f), h));
		}
	}
}
