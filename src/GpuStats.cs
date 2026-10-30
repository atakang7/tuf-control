namespace TufControl;

public record GpuStats(int? Temperature, int? Usage, double? PowerWatts, int? ClockMhz, double? VramUsedGb, double? VramTotalGb);
