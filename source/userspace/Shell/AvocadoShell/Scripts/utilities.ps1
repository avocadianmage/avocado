# Utility function executed by root PowerShell instance to complete the remote
# download.
function DownloadToRoot
{
	Param(
		[Parameter(Mandatory = $true)][string]$RemoteComputerName,
		[Parameter(Mandatory = $true)][string[]]$SourcePaths)

	# Get the local machine destination directory.
	$dest = Join-Path `
		([Environment]::GetFolderPath("MyDocuments")) `
		"Avocado\Downloads"
	New-Item -ItemType Directory -Force -Path $dest | Out-Null

	# Perform the download.
	Copy-Item `
		-FromSession (New-PSSession $RemoteComputerName) `
		-Path $SourcePaths `
		-Destination $dest `
		-Recurse
}