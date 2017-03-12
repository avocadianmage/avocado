# Natively edit the specified file.
function edit
{
    Param([Parameter(Mandatory=$true)][string]$Path)
    
    # Invoke the host to open the native editor.
    $Path = `
        $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath( `
        $Path)
    $nativeCommand = New-Object `
        -TypeName AvocadoShell.PowerShellService.Host.NativeCommand `
        -ArgumentList "EditFile", $Path
    Write-Information -MessageData $nativeCommand
}
