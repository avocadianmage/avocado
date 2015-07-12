# avocado

Visual Studio project post-build event for app installation:

    Powershell.exe -ExecutionPolicy RemoteSigned -File %UserProfile%\avocado\sys\devtools\assembly-install.ps1 <1> <2>

Where <1> is the name of the app, and <2> is an optional parameter specifying the name of the executable to be copied over.
