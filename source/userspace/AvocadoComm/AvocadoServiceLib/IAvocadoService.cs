using System.ServiceModel;

namespace AvocadoServiceLib
{
    [ServiceContract(
        Namespace = "http://AvocadoServiceLib", 
        Name = "AvocadoService")]
    public interface IAvocadoService
    {
        [OperationContract] bool Ping();
    }
}
