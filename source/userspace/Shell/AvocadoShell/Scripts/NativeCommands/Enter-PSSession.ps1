# PowerShell remoting interactive session support.
function Enter-PSSession
{
	Param([Parameter(Mandatory=$true)][string]$ComputerName)

	# Throw error if this was called from a remote session.
	if (IsRemoteSession) { Throw "Nested remoting is not supported." }

	# Verify the connection is valid.
	New-PSSession $ComputerName | Out-Null
	if (-not $?) { Return }
	Remove-PSSession $ComputerName

	# Run native command.
	RunNativeCommand "Enter-PSSession $ComputerName"
}