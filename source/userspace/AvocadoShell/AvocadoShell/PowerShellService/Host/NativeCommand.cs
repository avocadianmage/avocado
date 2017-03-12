using AvocadoShell.Interfaces;
using System.Linq;
using System.Reflection;

namespace AvocadoShell.PowerShellService.Host
{
    public sealed class NativeCommand
    {
        readonly MethodInfo methodInfo;
        readonly string[] args;

        public NativeCommand(string command, params string[] args)
        {
            methodInfo = typeof(IShellController).GetMethods()
                .Single(m => m.Name == command);
            this.args = args;
        }

        public void Invoke(IShellController shellController)
            => methodInfo.Invoke(shellController, args);
    }
}