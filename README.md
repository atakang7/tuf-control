# TUF Control

TUF Control sets fan curves, CPU power limits and GPU modes on the ASUS TUF Gaming A16 FA607NUQ without Armoury Crate. It keeps profiles for AC power, battery use and selected applications.

## Quick start

Requires x64 Windows 10 2004 (build 19041) or later, the ASUS System Control Interface v3 driver and the [.NET 10 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/10.0).

1. Download `TufControl-win-x64.zip` from [Releases](https://github.com/atakang7/tuf-control/releases/latest) and extract it.
2. Open PowerShell as administrator in the extracted folder.
3. Run:

```powershell
Get-ChildItem -File | Unblock-File
.\Install.ps1
```

The installer copies the app to `C:\Program Files\TufControl` and registers `TufControl.Logon` to start at sign-in.

To build instead, install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), then run:

```powershell
git clone https://github.com/atakang7/tuf-control.git
cd tuf-control
dotnet publish src/TufControl.csproj -c Release --self-contained false -o publish
.\Install.ps1 -Source .\publish
```

Run the install command from an administrator PowerShell session.

## Configuration

Create and edit profiles in the app. Assign a profile for plugged-in and battery operation, and optionally set application rules. Profiles and preferences are saved in `%LOCALAPPDATA%\TufControl\profiles.json`; events are logged in the same folder.

A profile can set an eight-point CPU and GPU fan curve, CPU power and temperature limits, GPU mode, and Windows processor settings. Changing Windows processor settings also changes the active Windows power plan.

## Usage

The scheduled task applies the selected profile at sign-in. You can switch profiles in the UI or use these commands from an administrator PowerShell session:

```powershell
& "$env:ProgramFiles\TufControl\TufControl.exe" --verify
& "$env:ProgramFiles\TufControl\TufControl.exe" --diagnose "$env:TEMP\tuf-diagnostics.json"
& "$env:ProgramFiles\TufControl\TufControl.exe" --read-only
```

`--verify` saves its report to `%LOCALAPPDATA%\TufControl\verify.json`. To uninstall, run `.\Uninstall.ps1` from the extracted release folder. Profiles remain unless you pass `-RemoveSettings`.

## How it works

The app writes allow-listed firmware settings through the ASUS `ATKACPI` driver. The optional PawnIO driver handles the Ryzen SMU temperature limit and live CPU temperature; without it, other controls remain available. A 30-second check reapplies settings that have drifted. See [implementation notes](docs/details.md).

## Limitations

Only the FA607NUQ is permitted to receive hardware writes. Other models run read-only. BIOS FA607NUQ.300 is tested; other versions show a warning. NVIDIA telemetry needs its driver, and the CPU temperature limit needs PawnIO 2.x on a supported Ryzen Rembrandt CPU.

The firmware does not expose active custom fan curves or power caps for readback. `--verify` checks readable settings but cannot confirm those values. Hardware changes require administrator access. This is an independent project, not an ASUS product.

## Measurements

Seven days of measurements on one FA607NUQ, comparing stock firmware with my profiles:

| Metric | Stock | TUF Control |
|---|---:|---:|
| Plugged-in CPU temperature | 59.1 °C | 55.7 °C |
| Plugged-in CPU fan | 2,907 rpm | 2,372 rpm |
| Battery CPU fan | 5,185 rpm | 771 rpm |
| Battery power draw | 14.4 W | 10.9 W |

Sampled time was 5.6 h stock versus 73.7 h with TUF Control on AC power, and 0.5 h versus 1.5 h on battery. The 15 W AC profile reduced average CPU clock from 3.3 to 2.8 GHz. These figures are from one laptop, not a controlled benchmark.

![TUF Control dashboard](docs/screenshots/dashboard.png)

[License: GPL-3.0](LICENSE) · [Third-party notices](THIRD-PARTY-NOTICES.md) · [Contributing](CONTRIBUTING.md)
