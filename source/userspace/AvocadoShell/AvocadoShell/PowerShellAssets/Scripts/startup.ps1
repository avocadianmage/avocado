# Import formatting file.
$exePath = Split-Path ([System.Diagnostics.Process]::GetCurrentProcess().Path)
Update-FormatData -PrependPath `
	$exePath/PowerShellAssets/Formatting/Avocado.format.ps1xml
Remove-Variable exePath

# Import user profile.
$PROFILE = Join-Path `
	([Environment]::GetFolderPath("MyDocuments")) `
	"WindowsPowerShell\profile.ps1"
if (Test-Path $PROFILE) { . $PROFILE }