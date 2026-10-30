using System;

namespace TufControl;

public record SystemStats(int? CpuUsage, double? CpuClockGhz, double MemUsedGb, double MemTotalGb, double DiskUsedGb, double DiskTotalGb, double? BatteryWatts, TimeSpan? BatteryTime)
{
	public double? SystemDraw
	{
		get
		{
			double? batteryWatts = BatteryWatts;
			if (batteryWatts.HasValue)
			{
				double w = batteryWatts.GetValueOrDefault();
				if (w < 0.0)
				{
					return 0.0 - w;
				}
			}
			return null;
		}
	}
}
