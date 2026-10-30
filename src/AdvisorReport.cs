using System.Collections.Generic;

namespace TufControl;

public record AdvisorReport(int Rows, double Hours, double BatteryHours, double? AvgBatteryDrawW, bool Enough, List<ProfileStat> Profiles, List<Advice> Advice);
