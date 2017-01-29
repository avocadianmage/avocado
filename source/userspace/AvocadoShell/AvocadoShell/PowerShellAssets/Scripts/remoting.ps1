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
