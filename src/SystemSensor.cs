using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace TufControl;

public sealed class SystemSensor : IDisposable
{
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

	private struct BatteryState
	{
		public byte AcOnLine;

		public byte Present;

		public byte Charging;

		public byte Discharging;

		public byte S1;

		public byte S2;

		public byte S3;

		public byte S4;

		public uint MaxCapacity;

		public uint RemainingCapacity;

		public int Rate;

		public uint Estimated;

		public uint DefaultAlert1;

		public uint DefaultAlert2;
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct PdhValue
	{
		[FieldOffset(0)]
		public uint Status;

		[FieldOffset(8)]
		public double Value;
	}

	private long lastIdle;

	private long lastTotal;

	private nint query;

	private nint perfCounter;

	private readonly double baseMhz = (Registry.GetValue("HKEY_LOCAL_MACHINE\\HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0", "~MHz", null) as int?).GetValueOrDefault();

	public SystemSensor()
	{
		if (PdhOpenQuery(null, IntPtr.Zero, out query) == 0 && PdhAddEnglishCounter(query, "\\Processor Information(_Total)\\% Processor Performance", IntPtr.Zero, out perfCounter) == 0)
		{
			PdhCollectQueryData(query);
		}
		else
		{
			perfCounter = IntPtr.Zero;
		}
	}

	public SystemStats Read()
	{
		int? usage = null;
		if (GetSystemTimes(out var idle, out var kernel, out var user))
		{
			long i = idle;
			long total = kernel + user;
			if (lastTotal > 0 && total > lastTotal)
			{
				usage = (int)Math.Round(Math.Clamp(100.0 * (1.0 - (double)(i - lastIdle) / (double)(total - lastTotal)), 0.0, 100.0));
			}
			lastIdle = i;
			lastTotal = total;
		}
		double? clock = null;
		if (perfCounter != IntPtr.Zero && baseMhz > 0.0 && PdhCollectQueryData(query) == 0 && PdhGetFormattedCounterValue(perfCounter, 512u, IntPtr.Zero, out var v) == 0 && v.Status == 0)
		{
			clock = baseMhz * v.Value / 100.0 / 1000.0;
		}
		MemoryStatus mem = new MemoryStatus
		{
			Length = (uint)Marshal.SizeOf<MemoryStatus>()
		};
		GlobalMemoryStatusEx(ref mem);
		double gb = 1073741824.0;
		double disk = 0.0;
		double diskTotal = 0.0;
		try
		{
			DriveInfo d = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory));
			diskTotal = (double)d.TotalSize / gb;
			disk = (double)(d.TotalSize - d.TotalFreeSpace) / gb;
		}
		catch
		{
		}
		double? watts = null;
		TimeSpan? remaining = null;
		if (CallNtPowerInformation(5, IntPtr.Zero, 0, out var b, Marshal.SizeOf<BatteryState>()) == 0 && b.Present != 0)
		{
			if (b.Rate != 0 && Math.Abs(b.Rate) < 300000)
			{
				watts = (double)b.Rate / 1000.0;
			}
			if (b.Discharging != 0)
			{
				uint estimated = b.Estimated;
				if (estimated != 0 && estimated < 86400)
				{
					remaining = TimeSpan.FromSeconds(b.Estimated);
				}
			}
		}
		return new SystemStats(usage, clock, (double)(mem.Total - mem.Available) / gb, (double)mem.Total / gb, disk, diskTotal, watts, remaining);
	}

	public void Dispose()
	{
		if (query != IntPtr.Zero)
		{
			PdhCloseQuery(query);
			query = IntPtr.Zero;
		}
	}

	public static bool ExternalDisplayConnected()
	{
		try
		{
			if (GetDisplayConfigBufferSizes(2u, out var pathCount, out var modeCount) != 0 || pathCount == 0)
			{
				return false;
			}
			byte[] paths = new byte[pathCount * 72];
			byte[] modes = new byte[modeCount * 64];
			if (QueryDisplayConfig(2u, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero) != 0)
			{
				return false;
			}
			for (int i = 0; i < pathCount; i++)
			{
				int tech = BitConverter.ToInt32(paths, i * 72 + 36);
				if (tech != int.MinValue && tech != 11 && tech != 13)
				{
					return true;
				}
			}
		}
		catch
		{
		}
		return false;
	}

	[DllImport("user32.dll")]
	private static extern int GetDisplayConfigBufferSizes(uint flags, out uint pathCount, out uint modeCount);

	[DllImport("user32.dll")]
	private static extern int QueryDisplayConfig(uint flags, ref uint pathCount, [Out] byte[] paths, ref uint modeCount, [Out] byte[] modes, nint topology);

	[DllImport("kernel32.dll")]
	private static extern bool GetSystemTimes(out long idle, out long kernel, out long user);

	[DllImport("kernel32.dll")]
	private static extern bool GlobalMemoryStatusEx(ref MemoryStatus s);

	[DllImport("powrprof.dll")]
	private static extern uint CallNtPowerInformation(int level, nint input, int inputSize, out BatteryState output, int outputSize);

	[DllImport("pdh.dll", CharSet = CharSet.Unicode)]
	private static extern int PdhOpenQuery(string? source, nint user, out nint query);

	[DllImport("pdh.dll", CharSet = CharSet.Unicode)]
	private static extern int PdhAddEnglishCounter(nint query, string path, nint user, out nint counter);

	[DllImport("pdh.dll")]
	private static extern int PdhCollectQueryData(nint query);

	[DllImport("pdh.dll")]
	private static extern int PdhGetFormattedCounterValue(nint counter, uint format, nint type, out PdhValue value);

	[DllImport("pdh.dll")]
	private static extern int PdhCloseQuery(nint query);
}
