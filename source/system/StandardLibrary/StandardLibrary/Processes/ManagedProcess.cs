using System.Diagnostics;
using System.Threading.Tasks;

namespace StandardLibrary.Processes
{
    public sealed class ManagedProcess
    {
        public event DataReceivedEventHandler OutputReceived;
        public event DataReceivedEventHandler ErrorReceived;

        readonly Process proc;

        public ManagedProcess(string fileName, params string[] args)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = EnvUtils.BuildArgStr(args)
            };
            proc = new Process { StartInfo = startInfo };
        }

        public string WorkingDirectory
        {
            get { return proc.StartInfo.WorkingDirectory; }
            set { proc.StartInfo.WorkingDirectory = value; }
        }

        public void RunForeground()
        {
            proc.Start();
            proc.WaitForExit();
        }

        public Task<string> RunBackground()
        {
            applyBackgroundProperties(proc.StartInfo);
            proc.Start();
            return proc.StandardOutput.ReadToEndAsync();
        }

        public void RunBackgroundLive()
        {
            applyBackgroundProperties(proc.StartInfo);
            proc.OutputDataReceived += OutputReceived;
            proc.ErrorDataReceived += ErrorReceived;
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();
        }

        static void applyBackgroundProperties(ProcessStartInfo startInfo)
        {
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
        }
    }
}