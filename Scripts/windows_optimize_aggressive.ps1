[CmdletBinding()]
param(
    [switch]$DisableHyperVServices,
    [switch]$DisableHyperVFeatures,
    [switch]$DisableWindowsSearch,
    [switch]$AddDefenderDevExclusions
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

function Disable-RunEntry {
    param(
        [Parameter(Mandatory = $true)][string]$RegistryPath,
        [Parameter(Mandatory = $true)][string]$Name
    )

    try {
        Remove-ItemProperty -Path $RegistryPath -Name $Name -ErrorAction Stop
        Write-Host "Removido autostart: $Name [$RegistryPath]" -ForegroundColor Yellow
    }
    catch {
        Write-Host "Nao encontrado/autorizado: $Name [$RegistryPath]" -ForegroundColor DarkGray
    }
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

    Set-Service -Name $Name -StartupType $StartupType
    Write-Host "Startup do servico ajustado: $Name -> $StartupType" -ForegroundColor Yellow
}

function Disable-TaskSafe {
    param(
        [Parameter(Mandatory = $true)][string]$TaskPath,
        [Parameter(Mandatory = $true)][string]$TaskName
    )

    try {
        Disable-ScheduledTask -TaskPath $TaskPath -TaskName $TaskName -ErrorAction Stop | Out-Null
        Write-Host "Tarefa desabilitada: $TaskPath$TaskName" -ForegroundColor Yellow
    }
    catch {
        Write-Host "Nao foi possivel desabilitar ou tarefa ausente: $TaskPath$TaskName" -ForegroundColor DarkGray
    }
}

function Remove-StartupShortcutSafe {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (Test-Path $Path) {
        Remove-Item $Path -Force
        Write-Host "Atalho removido do Startup: $Path" -ForegroundColor Yellow
    }
    else {
        Write-Host "Atalho nao encontrado: $Path" -ForegroundColor DarkGray
    }
}

function Disable-WindowsFeatureSafe {
    param([Parameter(Mandatory = $true)][string]$FeatureName)

    try {
        $feature = Get-WindowsOptionalFeature -Online -FeatureName $FeatureName -ErrorAction Stop

        if ($feature.State -eq "Disabled") {
            Write-Host "Recurso ja desabilitado: $FeatureName" -ForegroundColor DarkGray
            return
        }

        Disable-WindowsOptionalFeature -Online -FeatureName $FeatureName -NoRestart -ErrorAction Stop | Out-Null
        Write-Host "Recurso marcado para desabilitacao: $FeatureName" -ForegroundColor Yellow
    }
    catch {
        Write-Host "Falha ao desabilitar recurso: $FeatureName" -ForegroundColor Red
        Write-Host "Detalhe: $($_.Exception.Message)" -ForegroundColor DarkYellow
        Write-Host "Sugestao: reinicie o Windows e execute novamente apenas a etapa de features." -ForegroundColor DarkYellow
    }
}

if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Execute este script em um PowerShell aberto como Administrador."
}

$backupRoot = "C:\Users\User\Desktop\otimizacao_admin_backup_$(Get-Date -Format 'yyyy-MM-dd_HHmmss')"
New-Item -ItemType Directory -Path $backupRoot -Force | Out-Null

Write-Step "Criando backups"
reg export "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" "$backupRoot\HKCU_Run.reg" /y | Out-Null
reg export "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" "$backupRoot\HKLM_Run.reg" /y | Out-Null
reg export "HKCU\Control Panel\Desktop" "$backupRoot\HKCU_Desktop.reg" /y | Out-Null
reg export "HKLM\SYSTEM\CurrentControlSet\Control" "$backupRoot\HKLM_Control.reg" /y | Out-Null
Get-Service | Select-Object Name, Status, StartType | ConvertTo-Json -Depth 3 | Set-Content "$backupRoot\services-before.json"
Get-ScheduledTask | Select-Object TaskPath, TaskName, State | ConvertTo-Json -Depth 3 | Set-Content "$backupRoot\tasks-before.json"

$startupFolder = "C:\Users\User\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup"

Write-Step "Limpando startup"
Disable-RunEntry -RegistryPath "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "OneDrive"
Disable-RunEntry -RegistryPath "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "Agent Tray"
Disable-RunEntry -RegistryPath "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" -Name "Everything"
Disable-RunEntry -RegistryPath "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" -Name "NvBackend"
Disable-RunEntry -RegistryPath "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" -Name "RTHDVCPL"
Remove-StartupShortcutSafe -Path (Join-Path $startupFolder "Everything.lnk")
Remove-StartupShortcutSafe -Path (Join-Path $startupFolder "Ollama.lnk")

Write-Step "Reduzindo servicos de desenvolvimento no boot"
Set-ServiceStartupSafe -Name "Apache24" -StartupType "Manual"
Set-ServiceStartupSafe -Name "MySQL" -StartupType "Manual"
Set-ServiceStartupSafe -Name "postgresql-x64-18" -StartupType "Manual"

