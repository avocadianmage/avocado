# Start an interactive remote session.
function rsh
{
    Param([Parameter(Mandatory=$true)][string]$ComputerName)

    # Create remote session.
    $sess = newRemoteSession($ComputerName)

    # Save a reference in the new session to the root computer name.
    Invoke-Command -Session $sess -ArgumentList $env:COMPUTERNAME { 
        Param($RootComputerName)
        $env:RootComputerName = $RootComputerName 
    }

    # Enter the session interactively.
    Enter-PSSession -Session $sess
}
