# راهنمای اجرا 
# ایتدا در powerShell به محل فایل برویم
# cd C:\Users\Mahar\source\repos\csharperBoy\NexusPlatform
# داخل PowerShell دستور زیر رو با پارامتر های صحیح اجرا کنید
#.\Export-Modules.ps1 
# یا
#.\Export-Modules.ps1 -SolutionPath "C:\Users\Mahar\source\repos\csharperBoy\NexusPlatform" -OutputPath "C:\Users\Mahar\source\repos\csharperBoy\NexusPlatform\TreeOutput.txt"
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

# پوشه خروجی را از لیست ماژول‌ها حذف کن
$modules = Get-ChildItem -Path $Root -Directory |
    Where-Object { $_.Name -ne (Split-Path $Output -Leaf) } |
    ForEach-Object {
        [PSCustomObject]@{
            Base = $_.Name.Split('.')[0]  # نام پایه ماژول (مثلاً Core از Core.Application)
            Name = $_.Name                # نام کامل فولدر (مثلاً Core.Application)
            Path = $_.FullName            # مسیر کامل
        }
    } |
    Group-Object Base                    # گروه‌بندی همه زیرپروژه‌های هر ماژول

foreach ($group in $modules) {
    $moduleName = $group.Name
    $outFile = Join-Path $Output "$moduleName.cs"

    # یک‌بار فایل خروجی را خالی کن
    "" | Out-File $outFile -Encoding utf8

    # برای هر زیرپروژه‌ی این ماژول
    foreach ($proj in $group.Group) {

        # هدر پروژه برای خوانایی
        "//// PROJECT: $($proj.Name)" | Out-File $outFile -Append -Encoding utf8
        "`n" | Out-File $outFile -Append -Encoding utf8

        # فایل‌های cs و فقط jsonهایی که با appsettings شروع می‌شوند
        Get-ChildItem -Path $proj.Path -Recurse |
            Where-Object {
               ($_.Extension -eq ".cs" -or
             ($_.Extension -eq ".json" -and $_.BaseName -like "appsettings*")) -and
            ($_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\")
            } |
            Sort-Object FullName |                # برای خروجی پایدار و قابل مقایسه
            ForEach-Object {
                "//// FILE: $($_.FullName)" | Out-File $outFile -Append -Encoding utf8
                Get-Content $_.FullName | Out-File $outFile -Append -Encoding utf8
                "`n" | Out-File $outFile -Append -Encoding utf8
            }
    }

    Write-Host "✅ Exported $moduleName to $outFile"
}
