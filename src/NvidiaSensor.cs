using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace TufControl;

public sealed class NvidiaSensor : IDisposable
{
	private struct Utilization
	{
		public uint Gpu;

		public uint Memory;
	}

	private struct Memory
	{
		public ulong Total;

		public ulong Free;

		public ulong Used;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int Init();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int GetHandle(uint index, out nint device);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int GetUInt(nint device, uint arg, out uint value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int GetPower(nint device, out uint milliwatts);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int GetUtil(nint device, out Utilization value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int GetMemory(nint device, out Memory value);

	private struct ProcessInfo
	{
		public uint Pid;

		public ulong UsedMemory;

		public uint GpuInstance;

		public uint ComputeInstance;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int GetProcesses(nint device, ref uint count, [Out] ProcessInfo[] infos);

	private nint library;

	private bool initialized;

	private T Export<T>(string name) where T : Delegate
	{
		return Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, name));
	}

	private nint? Device()
	{
		if (library == IntPtr.Zero)
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "nvml.dll");
			if (!File.Exists(path))
			{
				return null;
			}
			library = NativeLibrary.Load(path);
		}
		if (!initialized)
		{
			if (Export<Init>("nvmlInit_v2")() != 0)
			{
				return null;
			}
			initialized = true;
		}
		if (Export<GetHandle>("nvmlDeviceGetHandleByIndex_v2")(0u, out var device) != 0)
		{
			return null;
		}
		return device;
	}

	public int? ReadTemperature()
	{
		return Read().Temperature;
	}

	public int[]? ActiveProcesses()
	{
		try
		{
			nint? num = Device();
			if (num.HasValue)
			{
				nint d = num.GetValueOrDefault();
				HashSet<int> pids = new HashSet<int>();
				string[] array = new string[2] { "nvmlDeviceGetGraphicsRunningProcesses_v3", "nvmlDeviceGetComputeRunningProcesses_v3" };
				foreach (string name in array)
				{
					ProcessInfo[] infos = new ProcessInfo[128];
					uint count = (uint)infos.Length;
					if (Export<GetProcesses>(name)(d, ref count, infos) != 0)
					{
						return null;
					}
					for (int j = 0; j < count && j < infos.Length; j++)
					{
						pids.Add((int)infos[j].Pid);
					}
				}
				return pids.ToArray();
			}
			return null;
		}
		catch
		{
			return null;
		}
	}

	public GpuStats Read()
	{
		try
		{
			nint? num = Device();
			if (num.HasValue)
			{
				nint d = num.GetValueOrDefault();
				int? temperature = ((Export<GetUInt>("nvmlDeviceGetTemperature")(d, 0u, out var t) == 0 && t != 0 && t < 120) ? new int?((int)t) : ((int?)null));
				int? usage = ((Export<GetUtil>("nvmlDeviceGetUtilizationRates")(d, out var u) == 0 && u.Gpu <= 100) ? new int?((int)u.Gpu) : ((int?)null));
				double? power = ((Export<GetPower>("nvmlDeviceGetPowerUsage")(d, out var mw) == 0 && mw < 400000) ? new double?((double)mw / 1000.0) : ((double?)null));
				int? clock = ((Export<GetUInt>("nvmlDeviceGetClockInfo")(d, 0u, out var mhz) == 0 && mhz < 5000) ? new int?((int)mhz) : ((int?)null));
				bool mem = Export<GetMemory>("nvmlDeviceGetMemoryInfo")(d, out var m) == 0 && m.Total != 0;
				return new GpuStats(temperature, usage, power, clock, mem ? new double?((double)m.Used / 1073741824.0) : ((double?)null), mem ? new double?((double)m.Total / 1073741824.0) : ((double?)null));
			}
			return new GpuStats(null, null, null, null, null, null);
		}
		catch
		{
			return new GpuStats(null, null, null, null, null, null);
		}
	}

	public void Dispose()
	{
		if (library == IntPtr.Zero)
		{
			return;
		}
		try
		{
			if (initialized)
			{
				Export<Init>("nvmlShutdown")();
			}
		}
		catch
		{
		}
		NativeLibrary.Free(library);
		library = IntPtr.Zero;
		initialized = false;
	}
}
