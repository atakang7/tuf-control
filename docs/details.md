# TUF Control details

Build, command-line options and firmware limits for the FA607NUQ.

## Run from source

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), then use administrator PowerShell on Windows x64:

```powershell
git clone https://github.com/atakang7/tuf-control.git
cd tuf-control
dotnet build src/TufControl.csproj -c Release
dotnet publish src/TufControl.csproj -c Release -o publish
.\Install.ps1 -Source .\publish
```

## Commands

| option | result |
|---|---|
| `--verify [profile]` | Writes `verify.json` under `%LOCALAPPDATA%\TufControl`; exit `0` if readable values match |
| `--diagnose [file]` | Writes hardware diagnostics as JSON |
| `--read-only` | Starts the UI without hardware writes |
| `--render <folder> [--lang en]` | Captures UI pages as PNG files without hardware writes |

## Config

Profile values are stored in `%LOCALAPPDATA%\TufControl\profiles.json` and validated before writes.

| var | default | what |
|---|---|---|
| `LimitPower` | `false` | Enables the CPU power cap |
| `CpuWatts` | `35` | 15-45 W cap when enabled |
| `LimitTemperature` | `false` | Enables the CPU temperature limit |
| `CpuTarget` | `85` | 60-90 C limit when enabled |
| `TuneWindows` | `false` | Changes the active Windows power plan |
| `GpuMode` | `0` | Leaves GPU mode unchanged |

## Limits

- Hardware writes require `Platform.VerifiedModels`; only FA607NUQ is verified.
- The tested BIOS is FA607NUQ.300; other versions produce a warning.
- `AsusHardware` writes only to an allow-list of `ATKACPI` endpoints.
- `Thermal` requires a Rembrandt CPU and PawnIO 2.x for temperature limits.
- GPU mode, CPU temperature and `WindowsPower` settings have readback; fan curves and power caps do not.
- Fan curves require at least 30% at 70 C, 70% at 80 C and 100% at 90 C.
- `WindowsPower` writes AC and DC values on the active plan and restores defaults only after this session changed them.
- The 30-second drift check reapplies settings that no longer match.

Tested with Windows 11 Pro 26H2 (build 26300.9711), Ryzen 7 170, RTX 4050, ASUS System Control Interface v3 3.1.73.0 and PawnIO 2.2.0.0.

Adding a model: [CONTRIBUTING.md](../CONTRIBUTING.md).
