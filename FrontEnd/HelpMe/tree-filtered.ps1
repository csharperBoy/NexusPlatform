$path = "D:\868\Projects\NexusPlatform\FrontEnd"
$outputFile = "D:\output.txt"

function Get-Tree($dir, $indent = "") {
    $items = Get-ChildItem $dir -Force | Where-Object {
        $_.Name -notin @("node_modules", "dist")
    }

    foreach ($item in $items) {
        $line = "$indent|-- $($item.Name)"
        Add-Content -Path $outputFile -Value $line

        if ($item.PSIsContainer) {
            Get-Tree $item.FullName "$indent|   "
        }
    }
}

# پاک کردن فایل خروجی قبلی
Remove-Item $outputFile -ErrorAction SilentlyContinue

# شروع ساخت درخت
Add-Content -Path $outputFile -Value $path
Get-Tree $path


# نحوه اجرا
# در powerShell با دسترسی ادمین برو به آدرس روت پروژه
#cd D:\868\Projects\NexusPlatform\FrontEnd\HelpMe
# .\tree-filtered.ps1
