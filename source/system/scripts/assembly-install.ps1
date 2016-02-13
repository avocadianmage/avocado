# Command line parameters declaration.
param
(
    [string]$installPath = $(throw "Parameter for install path is required.")
)

# Create app directory if it does not exist.
if (!(Test-Path -PathType Container $installPath))
{
    New-Item $installPath -Type Directory > $null
}

$proj = (Get-ChildItem ../.. -Filter *.csproj).BaseName
$exe = Get-ChildItem -Filter *.exe | where { $_.BaseName -eq $proj }

# Copy over the project executable and any associated config files.
Get-ChildItem * -Include *.exe, *.config |
    where { ($_.Name -eq $exe) -Or ($_.BaseName -eq $exe) } |
    Copy-Item -Destination $installPath

# Copy over DLLs.
Get-ChildItem -Filter *.dll | Copy-Item -Destination $installPath

# Add app path to the environment path if it hasn't been yet.
$fullInstallPath = Resolve-Path $installPath
$userPathStr = [Environment]::GetEnvironmentVariable(
    "PATH", [System.EnvironmentVariableTarget]::User)
if ($userPathStr -And ($userPathStr.Split(";") -Contains $fullInstallPath))
{
    return
}
[Environment]::SetEnvironmentVariable(
    "PATH", $userPathStr + ";$fullInstallPath", [System.EnvironmentVariableTarget]::User)
