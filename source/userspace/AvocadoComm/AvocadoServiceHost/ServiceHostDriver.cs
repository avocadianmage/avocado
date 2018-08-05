using AvocadoServiceLib;
using System;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Windows.Media;

namespace AvocadoServiceHost
{
    sealed class ServiceHostDriver
    {
        public void Start()
        {
            Logging.WriteLine("Initializing service host...");
            using (var host = createServiceHost())
            {
                Logging.WriteLine("Enabling metadata exchange...");
                enableMetadataExchange(host);

                Logging.WriteLine("Opening host communication...");
                host.Open();

                Logging.WriteLine(new(string, Color?)[] {
                    ("AvocadoService is now running at ", null),
                    (getHostEndpoint(host).ToString(), Colors.SkyBlue)
                });
                Console.ReadLine();
            }
        }

        ServiceHost createServiceHost()
        {
            var serviceBaseAddress
                = ConfigurationManager.AppSettings["ServiceBaseAddress"];
            return new ServiceHost(
                typeof(AvocadoService), new Uri(serviceBaseAddress));
        }

        void enableMetadataExchange(ServiceHost host)
        {
            var serviceMetaDataBehavior = new ServiceMetadataBehavior
            {
                HttpGetEnabled = true
            };
            host.Description.Behaviors.Add(serviceMetaDataBehavior);
        }

        Uri getHostEndpoint(ServiceHost host)
            => host.Description.Endpoints.Single().Address.Uri;
    }
}