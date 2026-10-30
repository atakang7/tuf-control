using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;

namespace TufControl;

public partial class MainWindow : Window, IComponentConnector, IStyleConnector
{
	private sealed class ProfileAcc
	{
		public double Seconds;

		public readonly RunningStats CpuTemp = new RunningStats();

		public readonly RunningStats CpuLoad = new RunningStats();

		public readonly RunningStats CpuFan = new RunningStats();

		public readonly RunningStats BatteryDraw = new RunningStats();
	}

	private sealed record MonitorTile(Metric Metric, MetricChart Chart, TextBlock Current, TextBlock Min, TextBlock Avg, TextBlock Max, TextBlock Seg);

	public sealed record HeaderItem(string Name, string Icon, bool IsActive, string Hint);

	private sealed record StartupChoice(string Mode, string Profile, string Label);

	private AdvisorReport? adviceReport;

	private int adviceRun;

	private MetricLog history;

	private DateTime segStart = DateTime.Now;

	private DateTime lastSample = DateTime.Now;

	private DateTime lastCurrent = DateTime.Now;

	private readonly Dictionary<string, ProfileAcc> profileAcc = new Dictionary<string, ProfileAcc>();

	private double onBatterySec;

	private double energyWh;

	private readonly bool readOnly;

	private readonly AsusHardware hardware;

	private readonly Thermal thermal;

	private readonly NvidiaSensor nvidia = new NvidiaSensor();

	private readonly SystemSensor system = new SystemSensor();

	private readonly Controller controller;

	private readonly Store store;

	private Settings settings = new Settings();

	private string selected = "Dengeli";

	private bool loading = true;

	private bool quitting;

	private bool autoPaused;

	private bool ticking;

	private bool editDirty;

	private bool ordering;

	private bool navigating;

