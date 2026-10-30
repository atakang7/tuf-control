using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace TufControl;

// Platform rules. Keep in sync with the "Compatibility" section of README.md.
//   Blocker()  -> the app refuses to start (wrong OS / architecture).
//   Machine.ModelAllowed -> hardware writes are enabled only on verified models; others run read-only.
//   Warnings() -> runs, but something is untested or a feature is unavailable.
public static class Platform
{
	// Windows 10 2004 (build 19041) is the oldest build the ATKACPI / PawnIO stack is known to work on.
	public const int MinWindowsBuild = 19041;

	// Models whose ACPI endpoints, power limits and fan tables were verified end to end.
	public static readonly string[] VerifiedModels = new string[1] { "FA607NUQ" };

	// BIOS versions tested on the verified models. Other versions run, with a warning.
	public static readonly string[] VerifiedBios = new string[1] { "FA607NUQ.300" };

	public static string Bios => Registry.GetValue("HKEY_LOCAL_MACHINE\\HARDWARE\\DESCRIPTION\\System\\BIOS", "BIOSVersion", null)?.ToString()?.Trim() ?? "?";

	public static int WindowsBuild => Environment.OSVersion.Version.Build;

	public static string? Blocker()
	{
		if (!OperatingSystem.IsWindows())
		{
			return Lang.T("TUF Control yalnızca Windows'ta çalışır.");
		}
		if (RuntimeInformation.OSArchitecture != Architecture.X64 || RuntimeInformation.ProcessArchitecture != Architecture.X64)
		{
			return Lang.F("TUF Control yalnızca x64 Windows'ta çalışır (bu sistem: {0}).", RuntimeInformation.OSArchitecture);
		}
		if (WindowsBuild < MinWindowsBuild)
		{
			return Lang.F("Windows 10 2004 (yapı {0}) veya daha yenisi gerekli; bu sistem yapı {1}.", MinWindowsBuild, WindowsBuild);
		}
		return null;
	}

	public static string[] Warnings()
	{
		List<string> list = new List<string>();
		if (!Machine.ModelAllowed)
		{
			list.Add(Lang.F("Bu model ({0}) doğrulanmadı; uygulama salt okunur çalışır. Desteklenen: {1}.", Machine.Model, string.Join(", ", VerifiedModels)));
		}
		else if (!VerifiedBios.Contains(Bios, StringComparer.OrdinalIgnoreCase))
		{
			list.Add(Lang.F("BIOS {0} test edilmedi (test edilen: {1}). Ayarlar uygulanır ama sorun görürsen bildir.", Bios, string.Join(", ", VerifiedBios)));
		}
		if (Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\PawnIO", "ImagePath", null) == null)
		{
			list.Add(Lang.T("PawnIO sürücüsü kurulu değil; CPU sıcaklık hedefi kullanılamaz (diğer özellikler çalışır)."));
		}
		return list.ToArray();
	}

	public static object Report()
	{
		return new
		{
			Os = RuntimeInformation.OSDescription,
			WindowsBuild = WindowsBuild,
			OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
			Runtime = RuntimeInformation.FrameworkDescription,
			Model = Machine.Model,
			Bios = Bios,
			Processor = Machine.Processor,
			Blocker = Blocker(),
			Warnings = Warnings()
		};
	}
}
