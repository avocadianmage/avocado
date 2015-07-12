# Command line parameters declaration.
param
(
    [string]$appName = $(throw "Parameter for app name is required."),
    [string]$alias
)

# Get the app's installation path.
$installPath = Join-Path "~/avocado/apps" $appName

# Create app directory if it does not exist.
if (!(Test-Path -PathType Container $installPath))
{
    New-Item $installPath -Type Directory > $null
}

# Copy over project executable and all DLLs to the Avocado app folder.
$proj = (Get-ChildItem ../.. -Filter *.csproj).BaseName
Get-ChildItem -Filter *.exe |
    where { $_.BaseName -eq $proj } |
    Copy-Item -Destination $installPath
Get-ChildItem -Filter *.dll | Copy-Item -Destination $installPath

# Rename the executable if an alias was specified.
if ($alias)
{
    Join-Path $installPath "$proj.exe" |
        Move-Item -Destination (Join-Path $installPath "$alias.exe") -Force
}

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
