param(
    [ValidateSet("Install", "Uninstall")]
    [string]$Mode = "Install"
)

$ErrorActionPreference = "Stop"

$ModDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Locate game root (either ..\.. when inside mods\ThaiLang, or parent if extracted elsewhere)
$GameDir = $null
$Candidates = @(
    (Split-Path -Parent (Split-Path -Parent $ModDir)),
    (Split-Path -Parent $ModDir),
    (Get-Location).Path
)
foreach ($c in $Candidates) {
    if ($c -and (Test-Path (Join-Path $c "Tavern Master_Data\ScriptingAssemblies.json"))) {
        $GameDir = $c
        break
    }
}

if (-not $GameDir) {
    Write-Host "[ERROR] ไม่พบโฟลเดอร์เกม Tavern Master (ไม่พบ Tavern Master_Data\ScriptingAssemblies.json)" -ForegroundColor Red
    Write-Host "กรุณาวางโฟลเดอร์ ThaiLang ไว้ใน <โฟลเดอร์เกม>\mods\ThaiLang แล้วลองใหม่อีกครั้ง"
    exit 1
}

$Running = Get-Process -Name "Tavern Master" -ErrorAction SilentlyContinue
if ($Running) {
    Write-Host "[ERROR] กรุณาปิดเกม Tavern Master ก่อนติดตั้งหรือถอนการติดตั้งม็อด" -ForegroundColor Red
    exit 1
}

$DataDir = Join-Path $GameDir "Tavern Master_Data"
$ManagedDir = Join-Path $DataDir "Managed"
$BackupDir = Join-Path $ModDir "backup"
$SaPath = Join-Path $DataDir "ScriptingAssemblies.json"
$RiPath = Join-Path $DataDir "RuntimeInitializeOnLoads.json"
$ResPath = Join-Path $DataDir "resources.assets"
$ResGz = Join-Path $ModDir "resources.assets.gz"
$DllSrc = Join-Path $ModDir "ThaiMod.dll"
$DllDst = Join-Path $ManagedDir "ThaiMod.dll"

if ($Mode -eq "Install") {
    if (-not (Test-Path $DllSrc)) {
        Write-Host "[ERROR] ไม่พบไฟล์ $DllSrc" -ForegroundColor Red
        exit 1
    }

    # Ensure mod files are also in <GameDir>\mods\ThaiLang if user extracted elsewhere
    $TargetModDir = Join-Path $GameDir "mods\ThaiLang"
    if ((Resolve-Path $ModDir).Path -ne (Resolve-Path $TargetModDir -ErrorAction SilentlyContinue).Path) {
        New-Item -ItemType Directory -Path $TargetModDir -Force | Out-Null
        Copy-Item (Join-Path $ModDir "*") $TargetModDir -Recurse -Force
        $BackupDir = Join-Path $TargetModDir "backup"
    }

    New-Item -ItemType Directory -Path $BackupDir -Force | Out-Null
    if (-not (Test-Path (Join-Path $BackupDir "ScriptingAssemblies.json"))) {
        Copy-Item $SaPath (Join-Path $BackupDir "ScriptingAssemblies.json") -Force
    }
    if (-not (Test-Path (Join-Path $BackupDir "RuntimeInitializeOnLoads.json"))) {
        Copy-Item $RiPath (Join-Path $BackupDir "RuntimeInitializeOnLoads.json") -Force
    }
    if (-not (Test-Path (Join-Path $BackupDir "resources.assets"))) {
        Copy-Item $ResPath (Join-Path $BackupDir "resources.assets") -Force
    }

    if (Test-Path $ResGz) {
        $inStream = [System.IO.File]::OpenRead($ResGz)
        $gzStream = New-Object System.IO.Compression.GzipStream($inStream, [System.IO.Compression.CompressionMode]::Decompress)
        $outStream = [System.IO.File]::Create($ResPath)
        $gzStream.CopyTo($outStream)
        $outStream.Close(); $gzStream.Close(); $inStream.Close()
    }

    Copy-Item $DllSrc $DllDst -Force

    # Register in ScriptingAssemblies.json
    $sa = Get-Content (Join-Path $BackupDir "ScriptingAssemblies.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $names = [System.Collections.Generic.List[string]]::new()
    $types = [System.Collections.Generic.List[int]]::new()
    foreach ($n in $sa.names) { $names.Add([string]$n) }
    foreach ($t in $sa.types) { $types.Add([int]$t) }
    if (-not $names.Contains("ThaiMod.dll")) {
        $names.Add("ThaiMod.dll")
        $types.Add(16)
    }
    $saOut = [PSCustomObject]@{ names = $names; types = $types } | ConvertTo-Json -Compress
    [System.IO.File]::WriteAllText($SaPath, $saOut, [System.Text.UTF8Encoding]::new($false))

    # Register in RuntimeInitializeOnLoads.json
    $ri = Get-Content (Join-Path $BackupDir "RuntimeInitializeOnLoads.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $roots = [System.Collections.Generic.List[object]]::new()
    foreach ($r in $ri.root) {
        if ($r.assemblyName -ne "ThaiMod") { $roots.Add($r) }
    }
    $roots.Add([PSCustomObject]@{
        assemblyName = "ThaiMod"
        nameSpace    = "ThaiMod"
        className    = "Bootstrap"
        methodName   = "Init"
        loadTypes    = 4
        isUnityClass = $false
    })
    $riOut = [PSCustomObject]@{ root = $roots } | ConvertTo-Json -Depth 5 -Compress
    [System.IO.File]::WriteAllText($RiPath, $riOut, [System.Text.UTF8Encoding]::new($false))

    Write-Host "[OK] ติดตั้งม็อดภาษาไทย Tavern Master สำเร็จ!" -ForegroundColor Green
    Write-Host "เปิดเกมแล้วไปที่ Settings > Language > เลือก 'ภาษาไทย (Thai)' ได้เลยครับ"
}
elseif ($Mode -eq "Uninstall") {
    $SaBak = Join-Path $BackupDir "ScriptingAssemblies.json"
    $RiBak = Join-Path $BackupDir "RuntimeInitializeOnLoads.json"
    $ResBak = Join-Path $BackupDir "resources.assets"
    if (Test-Path $SaBak) { Copy-Item $SaBak $SaPath -Force }
    if (Test-Path $RiBak) { Copy-Item $RiBak $RiPath -Force }
    if (Test-Path $ResBak) { Copy-Item $ResBak $ResPath -Force }
    if (Test-Path $DllDst) { Remove-Item $DllDst -Force }

    Write-Host "[OK] ถอนการติดตั้งม็อดภาษาไทย Tavern Master สำเร็จ!" -ForegroundColor Green
}
