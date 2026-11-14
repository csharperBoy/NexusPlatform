<#
.SYNOPSIS
    ابزار جامع صادرسازی ساختار و کد پروژه
.DESCRIPTION
    4 کار را انجام می‌دهد:
      1) ساختار درختی پروژه را در فایل متنی ذخیره می‌کند
      2) همه پروژه‌ها را در یک فایل AllProjects.cs تجمیع می‌کند
      3) هر پروژه را در فایل جداگانه صادر می‌کند
      4) ماژول‌ها را بر اساس نام پایه (Base) گروه‌بندی و صادر می‌کند
.EXAMPLE
    .\Export-All.ps1
.EXAMPLE
    .\Export-All.ps1 -RootPath "C:\MyProject"
.EXAMPLE
    .\Export-All.ps1 -SkipTree -SkipModules

    نحوه استفاده
powershell
# اجرای کامل (هر ۴ کار)
.\Export-All.ps1 -RootPath "C:\Users\Mahar\source\repos\csharperBoy\NexusPlatform"

# یا فقط اجرای کامل با پیش‌فرض محل اسکریپت
.\Export-All.ps1

# فقط درخت (بقیه را رد کن)
.\Export-All.ps1 -SkipAllProjects -SkipPerProject -SkipModules

# همه به جز ماژول‌ها
.\Export-All.ps1 -SkipModules
#>

param(
    [string]$RootPath = "",
    [string]$TreeOutput = "",
    [string]$ExportFolder = "exports",
    [string[]]$ExcludedFolders = @("bin","obj",".vs","node_modules","packages","TestResults","Debug","Release"),
    [switch]$SkipTree,
    [switch]$SkipAllProjects,
    [switch]$SkipPerProject,
    [switch]$SkipModules
)

# اگر RootPath داده نشده بود، از محل اسکریپت استفاده کن
if ([string]::IsNullOrWhiteSpace($RootPath)) {
    $RootPath = $PSScriptRoot
}
$RootPath = $RootPath.Trim()  # حذف فاصله اضافی احتمالی

if ([string]::IsNullOrWhiteSpace($TreeOutput)) {
    $TreeOutput = Join-Path $RootPath "DirectoryTree.txt"
}

$ExportPath = Join-Path $RootPath $ExportFolder
if (!(Test-Path $ExportPath)) {
    New-Item -ItemType Directory -Path $ExportPath | Out-Null
}

# ---------- تابع کمکی: فیلتر فایل‌های مهم (.cs و appsettings*.json) ----------
function Get-ImportantFiles {
    param([string]$Path)
    Get-ChildItem -Path $Path -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object {
            ($_.Extension -eq ".cs" -or
             ($_.Extension -eq ".json" -and $_.BaseName -like "appsettings*")) -and
            $_.FullName -notmatch "\\bin\\" -and
            $_.FullName -notmatch "\\obj\\" -and
            $_.FullName -notmatch "\\node_modules\\"
        }
}

# ---------- 1) درخت پوشه‌ها ----------
function Export-DirectoryTree {
    param([string]$Path, [string]$OutFile, [int]$Level = 0)

    $indent = "│   " * $Level
    $name = Split-Path $Path -Leaf
    if ([string]::IsNullOrEmpty($name)) { $name = $Path }
    Add-Content -Path $OutFile -Value "$indent$name"

    Get-ChildItem -Path $Path -File -ErrorAction SilentlyContinue | ForEach-Object {
        Add-Content -Path $OutFile -Value "$indent│   $($_.Name)"
    }

    Get-ChildItem -Path $Path -Directory -ErrorAction SilentlyContinue |
        Where-Object { $ExcludedFolders -notcontains $_.Name } |
        ForEach-Object {
            Export-DirectoryTree -Path $_.FullName -OutFile $OutFile -Level ($Level + 1)
        }
}

if (-not $SkipTree) {
    if (Test-Path $TreeOutput) { Remove-Item $TreeOutput -Force }
    Export-DirectoryTree -Path $RootPath -OutFile $TreeOutput -Level 0
    Write-Host "✅ درخت پروژه ذخیره شد: $TreeOutput"
}

# لیست پروژه‌ها (فولدرهای سطح اول، به جز پوشه خروجی و پوشه‌های مستثنی)
$projects = Get-ChildItem -Path $RootPath -Directory |
    Where-Object {
        $_.Name -ne $ExportFolder -and
        $ExcludedFolders -notcontains $_.Name
    }

# ---------- 2) همه پروژه‌ها در یک فایل ----------
if (-not $SkipAllProjects) {
    $outFile = Join-Path $ExportPath "AllProjects.cs"
    "" | Out-File $outFile -Encoding utf8

    foreach ($proj in $projects) {
        "//// PROJECT: $($proj.Name)" | Out-File $outFile -Append -Encoding utf8
        "`n" | Out-File $outFile -Append -Encoding utf8

        Get-ImportantFiles -Path $proj.FullName |
            Sort-Object FullName |
            ForEach-Object {
                "//// FILE: $($_.FullName)" | Out-File $outFile -Append -Encoding utf8
                Get-Content $_.FullName -ErrorAction SilentlyContinue | Out-File $outFile -Append -Encoding utf8
                "`n" | Out-File $outFile -Append -Encoding utf8
            }
    }
    Write-Host "✅ همه پروژه‌ها تجمیع شدند: $outFile"
}

# ---------- 3) هر پروژه جداگانه ----------
if (-not $SkipPerProject) {
    foreach ($proj in $projects) {
        $outFile = Join-Path $ExportPath "$($proj.Name).cs"
        "" | Out-File $outFile -Encoding utf8

        "//// PROJECT: $($proj.Name)" | Out-File $outFile -Append -Encoding utf8
        "`n" | Out-File $outFile -Append -Encoding utf8

        Get-ImportantFiles -Path $proj.FullName |
            Sort-Object FullName |
            ForEach-Object {
                "//// FILE: $($_.FullName)" | Out-File $outFile -Append -Encoding utf8
                Get-Content $_.FullName -ErrorAction SilentlyContinue | Out-File $outFile -Append -Encoding utf8
                "`n" | Out-File $outFile -Append -Encoding utf8
            }
        Write-Host "✅ $($proj.Name) صادر شد: $outFile"
    }
}

# ---------- 4) گروه‌بندی ماژول‌ها ----------
if (-not $SkipModules) {
    $modules = $projects |
        ForEach-Object {
            [PSCustomObject]@{
                Base = $_.Name.Split('.')[0]
                Name = $_.Name
                Path = $_.FullName
            }
        } | Group-Object Base

    foreach ($group in $modules) {
        $moduleName = $group.Name
        $outFile = Join-Path $ExportPath "$moduleName.cs"
        "" | Out-File $outFile -Encoding utf8

        foreach ($proj in $group.Group) {
            "//// PROJECT: $($proj.Name)" | Out-File $outFile -Append -Encoding utf8
            "`n" | Out-File $outFile -Append -Encoding utf8

            Get-ImportantFiles -Path $proj.Path |
                Sort-Object FullName |
                ForEach-Object {
                    "//// FILE: $($_.FullName)" | Out-File $outFile -Append -Encoding utf8
                    Get-Content $_.FullName -ErrorAction SilentlyContinue | Out-File $outFile -Append -Encoding utf8
                    "`n" | Out-File $outFile -Append -Encoding utf8
                }
        }
        Write-Host "✅ ماژول $moduleName صادر شد: $outFile"
    }
}

Write-Host ""
Write-Host "🎉 همه عملیات انجام شد. پوشه خروجی: $ExportPath"