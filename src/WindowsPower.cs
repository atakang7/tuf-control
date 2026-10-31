using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace TufControl;

// Processor settings of the active Windows power plan. MaxMhz 0 = no frequency cap.
public record WindowsCpu(int MinState, int MaxMhz, bool Boost, int Epp)
{
	public static readonly WindowsCpu AcDefaults = new WindowsCpu(5, 0, Boost: true, 33);

	public static readonly WindowsCpu DcDefaults = new WindowsCpu(5, 0, Boost: true, 50);

	public static WindowsCpu From(Profile p)
	{
		return new WindowsCpu(p.MinCpuState, p.LimitCpuClock ? p.CpuClockMhz : 0, p.CpuBoost, p.CpuEpp);
	}

	public string Describe()
	{
		return $"min {MinState} % · " + ((MaxMhz > 0) ? $"≤ {MaxMhz} MHz" : Lang.T("sınırsız")) + " · boost " + Lang.T(Boost ? "açık" : "kapalı") + $" · EPP {Epp}";
	}
}

public interface IWindowsPower
{
	string PlanName { get; }

	WindowsCpu Read(bool ac);

	// Writes both the plugged-in and the battery value of the active plan, then re-activates it.
	void Write(WindowsCpu ac, WindowsCpu dc);
}

public sealed class WindowsPower : IWindowsPower
{
	private static Guid SubProcessor = new Guid("54533251-82be-4824-96c1-47b60b740d00");

	private static Guid MinState = new Guid("893dee8e-2bef-41e0-89c6-b55d0929964c");

	private static Guid MaxFrequency = new Guid("75b0ae3f-bce0-45a7-8c89-c9611c25e100");

	private static Guid BoostMode = new Guid("be337238-0d82-4146-a960-4f3749d470c7");

	private static Guid Epp = new Guid("36687f9e-e3a5-4dbf-b1dc-15eb381c6863");

	// PERFBOOSTMODE: 0 = disabled, 2 = aggressive (the Windows default).
	private const uint BoostOn = 2u;

	public string PlanName
	{
		get
		{
			Guid scheme = Active();
			uint size = 0u;
			PowerReadFriendlyName(IntPtr.Zero, ref scheme, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref size);
			if (size == 0)
			{
				return scheme.ToString();
			}
			IntPtr buffer = Marshal.AllocHGlobal((int)size);
			try
			{
				Check(PowerReadFriendlyName(IntPtr.Zero, ref scheme, IntPtr.Zero, IntPtr.Zero, buffer, ref size));
				return Marshal.PtrToStringUni(buffer) ?? scheme.ToString();
			}
			finally
			{
				Marshal.FreeHGlobal(buffer);
			}
		}
	}

	public WindowsCpu Read(bool ac)
	{
		Guid scheme = Active();
		return new WindowsCpu((int)Get(scheme, MinState, ac), (int)Get(scheme, MaxFrequency, ac), Get(scheme, BoostMode, ac) != 0, (int)Get(scheme, Epp, ac));
	}

	public void Write(WindowsCpu ac, WindowsCpu dc)
	{
		Validate(ac);
		Validate(dc);
		Guid scheme = Active();
		Put(scheme, ac, ac: true);
		Put(scheme, dc, ac: false);
		Check(PowerSetActiveScheme(IntPtr.Zero, ref scheme));
	}

	private static void Validate(WindowsCpu v)
	{
		if (v.MinState < 0 || v.MinState > 100 || v.Epp < 0 || v.Epp > 100 || (v.MaxMhz != 0 && (v.MaxMhz < 1400 || v.MaxMhz > 4800)))
		{
			throw new InvalidOperationException("Güvenli aralık dışı Windows işlemci ayarı reddedildi (" + v.Describe() + ").");
		}
	}

	private static void Put(Guid scheme, WindowsCpu v, bool ac)
	{
		Set(scheme, MinState, ac, (uint)v.MinState);
		Set(scheme, MaxFrequency, ac, (uint)v.MaxMhz);
		Set(scheme, BoostMode, ac, v.Boost ? BoostOn : 0u);
		Set(scheme, Epp, ac, (uint)v.Epp);
	}

	private static Guid Active()
	{
		Check(PowerGetActiveScheme(IntPtr.Zero, out var ptr));
		try
		{
			return Marshal.PtrToStructure<Guid>(ptr);
		}
		finally
		{
			LocalFree(ptr);
		}
	}

	private static uint Get(Guid scheme, Guid setting, bool ac)
	{
		uint value;
		Check(ac ? PowerReadACValueIndex(IntPtr.Zero, ref scheme, ref SubProcessor, ref setting, out value) : PowerReadDCValueIndex(IntPtr.Zero, ref scheme, ref SubProcessor, ref setting, out value));
		return value;
	}

	private static void Set(Guid scheme, Guid setting, bool ac, uint value)
	{
		Check(ac ? PowerWriteACValueIndex(IntPtr.Zero, ref scheme, ref SubProcessor, ref setting, value) : PowerWriteDCValueIndex(IntPtr.Zero, ref scheme, ref SubProcessor, ref setting, value));
	}

	private static void Check(uint error)
	{
		if (error != 0)
		{
			throw new Win32Exception((int)error);
		}
	}

	[DllImport("powrprof.dll")]
	private static extern uint PowerGetActiveScheme(nint userRootPowerKey, out nint activePolicyGuid);

	[DllImport("powrprof.dll")]
	private static extern uint PowerSetActiveScheme(nint userRootPowerKey, ref Guid schemeGuid);

	[DllImport("powrprof.dll")]
	private static extern uint PowerReadACValueIndex(nint rootPowerKey, ref Guid schemeGuid, ref Guid subGroupGuid, ref Guid settingGuid, out uint value);

	[DllImport("powrprof.dll")]
	private static extern uint PowerReadDCValueIndex(nint rootPowerKey, ref Guid schemeGuid, ref Guid subGroupGuid, ref Guid settingGuid, out uint value);

	[DllImport("powrprof.dll")]
	private static extern uint PowerWriteACValueIndex(nint rootPowerKey, ref Guid schemeGuid, ref Guid subGroupGuid, ref Guid settingGuid, uint value);

	[DllImport("powrprof.dll")]
	private static extern uint PowerWriteDCValueIndex(nint rootPowerKey, ref Guid schemeGuid, ref Guid subGroupGuid, ref Guid settingGuid, uint value);

	[DllImport("powrprof.dll")]
	private static extern uint PowerReadFriendlyName(nint rootPowerKey, ref Guid schemeGuid, nint subGroupGuid, nint settingGuid, nint buffer, ref uint bufferSize);

	[DllImport("kernel32.dll")]
	private static extern nint LocalFree(nint mem);
}
