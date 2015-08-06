using AvocadoUtilities;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace AvocadoServer.Jobs
{
    static class JobSerializer
    {
        static string filePath =>
            Path.Combine(RootDir.Avocado.Apps.MyAppDataPath, "jobs.bin");

        static readonly object fileLock = new object();

        public static async Task<IEnumerable<Job>> Load()
        {
            // Quit if serialized data does not exist.
            if (!File.Exists(filePath)) return null;

            // Deserialize the saved list of jobs.
            IEnumerable<Job> jobs = null;
            await Task.Run(() =>
            {
                lock (fileLock)
                {
                    using (var stream = new FileStream(filePath, FileMode.Open))
                    {
                        jobs = (IEnumerable<Job>)
                            new BinaryFormatter().Deserialize(stream);
                    }
                }
            });
            return jobs;
        }

        public static async Task Save(IEnumerable<Job> jobs)
        {
            await Task.Run(() =>
            {
                lock (fileLock)
                {
                    using (var stream = new FileStream(
                        filePath, 
                        FileMode.Create))
                    {
                        new BinaryFormatter().Serialize(stream, jobs);
                    }
                }
            });
        }
    }
}