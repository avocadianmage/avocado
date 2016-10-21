﻿using System.Threading.Tasks;

namespace UtilityLib.Extensions
{
    public static class AsyncExtensions
    {
        public static void RunAsync(this Task task)
        {
            if (task.Status == TaskStatus.Created) task.Start();
        }
    }
}