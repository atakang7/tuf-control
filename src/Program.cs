using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace TufControl;

public static class Program
{
	[STAThread]
	public static int Main(string[] args)
	{
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Expected Obj, but got Unknown
		string blocker = Platform.Blocker();
		if (blocker != null)
		{
			if (!args.Contains("--boot") && !args.Contains("--service") && !args.Contains("--verify") && !args.Contains("--diagnose"))
			{
				MessageBox.Show(blocker, "TUF Control");
			}
			return 4;
		}
		if (args.Contains("--boot"))
		{
			return BootRunner.Run(args);
		}
		if (args.Contains("--service"))
		{
			return BootService.Run(args);
		}
		bool readOnly = args.Contains("--read-only") || args.Contains("--diagnose") || args.Contains("--render");
		if (args.Contains("--diagnose"))
		{
			using (AsusHardware hw = new AsusHardware(readOnly: true))
			{
				using Thermal thermal = new Thermal(readOnly: true);
				var report = new
				{
					Time = DateTimeOffset.Now,
					Version = typeof(Program).Assembly.GetName().Version?.ToString(),
					Platform = Platform.Report(),
					Model = Machine.Model,
					Processor = Machine.Processor,
					Capabilities = new Controller(hw, (string _) =>
					{
					}, thermal).Probe(),
					Snapshot = Machine.Sample(hw),
					Conflicts = Machine.Conflicts(),
					ExternalDisplay = SystemSensor.ExternalDisplayConnected(),
					ReadOnly = true,
					ThermalDriverLoaded = false
				};
				int index = Array.IndexOf(args, "--diagnose");
				File.WriteAllText((index + 1 < args.Length) ? Path.GetFullPath(args[index + 1]) : Path.Combine(AppContext.BaseDirectory, "diagnostics.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions
				{
					WriteIndented = true
				}));
				return 0;
			}
		}
		if (args.Contains("--verify"))
		{
			return Verify(args);
		}
		bool created;
		using Mutex mutex = new Mutex(initiallyOwned: true, "Local\\TufControl.UI" + (readOnly ? ".ReadOnly" : ""), out created);
		if (!created && args.Contains("--wait-instance"))
		{
			try
			{
				created = mutex.WaitOne(8000);
			}
			catch (AbandonedMutexException)
			{
				created = true;
			}
		}
		try
		{
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
		}
		catch
		{
		}
		EventWaitHandle showSignal = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, "Local\\TufControl.Show" + (readOnly ? ".ReadOnly" : ""));
		try
		{
			if (!created)
			{
				showSignal.Set();
				return 0;
			}
			RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
			Application app = new Application
			{
				ShutdownMode = ShutdownMode.OnExplicitShutdown
			};
			MainWindow window = new MainWindow(readOnly, args.Contains("--startup"));
			app.MainWindow = window;
			Thread thread = new Thread(() =>
			{
				while (showSignal.WaitOne())
				{
					((DispatcherObject)window).Dispatcher.BeginInvoke((Delegate)new Action(window.Reveal), Array.Empty<object>());
				}
			});
			thread.IsBackground = true;
			thread.Name = "TufControl show";
			thread.Start();
			app.DispatcherUnhandledException += (object _, DispatcherUnhandledExceptionEventArgs e) =>
			{
				window.EmergencyPause();
				MessageBox.Show(e.Exception.Message, "TUF Control — işlem durduruldu");
				e.Handled = true;
			};
			if (args.Contains("--render"))
			{
				int index2 = Array.IndexOf(args, "--render");
				string dir = Path.GetFullPath(args[index2 + 1]);
				Directory.CreateDirectory(dir);
				window.ShowActivated = false;
				window.ShowInTaskbar = false;
				window.WindowStartupLocation = WindowStartupLocation.Manual;
				window.Left = -20000.0;
				app.Startup += async (object _, StartupEventArgs _) =>
				{
					window.Show();
					int li = Array.IndexOf(args, "--lang");
					if (li >= 0 && li + 1 < args.Length)
					{
						window.PreviewLanguage(args[li + 1]);
					}
					for (int k = 0; k < 10; k++)
					{
						await window.RefreshAsync(wait: true);
						await Task.Delay(600);
					}
					for (int k = -1; k < 7; k++)
					{
						if (k >= 0)
						{
							window.SelectPage(k);
						}
						if (k == 4)
						{
							await Task.Delay(2500);
						}
						window.UpdateLayout();
						await ((DispatcherObject)window).Dispatcher.InvokeAsync((Action)(() =>
						{
						}), (DispatcherPriority)7);
						FrameworkElement content = (FrameworkElement)window.Content;
						RenderTargetBitmap bitmap = new RenderTargetBitmap((int)content.ActualWidth, (int)content.ActualHeight, 96.0, 96.0, PixelFormats.Pbgra32);
						bitmap.Render(content);
						PngBitmapEncoder encoder = new PngBitmapEncoder
						{
							Frames = { BitmapFrame.Create(bitmap) }
						};
						using FileStream fs = File.Create(Path.Combine(dir, (k < 0) ? "screen-start.png" : $"screen-{k}.png"));
						encoder.Save(fs);
					}
					window.Quit();
				};
			}
			else
			{
				app.Startup += (object _, StartupEventArgs _) =>
				{
					if (!args.Contains("--startup"))
					{
						window.Show();
					}
					else
					{
						window.StartBackground();
					}
				};
			}
			return app.Run();
		}
		finally
		{
			if (showSignal != null)
			{
				((IDisposable)showSignal).Dispose();
			}
		}
	}

	// --verify [profile]: read back the live firmware state and compare it to the saved profile.
	// Writes verify.json next to profiles.json; exit code 0 = everything readable matches, 1 = mismatch.
	private static int Verify(string[] args)
	{
		Store store = new Store(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TufControl"));
		Settings settings = store.Load();
		int at = Array.IndexOf(args, "--verify");
		string name = (at + 1 < args.Length && !args[at + 1].StartsWith("--")) ? args[at + 1] : settings.LastProfile;
		Profile profile = settings.Profiles.FirstOrDefault((Profile p) => p.Name == name) ?? settings.Profiles.First();
		using AsusHardware hw = new AsusHardware(readOnly: true);
		VerifyItem[] items = new Controller(hw, (string _) =>
		{
		}, null, new WindowsPower()).Verify(profile);
		bool ok = items.All((VerifyItem v) => v.Match != false);
		var report = new
		{
			Time = DateTimeOffset.Now,
			Profile = profile.Name,
			Ok = ok,
			Conflicts = Machine.Conflicts(),
			Items = items
		};
		File.WriteAllText(Path.Combine(store.DirectoryPath, "verify.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions
		{
			WriteIndented = true
		}));
		return ok ? 0 : 1;
	}
}
