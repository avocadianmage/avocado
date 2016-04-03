using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace AvocadoServer.Jobs.Serialization
{
    static class JobSerializer
    {
        static string filePath => Path.Combine(Path.GetTempPath(), "jobs.xml");

        static readonly XmlSerializer serializer 
            = new XmlSerializer(typeof(List<XmlJob>));

        public static IEnumerable<Job> Load()
        {
            // Quit if serialized data does not exist.
            if (!File.Exists(filePath)) return null;

            // Deserialize the saved list of jobs.
            lock (serializer)
            {
                using (var reader = new StreamReader(filePath))
                {
                    var xmlJobs = serializer.Deserialize(reader)
                        as IEnumerable<XmlJob>;
                    return xmlJobs.Select(x => x.ToJob());
                }
            }
        }

        public static void Save(IEnumerable<Job> jobs)
        {
            lock (serializer)
            {
                using (var writer = new StreamWriter(filePath))
                {
                    var xmlJobs = jobs.Select(x => x.ToXml()).ToList();
                    serializer.Serialize(writer, xmlJobs);
                }
            }
        }
    }
}