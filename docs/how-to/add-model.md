# Add support for a laptop model

1. Run the published app on the new laptop. Unknown models start in read-only mode.
2. Export a hardware report from an elevated PowerShell window:

```powershell
.\TufControl.exe --diagnose model-diagnostics.json
```

3. Check the `Capabilities` values against the machine's actual firmware behavior.
4. After measuring the endpoint ranges and fan behavior on that model, update `Platform.VerifiedModels` and `Platform.VerifiedBios` in [Platform.cs](../../src/Platform.cs).
5. Apply each supported control and compare the result:

```powershell
.\TufControl.exe --verify
```

Include the diagnosis, measured ranges, BIOS version and `verify.json` in the pull request. Update the [compatibility reference](../reference/compatibility.md).
