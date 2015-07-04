using AvocadoShell.Engine;
using AvocadoUtilities;
using System.IO;
using System.Threading.Tasks;

namespace AvocadoShell.PowerShellService
{
    sealed class ShellEnvironment
    {
        readonly PSEngine engine;

        public ShellEnvironment(IShellUI ui)
        {
            engine = new PSEngine(ui);
        }

        string getIncludeScript(string dir, string script)
        {
            const string FMT = ". {0}";
            var path = Path.Combine(dir, script);
            return string.Format(FMT, path);
        }

        void addProfileScriptToExec()
        {
            var dir = RootDir.Avocado.Apps.MyAppPath;
            var script = getIncludeScript(dir, "profile.ps1");
            engine.AddScript(script);
        }

        void addUserCmdsToExec()
        {
            var cmdArg = Command.GetArg(1);
            if (cmdArg != null) engine.AddScript(cmdArg);
        }

        public void Init()
        {
            // Run user profile script.
            addProfileScriptToExec();

            // Execute any commands provided via commandline arguments to
            // this process.
            addUserCmdsToExec();

            // Perform the execute at once.
            engine.Execute();
        }

        public void Execute(string cmd)
        {
            engine.AddScript(cmd);
            engine.Execute();
        }

        public void StopExecution()
        {
            engine.Stop();
        }

        public async Task<string> GetCompletion(
            string input,
            int index,
            bool forward)
        {
            return await engine.Autocompleter.GetCompletion(
                input, 
                index, 
                forward);
        }
    }
}