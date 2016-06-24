using System.ServiceModel;

namespace AvocadoServer.ServerAPI
{
    [ServiceContract]
    public interface IServerAPI
    {
        [OperationContract]
        Pipeline Ping();

        [OperationContract]
        Pipeline<string> GetJobs();

        [OperationContract]
        Pipeline KillJob(int id);

        [OperationContract]
        Pipeline RunJob(string workingDirectory, int secInterval, string name);
    }
}