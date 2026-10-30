namespace TufControl;

public record Snapshot(int? CpuTemp, int? GpuTemp, int? CpuRpm, int? GpuRpm, int? Mode, int? Power, int? Thermal, int MemoryPercent, bool? OnAc, int? Battery, int? Eco = null);
