using System;
using System.Collections.Generic;

namespace TufControl;

public record SegmentSummary(DateTime Start, DateTime End, string Reason, double Seconds, double OnBatterySeconds, double? EnergyWh, Dictionary<string, MetricSummary> Metrics, Dictionary<string, ProfileSummary> Profiles);
