namespace AvocadoServer.Jobs.Serialization
{
    public sealed class XmlJob
    {
        public string Filename { get; set; }
        public int SecInterval { get; set; }

        public Job ToJob() => new Job(Filename, SecInterval);
    }
}