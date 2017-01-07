using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sundstrom.Tasks
{
    public abstract class TaskEventArgsBase : EventArgs
    {
        internal TaskEventArgsBase(string tag)
        {
            Tag = tag;
        }

        /// <summary>
        /// Gets the tag associated with this task. (if any)
        /// </summary>
        public string Tag { get; }
    }
}
