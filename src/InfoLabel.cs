using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TufControl;

public class InfoLabel : StackPanel
{
	public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(InfoLabel), new PropertyMetadata((object)"", (PropertyChangedCallback)((DependencyObject d, DependencyPropertyChangedEventArgs _) =>
	{
		((InfoLabel)(object)d).Sync();
	})));

	public static readonly DependencyProperty InfoProperty = DependencyProperty.Register("Info", typeof(string), typeof(InfoLabel), new PropertyMetadata((object)"", (PropertyChangedCallback)((DependencyObject d, DependencyPropertyChangedEventArgs _) =>
	{
		((InfoLabel)(object)d).Sync();
	})));

	public static readonly DependencyProperty LabelStyleProperty = DependencyProperty.Register("LabelStyle", typeof(Style), typeof(InfoLabel), new PropertyMetadata((object)null, (PropertyChangedCallback)((DependencyObject d, DependencyPropertyChangedEventArgs _) =>
	{
		((InfoLabel)(object)d).Sync();
	})));

	private readonly TextBlock label = new TextBlock
	{
		VerticalAlignment = VerticalAlignment.Center
	};

	private readonly TextBlock icon = new TextBlock
	{
		Text = "\ue946",
		FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
		FontSize = 12.0,
		Margin = new Thickness(7.0, 1.0, 0.0, 0.0),
		VerticalAlignment = VerticalAlignment.Center,
		Cursor = Cursors.Help,
		Background = Brushes.Transparent,
		Opacity = 0.55
	};

	public string Text
	{
		get
		{
			return (string)((DependencyObject)this).GetValue(TextProperty);
		}
		set
		{
			((DependencyObject)this).SetValue(TextProperty, (object)value);
		}
	}

	public string Info
	{
		get
		{
			return (string)((DependencyObject)this).GetValue(InfoProperty);
		}
		set
		{
			((DependencyObject)this).SetValue(InfoProperty, (object)value);
		}
	}

	public Style? LabelStyle
	{
		get
		{
			return (Style)((DependencyObject)this).GetValue(LabelStyleProperty);
		}
		set
		{
			((DependencyObject)this).SetValue(LabelStyleProperty, (object)value);
		}
	}

	public InfoLabel()
	{
		Orientation = Orientation.Horizontal;
		VerticalAlignment = VerticalAlignment.Center;
		ToolTipService.SetInitialShowDelay((DependencyObject)(object)icon, 60);
		ToolTipService.SetShowDuration((DependencyObject)(object)icon, 60000);
		icon.MouseEnter += (object _, MouseEventArgs _) =>
		{
			icon.Opacity = 1.0;
		};
		icon.MouseLeave += (object _, MouseEventArgs _) =>
		{
			icon.Opacity = 0.55;
		};
		Children.Add(label);
		Children.Add(icon);
		Sync();
	}

	private void Sync()
	{
		label.Text = Text;
		if (LabelStyle != null)
		{
			label.Style = LabelStyle;
		}
		icon.ToolTip = (string.IsNullOrEmpty(Info) ? null : Info);
		icon.Visibility = (string.IsNullOrEmpty(Info) ? Visibility.Collapsed : Visibility.Visible);
		icon.Foreground = label.Foreground;
	}

	static InfoLabel()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected Obj, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected Obj, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected Obj, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected Obj, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected Obj, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected Obj, but got Unknown
	}
}
