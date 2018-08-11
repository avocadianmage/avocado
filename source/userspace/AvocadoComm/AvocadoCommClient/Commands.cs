using AvocadoCommClient.AvocadoServiceReference;
using AvocadoLib.Basic;
using AvocadoLib.CommandLine.Arguments;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;

namespace AvocadoCommClient
{
    static class Commands
    {
        [Subcommand]
        public static void Ping(IEnumerable<string> args)
        {
            // Measure the time taken.
            var stopwatch = Stopwatch.StartNew();

            // Ping the server.
            var result = executeClientCommand(
                c => c.Ping(), out EndpointAddress endpoint);

            stopwatch.Stop();
            var secs = stopwatch.ElapsedMilliseconds / 1000d;
            var timestamp = secs.ToRoundedString(3);

            // Output result.
            Console.WriteLine(
                $"Ping to {endpoint} returned '{result}' in {timestamp}s.");
        }

        static T executeClientCommand<T>(
            Func<AvocadoServiceClient, T> code, out EndpointAddress endpoint)
        {
            AvocadoServiceClient client = null;
            endpoint = null;
            try
            {
                client = new AvocadoServiceClient();
                endpoint = client.Endpoint.Address;

                var result = code(client);

                client.Close();
                return result;
            }
            catch (CommunicationException ex)
            {
                client?.Abort();
                Console.Error.WriteLine(ex.Message);
            }
            catch (TimeoutException ex)
            {
                client?.Abort();
                Console.Error.WriteLine(ex.Message);
            }
            catch
            {
                client?.Abort();
                throw;
            }
            return default(T);
        }
    }
}