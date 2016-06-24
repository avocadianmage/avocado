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
        Pipeline RunJob(string workingDirectory, string name, int secInterval);

        [OperationContract]
        Pipeline Run(string workingDirectory, string name);
    }
}