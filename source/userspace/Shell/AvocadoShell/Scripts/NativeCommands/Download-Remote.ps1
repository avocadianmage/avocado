# Download a file from a remote PowerShell session.
function Download-Remote
{
	Param([Parameter(Mandatory = $true)][string]$File)

	# Throw error if this was called from a remote session.
	if (-not (IsRemoteSession)) { Throw "No remote session found." }

	# Verify the file exists.
	$filePath = Get-Item $File
	if (-not $?) { Return }

	# Run native command.
	RunNativeCommand "Download-Remote $filePath"
}