<#
.SYNOPSIS
    ابزار جامع صادرسازی ساختار و کد پروژه (بک‌اند + فرانت‌اند) - اصلاح انکودینگ
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


# اگر خطای دسترسی داد با دستور زیر دسترسی رو اوکی کن بعد اجرا کن
#Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass # موقت
#Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned # دائم


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

if ($ExcludedFolders -notcontains $ExportFolder) { $ExcludedFolders += $ExportFolder }

$TreeOutput = Join-Path $ExportPath "DirectoryTree.txt"

# ---------- انکودینگ استاندارد برای همه نوشتن‌ها (UTF-8 با BOM) ----------
$script:Utf8Bom = New-Object System.Text.UTF8Encoding($true)

# ---------- توابع کمکی انکودینگ ----------

# خواندن فایل با تشخیص خودکار BOM، پیش‌فرض UTF-8
function Read-FileContent {
    param([string]$Path)
    if (-not (Test-Path $Path)) { return "" }
    try {
        return [System.IO.File]::ReadAllText($Path)
    } catch {
        Write-Warning "خواندن ناموفق: $Path"
        return ""
    }
}

# نوشتن کل محتوا با UTF-8 BOM (یک‌جا، بدون append)
function Write-FileContent {
    param([string]$Path, [string]$Content)
    [System.IO.File]::WriteAllText($Path, $Content, $script:Utf8Bom)
}

# ---------- فیلتر فایل‌های بک‌اند ----------
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

# ---------- فیلتر فایل‌های فرانت‌اند ----------
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

# ---------- درخت بازگشتی (Builder-based، بدون I/O درونی) ----------
function Add-TreeLines {
    param(
        [string]$Path,
        [string]$Prefix,
        [System.Text.StringBuilder]$Sb
    )
    $folders = @(Get-ChildItem -Path $Path -Directory -ErrorAction SilentlyContinue |
        Where-Object { $ExcludedFolders -notcontains $_.Name } |
        Sort-Object Name)
    $files = @(Get-ChildItem -Path $Path -File -ErrorAction SilentlyContinue |
        Sort-Object Name)

    $all = @($folders) + @($files)
    $count = $all.Count

    for ($i = 0; $i -lt $count; $i++) {
        $item = $all[$i]
        $isLast = ($i -eq $count - 1)
        $branch = if ($isLast) { "└── " } else { "├── " }
        [void]$Sb.AppendLine("$Prefix$branch$($item.Name)")

        if ($item.PSIsContainer) {
            $newPrefix = $Prefix + $(if ($isLast) { "    " } else { "│   " })
            Add-TreeLines -Path $item.FullName -Prefix $newPrefix -Sb $Sb
        }
    }
}

