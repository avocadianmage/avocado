using System.Collections.Generic;
using System.ServiceModel;

namespace AvocadoServer.ServerAPI
{
    [ServiceContract]
    public interface IServerAPI
    {
        [OperationContract]
        bool Ping();

        [OperationContract]
        IEnumerable<string> GetJobs();

        [OperationContract]
        void RunJob(string app, string name, params string[] args);

        [OperationContract]
        void KillJob(int id);
    }
}