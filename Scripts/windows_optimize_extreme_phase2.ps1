[CmdletBinding()]
param(
    [switch]$RemoveAppPackages,
    [switch]$DisableWSLAndWSA,
    [switch]$DisableSyncAndConnectedDevices,
    [switch]$DisableIISAndSqlWriter,
    [switch]$DisableOpenShellAutoStart
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Set-RegistryValue {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)]$Value,
        [Parameter(Mandatory = $true)][Microsoft.Win32.RegistryValueKind]$Type
    )

    if (-not (Test-Path $Path)) {
        New-Item -Path $Path -Force | Out-Null
    }

    New-ItemProperty -Path $Path -Name $Name -Value $Value -PropertyType $Type -Force | Out-Null
}

function Set-ServiceStartupSafe {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][ValidateSet("Automatic", "Manual", "Disabled")][string]$StartupType
    )

    $service = Get-Service -Name $Name -ErrorAction SilentlyContinue
    if ($null -eq $service) {
        Write-Host "Servico nao encontrado: $Name" -ForegroundColor DarkGray
        return
    }

    try {
        if ($service.Status -eq "Running" -and $StartupType -eq "Disabled") {
            Stop-Service -Name $Name -Force -ErrorAction SilentlyContinue
        }

        Set-Service -Name $Name -StartupType $StartupType
        Write-Host "Servico ajustado: $Name -> $StartupType" -ForegroundColor Yellow
    }
    catch {
        Write-Host "Falha ao ajustar servico: $Name" -ForegroundColor Red
    }
}

function Disable-PerUserServiceSafe {
    param([Parameter(Mandatory = $true)][string]$ServiceName)

    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($null -eq $service) {
        Write-Host "Servico por usuario nao encontrado: $ServiceName" -ForegroundColor DarkGray
        return
    }

    $registryPath = "HKLM:\SYSTEM\CurrentControlSet\Services\$ServiceName"

    try {
        if (Test-Path $registryPath) {
            New-ItemProperty -Path $registryPath -Name "Start" -Value 4 -PropertyType DWord -Force | Out-Null
        }

        if ($service.Status -eq "Running") {
            Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
        }

        Write-Host "Servico por usuario desabilitado via registro: $ServiceName" -ForegroundColor Yellow
    }
    catch {
        Write-Host "Falha ao desabilitar servico por usuario: $ServiceName" -ForegroundColor Red
    }
}

function Remove-AppxPackageSafe {
    param([Parameter(Mandatory = $true)][string]$PackageName)

    try {
        $packages = Get-AppxPackage -Name $PackageName -ErrorAction SilentlyContinue

        if (-not $packages) {
            Write-Host "Pacote nao encontrado: $PackageName" -ForegroundColor DarkGray
            return
        }

        foreach ($pkg in $packages) {
            Remove-AppxPackage -Package $pkg.PackageFullName -ErrorAction SilentlyContinue
            Write-Host "Pacote removido: $($pkg.Name)" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "Falha ao remover pacote: $PackageName" -ForegroundColor Red
    }
}

if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Execute este script em um PowerShell aberto como Administrador."
}

$backupRoot = "C:\Users\User\Desktop\otimizacao_extrema_backup_$(Get-Date -Format 'yyyy-MM-dd_HHmmss')"
New-Item -ItemType Directory -Path $backupRoot -Force | Out-Null

Write-Step "Criando backups da fase extrema"
reg export "HKCU\Control Panel\Desktop" "$backupRoot\HKCU_Desktop.reg" /y | Out-Null
reg export "HKLM\SYSTEM\CurrentControlSet\Control" "$backupRoot\HKLM_Control.reg" /y | Out-Null
reg export "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" "$backupRoot\HKLM_Run.reg" /y | Out-Null
Get-Service | Select-Object Name, Status, StartType | ConvertTo-Json -Depth 3 | Set-Content "$backupRoot\services-before.json"
Get-AppxPackage | Select-Object Name, PackageFullName | ConvertTo-Json -Depth 3 | Set-Content "$backupRoot\appx-before.json"

Write-Step "Apertando responsividade e tempos de encerramento ao extremo"
Set-RegistryValue -Path "HKCU:\Control Panel\Desktop" -Name "MenuShowDelay" -Value "0" -Type String
Set-RegistryValue -Path "HKCU:\Control Panel\Desktop" -Name "AutoEndTasks" -Value "1" -Type String
Set-RegistryValue -Path "HKCU:\Control Panel\Desktop" -Name "HungAppTimeout" -Value "2000" -Type String
Set-RegistryValue -Path "HKCU:\Control Panel\Desktop" -Name "WaitToKillAppTimeout" -Value "2000" -Type String
Set-RegistryValue -Path "HKLM:\SYSTEM\CurrentControlSet\Control" -Name "WaitToKillServiceTimeout" -Value "2000" -Type String
Set-RegistryValue -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects" -Name "VisualFXSetting" -Value 2 -Type DWord
Set-RegistryValue -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name "TaskbarAnimations" -Value 0 -Type DWord
Set-RegistryValue -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name "IconsOnly" -Value 1 -Type DWord

