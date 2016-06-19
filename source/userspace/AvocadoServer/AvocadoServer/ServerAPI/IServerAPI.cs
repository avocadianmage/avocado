﻿using System.ServiceModel;

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
        Pipeline RunJob(string workingDirectory, string name, int? secInterval);

        [OperationContract]
        Pipeline KillJob(int id);
    }
}