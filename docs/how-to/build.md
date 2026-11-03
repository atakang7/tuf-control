# Build from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) on x64 Windows.

1. Build the WPF project:

```powershell
dotnet build src/TufControl.csproj -c Release
```

2. Publish a framework-dependent build:

```powershell
dotnet publish src/TufControl.csproj -c Release --self-contained false -o publish
```

3. In an elevated PowerShell window, render screenshots to check that the UI starts:

```powershell
.\publish\TufControl.exe --render .\rendered --lang en
```

To install that build, run `./Install.ps1` from the repository root as administrator.
