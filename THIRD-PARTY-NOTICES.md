# Third-party notices

TUF Control is licensed under the GNU General Public License v3.0 (see [LICENSE](LICENSE)).
It contains or depends on the following third-party work.

## Included in this repository

| Component | Files | Source | License |
|---|---|---|---|
| G-Helper ATKACPI transport and endpoint conventions | `src/AsusHardware.cs`, `src/Endpoints.cs` | [seerge/g-helper](https://github.com/seerge/g-helper) | GPL-3.0 |
| G-Helper Ryzen SMU access (thermal-limit subset) | `src/Thermal.cs` (adapted from `app/Pawn/RyzenSmu.cs`) | [seerge/g-helper](https://github.com/seerge/g-helper) | GPL-3.0 |
| PawnIO managed wrapper | `src/PawnIO/PawnIOWrapper.cs` (adapted from `app/Pawn/PawnIOWrapper.cs`) | [seerge/g-helper](https://github.com/seerge/g-helper) | GPL-3.0 |
| PawnIO RyzenSMU module (compiled, signed) | `src/TufControl.RyzenSMU.bin` | [namazso/PawnIO.Modules](https://github.com/namazso/PawnIO.Modules) (`RyzenSMU.p`) | LGPL-2.1 |

The RyzenSMU module is shipped unmodified, as the signed binary the PawnIO driver
requires. Its source code is available at the link above. The app checks its SHA-256
before loading it.

## Required at runtime, not included

| Component | Purpose | License |
|---|---|---|
| ASUS System Control Interface v3 (`ATKACPI` driver) | Fan, mode and power endpoints. Ships with Windows Update / MyASUS on ASUS laptops. | Proprietary (ASUS) |
| [PawnIO](https://github.com/namazso/PawnIO) driver | Optional. Needed only for the CPU temperature target. | GPL-2.0 |
| NVIDIA display driver (`nvapi64.dll`) | Optional. GPU temperature, clock and power readings. | Proprietary (NVIDIA) |
| [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) | Runs the app. | MIT |

## Trademarks

ASUS, TUF Gaming, ROG and Armoury Crate are trademarks of ASUSTeK Computer Inc.
AMD and Ryzen are trademarks of Advanced Micro Devices, Inc. NVIDIA and GeForce RTX
are trademarks of NVIDIA Corporation. TUF Control is an independent project and is
not affiliated with, endorsed by or sponsored by any of them.
