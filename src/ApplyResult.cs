namespace TufControl;

public record ApplyResult(bool Success, string Message, bool GpuBusy = false, bool ThermalPending = false);
