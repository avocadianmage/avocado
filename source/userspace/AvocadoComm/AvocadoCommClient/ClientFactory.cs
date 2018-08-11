using AvocadoCommClient.AvocadoServiceReference;
using System;
using System.ServiceModel;

namespace AvocadoCommClient
{
    static class ClientFactory
    {
        public static T ExecuteClientCommand<T>(
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
