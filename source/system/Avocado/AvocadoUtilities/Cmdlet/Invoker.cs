using System;
using System.Diagnostics;
using System.Management.Automation;

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
            proc.WaitForExit();
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
                Arguments = getArgStr(args),
                WorkingDirectory 
                    = cmdlet.SessionState.Path.CurrentFileSystemLocation.Path,
            };
            return new Process { StartInfo = startInfo, };
        }

        static string getArgStr(params string[] args)
        {
            // Escape each argument with quotes.
            return string.Format("\"{0}\"", string.Join("\" \"", args));
        }
    }
}