# ---------- ساخت محتوا برای یک مجموعه فایل (با هدر PROJECT/MODULE/FILE) ----------
function Build-ContentBlock {
    param(
        [object[]]$Files,
        [string]$HeaderLine = "",
        [string]$BasePath = ""   # اگر داده شود، مسیرها نسبی می‌شوند
    )
    $sb = New-Object System.Text.StringBuilder
    if ($HeaderLine) {
        [void]$sb.AppendLine($HeaderLine)
        [void]$sb.AppendLine()
    }
    foreach ($f in $Files) {
        $displayPath = if ($BasePath) { $f.FullName.Substring($BasePath.Length).TrimStart('\', '/') } else { $f.FullName }
        [void]$sb.AppendLine("//// FILE: $displayPath")
        $text = Read-FileContent -Path $f.FullName
        if ($text.Length -gt 0) { [void]$sb.Append($text) }
        if (-not $text.EndsWith("`n")) { [void]$sb.AppendLine() }
        [void]$sb.AppendLine()
    }
    return $sb.ToString()
}

# ---------- 1) درخت ----------
if (-not $SkipTree) {
    $sb = New-Object System.Text.StringBuilder
    $rootName = Split-Path $RootPath -Leaf
    if ([string]::IsNullOrEmpty($rootName)) { $rootName = $RootPath }
    [void]$sb.AppendLine($rootName)
    Add-TreeLines -Path $RootPath -Prefix "" -Sb $sb
    Write-FileContent -Path $TreeOutput -Content $sb.ToString()
    Write-Host "[OK] Directory tree saved: $TreeOutput"
}

# ---------- لیست پروژه‌های بک‌اند ----------
$projects = @(Get-ChildItem -Path $RootPath -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -ne $ExportFolder -and $ExcludedFolders -notcontains $_.Name })

# ---------- 2) همه پروژه‌ها در AllProjects.cs ----------
if (-not $SkipAllProjects) {
    $outFile = Join-Path $ExportPath "AllProjects.cs"
    $sb = New-Object System.Text.StringBuilder

    foreach ($proj in $projects) {
        [void]$sb.AppendLine("//// PROJECT: $($proj.Name)")
        [void]$sb.AppendLine()
        $files = @(Get-ImportantFiles -Path $proj.FullName | Sort-Object FullName)
        foreach ($f in $files) {
            [void]$sb.AppendLine("//// FILE: $($f.FullName)")
            $text = Read-FileContent -Path $f.FullName
            if ($text.Length -gt 0) { [void]$sb.Append($text) }
            if (-not $text.EndsWith("`n")) { [void]$sb.AppendLine() }
            [void]$sb.AppendLine()
        }
    }
    Write-FileContent -Path $outFile -Content $sb.ToString()
    Write-Host "[OK] All projects merged: $outFile"
}

# ---------- 3) هر پروژه جداگانه ----------
if (-not $SkipPerProject) {
    foreach ($proj in $projects) {
        $outFile = Join-Path $ExportPath "$($proj.Name).cs"
        $files = @(Get-ImportantFiles -Path $proj.FullName | Sort-Object FullName)
        $content = Build-ContentBlock -Files $files -HeaderLine "//// PROJECT: $($proj.Name)"
        Write-FileContent -Path $outFile -Content $content
        Write-Host "[OK] $($proj.Name) exported: $outFile"
    }
}

# ---------- 4) گروه‌بندی ماژول‌های بک‌اند ----------
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
        $sb = New-Object System.Text.StringBuilder
        foreach ($proj in $group.Group) {
            [void]$sb.AppendLine("//// PROJECT: $($proj.Name)")
            [void]$sb.AppendLine()
            $files = @(Get-ImportantFiles -Path $proj.Path | Sort-Object FullName)
            foreach ($f in $files) {
                [void]$sb.AppendLine("//// FILE: $($f.FullName)")
                $text = Read-FileContent -Path $f.FullName
                if ($text.Length -gt 0) { [void]$sb.Append($text) }
                if (-not $text.EndsWith("`n")) { [void]$sb.AppendLine() }
                [void]$sb.AppendLine()
            }
        }
        Write-FileContent -Path $outFile -Content $sb.ToString()
        Write-Host "[OK] Backend module $moduleName exported: $outFile"
    }
}

# ---------- 5) ماژول‌های فرانت‌اند ----------
if (-not $SkipFront) {
    $frontModulesPath = Join-Path $RootPath "FrontEnd\src\modules"
    if (Test-Path $frontModulesPath) {
        $frontModules = @(Get-ChildItem -Path $frontModulesPath -Directory -ErrorAction SilentlyContinue)
        foreach ($mod in $frontModules) {
            $moduleName = $mod.Name
            $outFile = Join-Path $ExportPath "$moduleName.Front.txt"
            $files = @(Get-FrontEndFiles -Path $mod.FullName | Sort-Object FullName)
            # مسیرها نسبت به پوشه ماژول
            $content = Build-ContentBlock `
                -Files $files `
                -HeaderLine "//// FRONTEND MODULE: $moduleName" `
                -BasePath $mod.FullName
            Write-FileContent -Path $outFile -Content $content
            Write-Host "[OK] Frontend module $moduleName exported: $outFile"
        }
    } else {
        Write-Host "[SKIP] FrontEnd/src/modules not found: $frontModulesPath"
    }
}

Write-Host ""
Write-Host "Done. Output folder: $ExportPath"