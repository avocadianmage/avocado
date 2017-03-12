# Send a command to be invoked directly by the shell client.
#  - $command: The name of a method from IShellController.cs
function runNativeCommand($command, $argument)
{
    Write-Verbose "avocado:$command $argument" -Verbose
}