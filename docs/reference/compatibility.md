# Compatibility

| Item | Requirement or behavior |
|---|---|
| Operating system | Windows 10 build 19041 or later, x64 |
| Runtime | .NET 10 Desktop Runtime, x64 |
| Verified model | ASUS TUF Gaming A16 FA607NUQ |
| Other laptop models | Monitoring only; hardware writes disabled |
| Tested BIOS | FA607NUQ.300; other versions log a warning |
| Fan, power and GPU controls | ASUS System Control Interface v3 (`ATKACPI`) |
| CPU temperature target and live Tctl sensor | PawnIO 2.x with a supported Ryzen Rembrandt CPU |
| NVIDIA telemetry | NVIDIA display driver |
| Linux and macOS | Unsupported |

Test machine: FA607NUQ, Ryzen 7 170, RTX 4050 Laptop, Windows 11 Pro 26H2 build 26300.9711, ASUS System Control Interface v3 3.1.73.0, PawnIO 2.2.0.0.

## Firmware write limits

| Control | Allowed values |
|---|---|
| Sustained CPU power | 15 to 45 W |
| CPU slow power | 15 to 54 W |
| CPU fast power | 15 to 65 W |
| CPU temperature target | 60 to 90 C |
| GPU boost | 5 to 20 W |
| GPU temperature target | 65 to 87 C |
| Fan curves | 8 points, ascending temperatures |
| Fan floor from 70 C | 30% |
| Fan floor from 80 C | 70% |
| Fan floor from 90 C | 100% |

`AsusHardware.Write` has an allow-list of ACPI endpoint IDs. The signed RyzenSMU module is verified with SHA-256 before loading. Firmware thermal protection remains active.
