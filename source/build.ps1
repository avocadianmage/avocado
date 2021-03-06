#
# Install avocado.
#

$buildFunc = 
{
    param($path, $outputPath, $buildAsApp = $FALSE)
    $msbuild = (Get-ChildItem `
        -Path "C:\Program Files (x86)\Microsoft Visual Studio\2017" `
        -Filter "MSBuild.exe" `
        -Recurse)[0].FullName

    # Build the project as a reference.
    & $msbuild $path /t:Rebuild /p:Configuration=Release

    # Build the project as an application.
    if ($buildAsApp) 
    { 
        & $msbuild $path /t:Rebuild /p:Configuration=Release /p:OutputPath=$outputPath 
    }
}

# Ensure shortcut path is included in environment path variable.
$shortcutPath = Join-Path $env:APPDATA "Avocado"
if (-not $env:PATH.Split(";").Contains($shortcutPath))
{
    [Environment]::SetEnvironmentVariable( `
        "Path", $env:PATH + ";$shortcutPath", [EnvironmentVariableTarget]::Machine)
}

# Build system solutions.
& $buildFunc $PSScriptRoot/system/StandardLibrary/StandardLibrary.sln $shortcutPath
& $buildFunc $PSScriptRoot/system/Avocado/Avocado.sln $shortcutPath

# Build userspace solutions.
Get-ChildItem -Recurse $PSScriptRoot\userspace *.sln | ForEach-Object {
    & $buildFunc $_.FullName $shortcutPath $TRUE
}
