# Install a release

1. On x64 Windows 10 build 19041 or newer, install the [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0).
2. Download and extract `TufControl-win-x64.zip` from the [latest release](https://github.com/atakang7/tuf-control/releases/latest).
3. Open PowerShell as administrator in the extracted folder:

```powershell
Get-ChildItem | Unblock-File
.\Install.ps1
```

The installer copies the app to `%ProgramFiles%\TufControl`, registers a logon task and verifies the selected profile.

## Remove the app

In an elevated PowerShell window in the extracted release folder:

```powershell
.\Uninstall.ps1
```

Profiles remain in `%LOCALAPPDATA%\TufControl`. Pass `-RemoveSettings` to delete them.
