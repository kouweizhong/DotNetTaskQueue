using System;
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
            Queue = queue;
            Action = action;
        }

        internal TaskInfo(TaskQueue queue, Func<TaskInfo, CancellationToken, Task> action, string taskTag)
        {
            Queue = queue;
            Action = action;
            Tag = taskTag;
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
