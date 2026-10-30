using System;

namespace TufControl;

public record BootState(DateTime AppliedAt, string Profile, int Mode, bool ThermalPending, int? ThermalBaseline, bool ThermalActive);
