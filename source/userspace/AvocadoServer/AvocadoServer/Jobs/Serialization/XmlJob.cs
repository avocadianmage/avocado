using System.Collections.Generic;

namespace AvocadoServer.Jobs.Serialization
{
    public sealed class XmlJob
    {
        public string App { get; set; }
        public string Name { get; set; }
        public int SecInterval { get; set; }
        public List<string> Args { get; set; }

        public Job ToJob() => new Job(App, Name, SecInterval, Args);
    }
}