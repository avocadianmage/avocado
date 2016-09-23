# PowerShell remoting interactive session support.
function Enter-PSSession
{
	Param([Parameter(Mandatory = $true)][string]$ComputerName)

	# Throw error if this was called from a remote session.
	if (IsRemoteSession) { Throw "Nested remoting is not supported." }

	# Collect login credentials.
	$cred = Get-Credential

	# Verify the connection is valid.
	New-PSSession -ComputerName $ComputerName -Credential $cred | Out-Null
	if (-not $?) { Return }
	Remove-PSSession $ComputerName

	# Run native command.
	$Host.PrivateData.OpenRemoteSession($ComputerName, $cred)
}

# Shortcut.
New-Alias rsh Enter-PSSession