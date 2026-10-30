using System;

namespace TufControl;

public record HistoryRow(DateTime Time, string Profile, bool? Ac, double? CpuTemp, double? GpuTemp, double? CpuLoad, double? GpuLoad, double? CpuClock, double? GpuClock, double? GpuPower, double? CpuFan, double? GpuFan, double? Mem, double? BatteryFlow, double? Vram, double? BatteryPct);
