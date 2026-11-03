<p align="center">
  <img src="docs/banner.svg" alt="TUF Control" width="100%">
</p>

Fan curves, power limits and GPU mode for the ASUS TUF Gaming A16 (FA607NUQ), without Armoury Crate.

## Results

Averages measured on my TUF A16 FA607NUQ over 7 days, stock firmware vs. my TUF Control profiles.

| | Stock firmware | TUF Control |
|---|---|---|
| **Plugged in: CPU temp** | 59.1 °C | **55.7 °C** |
| **Plugged in: CPU fan** | 2,907 rpm | **2,372 rpm (−18 %)** |
| **On battery: CPU fan** | 5,185 rpm | **771 rpm (−85 %)** |
| **On battery: power drain** | 14.4 W | **10.9 W (−24 %)** |

Recorded time: plugged in 5.6 h stock / 73.7 h TUF Control, on battery 0.5 h / 1.5 h.
Trade-off: with the 15 W plugged-in profile the average CPU clock is 2.8 GHz instead of 3.3 GHz.
Raise the limit in a second profile for gaming.

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
