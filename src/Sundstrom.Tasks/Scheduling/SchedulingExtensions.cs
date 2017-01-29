using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sundstrom.Tasks;

namespace Sundstrom.Tasks.Scheduling
{
    public static class SchedulingExtensions
    {
        /// <summary>
        /// Get SchedulerContext.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static SchedulerContext GetContext(this TaskQueue source)
        {
            return source._context;
        }

        public static TaskInfo GetCurrentTask(this TaskQueue source)
        {
            return source.Scheduler.Current;
        }
    }
}