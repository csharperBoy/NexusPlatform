# راهنمای اجرا 
# ایتدا در powerShell به محل فایل برویم
# cd C:\Users\Mahar\source\repos\csharperBoy\NexusPlatform
# داخل PowerShell دستور زیر رو با پارامتر های صحیح اجرا کنید
#.\Export-SolutionTree.ps1 -SolutionPath "C:\Users\Mahar\source\repos\csharperBoy\NexusPlatform" -OutputPath "C:\Users\Mahar\source\repos\csharperBoy\NexusPlatform\TreeOutput.txt"
# اگر خطای دسترسی داد با دستور زیر دسترسی رو اوکی کن بعد اجرا کن
#Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass # موقت
#Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned # دائم

param (
    [string]$RootPath = "C:\Users\Mahar\source\repos\csharperBoy\NexusPlatform",
    [string]$OutputPath = "$RootPath\DirectoryTree.txt"
)

# لیست پوشه‌هایی که باید نادیده گرفته بشن
$excludedFolders = @("bin", "obj", ".vs", "node_modules", "packages", "TestResults", "Debug", "Release")

function Write-Tree {
    param (
        [string]$Path,
        [int]$Level
    )

    $indent = "│   " * $Level
    $name = Split-Path $Path -Leaf
    Add-Content -Path $OutputPath -Value "$indent$name"

    # فایل‌ها
    Get-ChildItem -Path $Path -File | ForEach-Object {
        Add-Content -Path $OutputPath -Value "$indent│   $($_.Name)"
    }

    # فولدرها (با فیلتر)
    Get-ChildItem -Path $Path -Directory | Where-Object {
        $excludedFolders -notcontains $_.Name
    } | ForEach-Object {
        Write-Tree -Path $_.FullName -Level ($Level + 1)
    }
}

# پاک کردن خروجی قبلی
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath
}

# شروع از ریشه
Write-Tree -Path $RootPath -Level 0

Write-Host "✅ ساختار تمیز پروژه ذخیره شد در: $OutputPath"
