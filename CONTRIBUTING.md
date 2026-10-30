# Contributing

## Build

Requirements: Windows 10 2004+ x64 and the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```powershell
dotnet build src/TufControl.csproj -c Release
dotnet publish src/TufControl.csproj -c Release -o publish
.\Install.ps1   # administrator PowerShell
```

`TufControl.exe --render <folder> --lang en` renders every page to PNG without touching the
hardware. Use it for screenshots in pull requests.

## Adding a laptop model

1. Run the app on the new model. It is read-only there, so this is safe.
2. Export `--diagnose` and check that `Capabilities` reports the endpoints you need.
3. Add the model to `Platform.VerifiedModels` and its BIOS to `Platform.VerifiedBios` (`src/Platform.cs`).
4. Apply every feature once, run `TufControl.exe --verify` and include `verify.json` in the pull request.
5. Update the compatibility table in `README.md`.

## Rules

- Never widen a safety range without measurements from the real machine.
- Any new ACPI endpoint must be added to the allow-list in `AsusHardware.Write`.
- Code adapted from other projects must be GPL-3.0-compatible and listed in `THIRD-PARTY-NOTICES.md`.
