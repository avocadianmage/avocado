using AvocadoServer.ServerCore;

namespace AvocadoServer.ServerAPI
{
    public class ServerAPIService : IServerAPI
    {
        public void RunJob(string app, string name)
        {
            Logger.WriteLogItem("Job {0}.{1} was started.", app, name);
        }
    }
}