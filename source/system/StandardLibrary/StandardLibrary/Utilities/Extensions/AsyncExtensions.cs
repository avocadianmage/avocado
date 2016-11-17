using System.Threading.Tasks;

namespace StandardLibrary.Utilities.Extensions
{
    public static class AsyncExtensions
    {
        public static void RunAsync(this Task task)
        {
            if (task.Status == TaskStatus.Created) task.Start();
        }
    }
}