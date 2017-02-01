using System;

namespace Sundstrom.Tasks
{
    public abstract class TaskEventArgsBase : EventArgs
    {
        internal TaskEventArgsBase(TaskInfo task)
        {
            Task = task;
        }

        /// <summary>
        /// Gets the task.
        /// </summary>
        public TaskInfo Task { get; }
    }
}
