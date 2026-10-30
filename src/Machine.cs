using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace TufControl;

public static class Machine
{
	private struct PowerStatus
	{
		public byte Ac;

		public byte Flag;

		public byte Percent;

		public byte Reserved;

		public uint Life;

		public uint FullLife;
	}

	private struct MemoryStatus
	{
		public uint Length;

		public uint Load;

		public ulong Total;

		public ulong Available;

		public ulong Page;

		public ulong AvailPage;

		public ulong Virtual;

		public ulong AvailVirtual;

		public ulong Extended;
	}

	private struct ServiceStatus
	{
		public uint Type;

		public uint State;

		public uint Accept;

		public uint Win32;

		public uint Specific;

		public uint Check;

		public uint Wait;
	}

	private sealed class NativeHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		public NativeHandle(nint p)
			: base(ownsHandle: true)
		{
			SetHandle(p);
		}

		protected override bool ReleaseHandle()
		{
			return CloseServiceHandle(handle);
		}
	}

	public static string Model => Registry.GetValue("HKEY_LOCAL_MACHINE\\HARDWARE\\DESCRIPTION\\System\\BIOS", "SystemProductName", null)?.ToString() ?? "Bilinmeyen model";

	public static bool ModelAllowed => Platform.VerifiedModels.Any((string m) => Model.Contains(m, StringComparison.OrdinalIgnoreCase));

	public static string Processor => Registry.GetValue("HKEY_LOCAL_MACHINE\\HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0", "ProcessorNameString", null)?.ToString()?.Trim() ?? "CPU";

	public static Snapshot Sample(IHardware hw)
	{
		PowerStatus p = default;
		bool ok = GetSystemPowerStatus(out p);
		MemoryStatus mem = new MemoryStatus
		{
			Length = (uint)Marshal.SizeOf<MemoryStatus>()
		};
		GlobalMemoryStatusEx(ref mem);
		return new Snapshot(Temperature(hw.Read(1179796u)), Temperature(hw.Read(1179799u)), Fan(hw.Read(1114131u)), Fan(hw.Read(1114132u)), hw.Read(1179765u), hw.Read(1179811u), hw.Read(1179806u), (int)mem.Load, (ok && p.Ac != byte.MaxValue) ? new bool?(p.Ac == 1) : ((bool?)null), (ok && p.Percent <= 100) ? new int?(p.Percent) : ((int?)null), hw.Read(589856u));
	}

	private static int? Temperature(int? x)
	{
		if (x.HasValue)
		{
			int valueOrDefault = x.GetValueOrDefault();
			if (valueOrDefault > 0 && valueOrDefault <= 120)
			{
				return x;
			}
		}
		return null;
	}

	private static int? Fan(int? x)
	{
		if (x.HasValue)
		{
			int valueOrDefault = x.GetValueOrDefault();
			if (valueOrDefault >= 0 && valueOrDefault <= 120)
			{
				return x * 100;
			}
		}
		return null;
	}

	public static string[] Conflicts()
	{
		List<string> names = new List<string>();
		string[] array = new string[3] { "ArmouryCrateService", "ArmouryCrateControlInterface", "ASUSOptimization" };
		foreach (string name in array)
		{
			using NativeHandle scm = new NativeHandle(OpenSCManager(null, null, 1u));
			if (scm.IsInvalid)
			{
				names.Add("Servis durumu okunamadı");
				break;
			}
			using NativeHandle svc = new NativeHandle(OpenService(scm.DangerousGetHandle(), name, 4u));
			ServiceStatus status;
			if (svc.IsInvalid)
			{
				if (Marshal.GetLastWin32Error() != 1060)
				{
					names.Add(name + " durumu bilinmiyor");
				}
			}
			else if (!QueryServiceStatus(svc.DangerousGetHandle(), out status))
			{
				names.Add(name + " durumu bilinmiyor");
			}
			else if (status.State != 1)
			{
				names.Add(name);
			}
		}
		Process[] processesByName = Process.GetProcessesByName("GHelper");
		foreach (Process obj in processesByName)
		{
			names.Add("G-Helper");
			obj.Dispose();
		}
		return names.ToArray();
	}

	[DllImport("kernel32.dll")]
	private static extern bool GetSystemPowerStatus(out PowerStatus s);

	[DllImport("kernel32.dll")]
	private static extern bool GlobalMemoryStatusEx(ref MemoryStatus s);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern nint OpenSCManager(string? machine, string? database, uint access);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern nint OpenService(nint scm, string name, uint access);

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern bool QueryServiceStatus(nint service, out ServiceStatus status);

	[DllImport("advapi32.dll")]
	private static extern bool CloseServiceHandle(nint handle);
}
