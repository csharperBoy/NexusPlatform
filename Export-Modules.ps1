$root = ""   # مسیر ریشه پروژه‌ات
$output = "exports"  # جایی که خروجی‌ها ذخیره میشه

# اگر پوشه خروجی وجود نداشت بساز
if (!(Test-Path $output)) {
    New-Item -ItemType Directory -Path $output | Out-Null
}

# برای هر ماژول سطح اول
Get-ChildItem -Path $root -Directory | ForEach-Object {
    $moduleName = $_.Name
    $outFile = Join-Path $output "$moduleName.txt"

    # همه فایل‌های cs داخل اون ماژول
    Get-ChildItem -Path $_.FullName -Recurse -Filter *.cs |
        ForEach-Object {
            "//// FILE: $($_.FullName)" | Out-File $outFile -Append -Encoding utf8
            Get-Content $_.FullName | Out-File $outFile -Append -Encoding utf8
            "`n" | Out-File $outFile -Append -Encoding utf8
        }

    Write-Host "✅ Exported $moduleName to $outFile"
}
