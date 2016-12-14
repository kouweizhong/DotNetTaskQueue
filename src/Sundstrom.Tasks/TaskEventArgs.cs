using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sundstrom.Tasks
{
    public class TaskEventArgs : EventArgs
    {
        internal TaskEventArgs(string tag)
        {;
            Tag = tag;
        }

        internal TaskEventArgs(string tag, Exception exception)
        {
            Exception = exception;
            Tag = tag;
        }

        /// <summary>
        /// Gets the tag associated with this task. (if any)
        /// </summary>
        public string Tag { get; }

        /// <summary>
        /// Gets the thrown exception.
        /// </summary>
        public Exception Exception { get; }
    }
}
