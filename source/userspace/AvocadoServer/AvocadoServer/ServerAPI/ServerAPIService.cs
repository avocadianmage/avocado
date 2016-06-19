﻿using AvocadoServer.ServerCore;
using System.Collections.Generic;
using System.Reflection;
using static AvocadoServer.ServerCore.CommandContext;

namespace AvocadoServer.ServerAPI
{
    public sealed class ServerAPIService : IServerAPI
    {
        [AllowedClient(ClientType.LAN)]
        public Pipeline Ping()
        {
            return ExecuteRequest(p => { }, MethodBase.GetCurrentMethod());
        }
        
        [AllowedClient(ClientType.LAN)]
        public Pipeline<IEnumerable<string>> GetJobs()
        {
            return ExecuteRequest(
                p => EntryPoint.Jobs.GetJobTableInfo(), 
                MethodBase.GetCurrentMethod());
        }

        [AllowedClient(ClientType.ThisMachine)]
        public Pipeline RunJob(
            string workingDirectory, string filename, int? secInterval)
        {
            return ExecuteRequest(
                p => EntryPoint.Jobs.StartJob(
                    workingDirectory, filename, secInterval), 
                MethodBase.GetCurrentMethod(), 
                workingDirectory, filename, secInterval);
        }
        
        [AllowedClient(ClientType.LAN)]
        public Pipeline KillJob(int id)
        {
            return ExecuteRequest(
                p => EntryPoint.Jobs.KillJob(p, id),
                MethodBase.GetCurrentMethod(), 
                id);
        }
    }
}