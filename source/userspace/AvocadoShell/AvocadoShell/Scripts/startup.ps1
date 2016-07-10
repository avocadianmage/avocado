# Import formatting file.
Update-FormatData -PrependPath $PSHOME\Avocado.format.ps1xml

# Import user profile.
$PROFILE = Join-Path `
	([Environment]::GetFolderPath("MyDocuments")) `
	"WindowsPowerShell\profile.ps1"
if (Test-Path $PROFILE) { . $PROFILE }