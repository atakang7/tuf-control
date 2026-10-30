# Installs the published build to C:\Program Files\TufControl as the single copy,
# points the logon task there, removes old copies, restarts and verifies.
# Run as administrator after: dotnet publish src\TufControl.csproj -c Release -r win-x64 --self-contained false -o publish
#Requires -RunAsAdministrator
$ErrorActionPreference = 'Stop'
$src = Join-Path $PSScriptRoot 'publish'
$dst = 'C:\Program Files\TufControl'
$exe = Join-Path $dst 'TufControl.exe'
$data = Join-Path $env:LOCALAPPDATA 'TufControl'
$old = @('C:\Users\atakan\Documents\Codex\2026-10-22\dostum-bu-bilgisayarda-asus-2-i\outputs\TufControl')

if (-not (Test-Path (Join-Path $src 'TufControl.exe'))) { throw "No build in $src; run dotnet publish first." }

Write-Host '1. Stopping TufControl'
Stop-ScheduledTask -TaskName 'TufControl.Logon' -ErrorAction SilentlyContinue
Get-Process TufControl -ErrorAction SilentlyContinue | Stop-Process -Force
if ((Get-Service TufControlBoot -ErrorAction SilentlyContinue).Status -eq 'Running') { Stop-Service TufControlBoot -Force }
Start-Sleep 2

Write-Host "2. Installing to $dst"
New-Item -ItemType Directory -Force $dst | Out-Null
Get-ChildItem $dst -File | Remove-Item -Force
Copy-Item (Join-Path $src '*') $dst -Recurse -Force
foreach ($f in Get-ChildItem $src -File) {
    if ((Get-FileHash $f.FullName).Hash -ne (Get-FileHash (Join-Path $dst $f.Name)).Hash) { throw "Copy mismatch: $($f.Name)" }
}

Write-Host '3. Pointing logon task and boot service at the installed copy'
Set-ScheduledTask -TaskName 'TufControl.Logon' -Action (New-ScheduledTaskAction -Execute $exe -Argument '--startup' -WorkingDirectory $dst) | Out-Null
sc.exe config TufControlBoot binPath= "`"$exe`" --service --settings `"$data\profiles.json`"" | Out-Null

Write-Host '4. Removing old copies'
foreach ($o in $old) { if (Test-Path $o) { Remove-Item $o -Recurse -Force; Write-Host "   removed $o" } }

Write-Host '5. Starting TufControl'
Start-ScheduledTask -TaskName 'TufControl.Logon'
Start-Sleep 20

Write-Host '6. Checking'
$copies = @(Get-ChildItem C:\Users\atakan, 'C:\Program Files', 'C:\Program Files (x86)' -Recurse -Filter TufControl.exe -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notlike 'C:\Users\atakan\TufControl\*' })
Write-Host "   installed copies: $($copies.FullName -join ', ')"
Write-Host "   task:    $((Get-ScheduledTask TufControl.Logon).Actions.Execute)"
Write-Host "   service: $((Get-CimInstance Win32_Service -Filter "Name='TufControlBoot'").PathName)"
Write-Host "   running: $((Get-Process TufControl -ErrorAction SilentlyContinue | ForEach-Object { $_.Path }) -join ', ')"
$v = Start-Process $exe -ArgumentList '--verify' -Wait -PassThru
Write-Host "   verify exit code: $($v.ExitCode) (0 = matches)"
Get-Content (Join-Path $data 'verify.json')
Write-Host '--- events.log'
Get-Content (Join-Path $data 'events.log') -Tail 15
