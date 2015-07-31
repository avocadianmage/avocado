using AvocadoServer.ServerCore;
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
        IEnumerable<Job> GetJobs();

        [OperationContract]
        void RunJob(string app, string name, int secInterval, string[] args);

        [OperationContract]
        void KillJob(int id);
    }
}