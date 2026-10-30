# TUF Control installer.
# Installs one copy to C:\Program Files\TufControl, points the logon task and the
# early-boot service at it, restarts the app and verifies the applied profile.
#
# Usage (administrator PowerShell):
#   from a release zip:  .\Install.ps1
#   from source:         dotnet publish src\TufControl.csproj -c Release -o publish; .\Install.ps1
#Requires -RunAsAdministrator
param([string]$Source)
$ErrorActionPreference = 'Stop'

if (-not $Source) {
    $Source = if (Test-Path (Join-Path $PSScriptRoot 'TufControl.exe')) { $PSScriptRoot } else { Join-Path $PSScriptRoot 'publish' }
}
$dst = Join-Path $env:ProgramFiles 'TufControl'
$exe = Join-Path $dst 'TufControl.exe'
$data = Join-Path $env:LOCALAPPDATA 'TufControl'

if (-not (Test-Path (Join-Path $Source 'TufControl.exe'))) { throw "No build found in $Source." }
if (-not [Environment]::Is64BitOperatingSystem) { throw 'TUF Control needs 64-bit Windows.' }
if ([Environment]::OSVersion.Version.Build -lt 19041) { throw 'TUF Control needs Windows 10 2004 (build 19041) or newer.' }

Write-Host '1. Stopping TufControl'
Stop-ScheduledTask -TaskName 'TufControl.Logon' -ErrorAction SilentlyContinue
Get-Process TufControl -ErrorAction SilentlyContinue | Stop-Process -Force
if ((Get-Service TufControlBoot -ErrorAction SilentlyContinue).Status -eq 'Running') { Stop-Service TufControlBoot -Force }
Start-Sleep 2

Write-Host "2. Installing to $dst"
New-Item -ItemType Directory -Force $dst | Out-Null
if ((Resolve-Path $Source).Path -ne $dst) {
    Get-ChildItem $dst -File | Remove-Item -Force
    Get-ChildItem $Source -File | Where-Object { $_.Extension -ne '.ps1' } | Copy-Item -Destination $dst -Force
    foreach ($f in Get-ChildItem $Source -File | Where-Object { $_.Extension -ne '.ps1' }) {
        if ((Get-FileHash $f.FullName).Hash -ne (Get-FileHash (Join-Path $dst $f.Name)).Hash) { throw "Copy mismatch: $($f.Name)" }
    }
}

Write-Host '3. Registering the logon task (and repointing the boot service if it exists)'
$action = New-ScheduledTaskAction -Execute $exe -Argument '--startup' -WorkingDirectory $dst
if (Get-ScheduledTask -TaskName 'TufControl.Logon' -ErrorAction SilentlyContinue) {
    Set-ScheduledTask -TaskName 'TufControl.Logon' -Action $action | Out-Null
} else {
    $user = [Security.Principal.WindowsIdentity]::GetCurrent().Name
    $trigger = New-ScheduledTaskTrigger -AtLogOn -User $user
    $trigger.Delay = 'PT5S'
    $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit 0 -StartWhenAvailable
    $principal = New-ScheduledTaskPrincipal -UserId $user -LogonType Interactive -RunLevel Highest
    Register-ScheduledTask -TaskName 'TufControl.Logon' -Action $action -Trigger $trigger -Settings $settings -Principal $principal | Out-Null
}
if (Get-Service TufControlBoot -ErrorAction SilentlyContinue) {
    sc.exe config TufControlBoot binPath= "`"$exe`" --service --settings `"$data\profiles.json`"" | Out-Null
}

Write-Host '4. Starting TufControl'
Start-ScheduledTask -TaskName 'TufControl.Logon'
Start-Sleep 20

Write-Host '5. Checking'
$copies = @(Get-ChildItem $env:ProgramFiles, ${env:ProgramFiles(x86)}, $env:USERPROFILE -Recurse -Filter TufControl.exe -ErrorAction SilentlyContinue |
    Where-Object { $_.DirectoryName -ne $dst -and $_.FullName -notlike "$PSScriptRoot*" -and $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*" })
if ($copies.Count) { Write-Warning "Other TufControl copies found (remove them): $($copies.FullName -join ', ')" }
Write-Host "   task:    $((Get-ScheduledTask TufControl.Logon).Actions.Execute)"
Write-Host "   running: $((Get-Process TufControl -ErrorAction SilentlyContinue | ForEach-Object { $_.Path }) -join ', ')"
$v = Start-Process $exe -ArgumentList '--verify' -Wait -PassThru
Write-Host "   verify exit code: $($v.ExitCode) (0 = everything readable matches the profile)"
if (Test-Path (Join-Path $data 'verify.json')) { Get-Content (Join-Path $data 'verify.json') }
