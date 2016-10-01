# Get path to the credentials saved to disk.
function credentialPath { Join-Path (Split-Path $PROFILE) "credential.txt" }

# Securely save credentials to disk.
function setCredential
{
    $path = credentialPath
    Read-Host -Prompt "Username" | Out-File $path
    Read-Host -Prompt "Password" -AsSecureString `
        | ConvertFrom-SecureString `
        | Out-File $path -Append
}

# Securely load credentials saved to disk.
function loadCredential
{
    $path = credentialPath
    if (!(Test-Path $path)) { setCredential }
    $content = Get-Content $path
    $username = $content[0]  
    $password = $content[1] | ConvertTo-SecureString
    New-Object `
        -TypeName System.Management.Automation.PSCredential `
        -ArgumentList $username,$password
}

# Create a new remote session.
function newRemoteSession
{
    Param([Parameter(Mandatory=$true)][string]$ComputerName)
    New-PSSession -ComputerName $ComputerName -Credential (loadCredential)
}

# Start an interactive remote session.
function rsh
{
    Param([Parameter(Mandatory=$true)][string]$ComputerName)

    # Create remote session.
    $sess = newRemoteSession($ComputerName)

    # Save a reference in the new session to the root computer name.
    Invoke-Command -Session $sess -ArgumentList $env:COMPUTERNAME { 
        Param([string]$RootComputerName)
        $env:RootComputerName = $RootComputerName 
    }

    # Enter the session interactively.
    Enter-PSSession -Session $sess
}

# Download files across a remote session.
function rdl
{
    Param([Parameter(Mandatory = $true)][string[]]$Path)
    
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