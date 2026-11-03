using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using System.ServiceProcess;

namespace TufControl;

public static class StartupTask
{
	private const string Name = "TufControl.Logon";

	private const string BootName = "TufControl.Boot";

	public static bool IsAdmin => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

	public static string InstallDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "TufControl");

	private static string InstalledExe => Path.Combine(InstallDir, "TufControl.exe");

	public static string Status()
	{
		try
		{
			dynamic s = Connect();
			dynamic task = s.GetFolder("\\").GetTask("TufControl.Logon");
			return (task.Enabled) ? "Oturum açılışında etkin" : "Başlangıç görevi devre dışı";
		}
		catch
		{
			return "Başlangıç görevi yok";
		}
	}

	private static dynamic Connect()
	{
		dynamic service = Activator.CreateInstance(Type.GetTypeFromProgID("Schedule.Service"));
		service.Connect();
		return service;
	}

	public static void Enable()
	{
		if (!IsAdmin)
		{
			throw new InvalidOperationException("Başlangıç görevini oluşturmak için uygulamayı yönetici olarak açın.");
		}
		string exe = Environment.ProcessPath ?? throw new IOException("Uygulama yolu bulunamadı.");
		if (!Path.GetFileName(exe).Equals("TufControl.exe", StringComparison.OrdinalIgnoreCase))
		{
			throw new IOException("Yayınlanmış TufControl.exe üzerinden çalıştırın.");
		}
		dynamic s = Connect();
		dynamic task = s.NewTask(0);
		string user = WindowsIdentity.GetCurrent().Name;
		task.RegistrationInfo.Description = "TUF Control: kullanıcı oturumu açıldıktan 5 saniye sonra profil kontrolü.";
		task.Principal.UserId = user;
		task.Principal.LogonType = 3;
		task.Principal.RunLevel = 1;
		task.Settings.Enabled = true;
		task.Settings.StartWhenAvailable = true;
		task.Settings.DisallowStartIfOnBatteries = false;
		task.Settings.StopIfGoingOnBatteries = false;
		task.Settings.ExecutionTimeLimit = "PT0S";
		task.Settings.MultipleInstances = 2;
		task.Settings.Priority = 4;
		dynamic trigger = task.Triggers.Create(9);
		trigger.UserId = user;
		trigger.Delay = "PT5S";
		dynamic action = task.Actions.Create(0);
		action.Path = exe;
		action.Arguments = "--startup";
		action.WorkingDirectory = Path.GetDirectoryName(exe);
		s.GetFolder("\\").RegisterTaskDefinition("TufControl.Logon", task, 6, user, null, 3, null);
	}

	public static string BootStatus()
	{
		try
		{
			using ServiceController sc = new ServiceController("TufControlBoot");
			_ = sc.Status;
			return "etkin";
		}
		catch
		{
			return "yok";
		}
	}

	private static string Hash(string file)
	{
		using FileStream fs = File.OpenRead(file);
		return Convert.ToHexString(SHA256.HashData(fs));
	}

	public static bool InstallOutdated()
	{
		try
		{
			string src = Path.Combine(AppContext.BaseDirectory, "TufControl.dll");
			string dst = Path.Combine(InstallDir, "TufControl.dll");
			return !File.Exists(dst) || Hash(src) != Hash(dst);
		}
		catch
		{
			return true;
		}
	}

	public static void SyncInstall()
	{
		if (!IsAdmin)
		{
			throw new InvalidOperationException("Korumalı klasöre kopyalamak için uygulamayı yönetici olarak açın.");
		}
		string src = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
		if (string.Equals(Path.GetFullPath(src), Path.GetFullPath(InstallDir), StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		Directory.CreateDirectory(InstallDir);
		string[] directories = Directory.GetDirectories(src, "*", SearchOption.AllDirectories);
		foreach (string dir in directories)
		{
			Directory.CreateDirectory(Path.Combine(InstallDir, Path.GetRelativePath(src, dir)));
		}
		directories = Directory.GetFiles(src, "*", SearchOption.AllDirectories);
		foreach (string file in directories)
		{
			if (!file.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase))
			{
				File.Copy(file, Path.Combine(InstallDir, Path.GetRelativePath(src, file)), overwrite: true);
			}
		}
	}

	private static int Sc(string arguments)
	{
		using Process p = Process.Start(new ProcessStartInfo("sc.exe", arguments)
		{
			CreateNoWindow = true,
			UseShellExecute = false
		}) ?? throw new IOException("sc.exe başlatılamadı.");
		if (!p.WaitForExit(10000))
		{
			p.Kill(entireProcessTree: true);
			throw new System.TimeoutException("sc.exe 10 saniye içinde tamamlanmadı.");
		}
		return p.ExitCode;
	}

	private static bool ServiceExists()
	{
		return Sc("query TufControlBoot") == 0;
	}

	public static void EnableBoot(string settingsPath)
	{
		if (!IsAdmin)
		{
			throw new InvalidOperationException("Erken başlatma servisini kurmak için uygulamayı yönetici olarak açın.");
		}
		SyncInstall();
		try
		{
			dynamic s = Connect();
			s.GetFolder("\\").DeleteTask("TufControl.Boot", 0);
		}
		catch
		{
		}
		string bin = $"\\\"{InstalledExe}\\\" --service --settings \\\"{settingsPath}\\\"";
		if (ServiceExists())
		{
			Sc("stop TufControlBoot");
		}
		int rc = (ServiceExists() ? Sc($"config {"TufControlBoot"} binPath= \"{bin}\" start= auto obj= LocalSystem DisplayName= \"TUF Control (erken başlatma)\"") : Sc($"create {"TufControlBoot"} binPath= \"{bin}\" start= auto obj= LocalSystem DisplayName= \"TUF Control (erken başlatma)\""));
		if (rc != 0)
		{
			throw new InvalidOperationException($"Erken başlatma servisi kurulamadı (sc.exe kod {rc}).");
		}
		Sc("description TufControlBoot \"Windows açılırken seçili TUF Control açılış profilini uygular, sonra kendiliğinden durur.\"");
	}

	public static void DisableBoot()
	{
		if (!IsAdmin)
		{
			throw new InvalidOperationException("Erken başlatma servisini kaldırmak için uygulamayı yönetici olarak açın.");
		}
		try
		{
			Sc("stop TufControlBoot");
		}
		catch
		{
		}
		try
		{
			Sc("delete TufControlBoot");
		}
		catch
		{
		}
		try
		{
			dynamic s = Connect();
			s.GetFolder("\\").DeleteTask("TufControl.Boot", 0);
		}
		catch
		{
		}
		try
		{
			if (Directory.Exists(InstallDir))
			{
				Directory.Delete(InstallDir, recursive: true);
			}
		}
		catch
		{
		}
	}

	public static void Disable()
	{
		dynamic s = Connect();
		s.GetFolder("\\").DeleteTask("TufControl.Logon", 0);
	}
}
