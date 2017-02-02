using Sundstrom.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sundstrom.Tasks
{
    public sealed class TaskQueue : TaskQueue<TaskInfo>
    {
        private static volatile TaskQueue<TaskInfo> _default;
        private static object syncRoot = new Object();

        private TaskQueue()
        {

        }

        /// <summary>
        /// Gets the default queue.
        /// </summary>
        public static TaskQueue<TaskInfo> Default
        {
            get
            {
                if (_default == null)
                {
                    lock (syncRoot)
                    {
                        _default = new TaskQueue<TaskInfo>(string.Empty, null);
                        _queues[string.Empty] = _default;
                    }
                }
                return _default;
            }
        }
    }
}