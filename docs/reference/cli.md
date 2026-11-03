# CLI and local files

Run `TufControl.exe` from a published build. Windows and administrator requirements are in [compatibility](compatibility.md).

## Commands

| Arguments | Result |
|---|---|
| `--read-only` | Open the UI with hardware writes disabled |
| `--verify [profile]` | Compare a saved profile with readable firmware state; write `verify.json` |
| `--diagnose [file]` | Write a platform and sensor report to the named JSON file |
| `--render <folder> [--lang en]` | Write `screen-start.png` and `screen-0.png` through `screen-6.png` |
| `--software-render` | Force CPU rendering for the UI |
| `--startup` | Start hidden for the logon task |
| `--wait-instance` | Wait up to 8 seconds for the UI instance mutex |
| `--service --settings <file>` | Run the early-boot Windows service |
| `--boot --settings <file>` | Run the early-boot profile application directly |

`--verify` returns 0 if every readable item matches, 1 on mismatch. Missing render folder returns 2. Platform blockers return 4.

Without `[file]`, `--diagnose` writes `diagnostics.json` beside the executable.

## Files

The default state directory is `%LOCALAPPDATA%\TufControl`.

| Path in state directory | Content |
|---|---|
| `profiles.json` | Profiles and application settings |
| `profiles.json.bak` | Previous saved settings |
| `events.log` | Application events |
| `verify.json` | Last firmware comparison |
| `history/metrics-YYYY-MM-DD.csv` | Samples, written at 15 second intervals |
| `history/segments.jsonl` | Completed history segments |
| `history/current-segment.json` | Current history segment |
