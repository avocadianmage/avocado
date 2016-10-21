﻿using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using StandardLibrary.Extensions;

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
                NamedPipeServerStream.MaxAllowedServerInstances);
        }
    }
}