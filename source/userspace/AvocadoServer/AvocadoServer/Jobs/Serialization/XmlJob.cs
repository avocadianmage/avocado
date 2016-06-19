namespace AvocadoServer.Jobs.Serialization
{
    public sealed class XmlJob
    {
        public string WorkingDirectory { get; set; }
        public string Filename { get; set; }
        public int SecInterval { get; set; }

        public Job ToJob() => new Job(WorkingDirectory, Filename, SecInterval);
    }
}