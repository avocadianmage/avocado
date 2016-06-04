# Utility function executed by local PowerShell instance to complete the remote
# download.
function SendToLocal
{
	Param(
		[Parameter(Mandatory = $true)][string]$RemoteComputerName,
		[Parameter(Mandatory = $true)][string[]]$SourcePaths)

	# Get the Downloads directory of the local machine.
	$dest = (Get-ItemProperty `
		"HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders" `
		)."{374DE290-123F-4565-9164-39C4925E467B}"

	# Perform the download.
	$session = New-PSSession $RemoteComputerName
	Copy-Item `
		-FromSession $session `
		-Path $SourcePaths `
		-Destination $dest `
		-Recurse
	Remove-PSSession $session
}