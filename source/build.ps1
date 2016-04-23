function build($path) { msbuild $path /t:Build /p:Configuration=Release }

# Build system solutions.
build system/UtilityLibraries/UtilityLibraries.sln
build system/Avocado/Avocado.sln

# Build userspace solutions.
build userspace/Shell/Shell.sln
build userspace/AvocadoServer/AvocadoServer.sln
