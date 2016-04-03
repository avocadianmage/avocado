# Import the user profile.
$profile = Join-Path `
	([Environment]::GetFolderPath("MyDocuments")) `
	"WindowsPowerShell\profile.ps1"
if (Test-Path $profile) { . $profile }