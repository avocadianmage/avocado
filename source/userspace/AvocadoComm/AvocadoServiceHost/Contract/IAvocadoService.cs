using System.ServiceModel;

namespace AvocadoServiceHost.Contract
{
    [ServiceContract(
        Namespace = "http://AvocadoServiceLib", 
        Name = "AvocadoService")]
    public interface IAvocadoService
    {
        [OperationContract] bool Ping();
    }
}