Write-Step "Desligando servicos opcionais de sincronizacao/dispositivos conectados"
if ($DisableSyncAndConnectedDevices) {
    Get-Service | Where-Object {
        $_.Name -like "OneSyncSvc_*" -or
        $_.Name -like "CDPUserSvc_*" -or
        $_.Name -like "cbdhsvc_*"
    } | ForEach-Object {
        Disable-PerUserServiceSafe -ServiceName $_.Name
    }

    Set-ServiceStartupSafe -Name "CDPSvc" -StartupType "Disabled"
    Set-ServiceStartupSafe -Name "InventorySvc" -StartupType "Disabled"
}

Write-Step "Desligando IIS e SQLWriter se solicitado"
if ($DisableIISAndSqlWriter) {
    Set-ServiceStartupSafe -Name "W3SVC" -StartupType "Disabled"
    Set-ServiceStartupSafe -Name "SQLWriter" -StartupType "Disabled"
    Set-ServiceStartupSafe -Name "Stereo Service" -StartupType "Disabled"
}

Write-Step "Desligando WSL/WSA se solicitado"
if ($DisableWSLAndWSA) {
    Set-ServiceStartupSafe -Name "WSLService" -StartupType "Disabled"
    Set-ServiceStartupSafe -Name "WSAIFabricSvc" -StartupType "Disabled"

    try {
        Disable-WindowsOptionalFeature -Online -FeatureName "Microsoft-Windows-Subsystem-Linux" -NoRestart -ErrorAction SilentlyContinue | Out-Null
        Disable-WindowsOptionalFeature -Online -FeatureName "VirtualMachinePlatform" -NoRestart -ErrorAction SilentlyContinue | Out-Null
        Write-Host "WSL/VirtualMachinePlatform marcados para desabilitacao, se presentes." -ForegroundColor Yellow
    }
    catch {
        Write-Host "Falha ao desabilitar features de WSL/virtualizacao. Reinicie e tente novamente se necessario." -ForegroundColor DarkYellow
    }
}

Write-Step "Removendo autostart do Open-Shell se solicitado"
if ($DisableOpenShellAutoStart) {
    try {
        Remove-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" -Name "Open-Shell Start Menu" -ErrorAction SilentlyContinue
        Write-Host "Autostart removido: Open-Shell Start Menu" -ForegroundColor Yellow
    }
    catch {
        Write-Host "Falha ao remover autostart do Open-Shell." -ForegroundColor DarkYellow
    }
}

Write-Step "Removendo pacotes UWP opcionais se solicitado"
if ($RemoveAppPackages) {
    $packages = @(
        "Microsoft.Windows.DevHome",
        "Microsoft.OutlookForWindows",
        "DropboxInc.Dropbox",
        "5319275A.WhatsAppDesktop",
        "MicrosoftCorporationII.WindowsSubsystemForLinux"
    )

    foreach ($pkg in $packages) {
        Remove-AppxPackageSafe -PackageName $pkg
    }
}

Write-Step "Observacao sobre GPU"
Write-Host "Sua GPU e uma NVIDIA GeForce 9600 GT. Ela e muito antiga para varias aceleracoes modernas do Windows." -ForegroundColor DarkYellow
Write-Host "Nao existe como forcar aceleracao global significativa para VS2022/compilacao via GPU nessa maquina." -ForegroundColor DarkYellow
Write-Host "O maior ganho continua vindo de cortar servicos, sincronizacao, apps residentes e indexacao." -ForegroundColor DarkYellow

Write-Step "Resumo final"
Write-Host "Backup salvo em: $backupRoot" -ForegroundColor Green
Write-Host "Reinicie o Windows para consolidar as mudancas extremas." -ForegroundColor Green
Write-Host ""
Write-Host "Uso sugerido da fase extrema:" -ForegroundColor Cyan
Write-Host "  powershell -ExecutionPolicy Bypass -File .\scripts\windows_optimize_extreme_phase2.ps1 -DisableWSLAndWSA -DisableSyncAndConnectedDevices -DisableIISAndSqlWriter -DisableOpenShellAutoStart -RemoveAppPackages"
