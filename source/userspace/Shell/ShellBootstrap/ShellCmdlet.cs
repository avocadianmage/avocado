using AvocadoUtilities;
using AvocadoUtilities.Cmdlet;
using System.IO;
using System.Management.Automation;

namespace ShellBootstrap
{
    [Cmdlet(VerbsCommon.Open, "Shell")]
    public class ShellCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = false)]
        public string Cmds { get; set; }

        protected override void ProcessRecord()
        {
            var dir = RootDir.Avocado.Apps.MyAppPath;
            var filePath = Path.Combine(dir, "AvocadoShell.exe");
            this.RunApp(filePath, Cmds);
        }
    }
}
