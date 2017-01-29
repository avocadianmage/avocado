# Download files across a remote session.
function rdl
{
    Param([Parameter(Mandatory=$true)][string[]]$Path)
    
    # Establish session back to the remote computer.
    $sess = newRemoteSession($env:RootComputerName)

    # Retrieve the destination path (downloads folder of root computer).
    $dest = Invoke-Command -Session $sess {
        (Get-ItemProperty `
            "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders" `
            )."{374DE290-123F-4565-9164-39C4925E467B}"
    }

    # Perform the download.
    Copy-Item -ToSession $sess -Path $Path -Destination $dest -Recurse
}
