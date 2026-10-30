namespace TufControl;

public readonly record struct MetricStats(double? Current, double? Min, double? Avg, double? Max, double? Peak, int Count);
