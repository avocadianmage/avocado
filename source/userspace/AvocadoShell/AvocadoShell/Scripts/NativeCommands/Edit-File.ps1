# PowerShell remoting interactive session support.
function Edit-File
{
	Param([Parameter(Mandatory = $true)][string]$FileName)

	# Run native command.
	RunNativeCommand "Edit-File $FileName"
}

# Shortcut.
New-Alias edit Edit-File