# Import formatting file.
Update-FormatData -PrependPath $PSHOME\Avocado.format.ps1xml

# Import the user profile.
$PROFILE = Join-Path `
	([Environment]::GetFolderPath("MyDocuments")) `
	"WindowsPowerShell\profile.ps1"
if (Test-Path $PROFILE) { . $PROFILE }

# Communicate back to the core host (C# code).
function RunNativeCommand
{
	Param([Parameter(Mandatory = $true)][string]$Command)

	Write-Verbose "avocado:$Command" -Verbose
}

# Returns true if PowerShell code is executing in a remote session.
function IsRemoteSession { (Get-Host).Name -eq "ServerRemoteHost" }