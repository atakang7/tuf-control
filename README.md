# TUF Control

Windows WPF app for ASUS TUF Gaming A16 FA607NUQ owners to manage fan curves, power limits and GPU modes.

```text
+------------+  profile  +------------+  calls  +--------------+  ACPI  +---------+
| MainWindow | --------> | Controller | ------> | AsusHardware | -----> | ATKACPI |
+------------+           +------------+         +--------------+        +---------+
                              |
                         +----+-------+
                         |            |
                         v SMU        v plan
                    +---------+  +--------------+
                    | Thermal |  | WindowsPower |
                    +---------+  +--------------+
                         |
                         v driver
                    +--------+
                    | PawnIO |
                    +--------+
```

## Quick start

On x64 Windows with the .NET 10 SDK, run these commands from an elevated PowerShell window:

```powershell
dotnet publish src/TufControl.csproj -c Release -o publish
.\Install.ps1
```

For the prebuilt ZIP, use the [release install guide](docs/how-to/install.md).

## Layout

| Path | What it is |
|---|---|
| [src/](src/README.md) | WPF UI, profile controller, hardware adapters and startup service |

## Docs

- [Documentation index](docs/README.md)
- [Build from source](docs/how-to/build.md)
- [CLI and data files](docs/reference/cli.md)
- [Architecture](docs/explanation/architecture.md)

## Limits

- Hardware writes are restricted to the FA607NUQ. Other models are read-only.
- The CPU temperature target requires PawnIO and a supported Ryzen CPU.
- Adapted G-Helper code is covered by [GPL-3.0](LICENSE); see [third-party notices](THIRD-PARTY-NOTICES.md).
