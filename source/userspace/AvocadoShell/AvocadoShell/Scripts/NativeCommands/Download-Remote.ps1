# Download a file from a remote PowerShell session.
function Download-Remote
{
	Param([Parameter(Mandatory = $true)][string]$Path)

	# Throw error if this was called from a remote session.
	if (-not (IsRemoteSession)) { Throw "No remote session found." }

	# Verify the file exists.
	$fullPaths = Get-Item $Path
	if (-not $?) { Return }

	# Format path, handling multiple.
	$pathList = $fullPaths -join "`",`""
	$pathList = "`"$pathList`""

	# Run native command.
	RunNativeCommand "Download-Remote $pathList"
}

# Shortcut.
New-Alias rdl Download-Remote