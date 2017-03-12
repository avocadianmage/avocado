# Stream media across a remote session.
function rstr
{
    Param([Parameter(Mandatory=$true)][string]$Path)
    
    # Establish session back to the remote computer.
    $rootSession = newRemoteSession($env:RootComputerName)

    # Retrieve the temporary download location for the file on the root
    # computer.
    $rootDownloadDirectory = Invoke-Command -Session $rootSession -ScriptBlock { 
        $env:TEMP 
    }
    
    # Asynchronously perform the download.
    $job = Start-Job -ScriptBlock {
        Copy-Item -ToSession $rootSession `
                  -Path $Path `
                  -Destination $rootDownloadDirectory `
                  -Recurse
    }

    # Wait for the download to start.
    Start-Sleep -Seconds 3

    # Begin running the media while it is downloading.
    $destFilePath = Join-Path $rootDownloadDirectory (Split-Path $Path -Leaf)
    runNativeCommand "RunForeground" $destFilePath
    
    # Clean up file after user is finished.
    Stop-Job $job.Id
    Invoke-Command -Session $rootSession -ScriptBlock { 
        Param($destFilePath)
        Remove-Item $destFilePath 
    } -ArgumentList $destFilePath
}
