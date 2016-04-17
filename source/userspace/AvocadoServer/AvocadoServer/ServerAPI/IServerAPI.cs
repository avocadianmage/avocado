using System.Collections.Generic;
using System.ServiceModel;

namespace AvocadoServer.ServerAPI
{
    [ServiceContract]
    public interface IServerAPI
    {
        [OperationContract]
        Pipeline Ping();

        [OperationContract]
        Pipeline<IEnumerable<string>> GetJobs();

        [OperationContract]
        Pipeline RunJob(string name, int secInterval, string[] args);

        [OperationContract]
        Pipeline KillJob(int id);
    }
}