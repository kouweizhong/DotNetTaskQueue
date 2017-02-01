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
        readonly internal Func<TaskInfo, CancellationToken, Task> Action;

        public TaskInfo(Func<TaskInfo, CancellationToken, Task> action)
        {
            Queue = null;
            Action = action;
        }

        public TaskInfo(Func<TaskInfo, CancellationToken, Task> action, string taskTag)
        {
            Queue = null;
            Action = action;
            Tag = taskTag;
        }

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
        public TaskQueue Queue { get; internal set; }

        /// <summary>
        /// Gets the tag. (if any)
        /// </summary>
        public string Tag { get; }
    }
}
