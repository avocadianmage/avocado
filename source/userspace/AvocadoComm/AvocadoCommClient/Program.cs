using AvocadoCommClient.AvocadoServiceReference;
using System;
using System.ServiceModel;

namespace AvocadoCommClient
{
    sealed class Program
    {
        static void Main()
        {
            AvocadoServiceClient client = null;

            try
            {
                client = new AvocadoServiceClient();
                Console.WriteLine(client.Ping());
                client.Close();
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

            Console.ReadLine();
        }
    }
}