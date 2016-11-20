using StandardLibrary.Utilities.Extensions;
using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;

namespace StandardLibrary.Processes.NamedPipes
{
    public sealed class NamedPipeServer
    {
        public event EventHandler<MessageEventArgs> MessageReceived;

        public async Task Start(string name)
        {
            // Initialize server.
            using (var server = createServerStream(name))
            {
                // Wait for client connection.
                await server.WaitForConnectionAsync().ConfigureAwait(false);

                // A client has connected, so spawn another server instance for 
                // the next client.
                Start(name).RunAsync();

                // Read data from client.
                var reader = new StreamReader(server);
                while (!reader.EndOfStream)
                {
                    // Each line is an individual message.
                    var msg 
                        = await reader.ReadLineAsync().ConfigureAwait(false);
                    MessageReceived?.Invoke(this, new MessageEventArgs(msg));
                }
            }
        }

        NamedPipeServerStream createServerStream(string name)
        {
            return new NamedPipeServerStream(
                name,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances, 
                PipeTransmissionMode.Message, 
                PipeOptions.None, 
                1024, 1024,
                createPipeSecurity());
        }

        PipeSecurity createPipeSecurity()
        {
            var pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow));
            pipeSecurity.AddAccessRule(new PipeAccessRule(
                WindowsIdentity.GetCurrent().Owner,
                PipeAccessRights.FullControl,
                AccessControlType.Allow));
            return pipeSecurity;
        }
    }
}