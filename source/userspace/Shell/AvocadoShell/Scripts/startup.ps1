# Import the user profile.
$PROFILE = Join-Path `
	([Environment]::GetFolderPath("MyDocuments")) `
	"WindowsPowerShell\profile.ps1"
if (Test-Path $PROFILE) { . $PROFILE }