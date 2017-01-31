﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sundstrom.Tasks
{
    /// <summary>
    /// Holds the context of a task.
    /// </summary>
    public class TaskInfo
    {
        internal Func<TaskInfo, CancellationToken, Task> Action;

        internal TaskInfo(TaskQueue queue, Func<TaskInfo, CancellationToken, Task> action)
        {
            this.Queue = queue;
            this.Action = action;
        }

        internal TaskInfo(TaskQueue queue, Func<TaskInfo, CancellationToken, Task> action, string taskTag)
        {
            this.Queue = queue;
            this.Action = action;
            this.Tag = taskTag;
        }

        /// <summary>
        /// Gets the queue.
        /// </summary>
        public TaskQueue Queue { get; }

        /// <summary>
        /// Gets the tag. (if any)
        /// </summary>
        public string Tag { get; }
    }
}