	private readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);

	private readonly DispatcherTimer timer = new DispatcherTimer
	{
		Interval = TimeSpan.FromSeconds(3L)
	};

	private readonly NotifyIcon tray;

	private string[] conflicts = Array.Empty<string>();

	private Snapshot? latest;

	private Capabilities? capabilities;

	private DateTimeOffset nextAuto = DateTimeOffset.MaxValue;

	private DateTimeOffset lastThermalCheck = DateTimeOffset.MinValue;

	private Profile? applied;

	private int retries;

	private bool? previousAc;

	private string[] logLines = Array.Empty<string>();

	private DateTimeOffset nextGpu = DateTimeOffset.MinValue;

	private GpuStats gpuStats = new GpuStats(null, null, null, null, null, null);

	private SystemStats? lastSys;

	private string? autoProfile;

	private int autoStable;

	private bool externalDisplay = SystemSensor.ExternalDisplayConnected();

	private bool startupApply;

	private bool ecoPending;

	private bool thermalPending;

	private int ecoTries;

	private int thermalTries;

	private DateTimeOffset ecoRetryAt;

	private DateTimeOffset thermalRetryAt;

	private string? pendingName;

	private Profile? requested;

	private readonly List<MonitorTile> tiles = new List<MonitorTile>();

	private readonly List<(Metric Metric, TextBlock Text)> historyStats = new List<(Metric, TextBlock)>();

	private readonly DateTime sessionStart = DateTime.Now;

	private TimeSpan monitorWindow = TimeSpan.FromMinutes(5L);

	private TimeSpan historyWindow = TimeSpan.FromMinutes(6L);

	private Metric mCpuTemp;

	private Metric mGpuTemp;

	private Metric mCpuLoad;

	private Metric mGpuLoad;

	private Metric mCpuClock;

	private Metric mGpuClock;

	private Metric mGpuPower;

	private Metric mCpuFan;

	private Metric mGpuFan;

	private Metric mMem;

	private Metric mBattery;

	private Metric mVram;

	private readonly ObservableCollection<AppRule> rules = new ObservableCollection<AppRule>();

	private static readonly HashSet<string> GpuServiceProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"dwm", "csrss", "explorer", "nvcontainer", "NVDisplay.Container", "nvsphelper64", "NVIDIA Share", "NVIDIA Overlay", "NVIDIA Web Helper", "NVIDIA app",
		"TufControl", "System", "Idle", "LockApp", "CrossDeviceResume", "SearchHost", "StartMenuExperienceHost", "ShellExperienceHost", "TextInputHost", "PhoneExperienceHost",
		"Widgets", "WidgetService", "ApplicationFrameHost", "SystemSettings", "msedgewebview2", "RuntimeBroker", "ShellHost"
	};

	private const int GpuIdleUsage = 10;

	private const double MaxRpm = 6500.0;

	private static readonly System.Windows.Media.Color[] ModeColors = new System.Windows.Media.Color[3]
	{
		System.Windows.Media.Color.FromRgb(byte.MaxValue, 45, 70),
		System.Windows.Media.Color.FromRgb(byte.MaxValue, 45, 70),
		System.Windows.Media.Color.FromRgb(byte.MaxValue, 45, 70)
	};

	private static readonly FanPoint[] SilentPreset = new FanPoint[8]
	{
		new FanPoint(30, 0),
		new FanPoint(40, 0),
		new FanPoint(50, 25),
		new FanPoint(60, 35),
		new FanPoint(70, 50),
		new FanPoint(80, 70),
		new FanPoint(90, 100),
		new FanPoint(100, 100)
	};

	private static readonly FanPoint[] PerformancePreset = new FanPoint[8]
	{
		new FanPoint(30, 30),
		new FanPoint(40, 35),
		new FanPoint(50, 45),
		new FanPoint(60, 60),
		new FanPoint(70, 75),
		new FanPoint(80, 90),
		new FanPoint(90, 100),
		new FanPoint(100, 100)
	};

	private static readonly System.Windows.Media.Brush SeriesCpuTemp = Ink.Freeze(new SolidColorBrush(System.Windows.Media.Color.FromRgb(byte.MaxValue, 138, 61)));

	private static readonly System.Windows.Media.Brush SeriesGpuTemp = Ink.Freeze(new SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 214, 140)));

	private static readonly System.Windows.Media.Brush SeriesCpuLoad = Ink.Freeze(new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 141, byte.MaxValue)));

	private static readonly System.Windows.Media.Brush SeriesGpuPower = Ink.Freeze(new SolidColorBrush(System.Windows.Media.Color.FromRgb(167, 139, 250)));

	private static readonly Style TileLabel = new Style(typeof(TextBlock))
	{
		Setters = 
		{
			(SetterBase)new Setter(TextBlock.FontSizeProperty, 14.0),
			(SetterBase)new Setter(TextBlock.FontWeightProperty, FontWeights.SemiBold)
		}
	};

	private TimeSpan AdviceSpan
	{
		get
		{
			if (AdvDay.IsChecked != true)
			{
				if (AdvMonth.IsChecked != true)
				{
					return TimeSpan.FromDays(7);
				}
				return TimeSpan.FromDays(30);
			}
			return TimeSpan.FromDays(1);
		}
	}

	private (string Key, Metric Metric)[] HistoryMetrics => new (string, Metric)[12]
	{
		("cpu_temp", mCpuTemp),
		("gpu_temp", mGpuTemp),
		("cpu_load", mCpuLoad),
		("gpu_load", mGpuLoad),
		("cpu_clock", mCpuClock),
		("gpu_clock", mGpuClock),
		("gpu_power", mGpuPower),
		("cpu_fan", mCpuFan),
		("gpu_fan", mGpuFan),
		("memory", mMem),
		("battery_flow", mBattery),
		("vram", mVram)
	};

	public string[] ProfileNames => settings.Profiles.Select((Profile p) => p.Name).ToArray();

	private System.Windows.Controls.RadioButton[] NavItems => new System.Windows.Controls.RadioButton[7] { NavHome, NavMonitor, NavCooling, NavPower, NavAdvice, NavLogs, NavSettings };

	private int BaseMode
	{
		get
		{
			if (BaseTurbo.IsChecked != true)
			{
				if (BaseSilent.IsChecked != true)
				{
					return 0;
				}
				return 2;
			}
			return 1;
		}
	}

	private async void RefreshAdvice()
	{
		if (history == null || AdviceStack == null)
		{
			return;
		}
		int run = ++adviceRun;
		TimeSpan span = AdviceSpan;
		string folder = history.Folder;
		Settings current = settings;
		AdvisorReport report;
		try
		{
			report = await Task.Run(() => Advisor.Analyze(HistoryReader.Load(folder, DateTime.Now - span), current));
		}
		catch (Exception ex)
		{
			SetStatus(ex.Message, "Bad");
			return;
		}
		if (run == adviceRun)
		{
			adviceReport = report;
			RenderAdvice(report);
		}
	}

	private void AdviceRangeChanged(object sender, RoutedEventArgs e)
	{
		if (IsLoaded)
		{
			RefreshAdvice();
		}
	}

	private void AdviceRefreshClick(object sender, RoutedEventArgs e)
	{
		RefreshAdvice();
	}

	private static string Dur(double hours)
	{
		int m = (int)Math.Round(hours * 60.0);
		return Lang.F("{0} sa {1} dk", m / 60, m % 60);
	}

	private void RenderAdvice(AdvisorReport r)
	{
		AdvHours.Text = Dur(r.Hours);
		AdvBattery.Text = Dur(r.BatteryHours);
		TextBlock advDraw = AdvDraw;
		double? avgBatteryDrawW = r.AvgBatteryDrawW;
		string text;
		if (avgBatteryDrawW.HasValue)
		{
			double w = avgBatteryDrawW.GetValueOrDefault();
			text = $"{w:0.0} W";
		}
		else
		{
			text = "—";
		}
		advDraw.Text = text;
		AdvRows.Text = r.Rows.ToString("N0");
		bool off = !settings.RecordHistory;
		AdviceNeedData.Visibility = ((r.Enough && !off) ? Visibility.Collapsed : Visibility.Visible);
		AdvNeedText.Text = (off ? Lang.T("Ölçüm geçmişi kapalı. Ayarlar'dan açarsan öneriler için veri toplanır.") : Lang.F("Öneri çıkarabilmek için en az {0} dk kayıt gerekiyor; şimdilik {1} dk var. Uygulama açık kaldıkça kayıt birikir.", 15, (int)Math.Round(r.Hours * 60.0)));
		AdvProgress.Value = Math.Clamp(r.Hours / 0.25, 0.0, 1.0);
		AdviceStack.Children.Clear();
		foreach (Advice a in r.Advice)
		{
			AdviceStack.Children.Add(AdviceCard(a));
		}
		AdviceProfiles.ItemsSource = r.Profiles.Select((ProfileStat p) =>
		{
			string profile = p.Profile;
			string hours = Dur(p.Hours);
			double? avgCpuTemp = p.AvgCpuTemp;
			string cpuTemp;
			if (avgCpuTemp.HasValue)
			{
				double valueOrDefault = avgCpuTemp.GetValueOrDefault();
				cpuTemp = $"{valueOrDefault:0} °C";
			}
			else
			{
				cpuTemp = "—";
			}
			avgCpuTemp = p.AvgCpuLoad;
			string cpuLoad;
			if (avgCpuTemp.HasValue)
			{
				double valueOrDefault2 = avgCpuTemp.GetValueOrDefault();
				cpuLoad = $"{valueOrDefault2:0} %";
			}
			else
			{
				cpuLoad = "—";
			}
			avgCpuTemp = p.AvgCpuFan;
			string fan;
			if (avgCpuTemp.HasValue)
			{
				double valueOrDefault3 = avgCpuTemp.GetValueOrDefault();
				fan = $"{valueOrDefault3:N0} rpm";
			}
			else
			{
				fan = "—";
			}
			avgCpuTemp = p.IdleBatteryDrawW;
			string draw;
			if (avgCpuTemp.HasValue)
			{
				double valueOrDefault4 = avgCpuTemp.GetValueOrDefault();
				draw = $"{valueOrDefault4:0.0} W";
			}
			else
			{
				draw = "—";
			}
			return new ProfileRow(profile, hours, cpuTemp, cpuLoad, fan, draw);
		}).ToList();
		AdvProfilesEmpty.Visibility = ((r.Profiles.Count != 0) ? Visibility.Collapsed : Visibility.Visible);
	}

	private Border AdviceCard(Advice a)
	{
		string accentKey = a.Kind switch
		{
			AdviceKind.Warn => "Warn", 
			AdviceKind.Good => "Good", 
			_ => "Accent", 
		};
		string icon = a.Kind switch
		{
			AdviceKind.Warn => "\ue7ba", 
			AdviceKind.Good => "\ue73e", 
			_ => "\ue82f", 
		};
		System.Windows.Media.Brush accent = (System.Windows.Media.Brush)FindResource(accentKey);
		Border tile = new Border
		{
			Width = 42.0,
			Height = 42.0,
			CornerRadius = new CornerRadius(2.0),
			Background = Ink.Alpha(accent, 38),
			VerticalAlignment = VerticalAlignment.Top
		};
		tile.Child = new TextBlock
		{
			Text = icon,
			FontFamily = (System.Windows.Media.FontFamily)FindResource("IconFont"),
			FontSize = 18.0,
			Foreground = accent,
			HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center
		};
		StackPanel body = new StackPanel
		{
			Margin = new Thickness(16.0, 0.0, 0.0, 0.0)
		};
		body.Children.Add(new TextBlock
		{
			Text = Lang.F(a.Title, a.Args),
			Style = (Style)FindResource("CardHead"),
			TextWrapping = TextWrapping.Wrap
		});
		body.Children.Add(new TextBlock
		{
			Text = Lang.F(a.Why, a.Args),
			Style = (Style)FindResource("T.Body"),
			FontSize = 12.5,
			Margin = new Thickness(0.0, 6.0, 0.0, 0.0)
		});
		StackPanel fix = new StackPanel
		{
			Orientation = System.Windows.Controls.Orientation.Horizontal,
			Margin = new Thickness(0.0, 8.0, 0.0, 0.0)
		};
		fix.Children.Add(new TextBlock
		{
			Text = "\ue76c",
			FontFamily = (System.Windows.Media.FontFamily)FindResource("IconFont"),
			FontSize = 11.0,
			Foreground = accent,
			VerticalAlignment = VerticalAlignment.Top,
			Margin = new Thickness(0.0, 3.0, 8.0, 0.0)
		});
		fix.Children.Add(new TextBlock
		{
			Text = Lang.F(a.Fix, a.Args),
			FontSize = 12.5,
			FontWeight = FontWeights.SemiBold,
			TextWrapping = TextWrapping.Wrap,
			MaxWidth = 760.0
		});
		body.Children.Add(fix);
		DockPanel row = new DockPanel();
		AdviceAction act = a.Action;
		if ((object)act != null)
		{
			System.Windows.Controls.Button button = new System.Windows.Controls.Button
			{
				Content = Lang.F(act.LabelTemplate, act.LabelArgs),
				Tag = "\ue73e",
				Style = (Style)FindResource("Primary"),
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(20.0, 0.0, 0.0, 0.0)
			};
			button.Click += (object _, RoutedEventArgs _) =>
			{
				ApplyAdvice(act);
			};
			DockPanel.SetDock(button, Dock.Right);
			row.Children.Add(button);
		}
		DockPanel.SetDock(tile, Dock.Left);
		row.Children.Add(tile);
		row.Children.Add(body);
		return new Border
		{
			Style = (Style)FindResource("Card"),
			Margin = new Thickness(0.0, 0.0, 0.0, 12.0),
			Padding = new Thickness(20.0, 18.0, 20.0, 18.0),
			Child = row
		};
	}

	private void ApplyAdvice(AdviceAction action)
	{
		if (readOnly)
		{
			SetStatus("Salt okunur oturum: ayar kaydedilmez.", "Warn");
			return;
		}
		if (editDirty)
		{
			SetStatus("Önce Soğutma/Güç sayfasındaki kaydedilmemiş değişiklikleri kaydet ya da geri al.", "Warn");
			return;
		}
		try
		{
			Settings candidate = Advisor.Apply(settings, action);
			store.Save(candidate);
			settings = candidate;
			FillPickers();
			LoadProfile();
			RefreshHeader();
			Log(Lang.F("Öneri uygulandı: {0} profili güncellendi.", action.Profile));
			SetStatus(Lang.F("{0} profili güncellendi. Donanıma göndermek için profili uygula.", action.Profile), "Good");
			RefreshAdvice();
		}
		catch (Exception ex)
		{
			SetStatus(ex.Message, "Bad");
		}
	}

	private void TryAdoptBootState()
	{
		BootState state = BootHandoff.Read(store.DirectoryPath);
		if ((object)state == null)
		{
			return;
		}
		DateTime appStart;
		using (Process self = Process.GetCurrentProcess())
		{
			appStart = self.StartTime.ToUniversalTime();
		}
		if (!BootHandoff.IsFresh(state, appStart))
		{
			return;
		}
		Profile profile = settings.Profiles.FirstOrDefault((Profile p) => p.Name == state.Profile);
		if (profile == null)
		{
			return;
		}
		controller.AssumeMode(state.Mode);
		if (state.ThermalActive)
		{
			int? thermalBaseline = state.ThermalBaseline;
			if (thermalBaseline.HasValue)
			{
				int baseline = thermalBaseline.GetValueOrDefault();
				controller.AssumeThermal(baseline);
			}
		}
		applied = profile.Clone();
		applied.GpuMode = 0;
		requested = profile.Clone();
		pendingName = null;
		if (state.ThermalPending)
		{
			applied.LimitTemperature = false;
			thermalPending = true;
			thermalTries = 0;
			thermalRetryAt = DateTimeOffset.Now.AddSeconds(4.0);
		}
		startupApply = profile.GpuMode != 0;
		bool onAc = SystemInformation.PowerStatus.PowerLineStatus != System.Windows.Forms.PowerLineStatus.Offline;
		autoProfile = settings.Desired(onAc, IsRunning);
		Log(Lang.F("Profil, oturum açılmadan önce uygulanmıştı: {0}.", profile.Name));
	}

	private void SyncBootInstall()
	{
		if (readOnly || !settings.BootEarly || !StartupTask.IsAdmin)
		{
			return;
		}
		Task.Run(() =>
		{
			try
			{
				if (StartupTask.InstallOutdated())
				{
					StartupTask.EnableBoot(store.FilePath);
					((DispatcherObject)this).Dispatcher.BeginInvoke((Delegate)(Action)(() =>
					{
						Log(Lang.T("Erken başlatma kopyası bu sürümle güncellendi."));
					}), Array.Empty<object>());
				}
			}
			catch (Exception ex)
			{
				Exception ex2 = ex;
				Exception e = ex2;
				((DispatcherObject)this).Dispatcher.BeginInvoke((Delegate)(Action)(() =>
				{
					Log(e.Message);
				}), Array.Empty<object>());
			}
		});
	}

	private void RefreshBootInfo()
	{
		if (BootEarlyState != null)
		{
			bool admin = StartupTask.IsAdmin;
			BootEarlyBox.IsEnabled = !readOnly & admin;
			BootState state = BootHandoff.Read(store.DirectoryPath);
			string last = (((object)state == null) ? "" : (" · " + Lang.F("son açılışta uygulanan: {0} ({1})", state.Profile, state.AppliedAt.ToLocalTime().ToString("dd.MM HH:mm:ss"))));
			BootEarlyState.Text = ((!admin) ? Lang.T("Açıp kapatmak için uygulamayı yönetici olarak aç (Windows ile başlat kartındaki düğme).") : (Lang.T(settings.BootEarly ? "Etkin" : "Kapalı") + last));
		}
	}

	private void BootEarlyClick(object sender, RoutedEventArgs e)
	{
		bool on = BootEarlyBox.IsChecked == true;
		if (readOnly)
		{
			BootEarlyBox.IsChecked = settings.BootEarly;
			SetStatus("Salt okunur oturum: ayar kaydedilmez.", "Warn");
			return;
		}
		try
		{
			if (on)
			{
				StartupTask.EnableBoot(store.FilePath);
			}
			else
			{
				StartupTask.DisableBoot();
			}
			Settings candidate = Copy(settings);
			candidate.BootEarly = on;
			store.Save(candidate);
			settings = candidate;
			Log(Lang.T(on ? "Erken başlatma etkinleştirildi." : "Erken başlatma kapatıldı."));
			SetStatus(on ? "Erken başlatma etkin: bir sonraki açılışta profil oturum açılmadan önce uygulanır." : "Erken başlatma kapatıldı.", "Good");
		}
		catch (Exception ex)
		{
			BootEarlyBox.IsChecked = settings.BootEarly;
			SetStatus(ex.Message, "Bad");
		}
		RefreshBootInfo();
	}

	private void InitHistory()
	{
		history = new MetricLog(System.IO.Path.Combine(store.DirectoryPath, "history"));
		if (!readOnly)
		{
			history.Cleanup(DateTime.Now);
		}
		System.Windows.Application.Current.SessionEnding += (object _, SessionEndingCancelEventArgs _) =>
		{
			EndSegment("shutdown");
		};
	}

	private void StartSegment()
	{
		lastCurrent = (lastSample = (segStart = DateTime.Now));
		(string, Metric)[] historyMetrics = HistoryMetrics;
		for (int i = 0; i < historyMetrics.Length; i++)
		{
			historyMetrics[i].Item2.Segment.Reset();
		}
		profileAcc.Clear();
		onBatterySec = 0.0;
		energyWh = 0.0;
	}

	private SegmentSummary Summary(DateTime now, string reason)
	{
		Dictionary<string, MetricSummary> metrics = HistoryMetrics.ToDictionary(((string Key, Metric Metric) x) => x.Key, ((string Key, Metric Metric) x) => new MetricSummary(x.Metric.Unit, x.Metric.Segment.Min, x.Metric.Segment.Avg, x.Metric.Segment.Max, x.Metric.Segment.Count));
		Dictionary<string, ProfileSummary> profiles = profileAcc.ToDictionary((KeyValuePair<string, ProfileAcc> x) => x.Key, (KeyValuePair<string, ProfileAcc> x) => new ProfileSummary(Math.Round(x.Value.Seconds), x.Value.CpuTemp.Avg, x.Value.CpuLoad.Avg, x.Value.CpuFan.Avg, x.Value.BatteryDraw.Avg));
		return new SegmentSummary(segStart, now, reason, Math.Round((now - segStart).TotalSeconds), Math.Round(onBatterySec), (energyWh > 0.0) ? new double?(Math.Round(energyWh, 3)) : ((double?)null), metrics, profiles);
	}

	private void EndSegment(string reason)
	{
		if (!readOnly && history != null && settings.RecordHistory && mCpuTemp.Segment.Count >= 3)
		{
			history.WriteSegment(Summary(DateTime.Now, reason));
		}
	}

	private void RecordSample(DateTime now, SystemStats sys)
	{
		double dt = Math.Clamp((now - lastSample).TotalSeconds, 0.0, 10.0);
		lastSample = now;
		string profile = applied?.Name ?? "(firmware)";
		if (!this.profileAcc.TryGetValue(profile, out ProfileAcc acc))
		{
			acc = (this.profileAcc[profile] = new ProfileAcc());
		}
		acc.Seconds += dt;
		double? last = mCpuTemp.Last;
		if (last.HasValue)
		{
			double t = last.GetValueOrDefault();
			acc.CpuTemp.Add(t);
		}
		last = mCpuLoad.Last;
		if (last.HasValue)
		{
			double l = last.GetValueOrDefault();
			acc.CpuLoad.Add(l);
		}
		last = mCpuFan.Last;
		if (last.HasValue)
		{
			double f = last.GetValueOrDefault();
			acc.CpuFan.Add(f);
		}
		Snapshot? snapshot = latest;
		if ((object)snapshot != null && snapshot.OnAc == false)
		{
			onBatterySec += dt;
			last = sys.SystemDraw;
			if (last.HasValue)
			{
				double w = last.GetValueOrDefault();
				acc.BatteryDraw.Add(w);
				energyWh += w * dt / 3600.0;
			}
		}
		if (!readOnly && settings.RecordHistory)
		{
			double?[] values = HistoryMetrics.Select(((string Key, Metric Metric) x) => x.Metric.Last).Append(latest?.Battery).ToArray();
			history.Row(now, profile, latest?.OnAc, values);
			if ((now - lastCurrent).TotalSeconds >= 60.0)
			{
				lastCurrent = now;
				history.WriteCurrent(Summary(now, "running"));
			}
		}
	}

	private void RefreshHistoryInfo()
	{
		if (history != null)
		{
			double mb = (double)history.SizeBytes() / 1048576.0;
			HistorySize.Text = Lang.F("{0} MB kullanılıyor · 30 günden eski ölçümler silinir", mb.ToString("0.0"));
		}
	}

	private void RecordHistoryClick(object sender, RoutedEventArgs e)
	{
		if (readOnly)
		{
			RecordHistoryBox.IsChecked = settings.RecordHistory;
			SetStatus("Salt okunur oturum: ayar kaydedilmez.", "Warn");
			return;
		}
		try
		{
			Settings candidate = Copy(settings);
			candidate.RecordHistory = RecordHistoryBox.IsChecked == true;
			store.Save(candidate);
			settings = candidate;
			SetStatus("Kaydedildi.", "Good");
		}
		catch (Exception ex)
		{
			SetStatus(ex.Message, "Bad");
		}
	}

	private void OpenHistory(object sender, RoutedEventArgs e)
	{
		try
		{
			Directory.CreateDirectory(history.Folder);
			Process.Start(new ProcessStartInfo("explorer.exe", history.Folder)
			{
				UseShellExecute = true
			});
		}
		catch (Exception ex)
		{
			SetStatus(ex.Message, "Bad");
		}
	}

	private static System.Windows.Media.Brush Hex(string hex)
	{
		return Ink.Freeze(new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex)));
	}

	private void BuildMonitor()
	{
		mCpuTemp = M("CPU sıcaklığı", "°C", "0", 20.0, 100.0, "#FF8A3D", "CPU'nun anlık sıcaklığı. 60 °C altı serin, 60–80 °C yük altında normal, 80 °C üstü sıcak. Ryzen işlemciler 95 °C'ye kadar dayanacak şekilde tasarlanır; sürekli 90 °C üstü fan eğrisini ya da güç sınırını gözden geçirmen gerektiğini gösterir.");
		mGpuTemp = M("GPU sıcaklığı", "°C", "0", 20.0, 100.0, "#3DD68C", "RTX'in sıcaklığı. 'RTX uykuda' ya da 'Eco' görüyorsan kart kullanılmıyor ve güç çekmiyor; pil için ideal olan budur. Oyunda 65–80 °C normaldir, NVIDIA kartı 87 °C'de yavaşlatır.");
		mCpuLoad = M("CPU kullanımı", "%", "0", 0.0, 100.0, "#4C8DFF", "Tüm çekirdeklerin ortalama yükü. Boşta %0–10 normaldir. Düşük kullanımda yüksek sıcaklık, arka planda bir şeyin çalıştığını ya da fanların yetersiz kaldığını gösterebilir.");
		mGpuLoad = M("GPU kullanımı", "%", "0", 0.0, 100.0, "#A78BFA", "RTX'in ne kadar meşgul olduğu. Oyunda %90 ve üstü normaldir. Masaüstünde %0 olmalı; değilse bir uygulama RTX'i boşuna çalıştırıyordur.");
		mCpuClock = M("CPU saat hızı", "GHz", "0.00", 0.0, 5.0, "#2DD4BF", "İşlemcinin anlık ortalama saat hızı. Boşta düşer, yükte yükselir. Yük altındayken hızın belirgin düşmesi, işlemcinin ısı ya da güç sınırına takıldığını gösterir.");
		mGpuClock = M("GPU saat hızı", "GHz", "0.00", 0.0, 3.0, "#F472B6", "RTX'in grafik çekirdeği saat hızı. Yükte yükselir; oyunda ani düşüşler ısı ya da güç sınırına işaret eder.");
		mGpuPower = M("GPU gücü", "W", "0", 0.0, 120.0, "#C084FC", "RTX'in anlık güç tüketimi. Boşta birkaç watt, oyunda 60–115 W olabilir. Bataryadayken yüksek görmek pili hızla bitirir.");
		mCpuFan = M("CPU fanı", "rpm", "N0", 0.0, 6500.0, "#4CC2FF", "Fanların dakikadaki devri. Yaklaşık 2.500 rpm altı sessiz, 4.500 rpm üstü belirgin gürültülüdür. 0 rpm, fanın eğrine göre durduğu anlamına gelir.");
		mGpuFan = M("GPU fanı", "rpm", "N0", 0.0, 6500.0, "#8B9CFF", "Fanların dakikadaki devri. Yaklaşık 2.500 rpm altı sessiz, 4.500 rpm üstü belirgin gürültülüdür. 0 rpm, fanın eğrine göre durduğu anlamına gelir.");
		mMem = M("Bellek kullanımı", "%", "0", 0.0, 100.0, "#60A5FA", "Kullanılan RAM. Sürekli %85 üstü bilgisayarı yavaşlatır; açık uygulamaları azaltmak yardımcı olur.");
		mBattery = M("Batarya akışı", "W", "+0.0;-0.0;0", -60.0, 60.0, "#F5B83D", "Bataryadan akan güç. Eksi (−) değer, bataryadayken bütün laptopun o anki toplam tüketimidir: ne kadar küçükse pil o kadar uzun gider. Artı (+) değer şarj gücüdür. Prizdeyken toplam tüketim ölçülemez.");
		mVram = M("VRAM kullanımı", "GB", "0.0", 0.0, 6.0, "#34D399", "Ekran kartının belleği. %100'e yaklaşması oyunlarda takılmaya yol açabilir; doku kalitesini düşürmek yardımcı olur.");
		Metric[] array = new Metric[12]
		{
			mCpuTemp, mGpuTemp, mCpuLoad, mGpuLoad, mCpuClock, mGpuClock, mGpuPower, mCpuFan, mGpuFan, mMem,
			mBattery, mVram
		};
		foreach (Metric m in array)
		{
			MonitorGrid.Children.Add(Tile(m));
		}
		array = new Metric[4] { mCpuTemp, mGpuTemp, mCpuLoad, mGpuPower };
		foreach (Metric m2 in array)
		{
			TextBlock text = new TextBlock
			{
				FontSize = 12.0,
				Foreground = Res("TextSec"),
				TextTrimming = TextTrimming.CharacterEllipsis
			};
			DockPanel row = new DockPanel
			{
				Margin = new Thickness(0.0, 0.0, 12.0, 0.0)
			};
			StackPanel head = new StackPanel
			{
				Orientation = System.Windows.Controls.Orientation.Horizontal,
				Margin = new Thickness(0.0, 0.0, 0.0, 3.0)
			};
			head.Children.Add(new System.Windows.Shapes.Rectangle
			{
				Width = 8.0,
				Height = 8.0,
				Fill = m2.Brush,
				VerticalAlignment = VerticalAlignment.Center
			});
			head.Children.Add(new TextBlock
			{
				Text = m2.Name,
				FontSize = 12.0,
				FontWeight = FontWeights.SemiBold,
				Margin = new Thickness(7.0, 0.0, 0.0, 0.0)
			});
			DockPanel.SetDock(head, Dock.Top);
			row.Children.Add(head);
			row.Children.Add(text);
			HistoryStats.Children.Add(row);
			historyStats.Add((m2, text));
		}
		static Metric M(string name, string unit, string fmt, double lo, double hi, string color, string info)
		{
			return new Metric(name, unit, fmt, lo, hi)
			{
				Brush = Hex(color),
				Info = info
			};
		}
	}

	private Border Tile(Metric m)
	{
		TextBlock current = new TextBlock
		{
			FontFamily = (System.Windows.Media.FontFamily)FindResource("DisplayFont"),
			FontSize = 24.0,
			FontWeight = FontWeights.SemiBold,
			Foreground = Res("Text"),
			VerticalAlignment = VerticalAlignment.Center
		};
		DockPanel head = new DockPanel();
		DockPanel.SetDock(current, Dock.Right);
		head.Children.Add(current);
		StackPanel name = new StackPanel
		{
			Orientation = System.Windows.Controls.Orientation.Horizontal,
			VerticalAlignment = VerticalAlignment.Center
		};
		name.Children.Add(new System.Windows.Shapes.Rectangle
		{
			Width = 9.0,
			Height = 9.0,
			Fill = m.Brush,
			VerticalAlignment = VerticalAlignment.Center
		});
		name.Children.Add(new InfoLabel
		{
			Text = m.Name,
			Info = m.Info,
			LabelStyle = TileLabel,
			Margin = new Thickness(9.0, 0.0, 0.0, 0.0)
		});
		head.Children.Add(name);
		MetricChart chart = new MetricChart
		{
			Metric = m,
			Height = 150.0,
			Margin = new Thickness(0.0, 12.0, 0.0, 10.0),
			Window = monitorWindow
		};
		UniformGrid stats = new UniformGrid
		{
			Columns = 3
		};
		TextBlock min = Value();
		TextBlock avg = Value();
		TextBlock max = Value();
		(string, TextBlock)[] array = new (string, TextBlock)[3]
		{
			("Min", min),
			("Ort", avg),
			("Maks", max)
		};
		for (int i = 0; i < array.Length; i++)
		{
			(string, TextBlock) tuple = array[i];
			string label = tuple.Item1;
			TextBlock box = tuple.Item2;
			StackPanel cell = new StackPanel();
			cell.Children.Add(new TextBlock
			{
				Text = label,
				Style = (Style)FindResource("T.Caption")
			});
			cell.Children.Add(box);
			stats.Children.Add(cell);
		}
		StackPanel body = new StackPanel();
		body.Children.Add(head);
		body.Children.Add(chart);
		body.Children.Add(new Border
		{
			Style = (Style)FindResource("Divider"),
			Margin = new Thickness(0.0, 0.0, 0.0, 10.0)
		});
		body.Children.Add(stats);
		TextBlock seg = new TextBlock
		{
			FontSize = 12.0,
			Foreground = Res("TextSec"),
			Margin = new Thickness(0.0, 10.0, 0.0, 0.0),
			TextWrapping = TextWrapping.Wrap
		};
		StackPanel segRow = new StackPanel();
		segRow.Children.Add(new TextBlock
		{
			Text = "Uyanıştan beri",
			Style = (Style)FindResource("T.Caption")
		});
		segRow.Children.Add(seg);
		body.Children.Add(new Border
		{
			Style = (Style)FindResource("Divider"),
			Margin = new Thickness(0.0, 10.0, 0.0, 0.0)
		});
		body.Children.Add(segRow);
		tiles.Add(new MonitorTile(m, chart, current, min, avg, max, seg));
		return new Border
		{
			Style = (Style)FindResource("Card"),
			Margin = new Thickness(8.0, 0.0, 8.0, 16.0),
			Padding = new Thickness(18.0, 16.0, 18.0, 14.0),
			Child = body
		};
		TextBlock Value()
		{
			return new TextBlock
			{
				FontSize = 13.0,
				FontWeight = FontWeights.SemiBold,
				Foreground = Res("Text"),
				Margin = new Thickness(0.0, 2.0, 0.0, 0.0)
			};
		}
	}

	private void PushMetrics(int? cpu, int? gpu, SystemStats sys)
	{
		DateTime now = DateTime.Now;
		mCpuTemp.Push(now, cpu);
		mGpuTemp.Push(now, gpu);
		mCpuLoad.Push(now, sys.CpuUsage);
		mGpuLoad.Push(now, gpuStats.Usage);
		mCpuClock.Push(now, sys.CpuClockGhz);
		mGpuClock.Push(now, (double?)gpuStats.ClockMhz / 1000.0);
		mGpuPower.Push(now, gpuStats.PowerWatts);
		mCpuFan.Push(now, latest?.CpuRpm);
		mGpuFan.Push(now, latest?.GpuRpm);
		mMem.Push(now, 100.0 * sys.MemUsedGb / Math.Max(1.0, sys.MemTotalGb));
		mBattery.Push(now, sys.BatteryWatts);
		mVram.Push(now, gpuStats.VramUsedGb);
		foreach (var historyStat in historyStats)
		{
			Metric m = historyStat.Metric;
			TextBlock item = historyStat.Text;
			MetricStats s = m.Stats(historyWindow, now);
			item.Text = ((s.Count == 0) ? Lang.T("veri yok") : Lang.F("min {0} · ort {1} · maks {2}", N(s.Min), N(s.Avg), m.Show(s.Max)));
			string N(double? v)
			{
				return v?.ToString(m.Format) ?? "—";
			}
		}
		if (Pages.SelectedIndex == 1)
		{
			UpdateMonitor();
		}
	}

	private void UpdateMonitor()
	{
		DateTime now = DateTime.Now;
		TimeSpan up = now - segStart;
		SessionText.Text = Lang.F("Uyanıştan beri {0}  ·  {1} örnek  ·  3 sn aralık", $"{(int)up.TotalHours:00}:{up.Minutes:00}:{up.Seconds:00}", mCpuTemp.Samples.Count);
		foreach (MonitorTile tile in tiles)
		{
			MonitorTile t = tile;
			MetricStats s = t.Metric.Stats(monitorWindow, now);
			t.Current.Text = t.Metric.Show(s.Current);
			t.Min.Text = t.Metric.Show(s.Min);
			t.Avg.Text = t.Metric.Show(s.Avg);
			t.Max.Text = t.Metric.Show(s.Max);
			RunningStats g = t.Metric.Segment;
			t.Seg.Text = ((g.Count == 0) ? "—" : Lang.F("min {0} · ort {1} · maks {2}", N(g.Min), N(g.Avg), t.Metric.Show(g.Max)));
			t.Chart.Window = monitorWindow;
			t.Chart.InvalidateVisual();
			string N(double? v)
			{
				return v?.ToString(t.Metric.Format) ?? "—";
			}
		}
	}

	private void MonitorRange(object sender, RoutedEventArgs e)
	{
		if (Range60 != null)
		{
			monitorWindow = TimeSpan.FromMinutes((sender == Range1) ? 1 : ((sender == Range15) ? 15 : ((sender == Range60) ? 60 : 5)));
			if (tiles.Count > 0)
			{
				UpdateMonitor();
			}
		}
	}

	private string? GpuBusyCheck()
	{
		int[] array = nvidia.ActiveProcesses();
		int? usage = nvidia.Read().Usage;
		SortedSet<string> names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
		if (array == null)
		{
			array = Array.Empty<int>();
		}
		int[] array2 = array;
		foreach (int pid in array2)
		{
			try
			{
				using Process p = Process.GetProcessById(pid);
				if (!GpuServiceProcesses.Contains(p.ProcessName))
				{
					names.Add(p.ProcessName + ".exe");
				}
			}
			catch
			{
			}
		}
		string list = string.Join(", ", names.Take(4)) + ((names.Count > 4) ? $" +{names.Count - 4}" : "");
		if (usage.HasValue)
		{
			int u = usage.GetValueOrDefault();
			if (u > 10)
			{
				if (names.Count <= 0)
				{
					return Lang.F("GPU kullanımı %{0}", u);
				}
				return Lang.F("GPU kullanımı %{0}: {1}", u, list);
			}
		}
		if (names.Count > 0)
		{
			((DispatcherObject)this).Dispatcher.BeginInvoke((Delegate)(Action)(() =>
			{
				Log(Lang.F("RTX boşta; bağlı uygulamalar dahili GPU'ya taşınıyor: {0}", list));
			}), Array.Empty<object>());
		}
		nvidia.Dispose();
		return null;
	}

	private static bool IsRunning(string name)
	{
		Process[] found = Process.GetProcessesByName(name);
		Process[] array = found;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Dispose();
		}
		return found.Length != 0;
	}

	public MainWindow(bool readOnly, bool startup)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected Obj, but got Unknown
		MainWindow mainWindow = this;
		this.readOnly = readOnly;
		InitializeComponent();
		Icon = Brand.WindowIcon();
		hardware = new AsusHardware(readOnly);
		thermal = new Thermal(readOnly);
		store = new Store(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TufControl"));
		controller = new Controller(hardware, (string message) =>
		{
			((DispatcherObject)mainWindow).Dispatcher.Invoke((Action)(() =>
			{
				mainWindow.Log(message);
			}));
		}, thermal)
		{
			GpuBusy = GpuBusyCheck
		};
		try
		{
			string path = System.IO.Path.Combine(store.DirectoryPath, "events.log");
			if (File.Exists(path))
			{
				logLines = File.ReadLines(path).TakeLast(200).ToArray();
				LogText.Text = string.Join(Environment.NewLine, logLines);
				LogText.ScrollToEnd();
			}
		}
		catch
		{
		}
		try
		{
			settings = store.Load();
		}
		catch (Exception ex)
		{
			SetStatus("Profil dosyası okunamadı; varsayılanlar yüklendi. " + ex.Message, "Warn");
			autoPaused = true;
		}
		History.Define(SeriesCpuTemp, SeriesGpuTemp, SeriesCpuLoad, SeriesGpuPower);
		BuildMonitor();
		InitHistory();
		CurvePreview.CpuBrush = Res("CpuBrush");
		CurvePreview.GpuBrush = Res("GpuBrush");
		bool onAcNow = SystemInformation.PowerStatus.PowerLineStatus != System.Windows.Forms.PowerLineStatus.Offline;
		pendingName = (readOnly ? null : (startup ? settings.StartupTarget(onAcNow, IsRunning) : (settings.AutoApply ? settings.Desired(onAcNow, IsRunning) : null)));
		selected = settings.AcProfile;
		FillPickers();
		LoadProfile();
		loading = false;
		CpuEditor.Changed += Dirty;
		GpuEditor.Changed += Dirty;
		System.Windows.Controls.CheckBox[] array = new System.Windows.Controls.CheckBox[6] { CustomFans, PowerLimitEnabled, TempLimitEnabled, BoostEnabled, GpuEnabled, GpuTempEnabled };
		foreach (System.Windows.Controls.CheckBox obj2 in array)
		{
			obj2.Checked += (object _, RoutedEventArgs _) =>
			{
				mainWindow.UpdatePowerChart();
				mainWindow.Dirty();
			};
			obj2.Unchecked += (object _, RoutedEventArgs _) =>
			{
				mainWindow.UpdatePowerChart();
				mainWindow.Dirty();
			};
		}
		System.Windows.Controls.RadioButton[] array2 = new System.Windows.Controls.RadioButton[3] { BaseSilent, BaseBalanced, BaseTurbo };
		for (int num = 0; num < array2.Length; num++)
		{
			array2[num].Checked += (object _, RoutedEventArgs _) =>
			{
				if (!mainWindow.loading)
				{
					mainWindow.ApplyAccent(mainWindow.BaseMode);
					mainWindow.Dirty();
				}
			};
		}
		array2 = new System.Windows.Controls.RadioButton[3] { GpuModeKeep, GpuModeStd, GpuModeEco };
		for (int num = 0; num < array2.Length; num++)
		{
			array2[num].Checked += (object o, RoutedEventArgs _) =>
			{
				if (!mainWindow.loading)
				{
					if (o == mainWindow.GpuModeEco)
					{
						mainWindow.GpuEnabled.IsChecked = false;
						mainWindow.GpuTempEnabled.IsChecked = false;
					}
					mainWindow.Dirty();
				}
			};
		}
		Slider[] array3 = new Slider[3] { PowerSlider, SlowSlider, FastSlider };
		for (int num = 0; num < array3.Length; num++)
		{
			array3[num].ValueChanged += (object o, RoutedPropertyChangedEventArgs<double> _) =>
			{
				mainWindow.OrderPower((Slider)o);
				mainWindow.UpdatePowerChart();
				mainWindow.Dirty();
			};
		}
		array3 = new Slider[3] { TempSlider, GpuBoostSlider, GpuTargetSlider };
		for (int num = 0; num < array3.Length; num++)
		{
			array3[num].ValueChanged += (object _, RoutedPropertyChangedEventArgs<double> _) =>
			{
				mainWindow.Dirty();
			};
		}
		(Slider, System.Windows.Controls.CheckBox)[] array4 = new (Slider, System.Windows.Controls.CheckBox)[6]
		{
			(PowerSlider, PowerLimitEnabled),
			(SlowSlider, BoostEnabled),
			(FastSlider, BoostEnabled),
			(GpuBoostSlider, GpuEnabled),
			(GpuTargetSlider, GpuTempEnabled),
			(TempSlider, TempLimitEnabled)
		};
		for (int num = 0; num < array4.Length; num++)
		{
			var (slider, box) = array4[num];
			slider.ValueChanged += (object _, RoutedPropertyChangedEventArgs<double> _) =>
			{
				if (!mainWindow.loading && !mainWindow.ordering && box.IsEnabled && box.IsChecked != true)
				{
					box.IsChecked = true;
				}
			};
		}
		ProfileName.TextChanged += (object _, TextChangedEventArgs _) =>
		{
			mainWindow.Dirty();
		};
		AcPicker.SelectionChanged += AutoSettingChanged;
		BatteryPicker.SelectionChanged += AutoSettingChanged;
		AutoApply.Checked += AutoSettingChanged;
		AutoApply.Unchecked += AutoSettingChanged;
		UpdatePowerChart();
		UpdateSummary();
		tray = new NotifyIcon
		{
			Text = "TUF Control",
			Icon = Brand.TrayIcon(),
			Visible = !readOnly
		};
		ContextMenuStrip menu = new ContextMenuStrip
		{
			Items = 
			{
				{
					"TUF Control'ü aç",
					(System.Drawing.Image?)null,
					(EventHandler?)((object? _, EventArgs _) =>
					{
						((DispatcherObject)mainWindow).Dispatcher.Invoke((Action)mainWindow.Reveal);
					})
				},
				{
					"Otomatik uygulamayı duraklat",
					(System.Drawing.Image?)null,
					(EventHandler?)((object? _, EventArgs _) =>
					{
						((DispatcherObject)mainWindow).Dispatcher.Invoke((Action)(() =>
						{
							mainWindow.autoPaused = true;
							mainWindow.nextAuto = DateTimeOffset.MaxValue;
							mainWindow.Log("Bu oturum için otomatik uygulama duraklatıldı.");
						}));
					})
				},
				(ToolStripItem)new ToolStripSeparator(),
				{
					"Çıkış",
					(System.Drawing.Image?)null,
					(EventHandler?)((object? _, EventArgs _) =>
					{
						((DispatcherObject)mainWindow).Dispatcher.Invoke((Action)mainWindow.Quit);
					})
				}
			}
		};
		tray.ContextMenuStrip = menu;
		tray.DoubleClick += (object? _, EventArgs _) =>
		{
			((DispatcherObject)mainWindow).Dispatcher.Invoke((Action)mainWindow.Reveal);
		};
		Loaded += async (object _, RoutedEventArgs _) =>
		{
			await mainWindow.RefreshAsync();
			mainWindow.timer.Start();
		};
		timer.Tick += async (object? _, EventArgs _) =>
		{
			if (mainWindow.ticking || mainWindow.quitting)
			{
				return;
			}
			mainWindow.ticking = true;
			try
			{
				await mainWindow.TickAsync();
			}
			finally
			{
				mainWindow.ticking = false;
			}
		};
		Closing += (object? _, CancelEventArgs e) =>
		{
			if (!mainWindow.quitting)
			{
				e.Cancel = true;
				if (readOnly)
				{
					mainWindow.Quit();
				}
				else
				{
					mainWindow.Hide();
					mainWindow.tray.ShowBalloonTip(1500, "TUF Control", "İzleme bildirim alanında sürüyor.", ToolTipIcon.Info);
				}
			}
		};
		SourceInitialized += (object? _, EventArgs _) =>
		{
			Brand.StyleFrame(new WindowInteropHelper(mainWindow).Handle);
		};
		StateChanged += (object? _, EventArgs _) =>
		{
			bool flag = mainWindow.WindowState == WindowState.Maximized;
			mainWindow.Root.Margin = new Thickness(flag ? 7 : 0);
			mainWindow.Repaint();
			mainWindow.MaxButton.Content = (flag ? "\ue923" : "\ue922");
		};
		SystemEvents.PowerModeChanged += PowerEvent;
		RefreshStartup();
		if (readOnly)
		{
			ApplyButton.IsEnabled = false;
			StartupToggle.IsEnabled = false;
			SetStatus("Salt okunur oturum: donanıma ayar gönderilmez.", "Warn");
		}
		if (startup && !readOnly && !autoPaused)
		{
			if (settings.StartupMode == "firmware")
			{
				autoProfile = settings.Desired(onAcNow, IsRunning);
			}
			else
			{
				startupApply = true;
				nextAuto = DateTimeOffset.Now.AddSeconds(3.0);
			}
			TryAdoptBootState();
		}
		SyncBootInstall();
		SelectPage(0);
		ApplyLanguage();
	}

	private void ApplyLanguage()
	{
		Lang.Set(settings.Language);
		LangTr.IsChecked = !Lang.English;
		LangEn.IsChecked = Lang.English;
		Lang.Apply((DependencyObject)(object)this);
		FillStartupChoices();
		RefreshHeader();
		UpdateSummary();
		if ((object)adviceReport != null)
		{
			RenderAdvice(adviceReport);
		}
		if (tiles.Count > 0)
		{
			UpdateMonitor();
		}
		int range = RangePicker.SelectedIndex;
		RangePicker.SelectedIndex = -1;
		RangePicker.SelectedIndex = range;
		FrameworkElement[] array = new FrameworkElement[5] { History, CurvePreview, PowerChart, CpuEditor, GpuEditor };
		for (int i = 0; i < array.Length; i++)
		{
			array[i].InvalidateVisual();
		}
		foreach (MonitorTile tile in tiles)
		{
			tile.Chart.InvalidateVisual();
		}
	}

	public void PreviewLanguage(string code)
	{
		settings.Language = code;
		ApplyLanguage();
	}

	private void LanguageClick(object sender, RoutedEventArgs e)
	{
		string code = ((LangEn.IsChecked == true) ? "en" : "tr");
		if (code == settings.Language)
		{
			return;
		}
		try
		{
			Settings candidate = Copy(settings);
			candidate.Language = code;
			if (!readOnly)
			{
				store.Save(candidate);
			}
			settings = candidate;
		}
		catch (Exception ex)
		{
			SetStatus(ex.Message, "Bad");
		}
		ApplyLanguage();
	}

	public void StartBackground()
	{
		timer.Start();
		RefreshAsync();
	}

	public void Reveal()
	{
		Show();
		if (WindowState == WindowState.Minimized)
		{
			WindowState = WindowState.Normal;
		}
		Activate();
		Topmost = true;
		Topmost = false;
		Repaint();
		RefreshAsync();
	}

	private void Repaint()
	{
		((DispatcherObject)this).Dispatcher.BeginInvoke((Delegate)(Action)(() =>
		{
			Root.InvalidateMeasure();
			Root.InvalidateVisual();
			UpdateLayout();
		}), (DispatcherPriority)7, Array.Empty<object>());
	}

	private void PowerEvent(object sender, PowerModeChangedEventArgs e)
	{
		if (e.Mode == PowerModes.Suspend)
		{
			try
			{
				((DispatcherObject)this).Dispatcher.Invoke((Action)(() =>
				{
					EndSegment("sleep");
				}));
			}
			catch
			{
			}
		}
		if (e.Mode != PowerModes.Resume)
		{
			return;
		}
		((DispatcherObject)this).Dispatcher.BeginInvoke((Delegate)(Action)(() =>
		{
			applied = null;
			autoProfile = null;
			controller.ForgetMode();
			StartSegment();
			if (settings.AutoApply && !readOnly && !autoPaused)
			{
				nextAuto = DateTimeOffset.Now.AddSeconds(15.0);
				retries = 0;
				Log("Uykudan dönüş: donanımın hazır olması bekleniyor.");
			}
		}), Array.Empty<object>());
	}

	private void Log(string message)
	{
		if (!readOnly)
		{
			store.Log(message);
		}
		logLines = logLines.Append($"{DateTime.Now:HH:mm:ss}  {message}").TakeLast(200).ToArray();
		LogText.Text = string.Join(Environment.NewLine, logLines);
		LogText.ScrollToEnd();
		StatusText.Text = message;
	}

	private void SetStatus(string message, string key)
	{
		StatusText.Text = Lang.T(message);
		StatusDot.Fill = Res(key);
	}

	private System.Windows.Media.Brush Res(string key)
	{
		return (System.Windows.Media.Brush)FindResource(key);
	}

	private static bool IsEditorPage(int index)
	{
		if ((uint)(index - 2) <= 1u)
		{
			return true;
		}
		return false;
	}

	public void SelectPage(int index)
	{
		if (Pages != null)
		{
			Pages.SelectedIndex = index;
			ProfileBar.Visibility = ((!IsEditorPage(index)) ? Visibility.Collapsed : Visibility.Visible);
			navigating = true;
			NavItems[index].IsChecked = true;
			navigating = false;
			if (IsEditorPage(index))
			{
				ApplyAccent(BaseMode);
			}
			if (index == 1)
			{
				UpdateMonitor();
			}
			if (index == 6)
			{
				RefreshStartup();
				RefreshHistoryInfo();
				RefreshBootInfo();
			}
			if (index == 4)
			{
				RefreshAdvice();
			}
			((DispatcherObject)this).Dispatcher.BeginInvoke((Delegate)(Action)(() =>
			{
				Lang.Apply((DependencyObject)(object)Pages);
			}), (DispatcherPriority)6, Array.Empty<object>());
		}
	}

	private void Navigate(object sender, RoutedEventArgs e)
	{
		if (!navigating && Pages != null)
		{
			object[] navItems = NavItems;
			SelectPage(Array.IndexOf(navItems, sender));
		}
	}

	private void ShowCooling(object sender, RoutedEventArgs e)
	{
		SelectPage(2);
	}

	private void RefreshHeader()
	{
		string active = applied?.Name ?? pendingName;
		HeaderProfiles.ItemsSource = settings.Profiles.Select((Profile p) =>
		{
			List<string> list = new List<string>();
			if (p.Name == settings.AcProfile)
			{
				list.Add(Lang.T("prizde"));
			}
			if (p.Name == settings.BatteryProfile)
			{
				list.Add(Lang.T("bataryada"));
			}
			list.AddRange(from r in settings.AppRules
				where r.Profile == p.Name
				select r.Process + Lang.T(".exe açıkken"));
			string hint = Lang.T("Tıkla ve uygula") + ((list.Count > 0) ? (Lang.T(" · otomatik: ") + string.Join(", ", list)) : "");
			string icon = ((p.GpuMode == 2) ? "\ue83f" : ((p.Mode == 1) ? "\ue945" : ((p.Mode == 2) ? "\ue708" : "\ue7e8")));
			return new HeaderItem(p.Name, icon, p.Name == active, hint);
		}).ToList();
	}

	private async void HeaderProfileClick(object sender, RoutedEventArgs e)
	{
		object dataContext = ((FrameworkElement)sender).DataContext;
		HeaderItem item = dataContext as HeaderItem;
		if ((object)item == null)
		{
			return;
		}
		if (readOnly)
		{
			SetStatus("Salt okunur oturum: " + item.Name + " uygulanmadı.", "Warn");
			RefreshHeader();
			return;
		}
		if (conflicts.Length != 0)
		{
			SetStatus("Armoury Crate / G-Helper açık; uygulanmadı. Ayrıntılar Günlük sayfasında.", "Warn");
			RefreshHeader();
			return;
		}
		Profile p = settings.Profiles.FirstOrDefault((Profile x) => x.Name == item.Name);
		if (p != null)
		{
			await ApplyAsync(p.Clone());
			RefreshHeader();
		}
	}

	private void HeaderNewProfile(object sender, RoutedEventArgs e)
	{
		SelectPage(2);
	}

	private void ApplyAccent(int mode)
	{
		System.Windows.Media.Color c = ModeColors[Math.Clamp(mode, 0, 2)];
		Resources["Accent"] = Ink.Freeze(new SolidColorBrush(c));
		Resources["AccentSoft"] = Ink.Freeze(new SolidColorBrush(System.Windows.Media.Color.FromArgb(34, c.R, c.G, c.B)));
		if (PowerChart != null)
		{
			PowerChart.Accent = (System.Windows.Media.Brush)Resources["Accent"];
			PowerChart.InvalidateVisual();
		}
	}

	private void Dirty()
	{
		if (!loading)
		{
			editDirty = true;
			SetStatus("Kaydedilmemiş değişiklikler var.", "Warn");
			UpdateSummary();
		}
	}

	private void OrderPower(Slider changed)
	{
		if (loading || ordering)
		{
			return;
		}
		ordering = true;
		if (changed == PowerSlider)
		{
			if (SlowSlider.Value < PowerSlider.Value)
			{
				SlowSlider.Value = PowerSlider.Value;
			}
			if (FastSlider.Value < SlowSlider.Value)
			{
				FastSlider.Value = SlowSlider.Value;
			}
		}
		else if (changed == SlowSlider)
		{
			if (PowerSlider.Value > SlowSlider.Value)
			{
				PowerSlider.Value = SlowSlider.Value;
			}
			if (FastSlider.Value < SlowSlider.Value)
			{
				FastSlider.Value = SlowSlider.Value;
			}
		}
		else
		{
			if (SlowSlider.Value > FastSlider.Value)
			{
				SlowSlider.Value = FastSlider.Value;
			}
			if (PowerSlider.Value > SlowSlider.Value)
			{
				PowerSlider.Value = SlowSlider.Value;
			}
		}
		ordering = false;
	}

	private void UpdatePowerChart()
	{
		if (PowerChart != null)
		{
			PowerChart.Spl = PowerSlider.Value;
			PowerChart.Slow = SlowSlider.Value;
			PowerChart.Fast = FastSlider.Value;
			PowerChart.SplOn = PowerLimitEnabled.IsChecked == true;
			PowerChart.BoostOn = BoostEnabled.IsChecked == true;
			PowerChart.InvalidateVisual();
		}
	}

	private void UpdateSummary()
	{
		if (SumCpuTarget != null)
		{
			SumCpuTarget.Text = ((TempLimitEnabled.IsChecked == true) ? $"{TempSlider.Value:0} °C" : "Firmware");
			SumGpuTarget.Text = ((GpuTempEnabled.IsChecked == true) ? $"{GpuTargetSlider.Value:0} °C" : "87 °C");
			bool custom = CustomFans.IsChecked == true;
			TextBlock sumFanMode = SumFanMode;
			string text;
			if (custom)
			{
				text = (CpuEditor.Points.All((FanPoint p) => p.Percent == 100) ? "Tam hız" : "Özel");
			}
			else
			{
				text = "Otomatik";
			}
			sumFanMode.Text = text;
			TextBlock sumProfile = SumProfile;
			object[] array = new object[1];
			string n = ProfileName.Text.Trim();
			array[0] = ((n != null && n.Length > 0) ? n : selected);
			sumProfile.Text = Lang.F("Fan eğrisi · {0}", array);
			CurvePreview.Cpu = CpuEditor.Points;
			CurvePreview.Gpu = GpuEditor.Points;
			CurvePreview.Custom = custom;
			CurvePreview.InvalidateVisual();
		}
	}

	private void FillPickers()
	{
		bool was = loading;
		loading = true;
		string[] names = settings.Profiles.Select((Profile p) => p.Name).ToArray();
		ProfilePicker.ItemsSource = names;
		ProfilePicker.SelectedItem = selected;
		AcPicker.ItemsSource = names;
		BatteryPicker.ItemsSource = names;
		AcPicker.SelectedItem = settings.AcProfile;
		BatteryPicker.SelectedItem = settings.BatteryProfile;
		AutoApply.IsChecked = settings.AutoApply;
		ExternalStd.IsChecked = settings.ExternalStandard;
		RecordHistoryBox.IsChecked = settings.RecordHistory;
		BootEarlyBox.IsChecked = settings.BootEarly;
		FillStartupChoices();
		rules.Clear();
		foreach (AppRule r in settings.AppRules)
		{
			rules.Add(r.Clone());
		}
		RulesList.ItemsSource = null;
		RulesList.ItemsSource = rules;
		RefreshHeader();
		loading = was;
	}

	private void RefreshRunningApps(object? sender, EventArgs e)
	{
		SortedSet<string> names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
		int session;
		using (Process self = Process.GetCurrentProcess())
		{
			session = self.SessionId;
		}
		Process[] processes = Process.GetProcesses();
		foreach (Process p in processes)
		{
			try
			{
				if (p.MainWindowHandle != IntPtr.Zero && p.SessionId == session && !p.ProcessName.Equals("TufControl", StringComparison.OrdinalIgnoreCase))
				{
					names.Add(p.ProcessName);
				}
			}
			catch
			{
			}
			finally
			{
				p.Dispose();
			}
		}
		RunningApps.ItemsSource = names.ToArray();
	}

	private void AddRuleFor(string process)
	{
		process = System.IO.Path.GetFileNameWithoutExtension(process.Trim());
		if (process.Length == 0)
		{
			SetStatus("Önce bir uygulama seç.", "Warn");
			return;
		}
		if (rules.Any((AppRule r) => r.Process.Equals(process, StringComparison.OrdinalIgnoreCase)))
		{
			SetStatus(process + " için zaten bir kural var.", "Warn");
			return;
		}
		if (rules.Count >= 20)
		{
			SetStatus("En fazla 20 uygulama kuralı olabilir.", "Warn");
			return;
		}
		rules.Add(new AppRule
		{
			Process = process,
			Profile = selected
		});
		SaveAutoSettings();
		SetStatus(process + " kuralı eklendi ve kaydedildi. Yanındaki listeden profilini seçebilirsin.", "Good");
	}

	private void AddRule(object sender, RoutedEventArgs e)
	{
		AddRuleFor((RunningApps.SelectedItem as string) ?? "");
	}

	private void BrowseRule(object sender, RoutedEventArgs e)
	{
		Microsoft.Win32.OpenFileDialog d = new Microsoft.Win32.OpenFileDialog
		{
			Filter = "Uygulama|*.exe",
			Title = "Kural için uygulama seç"
		};
		if (d.ShowDialog() == true)
		{
			AddRuleFor(d.FileName);
		}
	}

	private void RemoveRule(object sender, RoutedEventArgs e)
	{
		if (((FrameworkElement)sender).DataContext is AppRule r)
		{
			rules.Remove(r);
			SaveAutoSettings();
		}
	}

	private void LoadProfile()
	{
		bool was = loading;
		loading = true;
		Profile p = settings.Profiles.First((Profile profile) => profile.Name == selected);
		ProfileName.Text = p.Name;
		System.Windows.Controls.RadioButton toggleButton;
		if (p.Mode == 1)
		{
			toggleButton = BaseTurbo;
		}
		else
		{
			toggleButton = ((p.Mode == 2) ? BaseSilent : BaseBalanced);
		}
		toggleButton.IsChecked = true;
		CustomFans.IsChecked = p.CustomFans;
		CpuEditor.Points = p.Cpu;
		GpuEditor.Points = p.Gpu;
		PowerLimitEnabled.IsChecked = p.LimitPower;
		PowerSlider.Value = p.CpuWatts;
		BoostEnabled.IsChecked = p.LimitBoost;
		SlowSlider.Value = p.CpuSlowWatts;
		FastSlider.Value = p.CpuFastWatts;
		GpuEnabled.IsChecked = p.TuneGpu;
		GpuBoostSlider.Value = p.GpuBoostWatts;
		GpuTempEnabled.IsChecked = p.LimitGpuTemp;
		GpuTargetSlider.Value = p.GpuTempTarget;
		System.Windows.Controls.RadioButton toggleButton2;
		if (p.GpuMode == 1)
		{
			toggleButton2 = GpuModeStd;
		}
		else
		{
			toggleButton2 = ((p.GpuMode == 2) ? GpuModeEco : GpuModeKeep);
		}
		toggleButton2.IsChecked = true;
		TempLimitEnabled.IsChecked = p.LimitTemperature;
		TempSlider.Value = p.CpuTarget;
		loading = was;
		editDirty = false;
		UpdatePowerChart();
		UpdateSummary();
		if (Pages != null && IsEditorPage(Pages.SelectedIndex))
		{
			ApplyAccent(p.Mode);
		}
	}

	private Profile ReadEditor()
	{
		return new Profile
		{
			Name = ProfileName.Text.Trim(),
			Mode = BaseMode,
			CustomFans = (CustomFans.IsChecked == true),
			Cpu = CpuEditor.Points,
			Gpu = GpuEditor.Points,
			LimitPower = (PowerLimitEnabled.IsChecked == true),
			CpuWatts = (int)PowerSlider.Value,
			LimitBoost = (BoostEnabled.IsChecked == true),
			CpuSlowWatts = (int)SlowSlider.Value,
			CpuFastWatts = (int)FastSlider.Value,
			TuneGpu = (GpuEnabled.IsChecked == true),
			GpuBoostWatts = (int)GpuBoostSlider.Value,
			LimitGpuTemp = (GpuTempEnabled.IsChecked == true),
			GpuTempTarget = (int)GpuTargetSlider.Value,
			GpuMode = ((GpuModeStd.IsChecked == true) ? 1 : ((GpuModeEco.IsChecked == true) ? 2 : 0)),
			LimitTemperature = (TempLimitEnabled.IsChecked == true),
			CpuTarget = (int)TempSlider.Value
		};
	}

	private Profile SaveProfile()
	{
		if (readOnly)
		{
			throw new InvalidOperationException("Salt okunur oturumda kayıt değiştirilemez.");
		}
		Profile p = ReadEditor();
		p.Validate();
		Settings candidate = Copy(settings);
		int index = candidate.Profiles.FindIndex((Profile x) => x.Name == selected);
		candidate.Profiles[index] = p;
		if (candidate.AcProfile == selected)
		{
			candidate.AcProfile = p.Name;
		}
		if (candidate.BatteryProfile == selected)
		{
			candidate.BatteryProfile = p.Name;
		}
		foreach (AppRule item in candidate.AppRules.Where((AppRule r) => r.Profile == selected))
		{
			item.Profile = p.Name;
		}
		if (candidate.StartupProfile == selected)
		{
			candidate.StartupProfile = p.Name;
		}
		if (candidate.LastProfile == selected)
		{
			candidate.LastProfile = p.Name;
		}
		candidate.Validate();
		store.Save(candidate);
		settings = candidate;
		selected = p.Name;
		FillPickers();
		editDirty = false;
		Log(p.Name + " kaydedildi.");
		SetStatus(Lang.F("{0} kaydedildi.", p.Name), "Good");
		return p;
	}

	private static Settings Copy(Settings s)
	{
		return JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(s));
	}

	private void SaveClick(object sender, RoutedEventArgs e)
	{
		try
		{
			SaveProfile();
		}
		catch (Exception ex)
		{
			SetStatus(ex.Message, "Bad");
		}
	}

	private async void ApplyClick(object sender, RoutedEventArgs e)
	{
		if (conflicts.Length != 0)
		{
			SetStatus("Armoury Crate / G-Helper açık; uygulanmadı. Ayrıntılar Günlük sayfasında.", "Warn");
			return;
		}
		try
		{
			Profile p = SaveProfile();
			await ApplyAsync(p);
		}
		catch (Exception ex)
		{
			SetStatus(ex.Message, "Bad");
		}
	}

	private void RevertEdits(object sender, RoutedEventArgs e)
	{
		LoadProfile();
		SetStatus("Değişiklikler geri alındı.", "TextMuted");
	}

	private void ProfileChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!loading && ProfilePicker.SelectedItem is string name && !(name == selected))
		{
			bool discarded = editDirty;
			selected = name;
			LoadProfile();
			SetStatus(discarded ? (name + " yüklendi; kaydedilmemiş değişiklikler atıldı.") : Lang.F("{0} yüklendi.", name), "TextMuted");
		}
	}

	private void NewProfile(object sender, RoutedEventArgs e)
	{
		if (readOnly)
		{
			return;
		}
		try
		{
			Profile p = ReadEditor();
			p.Validate();
			if (settings.Profiles.Count >= 20)
			{
				throw new IOException("En fazla 20 profil olabilir.");
			}
			int n = 1;
			string name;
			do
			{
				name = "Profil " + n++;
			}
			while (settings.Profiles.Any((Profile x) => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
			p.Name = name;
			Settings candidate = Copy(settings);
			candidate.Profiles.Add(p);
			store.Save(candidate);
			settings = candidate;
			selected = name;
			FillPickers();
			LoadProfile();
			SetStatus(name + " oluşturuldu. Adını değiştirip kaydedebilirsin.", "Good");
		}
		catch (Exception ex)
		{
			SetStatus(ex.Message, "Bad");
		}
	}

	private void DeleteProfile(object sender, RoutedEventArgs e)
	{
		if (readOnly)
		{
			return;
		}
		if (settings.Profiles.Count <= 1)
		{
			SetStatus("En az bir profil kalmalı.", "Warn");
			return;
		}
		Settings candidate = Copy(settings);
		string gone = selected;
		candidate.Profiles.RemoveAll((Profile p) => p.Name == gone);
		string fallback = candidate.Profiles[0].Name;
		if (candidate.AcProfile == gone)
		{
			candidate.AcProfile = fallback;
		}
		if (candidate.BatteryProfile == gone)
		{
			candidate.BatteryProfile = fallback;
		}
		candidate.AppRules.RemoveAll((AppRule r) => r.Profile == gone);
		if (candidate.StartupProfile == gone)
		{
			candidate.StartupProfile = "";
			if (candidate.StartupMode == "profile")
			{
				candidate.StartupMode = "auto";
			}
		}
		if (candidate.LastProfile == gone)
		{
			candidate.LastProfile = "";
		}
		try
		{
			store.Save(candidate);
			settings = candidate;
			selected = fallback;
			FillPickers();
			LoadProfile();
			Log(gone + " silindi; önceki dosya .bak olarak saklandı.");
		}
		catch (Exception ex)
		{
			SetStatus(ex.Message, "Bad");
		}
	}

	private void CurveTab(object sender, RoutedEventArgs e)
	{
		if (GpuEditor != null)
		{
			CpuEditor.Visibility = ((CurveCpu.IsChecked != true) ? Visibility.Collapsed : Visibility.Visible);
			GpuEditor.Visibility = ((CurveGpu.IsChecked != true) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private void SetCurves(FanPoint[] curve)
	{
		CpuEditor.Points = curve;
		GpuEditor.Points = curve;
		CustomFans.IsChecked = true;
		Dirty();
	}

	private void SilentCurve(object sender, RoutedEventArgs e)
	{
		SetCurves(SilentPreset);
	}

	private void BalancedCurve(object sender, RoutedEventArgs e)
	{
		SetCurves(Profile.DefaultCurve());
	}

	private void PerformanceCurve(object sender, RoutedEventArgs e)
	{
		SetCurves(PerformancePreset);
	}

	private void FullCurve(object sender, RoutedEventArgs e)
	{
		SetCurves((from p in Profile.DefaultCurve()
			select p with
			{
				Percent = 100
			}).ToArray());
	}

	private void ShowCapabilities(Capabilities cap)
	{
		Gate(CustomFans, FanNote, cap.CpuCurve && cap.GpuCurve, "Kapalıyken firmware temel modun eğrisini kullanır.");
		Gate(PowerLimitEnabled, null, cap.Power, "");
		Gate(BoostEnabled, BoostNote, cap.Boost, "sPPT ≤ 54 W · fPPT ≤ 65 W");
		Gate(GpuEnabled, GpuNote, cap.Gpu, "CPU'dan GPU'ya aktarılan ek güç · 5–20 W");
		Gate(GpuTempEnabled, GpuTempNote, cap.Gpu, "65–87 °C · 75 altı deneysel");
		GpuModeHost.IsEnabled = cap.Eco;
		if (!cap.Eco)
		{
			GpuModeNote.Text = "Firmware Eco modunu bildirmiyor (ya da MUX yalnızca RTX modunda).";
			GpuModeNote.Foreground = Res("Warn");
		}
		Gate(TempLimitEnabled, TempNote, cap.Thermal, "60–90 °C · düşük hedef daha serin, daha yavaş");
		void Gate(System.Windows.Controls.CheckBox box, TextBlock? note, bool ok, string normal)
		{
			box.IsEnabled = ok;
			if (!ok && box.IsChecked == true)
			{
				bool was = loading;
				loading = true;
				box.IsChecked = false;
				loading = was;
			}
			if (note != null)
			{
				note.Text = (ok ? normal : "Firmware bu ayarı desteklediğini bildirmiyor");
				note.Foreground = Res(ok ? "TextMuted" : "Warn");
			}
		}
	}

	private void RememberLast(string name)
	{
		if (settings.LastProfile == name || !settings.Profiles.Any((Profile x) => x.Name == name))
		{
			return;
		}
		try
		{
			Settings c = Copy(settings);
			c.LastProfile = name;
			store.Save(c);
			settings = c;
		}
		catch
		{
		}
	}

	private static bool SameSettings(Profile? a, Profile b)
	{
		if (a != null)
		{
			return JsonSerializer.Serialize(a) == JsonSerializer.Serialize(b);
		}
		return false;
	}

	private async Task<bool> ApplyAsync(Profile p, bool waitForGate = false, bool auto = false, bool deferThermal = false)
	{
		if (readOnly)
		{
			SetStatus("Salt okunur oturum: işlem yapılmadı.", "Warn");
			return false;
		}
		Profile original = p;
		p = p.ForDisplays(externalDisplay, settings.ExternalStandard);
		bool promoted = p.GpuMode != original.GpuMode;
		if (!waitForGate && SameSettings(applied, p))
		{
			SetStatus(Lang.F("{0} zaten uygulanmış; değişiklik yok.", p.Name), "Good");
			return true;
		}
		if (waitForGate)
		{
			await gate.WaitAsync();
		}
		else if (!(await gate.WaitAsync(0)))
		{
			SetStatus("Önceki işlem tamamlanıyor; biraz sonra yeniden dene.", "Warn");
			return false;
		}
		ApplyButton.IsEnabled = false;
		SetStatus(Lang.F("{0} uygulanıyor…", p.Name), "TextMuted");
		try
		{
			bool ecoDeferred = false;
			ApplyResult result = await Task.Run(() =>
			{
				conflicts = Machine.Conflicts();
				ApplyResult applyResult = controller.Apply(p, Machine.ModelAllowed, conflicts.Length != 0, deferThermal);
				if (!applyResult.Success && applyResult.GpuBusy && p.GpuMode == 2)
				{
					Profile profile = p.Clone();
					profile.GpuMode = 0;
					ApplyResult applyResult2 = controller.Apply(profile, Machine.ModelAllowed, conflicts.Length != 0, deferThermal);
					if (applyResult2.Success)
					{
						ecoDeferred = true;
						return new ApplyResult(Success: true, "RTX meşgul; diğer ayarlar uygulandı, Eco birazdan yeniden denenecek.");
					}
					return applyResult2;
				}
				return applyResult;
			});
			if (result.Success)
			{
				if (ecoDeferred)
				{
					Profile rest = p.Clone();
					rest.GpuMode = 0;
					ecoPending = true;
					ecoTries = 0;
					ecoRetryAt = DateTimeOffset.Now.AddSeconds(45.0);
					Log(Lang.T(result.Message));
					p = rest;
				}
				else
				{
					ecoPending = false;
				}
				pendingName = null;
				RememberLast(original.Name);
				applied = p.Clone();
				requested = original.Clone();
				if (result.ThermalPending)
				{
					applied.LimitTemperature = false;
					thermalPending = true;
					thermalTries = 0;
					thermalRetryAt = DateTimeOffset.Now.AddSeconds(4.0);
					Log(Lang.T("CPU sıcaklık hedefi için sürücü henüz hazır değil; diğer ayarlar uygulandı, hedef birazdan yeniden denenecek."));
				}
				else
				{
					thermalPending = false;
				}
				autoPaused = false;
				retries = 0;
				nextAuto = DateTimeOffset.MaxValue;
				lastThermalCheck = DateTimeOffset.Now;
				if (promoted)
				{
					Log(Lang.T("Harici ekran bağlı: HDMI için RTX açık (Standart) tutuluyor."));
				}
				AppliedLabel.Text = Lang.F("Uygulandı: {0} · {1}", p.Name, DateTime.Now.ToString("HH:mm"));
				ApplyAccent(p.Mode);
				RefreshHeader();
				CpuLimit.Text = (p.LimitPower ? $"{p.CpuWatts} W" : "Firmware");
			}
			else
			{
				applied = null;
				if (result.Message.Contains("Geri dönüş de başarısız"))
				{
					autoPaused = true;
					nextAuto = DateTimeOffset.MaxValue;
				}
				else if (auto)
				{
					DateTimeOffset at = (nextAuto = StartupRetry.Next(++retries, DateTimeOffset.Now) ?? DateTimeOffset.Now.AddMinutes(5.0));
					Log(Lang.F("Uygulanamadı; {0} sn sonra yeniden denenecek.", (int)Math.Max(1.0, (at - DateTimeOffset.Now).TotalSeconds)));
				}
			}
			MainWindow mainWindow = this;
			string text;
			if (result.Success)
			{
				text = (ecoDeferred ? result.Message : Lang.F("{0} uygulandı.", p.Name));
			}
			else
			{
				text = result.Message;
			}
			MainWindow mainWindow2 = mainWindow;
			string message = text;
			string key;
			if (result.Success)
			{
				key = (ecoDeferred ? "Warn" : "Good");
			}
			else
			{
				key = "Bad";
			}
			mainWindow2.SetStatus(message, key);
			return result.Success;
		}
		catch (Exception ex)
		{
			autoPaused = true;
			nextAuto = DateTimeOffset.MaxValue;
			applied = null;
			Log(ex.Message);
			SetStatus(ex.Message, "Bad");
			return false;
		}
		finally
		{
			ApplyButton.IsEnabled = !readOnly;
			gate.Release();
		}
	}

	private async Task TickAsync()
	{
		await RefreshAsync();
		bool ext = await Task.Run((Func<bool>)SystemSensor.ExternalDisplayConnected);
		if (ext != externalDisplay)
		{
			externalDisplay = ext;
			Log(Lang.T(ext ? "Harici ekran bağlandı." : "Harici ekran çıkarıldı."));
			if (!readOnly && !quitting && settings.ExternalStandard)
			{
				Profile? profile = requested;
				if (profile != null && profile.GpuMode == 2 && conflicts.Length == 0)
				{
					await ApplyAsync(requested.Clone());
				}
			}
		}
		if (readOnly || autoPaused || quitting)
		{
			return;
		}
		if (ecoPending && DateTimeOffset.Now >= ecoRetryAt && requested != null && conflicts.Length == 0)
		{
			ecoRetryAt = DateTimeOffset.Now.AddSeconds(45.0);
			if (++ecoTries > 8)
			{
				ecoPending = false;
				Log(Lang.T("RTX hâlâ meşgul; Eco'ya geçilemedi. Profili elle uygulayabilirsin."));
			}
			else if (await ApplyAsync(requested.Clone(), waitForGate: false, auto: true, deferThermal: true) && !ecoPending)
			{
				Log(Lang.T("Eco uygulandı."));
			}
		}
		if (thermalPending && DateTimeOffset.Now >= thermalRetryAt && requested != null && conflicts.Length == 0)
		{
			thermalRetryAt = DateTimeOffset.Now.AddSeconds(5.0);
			if (++thermalTries > 12)
			{
				thermalPending = false;
				Log(Lang.T("CPU sıcaklık hedefi uygulanamadı; sürücü hazır olmadı. Profili elle uygulayabilirsin."));
			}
			else if (await ApplyAsync(requested.Clone(), waitForGate: false, auto: true, deferThermal: true) && !thermalPending)
			{
				Log(Lang.T("CPU sıcaklık hedefi uygulandı."));
			}
		}
		bool onAc;
		string situation;
		string text;
		if (settings.AutoApply || startupApply)
		{
			bool? flag = latest?.OnAc;
			if (flag.HasValue)
			{
				onAc = flag == true;
				Capabilities? capabilities = this.capabilities;
				if ((object)capabilities != null && capabilities.Mode)
				{
					situation = settings.Desired(onAc, IsRunning);
					if (startupApply)
					{
						string st = settings.StartupTarget(onAc, IsRunning);
						if (st != null)
						{
							text = st;
							goto IL_0571;
						}
					}
					text = situation;
					goto IL_0571;
				}
			}
			if (DateTimeOffset.Now >= nextAuto)
			{
				nextAuto = DateTimeOffset.MaxValue;
				DateTimeOffset? dateTimeOffset = StartupRetry.Next(++retries, DateTimeOffset.Now);
				if (dateTimeOffset.HasValue)
				{
					DateTimeOffset retryAt = dateTimeOffset.GetValueOrDefault();
					nextAuto = retryAt;
					Log("Açılış: donanım henüz hazır değil, sınırlı yeniden deneme planlandı.");
				}
				else
				{
					autoPaused = true;
					Log("Donanım hazır olmadı; bu oturum için otomatik uygulama durdu.");
				}
			}
			return;
		}
		goto IL_0711;
		IL_0711:
		if (!(applied?.LimitTemperature ?? false) || !(DateTimeOffset.Now - lastThermalCheck > TimeSpan.FromSeconds(30L)) || !(await gate.WaitAsync(0)))
		{
			return;
		}
		lastThermalCheck = DateTimeOffset.Now;
		try
		{
			await Task.Run(() =>
			{
				if (Machine.Conflicts().Length != 0)
				{
					throw new IOException("Başka bir kontrol uygulaması açıldı; sıcaklık takibi durduruldu.");
				}
				if (thermal.Read() != applied.CpuTarget)
				{
					thermal.Set(applied.CpuTarget);
					if (thermal.Read() != applied.CpuTarget)
					{
						throw new IOException("Sıcaklık limiti tekrar doğrulanamadı.");
					}
					((DispatcherObject)this).Dispatcher.Invoke((Action)(() =>
					{
						Log("Sıcaklık limiti sıfırlanmıştı; yeniden uygulanıp doğrulandı.");
					}));
				}
			});
			return;
		}
		catch (Exception ex)
		{
			autoPaused = true;
			Log(ex.Message + " Otomatik işlem durdu.");
			return;
		}
		finally
		{
			gate.Release();
		}
		IL_0571:
		string desired = text;
		bool due = DateTimeOffset.Now >= nextAuto;
		if (situation != autoProfile)
		{
			if (++autoStable >= 2)
			{
				due = true;
			}
		}
		else
		{
			autoStable = 0;
		}
		if (due)
		{
			nextAuto = DateTimeOffset.MaxValue;
			autoStable = 0;
			autoProfile = situation;
			AppRule rule = settings.AppRules.FirstOrDefault((AppRule r) => r.Profile == desired && IsRunning(r.Process));
			string why = ((rule != null) ? Lang.F("{0} açık", rule.Process) : Lang.T(onAc ? "prize takılı" : "bataryada"));
			if (await ApplyAsync(settings.Profiles.First((Profile p) => p.Name == desired).Clone(), waitForGate: false, auto: true, deferThermal: true))
			{
				startupApply = false;
				if (!ecoPending)
				{
					SetStatus(Lang.F("Otomatik: {0} uygulandı ({1}).", desired, why), "Good");
				}
			}
		}
		goto IL_0711;
	}

	public async Task RefreshAsync(bool wait = false)
	{
		if (wait)
		{
			await gate.WaitAsync();
		}
		else if (!(await gate.WaitAsync(0)))
		{
			return;
		}
		try
		{
			(Snapshot, string[], Capabilities, SystemStats) data = await Task.Run(() => (Machine.Sample(hardware), Machine.Conflicts(), controller.Probe(), system.Read()));
			latest = data.Item1;
			conflicts = data.Item2;
			capabilities = data.Item3;
			SystemStats sys = (lastSys = data.Item4);
			bool eco = latest.Eco == 1;
			bool mayWake = latest.OnAc == true || latest.GpuTemp.HasValue;
			if (eco || !mayWake)
			{
				gpuStats = new GpuStats(null, null, null, null, null, null);
				if (eco)
				{
					await Task.Run((Action)nvidia.Dispose);
				}
			}
			else if (IsVisible && DateTimeOffset.Now >= nextGpu)
			{
				nextGpu = DateTimeOffset.Now.AddSeconds(3.0);
				gpuStats = await Task.Run((Func<GpuStats>)nvidia.Read);
			}
			TextBlock gpuCaption = GpuCaption;
			string text;
			if (eco)
			{
				text = "Eco · RTX kapalı";
			}
			else
			{
				text = ((!mayWake) ? "RTX uykuda" : "NVIDIA GeForce RTX 4050");
			}
			gpuCaption.Text = text;
			GpuCaption.Foreground = Res(eco ? "Good" : "TextMuted");
			int? cpu = latest.CpuTemp;
			int? gpu = latest.GpuTemp ?? gpuStats.Temperature;
			CpuValue.Text = cpu?.ToString() ?? "—";
			ShowTemp(CpuTempBar, cpu);
			TextBlock cpuUsage = CpuUsage;
			int? cpuUsage2 = sys.CpuUsage;
			cpuUsage.Text = ((!cpuUsage2.HasValue) ? "—" : (cpuUsage2.GetValueOrDefault() + " %"));
			TextBlock cpuClock = CpuClock;
			double? cpuClockGhz = sys.CpuClockGhz;
			string text2;
			if (cpuClockGhz.HasValue)
			{
				double g = cpuClockGhz.GetValueOrDefault();
				text2 = $"{g:0.0} GHz";
			}
			else
			{
				text2 = "—";
			}
			cpuClock.Text = text2;
			GpuValue.Text = gpu?.ToString() ?? "—";
			ShowTemp(GpuTempBar, gpu);
			TextBlock gpuUsage = GpuUsage;
			cpuUsage2 = gpuStats.Usage;
			gpuUsage.Text = ((!cpuUsage2.HasValue) ? "—" : (cpuUsage2.GetValueOrDefault() + " %"));
			TextBlock gpuPower = GpuPower;
			cpuClockGhz = gpuStats.PowerWatts;
			string text3;
			if (cpuClockGhz.HasValue)
			{
				double gp = cpuClockGhz.GetValueOrDefault();
				text3 = $"{gp:0} W";
			}
			else
			{
				text3 = "—";
			}
			gpuPower.Text = text3;
			GpuPowerLive.Text = GpuPower.Text;
			TextBlock gpuClock = GpuClock;
			cpuUsage2 = gpuStats.ClockMhz;
			string text4;
			if (cpuUsage2.HasValue)
			{
				int gc = cpuUsage2.GetValueOrDefault();
				text4 = $"{(double)gc / 1000.0:0.00} GHz";
			}
			else
			{
				text4 = "—";
			}
			gpuClock.Text = text4;
			GpuClockLive.Text = GpuClock.Text;
			cpuClockGhz = gpuStats.VramUsedGb;
			if (cpuClockGhz.HasValue)
			{
				double vu = cpuClockGhz.GetValueOrDefault();
				cpuClockGhz = gpuStats.VramTotalGb;
				if (cpuClockGhz.HasValue)
				{
					double vt = cpuClockGhz.GetValueOrDefault();
					VramValue.Text = $"{vu:0.0} / {vt:0.0} GB · {100.0 * vu / vt:0} %";
					VramBar.Value = vu / vt;
				}
			}
			CpuFan.Text = Rpm(latest.CpuRpm);
			GpuFan.Text = Rpm(latest.GpuRpm);
			CpuFanBar.Value = (double)latest.CpuRpm.GetValueOrDefault() / 6500.0;
			GpuFanBar.Value = (double)latest.GpuRpm.GetValueOrDefault() / 6500.0;
			LiveCpu.Text = ((!cpu.HasValue) ? "—" : (cpu.GetValueOrDefault() + " °C")) + " · " + Rpm(latest.CpuRpm);
			LiveGpu.Text = ((!gpu.HasValue) ? "—" : (gpu.GetValueOrDefault() + " °C")) + " · " + Rpm(latest.GpuRpm);
			CpuEditor.Current = ((double?)cpu) ?? double.NaN;
			GpuEditor.Current = ((double?)gpu) ?? double.NaN;
			BatValue.Text = latest.Battery?.ToString() ?? "—";
			BatBar.Value = (double)latest.Battery.GetValueOrDefault() / 100.0;
			MeterBar batBar = BatBar;
			MainWindow mainWindow = this;
			cpuUsage2 = latest.Battery;
			string key;
			if (!cpuUsage2.HasValue || cpuUsage2.GetValueOrDefault() >= 20)
			{
				cpuUsage2 = latest.Battery;
				key = ((cpuUsage2.HasValue && cpuUsage2.GetValueOrDefault() < 40) ? "Warn" : "Good");
			}
			else
			{
				key = "Bad";
			}
			batBar.Fill = mainWindow.Res(key);
			TextBlock adapterValue = AdapterValue;
			string text5;
			if (latest.OnAc ?? false)
			{
				text5 = "Takılı";
			}
			else
			{
				text5 = (((!latest.OnAc) ?? false) ? "Çıkarılmış" : "—");
			}
			adapterValue.Text = text5;
			AdapterValue.Foreground = Res((latest.OnAc ?? false) ? "Good" : "Text");
			TextBlock rateValue = RateValue;
			cpuClockGhz = sys.BatteryWatts;
			string text6;
			if (cpuClockGhz.HasValue)
			{
				double bw = cpuClockGhz.GetValueOrDefault();
				text6 = $"{bw:+0.0;-0.0;0} W";
			}
			else
			{
				text6 = "—";
			}
			rateValue.Text = text6;
			TextBlock rateValue2 = RateValue;
			MainWindow mainWindow2 = this;
			cpuClockGhz = sys.BatteryWatts;
			string key2;
			if (!cpuClockGhz.HasValue || !(cpuClockGhz.GetValueOrDefault() < 0.0))
			{
				cpuClockGhz = sys.BatteryWatts;
				key2 = ((cpuClockGhz.HasValue && cpuClockGhz.GetValueOrDefault() > 0.0) ? "Good" : "Text");
			}
			else
			{
				key2 = "Warn";
			}
			rateValue2.Foreground = mainWindow2.Res(key2);
			TextBlock remainValue = RemainValue;
			TimeSpan? batteryTime = sys.BatteryTime;
			object text7;
			if (!batteryTime.HasValue)
			{
				text7 = ((latest.OnAc ?? false) ? "Şarjda" : "—");
			}
			else
			{
				TimeSpan rt = batteryTime.GetValueOrDefault();
				text7 = Lang.F("{0} sa {1} dk", (int)rt.TotalHours, rt.Minutes);
			}
			remainValue.Text = (string)text7;
			MemValue.Text = $"{sys.MemUsedGb:0.0} GB / {sys.MemTotalGb:0} GB";
			MemPercent.Text = $"{100.0 * sys.MemUsedGb / Math.Max(1.0, sys.MemTotalGb):0} %";
			MemBar.Value = sys.MemUsedGb / Math.Max(1.0, sys.MemTotalGb);
			DiskValue.Text = $"{sys.DiskUsedGb:0} GB / {sys.DiskTotalGb:0} GB";
			DiskPercent.Text = $"{100.0 * sys.DiskUsedGb / Math.Max(1.0, sys.DiskTotalGb):0} %";
			DiskBar.Value = sys.DiskUsedGb / Math.Max(1.0, sys.DiskTotalGb);
			DeviceLine.Text = $"Ryzen 7 170  •  RTX 4050  •  {Math.Round(sys.MemTotalGb):0} GB RAM";
			History.Push(cpu, gpu, sys.CpuUsage, gpuStats.PowerWatts);
			PushMetrics(cpu, gpu, sys);
			RecordSample(DateTime.Now, sys);
			UpdateStatusList(cpu, gpu);
			ShowCapabilities(capabilities);
			ConflictText.Text = ((conflicts.Length != 0) ? (Lang.T("Şu bileşenler açık olduğu için ayar uygulanamaz:") + "\n• " + string.Join("\n• ", conflicts)) : Lang.T("Armoury Crate, ASUSOptimization ve G-Helper kapalı. Ayarlar uygulanabilir."));
			TextBlock capabilityText = CapabilityText;
			string[] array = new string[5];
			InlineArray8<string> buffer = default;
			buffer[0] = Cap("Performans modu", capabilities.Mode);
			buffer[1] = Cap("Fan eğrileri", capabilities.CpuCurve && capabilities.GpuCurve);
			buffer[2] = Cap("CPU SPL", capabilities.Power);
			buffer[3] = Cap("CPU sPPT / fPPT", capabilities.Boost);
			buffer[4] = Cap("GPU boost / sıcaklık hedefi", capabilities.Gpu);
			buffer[5] = Cap("GPU Eco modu", capabilities.Eco);
			buffer[6] = Cap("CPU sıcaklık hedefi (PawnIO)", capabilities.Thermal);
			buffer[7] = Cap("Yönetici yetkisi", StartupTask.IsAdmin);
			array[0] = string.Join("\n", (ReadOnlySpan<string?>)buffer);
			array[1] = "\n\n";
			array[2] = Machine.Model;
			array[3] = " · ";
			array[4] = Machine.Processor;
			capabilityText.Text = string.Concat(array);
			Lang.Apply((DependencyObject)(object)Root);
			if (previousAc.HasValue && latest.OnAc.HasValue && previousAc != latest.OnAc && settings.AutoApply && !readOnly && !autoPaused)
			{
				nextAuto = DateTimeOffset.Now.AddSeconds(8.0);
				retries = 0;
			}
			previousAc = latest.OnAc;
		}
		catch (Exception ex)
		{
			latest = null;
			capabilities = null;
			CpuValue.Text = "—";
			GpuValue.Text = "—";
			SetStatus("İzleme: " + ex.Message, "Bad");
		}
		finally
		{
			gate.Release();
			timer.Interval = TimeSpan.FromSeconds(IsVisible ? 3 : 10);
		}
		static string Cap(string label, bool ok)
		{
			return Lang.T(label) + ": " + Yes(ok);
		}
	}

	private static string Yes(bool b)
	{
		return Lang.T(b ? "var" : "yok");
	}

	private static string Rpm(int? v)
	{
		if (v.HasValue)
		{
			int r = v.GetValueOrDefault();
			return $"{r:N0} rpm";
		}
		return "—";
	}

	private void ShowTemp(MeterBar bar, int? t)
	{
		bar.Value = (double)t.GetValueOrDefault() / 100.0;
		string key;
		if (t.HasValue)
		{
			int valueOrDefault = t.GetValueOrDefault();
			if (valueOrDefault < 60)
			{
				key = "Good";
			}
			else
			{
				key = ((valueOrDefault >= 80) ? "Bad" : "Warn");
			}
		}
		else
		{
			key = "TextMuted";
		}
		bar.Fill = Res(key);
	}

	private void UpdateStatusList(int? cpu, int? gpu)
	{
		int hot = Math.Max(cpu.GetValueOrDefault(), gpu.GetValueOrDefault());
		double rpm = (double)((latest?.CpuRpm).GetValueOrDefault() + (latest?.GpuRpm).GetValueOrDefault()) / 2.0;
		StatusList.ItemsSource = new StatusRow[4]
		{
			Row("Sıcaklık", (hot == 0) ? "—" : ((hot < 70) ? "İyi" : ((hot < 85) ? "Yüksek" : "Kritik")), (hot == 0) ? "TextMuted" : ((hot < 70) ? "Good" : ((hot < 85) ? "Warn" : "Bad"))),
			Row("Gürültü seviyesi", (rpm < 2500.0) ? "Sessiz" : ((rpm < 4500.0) ? "Orta" : "Yüksek"), (rpm < 2500.0) ? "Good" : ((rpm < 4500.0) ? "Warn" : "Bad")),
			Row("Kontrol çakışması", (conflicts.Length == 0) ? "Yok" : "Var", (conflicts.Length == 0) ? "Good" : "Warn"),
			Row("Donanım koruması", "Etkin", "Good")
		};
		StatusRow Row(string name, string value, string key)
		{
			return new StatusRow(Lang.T(name), Lang.T(value), Res(key));
		}
	}

	private void MinimizeClick(object sender, RoutedEventArgs e)
	{
		WindowState = WindowState.Minimized;
	}

	private void MaximizeClick(object sender, RoutedEventArgs e)
	{
		WindowState = ((WindowState != WindowState.Maximized) ? WindowState.Maximized : WindowState.Normal);
	}

	private void CloseClick(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void RangeChanged(object sender, SelectionChangedEventArgs e)
	{
		if (History != null)
		{
			SeriesChart seriesChart = History;
			SeriesChart seriesChart2 = History;
			(int, string) tuple = RangePicker.SelectedIndex switch
			{
				0 => (40, "2 dk"), 
				2 => (200, "10 dk"), 
				_ => (120, "6 dk"), 
			};
			seriesChart.Window = tuple.Item1;
			seriesChart2.Span = tuple.Item2;
			historyWindow = TimeSpan.FromMinutes(RangePicker.SelectedIndex switch
			{
				0 => 2, 
				2 => 10, 
				_ => 6, 
			});
		}
	}

	private void RefreshStartup()
	{
		string status = StartupTask.Status();
		StartupState.Text = Lang.T(status);
		StartupToggle.IsChecked = status.StartsWith("Oturum açılışında etkin");
	}

	private void StartupToggled(object sender, RoutedEventArgs e)
	{
		if (readOnly)
		{
			RefreshStartup();
			return;
		}
		try
		{
			if (StartupToggle.IsChecked == true)
			{
				StartupTask.Enable();
				Log("TUF Control Windows ile başlayacak.");
			}
			else
			{
				StartupTask.Disable();
				Log("Windows ile başlatma kapatıldı.");
			}
		}
		catch (Exception ex)
		{
			SetStatus(ex.Message, "Bad");
		}
		RefreshStartup();
	}

	private void AutoSettingChanged(object sender, RoutedEventArgs e)
	{
		if (!loading)
		{
			SaveAutoSettings();
		}
	}

	private void FillStartupChoices()
	{
		bool was = loading;
		loading = true;
		List<StartupChoice> items = new List<StartupChoice>
		{
			new StartupChoice("firmware", "", Lang.T("Bilgisayarın kendi ayarı (hiçbir şey uygulanmaz)")),
			new StartupChoice("auto", "", Lang.T("Priz / batarya profilim (otomatik)")),
			new StartupChoice("last", "", Lang.T("En son uyguladığım profil"))
		};
		items.AddRange(settings.Profiles.Select((Profile p) => new StartupChoice("profile", p.Name, Lang.F("Sabit profil: {0}", p.Name))));
		StartupChoiceBox.ItemsSource = items;
		StartupChoiceBox.SelectedItem = items.FirstOrDefault((StartupChoice i) => i.Mode == settings.StartupMode && (i.Mode != "profile" || i.Profile == settings.StartupProfile)) ?? items[1];
		loading = was;
	}

	private void StartupChoiceChanged(object sender, SelectionChangedEventArgs e)
	{
		if (loading || !(StartupChoiceBox.SelectedItem is StartupChoice c))
		{
			return;
		}
		if (readOnly)
		{
			FillStartupChoices();
			SetStatus("Salt okunur oturum: ayar kaydedilmez.", "Warn");
			return;
		}
		try
		{
			Settings candidate = Copy(settings);
			candidate.StartupMode = c.Mode;
			candidate.StartupProfile = c.Profile;
			candidate.Validate();
			store.Save(candidate);
			settings = candidate;
			SetStatus("Kaydedildi.", "Good");
		}
		catch (Exception ex)
		{
			SetStatus(ex.Message, "Bad");
		}
	}

	private async void ExternalStdClick(object sender, RoutedEventArgs e)
	{
		if (readOnly)
		{
			ExternalStd.IsChecked = settings.ExternalStandard;
			RecordHistoryBox.IsChecked = settings.RecordHistory;
			BootEarlyBox.IsChecked = settings.BootEarly;
			SetStatus("Salt okunur oturum: ayar kaydedilmez.", "Warn");
			return;
		}
		try
		{
			Settings candidate = Copy(settings);
			candidate.ExternalStandard = ExternalStd.IsChecked == true;
			store.Save(candidate);
			settings = candidate;
			SetStatus("Kaydedildi.", "Good");
			Profile? profile = requested;
			if (profile != null && profile.GpuMode == 2 && externalDisplay && conflicts.Length == 0)
			{
				await ApplyAsync(requested.Clone());
			}
		}
		catch (Exception ex)
		{
			SetStatus(ex.Message, "Bad");
		}
	}

	private void SaveAutoSettings()
	{
		if (readOnly)
		{
			SetStatus("Salt okunur oturum: ayar kaydedilmez.", "Warn");
			return;
		}
		try
		{
			Settings candidate = Copy(settings);
			candidate.AcProfile = AcPicker.SelectedItem?.ToString() ?? candidate.AcProfile;
			candidate.BatteryProfile = BatteryPicker.SelectedItem?.ToString() ?? candidate.BatteryProfile;
			candidate.AutoApply = AutoApply.IsChecked == true;
			candidate.AppRules = (from r in rules
				where r.Profile.Length > 0
				select r.Clone()).ToList();
			candidate.Validate();
			store.Save(candidate);
			settings = candidate;
			autoProfile = null;
			autoStable = 0;
			if (settings.AutoApply)
			{
				autoPaused = false;
				retries = 0;
			}
			RefreshHeader();
			SetStatus(settings.AutoApply ? Lang.F("Kaydedildi · prizde {0}, bataryada {1}.", settings.AcProfile, settings.BatteryProfile) : "Kaydedildi · otomatik geçiş kapalı.", "Good");
		}
		catch (Exception ex)
		{
			SetStatus(ex.Message, "Bad");
		}
	}

	private async void RefreshClick(object sender, RoutedEventArgs e)
	{
		await RefreshAsync();
	}

	private void Elevate(object sender, RoutedEventArgs e)
	{
		if (readOnly)
		{
			return;
		}
		if (StartupTask.IsAdmin)
		{
			SetStatus("Uygulama zaten yönetici yetkisiyle çalışıyor.", "Good");
			return;
		}
		try
		{
			Process.Start(new ProcessStartInfo(Environment.ProcessPath, "--wait-instance")
			{
				UseShellExecute = true,
				Verb = "runas"
			});
			Quit();
		}
		catch (Exception ex)
		{
			SetStatus("Yönetici oturumu açılmadı: " + ex.Message, "Bad");
		}
	}

	private void ExportProfiles(object sender, RoutedEventArgs e)
	{
		Microsoft.Win32.SaveFileDialog d = new Microsoft.Win32.SaveFileDialog
		{
			FileName = "TufControl-profiller.json",
			Filter = "JSON|*.json"
		};
		if (d.ShowDialog() != true)
		{
			return;
		}
		try
		{
			File.WriteAllText(d.FileName, JsonSerializer.Serialize(settings, new JsonSerializerOptions
			{
				WriteIndented = true
			}));
			Log("Profiller dışa aktarıldı.");
		}
		catch (Exception ex)
		{
			SetStatus(ex.Message, "Bad");
		}
	}

	private void ImportProfiles(object sender, RoutedEventArgs e)
	{
		if (readOnly)
		{
			return;
		}
		Microsoft.Win32.OpenFileDialog d = new Microsoft.Win32.OpenFileDialog
		{
			Filter = "JSON|*.json"
		};
		if (d.ShowDialog() != true)
		{
			return;
		}
		try
		{
			if (new FileInfo(d.FileName).Length > 131072)
			{
				throw new IOException("Profil dosyası çok büyük.");
			}
			Settings imported = JsonSerializer.Deserialize<Settings>(File.ReadAllText(d.FileName)) ?? throw new IOException("Boş dosya.");
			imported.Validate();
			imported.AutoApply = false;
			store.Save(imported);
			settings = imported;
			selected = settings.AcProfile;
			autoPaused = true;
			nextAuto = DateTimeOffset.MaxValue;
			FillPickers();
			LoadProfile();
			Log("Profiller alındı. Otomatik uygulama kapalı.");
		}
		catch (Exception ex)
		{
			SetStatus(ex.Message, "Bad");
		}
	}

	private void ExportDiagnostics(object sender, RoutedEventArgs e)
	{
		Microsoft.Win32.SaveFileDialog d = new Microsoft.Win32.SaveFileDialog
		{
			FileName = "TufControl-tani.json",
			Filter = "JSON|*.json"
		};
		if (d.ShowDialog() != true)
		{
			return;
		}
		try
		{
			File.WriteAllText(d.FileName, JsonSerializer.Serialize(new
			{
				Model = Machine.Model,
				Capabilities = capabilities,
				Snapshot = latest,
				Gpu = gpuStats,
				Conflicts = conflicts,
				Startup = StartupTask.Status(),
				Events = logLines
			}, new JsonSerializerOptions
			{
				WriteIndented = true
			}));
			Log("Tanı raporu dışa aktarıldı.");
		}
		catch (Exception ex)
		{
			SetStatus(ex.Message, "Bad");
		}
	}

	public async void Quit()
	{
		quitting = true;
		autoPaused = true;
		timer.Stop();
		EndSegment("exit");
		SystemEvents.PowerModeChanged -= PowerEvent;
		await gate.WaitAsync();
		tray.Visible = false;
		tray.Dispose();
		hardware.Dispose();
		thermal.Dispose();
		nvidia.Dispose();
		system.Dispose();
		gate.Release();
		System.Windows.Application.Current.Shutdown();
	}

	public void EmergencyPause()
	{
		autoPaused = true;
		nextAuto = DateTimeOffset.MaxValue;
		timer.Stop();
		SetStatus("Beklenmeyen hata nedeniyle otomatik işlemler durduruldu.", "Bad");
	}
}
