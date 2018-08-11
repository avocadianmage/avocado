# Get path to the credentials saved to disk.
function credentialPath { Join-Path (Split-Path $PROFILE) "credential.txt" }

# Securely save credentials to disk.
function setCredential
{
    Param([Parameter(Mandatory=$true)][string]$ComputerName)

    $path = credentialPath
    $username = Read-Host -Prompt "Username"
    $password = Read-Host -Prompt "Password" -AsSecureString `
        | ConvertFrom-SecureString
    $credentialInfo = "$ComputerName`t$username`t$password"
    $credentialInfo | Out-File $path -Append
    $credentialInfo.Split("`t")
}

# Securely load credentials saved to disk.
function loadCredential
{
    Param([Parameter(Mandatory=$true)][string]$ComputerName)

    # Search the file for saved credentials for the target computer.
    $path = credentialPath
    if (Test-Path $path)
    {
        foreach ($line in Get-Content $path)
        {
            $pieces = $line.Split("`t")
            if ($pieces[0].ToLower() -ne $ComputerName.ToLower()) { continue }
            $credentialInfo = $pieces
            break
        }
    }
    
    # If credentials are not found, prompt for them.
    if (!$credentialInfo) { $credentialInfo = setCredential($ComputerName) }

    # Build the PSCredential object to return.
    $username = $credentialInfo[1]  
    $password = $credentialInfo[2] | ConvertTo-SecureString
    New-Object `
        -TypeName System.Management.Automation.PSCredential `
        -ArgumentList $username,$password
}

# Create a new remote session.
function newRemoteSession
{
    Param([Parameter(Mandatory=$true)][string]$ComputerName)
    New-PSSession `
        -ComputerName $ComputerName `
        -Credential (loadCredential($ComputerName))
}
