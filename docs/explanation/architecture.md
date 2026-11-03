# Architecture

`Program.cs` selects the WPF UI, diagnostics, verifier or boot service. `MainWindow` owns profiles and telemetry. `Controller` applies settings to the hardware adapters.

ASUS firmware controls use the `ATKACPI` driver through `AsusHardware`. CPU thermal targets use `Thermal`, PawnIO and the Ryzen SMU. The live Tctl sensor is read through the SMU because the ASUS firmware temperature value is filtered and can remain flat for minutes.

Only the model IDs in `Platform.VerifiedModels` can write firmware settings. Other laptops can still report sensor values. `Profile.Validate` and the write layer constrain power and fan values. See the [hardware limits](../reference/compatibility.md).

`WindowsPower` writes processor settings to the active Windows power plan. The app keeps the current plan and restores defaults when a subsequent profile stops managing those values. A drift watch checks for changes made outside the app.

The optional early-boot service applies the startup profile before user sign-in. A scheduled task starts the UI after sign-in. The service and UI use the same profile file.

GPU mode, CPU temperature targets and Windows processor values have readback. Fan curves and firmware power limits depend on an accepted driver write response.
