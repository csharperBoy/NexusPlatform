<#
.SYNOPSIS
    ابزار جامع صادرسازی ساختار و کد پروژه (بک‌اند + فرانت‌اند)
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
    [string]$ExportFolder = "exports",
    [string[]]$ExcludedFolders = @("bin","obj",".vs","node_modules","packages","TestResults","Debug","Release"),
    [switch]$SkipTree,
    [switch]$SkipAllProjects,
    [switch]$SkipPerProject,
    [switch]$SkipModules,
    [switch]$SkipFront
)

if ([string]::IsNullOrWhiteSpace($RootPath)) { $RootPath = $PSScriptRoot }
$RootPath = $RootPath.Trim()

# پوشه خروجی
$ExportPath = Join-Path $RootPath $ExportFolder
if (!(Test-Path $ExportPath)) {
    New-Item -ItemType Directory -Path $ExportPath | Out-Null
}

# exports هم از درخت حذف شود تا خودش را نشان ندهد
if ($ExcludedFolders -notcontains $ExportFolder) { $ExcludedFolders += $ExportFolder }

# خروجی درخت حالا داخل exports است
$TreeOutput = Join-Path $ExportPath "DirectoryTree.txt"

# ---------- تابع کمکی: فایل‌های مهم بک‌اند (.cs و appsettings*.json) ----------
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

# ---------- تابع کمکی: فایل‌های سورس فرانت‌اند ----------
function Get-FrontEndFiles {
    param([string]$Path)
    $includeExt = @(".ts", ".tsx", ".js", ".jsx", ".css", ".scss", ".sass", ".less", ".html", ".json")
    $excludeNames = @("package-lock.json", "pnpm-lock.yaml", "yarn.lock", "tsconfig.tsbuildinfo", ".DS_Store")
    Get-ChildItem -Path $Path -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object {
            $includeExt -contains $_.Extension.ToLower() -and
            $excludeNames -notcontains $_.Name -and
            $_.FullName -notmatch "\\node_modules\\" -and
            $_.FullName -notmatch "\\dist\\" -and
            $_.FullName -notmatch "\\build\\"
        }
}

# ---------- تابع بازگشتی درخت (سبک کلاسیک) ----------
function Export-DirectoryTree {
    param(
        [string]$Path,
        [string]$Prefix = "",
        [string]$OutFile
    )

    $folders = @(Get-ChildItem -Path $Path -Directory -ErrorAction SilentlyContinue |
        Where-Object { $ExcludedFolders -notcontains $_.Name } |
        Sort-Object Name)
    $files = @(Get-ChildItem -Path $Path -File -ErrorAction SilentlyContinue |
        Sort-Object Name)

    # ترتیب کلاسیک: اول فولدرها، بعد فایل‌ها
    $all = @($folders) + @($files)
    $count = $all.Count

    for ($i = 0; $i -lt $count; $i++) {
        $item = $all[$i]
        $isLast = ($i -eq $count - 1)
        $branch = if ($isLast) { "└── " } else { "├── " }
        Add-Content -Path $OutFile -Value ("{0}{1}{2}" -f $Prefix, $branch, $item.Name) -Encoding utf8

        if ($item.PSIsContainer) {
            $newPrefix = $Prefix + $(if ($isLast) { "    " } else { "│   " })
            Export-DirectoryTree -Path $item.FullName -Prefix $newPrefix -OutFile $OutFile
        }
    }
}

# ---------- 1) درخت ----------
if (-not $SkipTree) {
    if (Test-Path $TreeOutput) { Remove-Item $TreeOutput -Force }
    $rootName = Split-Path $RootPath -Leaf
    if ([string]::IsNullOrEmpty($rootName)) { $rootName = $RootPath }
    Add-Content -Path $TreeOutput -Value $rootName -Encoding utf8
    Export-DirectoryTree -Path $RootPath -Prefix "" -OutFile $TreeOutput
    Write-Host "[OK] Directory tree saved: $TreeOutput"
}

# ---------- لیست پروژه‌های بک‌اند (فولدرهای سطح اول) ----------
$projects = @(Get-ChildItem -Path $RootPath -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -ne $ExportFolder -and $ExcludedFolders -notcontains $_.Name })

# ---------- 2) همه پروژه‌ها در AllProjects.cs ----------
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
    Write-Host "[OK] All projects merged: $outFile"
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
        Write-Host "[OK] $($proj.Name) exported: $outFile"
    }
}

# ---------- 4) گروه‌بندی ماژول‌های بک‌اند (Core.cs، HR.cs، ...) ----------
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
        Write-Host "[OK] Backend module $moduleName exported: $outFile"
    }
}

# ---------- 5) ماژول‌های فرانت‌اند (ModuleName.Front.txt) ----------
if (-not $SkipFront) {
    $frontModulesPath = Join-Path $RootPath "FrontEnd\src\modules"
    if (Test-Path $frontModulesPath) {
        $frontModules = @(Get-ChildItem -Path $frontModulesPath -Directory -ErrorAction SilentlyContinue)
        foreach ($mod in $frontModules) {
            $moduleName = $mod.Name
            $outFile = Join-Path $ExportPath "$moduleName.Front.txt"
            "" | Out-File $outFile -Encoding utf8

            "//// FRONTEND MODULE: $moduleName" | Out-File $outFile -Append -Encoding utf8
            "`n" | Out-File $outFile -Append -Encoding utf8

            Get-FrontEndFiles -Path $mod.FullName |
                Sort-Object FullName |
                ForEach-Object {
                    $relPath = $_.FullName.Substring($mod.FullName.Length).TrimStart('\', '/')
                    "//// FILE: $relPath" | Out-File $outFile -Append -Encoding utf8
                    Get-Content $_.FullName -ErrorAction SilentlyContinue | Out-File $outFile -Append -Encoding utf8
                    "`n" | Out-File $outFile -Append -Encoding utf8
                }
            Write-Host "[OK] Frontend module $moduleName exported: $outFile"
        }
    } else {
        Write-Host "[SKIP] FrontEnd/src/modules not found: $frontModulesPath"
    }
}

Write-Host ""
Write-Host "Done. Output folder: $ExportPath"