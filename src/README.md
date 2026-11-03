Part of [tuf-control](../README.md).

# Windows app

.NET 10 WPF executable with profile control, hardware monitoring and a Windows startup service.

## Run

From the repository root on x64 Windows:

```powershell
dotnet publish src/TufControl.csproj -c Release -o publish
.\publish\TufControl.exe --read-only
```

## Test

```powershell
dotnet build src/TufControl.csproj -c Release
.\publish\TufControl.exe --render .\rendered --lang en
```

The render command writes PNG screenshots without firmware writes. There is no automated test project.

## Config

Settings are stored in `%LOCALAPPDATA%\TufControl\profiles.json`.

| Setting | Default | What |
|---|---|---|
| `Language` | `tr` | UI language, `tr` or `en` |
| `StartupMode` | `auto` | Profile choice at sign-in |
| `RecordHistory` | `true` | Store hardware samples |
| `BootEarly` | `false` | Apply a profile from the boot service |

## Interfaces

- [CLI commands and local files](../docs/reference/cli.md)
- [Hardware and driver requirements](../docs/reference/compatibility.md)
- `Program.cs` starts the UI, diagnostic command, verifier or boot service.
- `Controller.cs` applies profiles through `AsusHardware.cs`, `Thermal.cs` and `WindowsPower.cs`.
