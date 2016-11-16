$shortcutPath = Join-Path $env:APPDATA "Avocado"

# Ensure shortcut path is included in environment path variable.
if (-not $env:PATH.Split(";").Contains($shortcutPath))
{
    [Environment]::SetEnvironmentVariable( `
        "Path", $env:PATH + ";$shortcutPath", [EnvironmentVariableTarget]::Machine)
}

function build($path)
{
    $msbuild = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
    & $msbuild $path /t:Rebuild /p:Configuration=Release
    & $msbuild $path /t:Rebuild /p:Configuration=Release /p:OutputPath=$shortcutPath
}

# Build system solutions.
build $PSScriptRoot/system/StandardLibrary/StandardLibrary.sln
build $PSScriptRoot/system/Avocado/Avocado.sln

# Build userspace solutions.
build $PSScriptRoot/userspace/AvocadoShell/AvocadoShell.sln
build $PSScriptRoot/userspace/AvocadoServer/AvocadoServer.sln
build $PSScriptRoot/userspace/AvocadoDownloader/AvocadoDownloader.sln
