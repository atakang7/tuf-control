# Removes TUF Control: logon task, early-boot service and the Program Files copy.
# Your profiles in %LOCALAPPDATA%\TufControl are kept unless you pass -RemoveSettings.
#Requires -RunAsAdministrator
param([switch]$RemoveSettings)
$ErrorActionPreference = 'Stop'

Stop-ScheduledTask -TaskName 'TufControl.Logon' -ErrorAction SilentlyContinue
Get-Process TufControl -ErrorAction SilentlyContinue | Stop-Process -Force
Unregister-ScheduledTask -TaskName 'TufControl.Logon' -Confirm:$false -ErrorAction SilentlyContinue
Unregister-ScheduledTask -TaskName 'TufControl.Boot' -Confirm:$false -ErrorAction SilentlyContinue
if (Get-Service TufControlBoot -ErrorAction SilentlyContinue) {
    Stop-Service TufControlBoot -Force -ErrorAction SilentlyContinue
    sc.exe delete TufControlBoot | Out-Null
}
Remove-Item (Join-Path $env:ProgramFiles 'TufControl') -Recurse -Force -ErrorAction SilentlyContinue
if ($RemoveSettings) { Remove-Item (Join-Path $env:LOCALAPPDATA 'TufControl') -Recurse -Force -ErrorAction SilentlyContinue }
Write-Host 'TUF Control removed. Firmware falls back to its own fan and power defaults on the next mode change or reboot.'
