using System.Runtime.Serialization;

namespace AvocadoServer.ServerAPI
{
    [DataContract]
    public class Pipeline
    {
        [DataMember]
        public bool Success { get; set; } = true;

        [DataMember]
        public string Message { get; set; }
    }

    [DataContract]
    public sealed class Pipeline<T> : Pipeline
    {
        [DataMember]
        public T Data { get; set; }
    }
}