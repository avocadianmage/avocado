using System.Diagnostics;
using System.Threading.Tasks;

namespace UtilityLib.Processes
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
        }

        public async Task<string> RunBackground()
        {
            applyBackgroundProperties(proc.StartInfo);
            proc.Start();
            return await proc.StandardOutput.ReadToEndAsync();
        }

        public async Task RunBackgroundLive()
        {
            applyBackgroundProperties(proc.StartInfo);
            proc.OutputDataReceived += OutputReceived;
            proc.ErrorDataReceived += ErrorReceived;
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            await Task.Run(() => proc.WaitForExit());
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