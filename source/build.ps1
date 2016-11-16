#
# Install avocado.
#

$buildFunc = 
{
    param($path, $outputPath, $buildAsRef = $TRUE)
    $msbuild = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"

    # Build the project as a reference.
    if ($buildAsRef) { & $msbuild $path /t:Rebuild /p:Configuration=Release }

    # Build the project as an application.
    else { & $msbuild $path /t:Rebuild /p:Configuration=Release /p:OutputPath=$outputPath }
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
    Start-Job -ScriptBlock $buildFunc -ArgumentList $_.FullName, $shortcutPath, $FALSE
}
Get-Job | ForEach-Object { Wait-Job $_.Id; Receive-Job $_.Id; Remove-Job $_.Id }
