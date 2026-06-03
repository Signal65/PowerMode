# PowerMode

This program allows you to adjust the Windows "power mode" from the command line, including setting AC (plugged in) and DC (battery) power modes independently.

## Requirements

 - Windows 10, version 1709 or later (AC/DC independent modes require Windows 11)
 - .NET 8.0 runtime

## Use

From the command line:
|Command|Result|
|--|--|
|`PowerMode.exe`|Report the current effective, AC, and DC power modes|
|`PowerMode.exe --json`|Report the current effective mode as a single line of JSON (for scripts/automation)|
|`PowerMode.exe <mode>`|Set the power mode|
|`PowerMode.exe /ac <mode>`|Set the AC (plugged in) power mode|
|`PowerMode.exe /dc <mode>`|Set the DC (battery) power mode|
|`PowerMode.exe /ac <mode> /dc <mode>`|Set AC and DC power modes independently|
|`PowerMode.exe <GUID>`|Set the power mode to a custom GUID|

Available modes: `BestPowerEfficiency`, `Balanced`, `BestPerformance`

The `/ac` and `/dc` flags can also be specified as `--ac`/`--dc` or `-ac`/`-dc`.

## How it works

When called with a single mode argument, the program uses `PowerSetActiveOverlayScheme` from `powrprof.dll` to set the active power overlay scheme.

When called with `/ac` or `/dc` flags, the program uses the documented Windows APIs [`PowerSetUserConfiguredACPowerMode`](https://learn.microsoft.com/en-us/windows/win32/api/powrprof/nf-powrprof-powersetuserconfiguredacpowermode) and [`PowerSetUserConfiguredDCPowerMode`](https://learn.microsoft.com/en-us/windows/win32/api/powrprof/nf-powrprof-powersetuserconfigureddcpowermode) to set AC and DC power modes independently.

When called with no arguments, the program reports the effective power mode along with the user-configured AC and DC modes using `PowerGetEffectiveOverlayScheme`, `PowerGetUserConfiguredACPowerMode`, and `PowerGetUserConfiguredDCPowerMode`.

Adding `--json` (also `-json` or `/json`) to the report invocation emits a single line of machine-readable JSON for the currently-effective power mode instead of the human-readable lines, for use by scripts and automation:

```json
{"name":"BestPerformance","guid":"ded574b5-45a0-4f42-8737-46345c09c238"}
```

`name` is the canonical mode token (`BestPowerEfficiency`, `Balanced`, `BestPerformance`, or `Unknown`); `guid` is the raw scheme GUID. The flag only affects the report (no-mode) invocation.

All API methods return zero on success and non-zero on failure.

### Power mode GUIDs

|Mode|GUID|
|--|--|
|Best Power Efficiency|`961cc777-2547-4f9d-8174-7d86181b8a7a`|
|Balanced|`00000000-0000-0000-0000-000000000000`|
|Best Performance|`ded574b5-45a0-4f42-8737-46345c09c238`|

## Building

Run `build.bat` to compile and produce a zip file for distribution. Requires the .NET 8.0 SDK.
