using System;

namespace Sundstrom.Tasks
{
    public class TaskExceptionEventArgs : TaskEventArgsBase
    {
        internal TaskExceptionEventArgs(TaskInfo task, Exception exception, bool cancel)
            : base(task)
        {
            Exception = exception;

            Cancel = cancel;
        }

        /// <summary>
        /// Gets the associated exception.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets or sets whether the queue should cancel or not.
        /// </summary>
        public bool Cancel { get; set; }
    }
}
