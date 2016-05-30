# PowerShell remoting interactive session support.
Function Enter-PSSession
{
	Param([Parameter(Mandatory=$true)][string]$ComputerName)

	# Throw error if this was called from a remote session.
	if ((Get-Host).Name -eq "ServerRemoteHost") 
	{
		Throw "Nested remoting is not supported."
	}

	# Verify the connection is valid.
	New-PSSession $ComputerName | Out-Null
	if (-not $?) { Return }
	Remove-PSSession $ComputerName

	# Run native command.
	Write-Information `
		-Tags "avocado" `
		-MessageData "Enter-PSSession $ComputerName" 
}