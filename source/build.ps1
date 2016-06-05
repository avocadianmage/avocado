$shortcutPath = Join-Path $env:APPDATA "Avocado"
if (Test-Path $shortcutPath) { Remove-Item $shortcutPath\* -Recurse }

function build($path) 
{ 
    & "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" $path `
        /t:Build /p:Configuration=Release /p:OutputPath=$shortcutPath
}

# Build system solutions.
build system/UtilityLibraries/UtilityLibraries.sln
build system/Avocado/Avocado.sln

# Build userspace solutions.
build userspace/Shell/Shell.sln
build userspace/AvocadoServer/AvocadoServer.sln

# Ensure shortcut path is included in environment path variable.
if (-not $env:PATH.Split(";").Contains($shortcutPath))
{
    [Environment]::SetEnvironmentVariable( `
        "Path", $env:PATH + ";$shortcutPath", [EnvironmentVariableTarget]::Machine)
}
