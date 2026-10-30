namespace TufControl;

public record ProfileStat(string Profile, double Hours, double? AvgCpuTemp, double? P95CpuTemp, double? AvgCpuLoad, double? AvgCpuFan, double? AvgGpuTemp, double? AvgBatteryDrawW, double? IdleBatteryDrawW);
