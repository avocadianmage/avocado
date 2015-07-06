using System;
using System.Management.Automation;

namespace AvocadoUtilities.Cmdlet
{
    public static class Extensions
    {
        public static void WriteErrorLine(this PSCmdlet cmdlet, string msg)
        {
            cmdlet.Host.UI.WriteErrorLine(msg);
        }

        public static void WriteNoNewLine(this PSCmdlet cmdlet, string msg)
        {
            cmdlet.Host.UI.Write(msg);
        }

        public static void Terminate(this PSCmdlet cmdlet, string msg)
        {
            var error = new ErrorRecord(
                new Exception(msg),
                null, ErrorCategory
                .InvalidOperation,
                null);
            cmdlet.ThrowTerminatingError(error);
        }

        public static string DoOperation(
            this PSCmdlet cmdlet, 
            string msg, 
            Func<string> op)
        {
            cmdlet.WriteNoNewLine(string.Format("{0}...", msg));
            try
            {
                return op();
            }
            finally
            {
                cmdlet.WriteObject("done.");
            }
        }
    }
}