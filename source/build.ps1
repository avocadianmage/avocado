function build($path) 
{ 
    & "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" $path /t:Build /p:Configuration=Release
}

# Build system solutions.
build system/UtilityLibraries/UtilityLibraries.sln
build system/Avocado/Avocado.sln

# Build userspace solutions.
build userspace/Shell/Shell.sln
build userspace/AvocadoServer/AvocadoServer.sln
