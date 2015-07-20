using System.Diagnostics;
using System.Management.Automation;
using UtilityLib.MiscTools;

namespace AvocadoUtilities.Cmdlet
{
    public static class Invoker
    {
        public static string RunProcess(
            this PSCmdlet cmdlet,
            string fileName,
            params string[] args)
        {
            var proc = cmdlet.getProc(fileName, args);
            var startInfo = proc.StartInfo;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            proc.Start();
            return proc.StandardOutput.ReadToEnd();
        }

        public static void RunApp(
            this PSCmdlet cmdlet,
            string fileName, 
            params string[] args)
        {
            cmdlet.getProc(fileName, args).Start();
        }

        static Process getProc(
            this PSCmdlet cmdlet, 
            string fileName, 
            params string[] args)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = EnvUtils.BuildArgStr(args),
                WorkingDirectory 
                    = cmdlet.SessionState.Path.CurrentFileSystemLocation.Path,
            };
            return new Process { StartInfo = startInfo, };
        }
    }
}