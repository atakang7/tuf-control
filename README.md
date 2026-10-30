<p align="center">
  <img src="docs/banner.svg" alt="TUF Control" width="100%">
</p>

Fan curves, power limits and GPU mode for the ASUS TUF Gaming A16 (FA607NUQ), without Armoury Crate.

## Results

Measured on my TUF A16 FA607NUQ over 7 days (25,330 samples), stock firmware vs. my TUF Control profiles.

| | Stock firmware | TUF Control |
|---|---|---|
| **Battery, idle: fan speed** | 5,200 rpm | **0 rpm (silent)** |
| **Battery, idle: power drain** | 14.6 W | **9.0 W (−38 %)** |
| **Plugged in, light load: CPU temp** | 69 °C | **59 °C** |
| **Plugged in, light load: fan speed** | 3,000 rpm | **2,300 rpm** |
| **Plugged in, idle: fan speed** | 2,900 rpm | **2,300 rpm** |

Medians within the same CPU-load band. Trade-off: with the 15 W plugged-in profile, light-load
CPU clock is 2.8 GHz instead of 3.6 GHz. Raise the limit in a second profile for gaming.

<img src="docs/screenshots/dashboard.png" alt="TUF Control dashboard">

## Install

1. Install the [.NET 10 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/10.0).
2. Download and extract [`TufControl-win-x64.zip`](https://github.com/atakang7/tuf-control/releases/latest).
3. In that folder, open **Terminal (Admin)** and run:
   ```powershell
   Get-ChildItem | Unblock-File; .\Install.ps1
   ```

It starts at every sign-in. Remove it with `.\Uninstall.ps1`.

---

[Details](docs/details.md) · [GPL-3.0](LICENSE) · Based on [G-Helper](https://github.com/seerge/g-helper) · Not affiliated with ASUS
