using System;
using Microsoft.Win32;

namespace TufControl;

// Pins this executable to the power-saving GPU (the Radeon iGPU that drives the internal panel) through
// the same per-app setting as Windows Settings > Display > Graphics. The UI then renders on the GPU
// without ever waking the RTX, and RTX sleep / Eco switches cannot take its Direct3D device away.
public static class GpuPreference
{
	private const string Key = "Software\\Microsoft\\DirectX\\UserGpuPreferences";

	private const string PowerSaving = "GpuPreference=1;";

	public static bool PinToPowerSaving()
	{
		try
		{
			string exe = Environment.ProcessPath;
			if (string.IsNullOrEmpty(exe))
			{
				return false;
			}
			using RegistryKey key = Registry.CurrentUser.CreateSubKey(Key);
			if (key.GetValue(exe) as string != PowerSaving)
			{
				key.SetValue(exe, PowerSaving, RegistryValueKind.String);
			}
			return true;
		}
		catch
		{
			return false;
		}
	}
}
