using AvocadoServer.ServerCore;
using System.Collections.Generic;
using System.ServiceModel;
using UtilityLib.WCF;

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
        WCFMessage RunJob(string app, string name, int secInterval, string[] args);

        [OperationContract]
        void KillJob(int id);
    }
}