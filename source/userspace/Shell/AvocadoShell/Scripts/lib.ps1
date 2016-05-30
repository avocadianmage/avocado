# Communicate back to the core host (C# code).
function RunNativeCommand($command)
{
	Write-Information -Tags "avocado" -MessageData $command
}

# Returns true if PowerShell code is executing in a remote session.
function IsRemoteSession { (Get-Host).Name -eq "ServerRemoteHost" }