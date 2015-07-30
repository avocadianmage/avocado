Set-Location $PSScriptRoot
Get-ChildItem ..\AvocadoLib\* -Filter *.dll | foreach { .\gacutil.exe -i $_ }
