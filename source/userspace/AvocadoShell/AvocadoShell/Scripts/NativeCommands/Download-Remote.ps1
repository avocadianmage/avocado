# Download a file from a remote PowerShell session.
function Download-Remote
{
	Param(
		[Parameter(Mandatory = $true)][string]$ComputerName,
		[Parameter(Mandatory = $true)][string[]]$Path)
	
	# Throw error if we are not in a remote session.
	if (-not (IsRemoteSession)) { Throw "No remote session found." }

	# Collect login credentials.
	$cred = Get-Credential

	# Establish target session.
	$sess = New-PSSession -ComputerName $ComputerName -Credential $cred

	# Perform the download.
	Copy-Item `
		-ToSession $sess `
		-Path $Path `
		-Destination "D:\User\Documents\junk" `
		-Recurse
}

# Shortcut.
New-Alias rdl Download-Remote