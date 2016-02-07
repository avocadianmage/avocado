Set-Location $PSScriptRoot
Get-ChildItem ..\Avocado\* -Filter *.dll | foreach { .\gacutil.exe -i $_ }
