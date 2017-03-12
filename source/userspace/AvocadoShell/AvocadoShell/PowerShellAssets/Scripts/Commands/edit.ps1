# Natively edit the specified file.
function edit
{
    Param([Parameter(Mandatory=$true)][string]$Path)
    
    # Invoke the host to open the native editor.
    $Path = `
        $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath( `
        $Path)
    $Host.PrivateData.ShellUI.EditFile($Path)
}
