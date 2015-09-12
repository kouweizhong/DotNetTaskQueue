using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sundstrom.Tasks
{
    public class TaskEventArgs : EventArgs
    {
        internal TaskEventArgs(TaskQueue taskQueue, string tag)
        {
            TaskQueue = taskQueue;
            Tag = tag;
        }

        internal TaskEventArgs(TaskQueue taskQueue, string tag, Exception exception)
        {
            TaskQueue = taskQueue;
            Exception = exception;
            Tag = tag;
        }

        /// <summary>
        /// Gets the tag associated with this task. (if any)
        /// </summary>
        public string Tag
        {
            get;
        }

        /// <summary>
        /// Gets the queue in this context.
        /// </summary>
        public TaskQueue TaskQueue
        {
            get;
        }

        /// <summary>
        /// Gets the thrown exception.
        /// </summary>
        public Exception Exception
        {
            get;
        }
    }
}
