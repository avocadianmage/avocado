# Download a file from a remote PowerShell session.
function Download-Remote
{
	Param([Parameter(Mandatory = $true)][string[]]$Path)

	# Throw error if this was called from a remote session.
	if (-not (IsRemoteSession)) { Throw "No remote session found." }

	# Get full paths.
	$Path = ($Path | foreach { Get-Item $_ }) -join ","

	# Run native command.
	RunNativeCommand "Download-Remote $Path"
}

# Shortcut.
New-Alias rdl Download-Remote