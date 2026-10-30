<p align="center">
  <img src="docs/banner.svg" alt="TUF Control" width="100%">
</p>

<p align="center">
  <a href="https://github.com/atakang7/tuf-control/releases/latest"><img alt="Release" src="https://img.shields.io/github/v/release/atakang7/tuf-control?style=flat-square&color=ff2d55"></a>
  <a href="LICENSE"><img alt="License: GPL-3.0" src="https://img.shields.io/badge/license-GPL--3.0-blue?style=flat-square"></a>
  <img alt="Windows 10/11 x64" src="https://img.shields.io/badge/Windows%2010%20%7C%2011-x64-0078D6?style=flat-square&logo=windows">
</p>

Set your own fan curves, power limits and GPU mode on an ASUS TUF laptop, without Armoury Crate.

**Works on:** ASUS TUF Gaming A16 **FA607NUQ**, Windows 10/11 x64. Other models open read-only.

## Install

1. Install the [.NET 10 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/10.0).
2. Download **`TufControl-win-x64.zip`** from [Releases](https://github.com/atakang7/tuf-control/releases/latest) and extract it.
3. Open **Terminal (Admin)** in the extracted folder and run:

   ```powershell
   Get-ChildItem | Unblock-File
   .\Install.ps1
   ```

Done. TUF Control starts now and at every sign-in, in the system tray.

*Optional:* install [PawnIO](https://github.com/namazso/PawnIO/releases) to unlock the CPU temperature target.

## Results

**Your settings are applied, and they stay applied.** The installer's last step checks that:

```text
verify exit code: 0 (0 = everything readable matches the profile)
```

If Armoury Crate or an ASUS service changes something, TUF Control puts it back within 30 seconds.

**One dashboard for temperatures, fans, power and battery:**

<img src="docs/screenshots/dashboard.png" alt="Dashboard">

**Your own fan curve and temperature targets:**

<img src="docs/screenshots/cooling.png" alt="Cooling">

**CPU and GPU power limits, and Eco mode (RTX off on battery):**

<img src="docs/screenshots/power.png" alt="Power">

**Profiles switch by themselves**: plugged in, on battery, or when a game starts.

## Uninstall

```powershell
.\Uninstall.ps1
```

---

[Requirements, command line, how it works](docs/details.md) · [Report a bug](https://github.com/atakang7/tuf-control/issues/new?template=bug_report.yml) · [GPL-3.0](LICENSE) · Based on [G-Helper](https://github.com/seerge/g-helper) ([notices](THIRD-PARTY-NOTICES.md)) · Not affiliated with ASUS.
