namespace TufControl;

public record Capabilities(bool Mode, bool CpuCurve, bool GpuCurve, bool Power, bool Thermal, bool Boost = false, bool Gpu = false, bool Eco = false);
