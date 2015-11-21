﻿using AvocadoUtilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AvocadoServer.Jobs.Serialization
{
    static class JobSerializer
    {
        static string filePath =>
            Path.Combine(RootDir.Avocado.Apps.MyAppDataPath, "jobs.xml");

        static readonly XmlSerializer serializer
            = new XmlSerializer(typeof(List<XmlJob>));

        public static async Task<IEnumerable<Job>> Load()
        {
            // Quit if serialized data does not exist.
            if (!File.Exists(filePath)) return null;

            // Deserialize the saved list of jobs.
            IEnumerable<Job> jobs = null;
            await Task.Run(() =>
            {
                lock (serializer)
                {
                    using (var reader = new StreamReader(filePath))
                    {
                        var xmlJobs = (IEnumerable<XmlJob>)
                            serializer.Deserialize(reader);
                        jobs = xmlJobs.Select(x => x.ToJob());
                    }
                }
            });
            return jobs;
        }

        public static async Task Save(IEnumerable<Job> jobs)
        {
            await Task.Run(() =>
            {
                lock (serializer)
                {
                    using (var writer = new StreamWriter(filePath))
                    {
                        var xmlJobs = jobs.Select(x => x.ToXml()).ToList();
                        serializer.Serialize(writer, xmlJobs);
                    }
                }
            });
        }
    }
}