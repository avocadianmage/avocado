# Command line parameters declaration.
param
(
    [string]$sln = $(throw "Parameter for solution file name is required.")
)

# Get the app's installation path.
$installPath = Join-Path "~/avocado/apps" $sln

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
