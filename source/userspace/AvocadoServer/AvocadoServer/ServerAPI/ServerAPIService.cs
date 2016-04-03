using AvocadoServer.ServerCore;
using System.Collections.Generic;
using System.Reflection;
using static AvocadoServer.ServerCore.CommandContext;

namespace AvocadoServer.ServerAPI
{
    public sealed class ServerAPIService : IServerAPI
    {
        public bool Ping() => true;
        
        [AllowedClient(ClientType.LAN)]
        public Pipeline<IEnumerable<string>> GetJobs()
        {
            return ExecuteRequest(
                p => EntryPoint.Jobs.GetJobTableInfo(), 
                MethodBase.GetCurrentMethod());
        }

        [AllowedClient(ClientType.ThisMachine)]
        public Pipeline RunJob(string filename, int secInterval, string[] args)
        {
            return ExecuteRequest(
                p => EntryPoint.Jobs.StartJob(filename, secInterval, args), 
                MethodBase.GetCurrentMethod(), 
                filename, secInterval, args);
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