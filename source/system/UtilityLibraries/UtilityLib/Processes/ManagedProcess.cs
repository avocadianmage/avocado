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
            get { return startInfo.WorkingDirectory; }
            set { startInfo.WorkingDirectory = value; }
        }

        ProcessStartInfo startInfo => proc.StartInfo;

        public void RunForeground()
        {
            proc.Start();
        }

        public async Task<string> RunBackground()
        {
            applyBackgroundProperties();
            proc.Start();
            return await proc.StandardOutput.ReadToEndAsync();
        }

        public async Task RunBackgroundLive()
        {
            applyBackgroundProperties();
            proc.OutputDataReceived += OutputReceived;
            proc.ErrorDataReceived += ErrorReceived;
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            await Task.Run(() => proc.WaitForExit());
        }

        void applyBackgroundProperties()
        {
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
        }
    }
}