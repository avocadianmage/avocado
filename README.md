# avocado

Visual Studio project post-build event for app installation:
    Powershell.exe -ExecutionPolicy RemoteSigned -File %UserProfile%\avocado\sys\devtools\assembly-install.ps1 $(SolutionName)
