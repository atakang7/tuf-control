# TUF Control

TUF Control applies fan, power and GPU profiles to the ASUS TUF Gaming A16 FA607NUQ on Windows.

```text
+------------+  profile  +------------+  writes  +--------------+  calls  +---------+
| MainWindow | --------> | Controller | -------> | AsusHardware | -------> | ATKACPI |
+------------+           +------------+          +--------------+          +---------+
                             |
                             | sets
                             v
                         +---------+  calls  +---------------+
                         | Thermal | ------> | PawnIOWrapper |
                         +---------+         +---------------+
```

## Run

Install the [.NET 10 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/10.0), download and extract [TufControl-win-x64.zip](https://github.com/atakang7/tuf-control/releases/latest), then run in administrator PowerShell from that folder:

```powershell
Get-ChildItem -File | Unblock-File
.\Install.ps1
```

The installer copies files to `C:\Program Files\TufControl` and creates the `TufControl.Logon` task; uninstall with `.\Uninstall.ps1` from the extracted folder.

## Config

Edit profiles in the UI; settings are stored in `%LOCALAPPDATA%\TufControl\profiles.json`.

| var | default | what |
|---|---|---|
| `AcProfile` | `Profilim` | Profile on AC power |
| `BatteryProfile` | `Profilim` | Profile on battery |
| `StartupMode` | `auto` | Profile selection at sign-in |
| `BootEarly` | `false` | Apply at boot through the service |
| `RecordHistory` | `true` | Save metric history |

## CI

```text
+----------+  source  +-------+  output  +---------+  files  +---------+
| checkout | -------> | Build | -------> | Publish | ------> | Package |
+----------+          +-------+          +---------+         +---------+
                                                              |
                                                              | zip
                                                              v
                                                    +-----------------+  zip  +---------+
                                                    | upload-artifact | ----> | Release |
                                                    +-----------------+       +---------+
```

`Release` runs only for `v*` tags; pull requests run the Windows build and package steps.

## Limits

- Only FA607NUQ receives hardware writes; other models run read-only.
- Windows x64 build 19041+ and ASUS System Control Interface v3 are required.
- BIOS FA607NUQ.300 is tested; other versions show a warning.
- PawnIO 2.x is needed for the Ryzen Rembrandt CPU temperature limit; NVIDIA telemetry needs its driver.
- The firmware does not expose active fan curves or power caps for readback.

Build commands, diagnostic flags and safety bounds: [details](docs/details.md). License: [GPL-3.0](LICENSE), with [third-party notices](THIRD-PARTY-NOTICES.md).
