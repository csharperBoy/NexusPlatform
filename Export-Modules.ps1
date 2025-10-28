# راهنمای اجرا 
# ایتدا در powerShell به محل فایل برویم
# cd C:\Users\Mahar\source\repos\csharperBoy\NexusPlatform
# داخل PowerShell دستور زیر رو با پارامتر های صحیح اجرا کنید
#.\Export-Modules.ps1 -SolutionPath "C:\Users\Mahar\source\repos\csharperBoy\NexusPlatform" -OutputPath "C:\Users\Mahar\source\repos\csharperBoy\NexusPlatform\TreeOutput.txt"
# اگر خطای دسترسی داد با دستور زیر دسترسی رو اوکی کن بعد اجرا کن
#Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass # موقت
#Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned # دائم

param(
    [string]$Root = "src",        # مسیر ریشه پروژه
    [string]$Output = "exports"   # مسیر خروجی
)

# اگر پوشه خروجی وجود نداشت بساز
if (!(Test-Path $Output)) {
    New-Item -ItemType Directory -Path $Output | Out-Null
}

# برای هر ماژول سطح اول (Auth, UserManagement, Caching, ...)
Get-ChildItem -Path $Root -Directory | ForEach-Object {
    $moduleName = $_.Name.Split('.')[0]   # فقط بخش اول اسم (Auth از Auth.Application)
    $outFile = Join-Path $Output "$moduleName.cs"

    # همه فایل‌های cs داخل همه‌ی زیرپروژه‌های این ماژول
    Get-ChildItem -Path $_.FullName -Recurse -Filter *.cs |
        ForEach-Object {
            "//// FILE: $($_.FullName)" | Out-File $outFile -Append -Encoding utf8
            Get-Content $_.FullName | Out-File $outFile -Append -Encoding utf8
            "`n" | Out-File $outFile -Append -Encoding utf8
        }

    Write-Host "✅ Exported $moduleName to $outFile"
}
