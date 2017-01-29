using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sundstrom.Tasks
{
    public class TaskCancelingEventArgs : TaskEventArgsBase
    {
        internal TaskCancelingEventArgs(string tag, bool cancel)
            : base(tag)
        {
            Cancel = cancel;
        }

        /// <summary>
        /// Gets or sets whether the queue should cancel or not.
        /// </summary>
        public bool Cancel { get; set; }
    }
}
