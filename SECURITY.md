# Security

TUF Control talks to firmware through the ASUS `ATKACPI` driver and, optionally, to the
CPU's SMU through the PawnIO driver. It runs elevated. Please treat bugs that could write
unsafe values to hardware, or let a non-admin process use TufControl to reach those
drivers, as security issues.

**Report them privately** through GitHub's *Report a vulnerability* button on the
Security tab, not in a public issue.

## Built-in safety rules

- Hardware writes are enabled only on verified models (`Platform.VerifiedModels`). Every other
  model runs read-only.
- Only an allow-list of ACPI endpoints can be written (`AsusHardware.Write`).
- Power limits, GPU boost and temperature targets are clamped to safe ranges in three layers:
  UI, profile validation and the hardware write layer.
- Fan curves are forced to at least 30 % from 70 °C, 70 % from 80 °C and 100 % from 90 °C.
- The CPU temperature target accepts 60–90 °C only. The SMU module is checked by SHA-256 and
  refuses to run on anything but Ryzen 6000 "Rembrandt" CPUs.
- The firmware's own thermal protection always stays active.
