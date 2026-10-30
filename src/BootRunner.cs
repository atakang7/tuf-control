using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace TufControl;

public static class BootRunner
{
	public static int Run(string[] args)
	{
		int at = Array.IndexOf(args, "--settings");
		if (at < 0 || at + 1 >= args.Length)
		{
			return 2;
		}
		string dir = Path.GetDirectoryName(Path.GetFullPath(args[at + 1]));
		Store store = new Store(dir);
		try
		{
			Log("Görev başladı (" + Up() + ").");
			Settings settings = store.Load();
			if (!settings.BootEarly)
			{
				Log("Erken başlatma ayarlardan kapatılmış; hiçbir şey yapılmadı.");
				return 0;
			}
			if (!Machine.ModelAllowed)
			{
				Log("Bu model için donanım yazma kapalı.");
				return 2;
			}
			using AsusHardware hardware = new AsusHardware();
			using Thermal thermal = new Thermal();
			Controller controller = new Controller(hardware, (string _) =>
			{
			}, thermal);
			for (int attempt = 0; attempt <= StartupRetry.Delays.Length; attempt++)
			{
				if (Machine.Conflicts().Length != 0)
				{
					Log("Armoury Crate / G-Helper etkin; hiçbir şey uygulanmadı.");
					return 3;
				}
				Snapshot sample = Machine.Sample(hardware);
				string target = settings.StartupTarget(sample.OnAc ?? true, (string _) => false);
				if (target == null)
				{
					Log("Açılış ayarı 'bilgisayarın kendi ayarı': firmware varsayılanı bırakıldı.");
					return 0;
				}
				Profile profile = settings.Profiles.First((Profile p) => p.Name == target).Clone();
				profile.GpuMode = 0;
				if (attempt == 0 && profile.CustomFans)
				{
					try
					{
						hardware.Write(1114148u, Profile.Encode(profile.Cpu));
						hardware.Write(1114149u, Profile.Encode(profile.Gpu));
						Log("Fan eğrisi hemen yazıldı (" + Up() + ").");
					}
					catch
					{
					}
				}
				ApplyResult result = controller.Apply(profile, modelAllowed: true, conflict: false, deferThermal: true);
				if (result.Success)
				{
					Save(dir, profile, controller, result.ThermalPending);
					Log($"{profile.Name} uygulandı: {attempt + 1}. denemede, {Up()} ({(result.ThermalPending ? "CPU sıcaklık hedefi sürücü hazır olunca" : "tamam")}).");
					for (int i = 0; i < 12; i++)
					{
						if (!result.ThermalPending)
						{
							break;
						}
						Thread.Sleep(5000);
						result = controller.Apply(profile, modelAllowed: true, conflict: false, deferThermal: true);
						if (!result.Success)
						{
							break;
						}
						Save(dir, profile, controller, result.ThermalPending);
					}
					if (result.Success && !result.ThermalPending)
					{
						Log(profile.Name + ": CPU sıcaklık hedefi de uygulandı (" + Up() + ").");
					}
					return 0;
				}
				if (attempt < StartupRetry.Delays.Length)
				{
					Thread.Sleep(StartupRetry.Delays[attempt] * 1000);
				}
			}
			Log("Profil uygulanamadı; uygulama oturum açılınca yeniden deneyecek.");
			return 1;
		}
		catch (Exception ex)
		{
			Log("Hata: " + ex.Message);
			return 1;
		}
		void Log(string message)
		{
			store.Log("[erken başlatma] " + message);
		}
		static string Up()
		{
			return $"açılıştan {(double)Environment.TickCount64 / 1000.0:0.0} sn sonra";
		}
	}

	private static void Save(string dir, Profile profile, Controller controller, bool thermalPending)
	{
		BootHandoff.Write(dir, new BootState(DateTime.UtcNow, profile.Name, profile.Mode, thermalPending, controller.ThermalBaseline, controller.ThermalActive));
	}
}