Write-Step "Desabilitando tarefas e telemetria de baixo valor"
Disable-TaskSafe -TaskPath "\Microsoft\Windows\Application Experience\" -TaskName "Microsoft Compatibility Appraiser"
Disable-TaskSafe -TaskPath "\Microsoft\Windows\Application Experience\" -TaskName "ProgramDataUpdater"
Disable-TaskSafe -TaskPath "\Microsoft\Windows\Application Experience\" -TaskName "MareBackup"
Disable-TaskSafe -TaskPath "\Microsoft\Windows\Application Experience\" -TaskName "StartupAppTask"
Disable-TaskSafe -TaskPath "\Microsoft\Windows\Customer Experience Improvement Program\" -TaskName "Consolidator"
Disable-TaskSafe -TaskPath "\Microsoft\Windows\Customer Experience Improvement Program\" -TaskName "UsbCeip"
Disable-TaskSafe -TaskPath "\Microsoft\Windows\Customer Experience Improvement Program\" -TaskName "KernelCeipTask"
Disable-TaskSafe -TaskPath "\Microsoft\Windows\Maps\" -TaskName "MapsToastTask"
Disable-TaskSafe -TaskPath "\Microsoft\Windows\Maps\" -TaskName "MapsUpdateTask"
Disable-TaskSafe -TaskPath "\Microsoft\Windows\Feedback\Siuf\" -TaskName "DmClient"
Disable-TaskSafe -TaskPath "\Microsoft\Windows\Feedback\Siuf\" -TaskName "DmClientOnScenarioDownload"

Write-Step "Aplicando ajustes de responsividade via registro"
Set-RegistryValue -Path "HKCU:\Control Panel\Desktop" -Name "MenuShowDelay" -Value "20" -Type String
Set-RegistryValue -Path "HKCU:\Control Panel\Desktop" -Name "AutoEndTasks" -Value "1" -Type String
Set-RegistryValue -Path "HKCU:\Control Panel\Desktop" -Name "HungAppTimeout" -Value "3000" -Type String
Set-RegistryValue -Path "HKCU:\Control Panel\Desktop" -Name "WaitToKillAppTimeout" -Value "5000" -Type String
Set-RegistryValue -Path "HKLM:\SYSTEM\CurrentControlSet\Control" -Name "WaitToKillServiceTimeout" -Value "5000" -Type String
Set-RegistryValue -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize" -Name "StartupDelayInMSec" -Value 0 -Type DWord
Set-RegistryValue -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" -Name "SubscribedContent-338388Enabled" -Value 0 -Type DWord
Set-RegistryValue -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" -Name "SubscribedContent-338389Enabled" -Value 0 -Type DWord
Set-RegistryValue -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" -Name "SubscribedContent-353694Enabled" -Value 0 -Type DWord
Set-RegistryValue -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" -Name "SubscribedContent-353696Enabled" -Value 0 -Type DWord
Set-RegistryValue -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name "TaskbarAnimations" -Value 0 -Type DWord
Set-RegistryValue -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name "ListviewAlphaSelect" -Value 0 -Type DWord
Set-RegistryValue -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name "ListviewShadow" -Value 0 -Type DWord

Write-Step "Aplicando ajustes de politicas de privacidade/telemetria"
Set-RegistryValue -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\DataCollection" -Name "AllowTelemetry" -Value 0 -Type DWord
Set-RegistryValue -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\CloudContent" -Name "DisableWindowsConsumerFeatures" -Value 1 -Type DWord
Set-RegistryValue -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Privacy" -Name "TailoredExperiencesWithDiagnosticDataEnabled" -Value 0 -Type DWord
Set-RegistryValue -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo" -Name "Enabled" -Value 0 -Type DWord

Write-Step "Desabilitando servicos opcionais de virtualizacao se solicitado"
if ($DisableHyperVServices) {
    Set-ServiceStartupSafe -Name "vmms" -StartupType "Manual"
    Set-ServiceStartupSafe -Name "vmcompute" -StartupType "Manual"
    Set-ServiceStartupSafe -Name "HvHost" -StartupType "Manual"
}

if ($DisableHyperVFeatures) {
    Disable-WindowsFeatureSafe -FeatureName "Microsoft-Hyper-V-All"
    Disable-WindowsFeatureSafe -FeatureName "VirtualMachinePlatform"
    Disable-WindowsFeatureSafe -FeatureName "Containers-DisposableClientVM"
    Write-Host "Recursos Hyper-V/virtualizacao marcados para desabilitacao. Reinicie para concluir." -ForegroundColor Yellow
}

Write-Step "Desabilitando Windows Search se solicitado"
if ($DisableWindowsSearch) {
    Set-ServiceStartupSafe -Name "WSearch" -StartupType "Disabled"
}

Write-Step "Adicionando exclusoes de desenvolvimento no Defender se solicitado"
if ($AddDefenderDevExclusions) {
    $paths = @(
        "C:\Users\User\source\repos",
        "C:\Users\User\.nuget",
        "C:\Users\User\AppData\Local\Temp",
        "C:\Users\User\AppData\Roaming\npm-cache"
    )

    foreach ($path in $paths) {
        if (Test-Path $path) {
            Add-MpPreference -ExclusionPath $path -ErrorAction SilentlyContinue
            Write-Host "Exclusao adicionada no Defender: $path" -ForegroundColor Yellow
        }
    }
}

Write-Step "Resumo final"
Write-Host "Backup salvo em: $backupRoot" -ForegroundColor Green
Write-Host "Reinicie o Windows para consolidar todas as mudancas de startup, servicos e registro." -ForegroundColor Green
Write-Host ""
Write-Host "Sugestao de uso agressivo mas equilibrado:" -ForegroundColor Cyan
Write-Host "  powershell -ExecutionPolicy Bypass -File .\scripts\windows_optimize_aggressive.ps1 -DisableHyperVServices -DisableWindowsSearch -AddDefenderDevExclusions"
Write-Host ""
Write-Host "Uso extremo, apenas se voce nao depende de virtualizacao:" -ForegroundColor Cyan
Write-Host "  powershell -ExecutionPolicy Bypass -File .\scripts\windows_optimize_aggressive.ps1 -DisableHyperVServices -DisableHyperVFeatures -DisableWindowsSearch -AddDefenderDevExclusions"
