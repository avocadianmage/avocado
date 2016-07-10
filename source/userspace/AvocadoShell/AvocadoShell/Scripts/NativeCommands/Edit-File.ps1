# PowerShell remoting interactive session support.
function Edit-File
{
	Param([Parameter(Mandatory = $true)][string]$File)

	# Run native command.
	RunNativeCommand "Edit-File $file"
}

# Shortcut.
New-Alias edit Edit-File