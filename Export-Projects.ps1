# راهنمای اجرا 
# ایتدا در powerShell به محل فایل برویم
# cd C:\Users\Mahar\source\repos\csharperBoy\NexusPlatform
# داخل PowerShell دستور زیر رو با پارامتر های صحیح اجرا کنید
#.\Export-Projects.ps1 
# یا
#.\Export-Projects.ps1 -SolutionPath "C:\Users\Mahar\source\repos\csharperBoy\NexusPlatform" -OutputPath "C:\Users\Mahar\source\repos\csharperBoy\NexusPlatform\TreeOutput.txt"
# اگر خطای دسترسی داد با دستور زیر دسترسی رو اوکی کن بعد اجرا کن
#Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass # موقت
#Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned # دائم



param(
    [string]$Root = "",        # مسیر ریشه پروژه
    [string]$Output = "exports"   # مسیر خروجی
)

# اگر پوشه خروجی وجود نداشت بساز
if (!(Test-Path $Output)) {
    New-Item -ItemType Directory -Path $Output | Out-Null
}

# برای هر فولدر سطح اول (به جز پوشه خروجی)
$projects = Get-ChildItem -Path $Root -Directory |
    Where-Object { $_.Name -ne (Split-Path $Output -Leaf) }

foreach ($proj in $projects) {
    $projName = $proj.Name
    $outFile = Join-Path $Output "$projName.cs"

    # یک‌بار فایل خروجی را خالی کن
    "" | Out-File $outFile -Encoding utf8

    # هدر پروژه برای خوانایی
    "//// PROJECT: $projName" | Out-File $outFile -Append -Encoding utf8
    "`n" | Out-File $outFile -Append -Encoding utf8

    # فایل‌های cs و فقط jsonهایی که با appsettings شروع می‌شوند
    Get-ChildItem -Path $proj.FullName -Recurse |
        Where-Object {
            $_.Extension -eq ".cs" -or
            ($_.Extension -eq ".json" -and $_.BaseName -like "appsettings*")
        } |
        Sort-Object FullName |
        ForEach-Object {
            "//// FILE: $($_.FullName)" | Out-File $outFile -Append -Encoding utf8
            Get-Content $_.FullName | Out-File $outFile -Append -Encoding utf8
            "`n" | Out-File $outFile -Append -Encoding utf8
        }

    Write-Host "✅ Exported $projName to $outFile"
}
