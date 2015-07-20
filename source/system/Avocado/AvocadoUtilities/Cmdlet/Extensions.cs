using System;
using System.Management.Automation;
using System.Threading.Tasks;

namespace AvocadoUtilities.Cmdlet
{
    public static class Extensions
    {
        public static void Terminate(this PSCmdlet cmdlet, string msg)
        {
            var error = new ErrorRecord(
                new Exception(msg),
                null, ErrorCategory
                .InvalidOperation,
                null);
            cmdlet.ThrowTerminatingError(error);
        }

        public static void Terminate(this PSCmdlet cmdlet)
        {
            cmdlet.Terminate(string.Empty);
        }

        public static T DoOperation<T>(
            this PSCmdlet cmdlet,  
            string msg, 
            Func<T> work)
        {
            cmdlet.Host.UI.Write(string.Format("{0}...", msg));

            var task = Task.Run(work);
            while (!task.IsCompleted)
            {
                if (cmdlet.Stopping) cmdlet.Terminate();
            }

            cmdlet.WriteObject("done.");

            return task.Result;
        }
    }
}