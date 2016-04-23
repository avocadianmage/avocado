function build($path) 
{ 
    C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe $path /t:Build /p:Configuration=Release 
}

# Build system solutions.
build system/UtilityLibraries/UtilityLibraries.sln
build system/Avocado/Avocado.sln

# Build userspace solutions.
build userspace/Shell/Shell.sln
build userspace/AvocadoServer/AvocadoServer.sln
