using System.ServiceModel;

namespace AvocadoServer.ServerAPI
{
    [ServiceContract]
    public interface IServerAPI
    {
        [OperationContract]
        void RunJob(string app, string name);
    }
}