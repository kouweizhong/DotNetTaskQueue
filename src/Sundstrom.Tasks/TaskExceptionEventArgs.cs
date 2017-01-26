using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sundstrom.Tasks
{
    public class TaskExceptionEventArgs : TaskEventArgsBase
    {
        internal TaskExceptionEventArgs(string tag, Exception exception, bool cancel)
            : base(tag)
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
