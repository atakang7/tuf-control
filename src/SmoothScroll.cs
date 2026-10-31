using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace TufControl;

// WPF scrolls a fixed 48 px per wheel event, however small the event is. Touchpads send many small
// events, so pages jump in steps. This scrolls in proportion to the wheel delta and glides to the
// target over a few frames, for every vertical ScrollViewer in the window.
public static class SmoothScroll
{
	private sealed class State
	{
		public double Target;

		public bool Running;

		public TimeSpan Last;
	}

	// Pixels per wheel unit: one mouse notch (120) ≈ 100 px.
	private const double PixelsPerDelta = 100.0 / 120.0;

	// Time constant of the glide; ~95 % of the distance is covered in 3 × this.
	private const double GlideSeconds = 0.12;

	private static readonly ConditionalWeakTable<ScrollViewer, State> states = new ConditionalWeakTable<ScrollViewer, State>();

	public static void Attach(Window window)
	{
		window.PreviewMouseWheel += OnWheel;
	}

	private static void OnWheel(object sender, MouseWheelEventArgs e)
	{
		ScrollViewer sv = Find(e.OriginalSource as DependencyObject);
		if (sv == null)
		{
			return;
		}
		State s = states.GetValue(sv, (ScrollViewer _) => new State());
		double from = (s.Running ? s.Target : sv.VerticalOffset);
		s.Target = Math.Clamp(from - (double)e.Delta * PixelsPerDelta, 0.0, sv.ScrollableHeight);
		e.Handled = true;
		if (!s.Running)
		{
			s.Running = true;
			s.Last = TimeSpan.Zero;
			Step(sv, s);
		}
	}

	private static void Step(ScrollViewer sv, State s)
	{
		EventHandler tick = null;
		tick = (object? _, EventArgs args) =>
		{
			TimeSpan now = ((RenderingEventArgs)args).RenderingTime;
			double dt = ((s.Last == TimeSpan.Zero) ? 0.016 : Math.Clamp((now - s.Last).TotalSeconds, 0.001, 0.05));
			if (now == s.Last)
			{
				return;
			}
			s.Last = now;
			double diff = s.Target - sv.VerticalOffset;
			if (Math.Abs(diff) < 0.5 || !sv.IsVisible)
			{
				sv.ScrollToVerticalOffset(s.Target);
				s.Running = false;
				CompositionTarget.Rendering -= tick;
				return;
			}
			sv.ScrollToVerticalOffset(sv.VerticalOffset + diff * (1.0 - Math.Exp(-dt / GlideSeconds)));
		};
		CompositionTarget.Rendering += tick;
	}

	// The innermost page ScrollViewer under the pointer that can scroll vertically. Text boxes and
	// sliders keep their own wheel behaviour.
	private static ScrollViewer Find(DependencyObject d)
	{
		while (d != null)
		{
			if (d is TextBoxBase || d is ComboBox)
			{
				return null;
			}
			if (d is ScrollViewer sv && sv.ScrollableHeight > 0.0 && sv.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
			{
				return sv;
			}
			d = (d is Visual || d is System.Windows.Media.Media3D.Visual3D) ? VisualTreeHelper.GetParent(d) : LogicalTreeHelper.GetParent(d);
		}
		return null;
	}
}
