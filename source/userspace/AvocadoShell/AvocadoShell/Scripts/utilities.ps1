# Returns true if PowerShell code is executing in a remote session.
function IsRemoteSession { (Get-Host).Name -eq "ServerRemoteHost" }