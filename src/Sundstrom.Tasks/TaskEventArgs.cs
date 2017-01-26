using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sundstrom.Tasks
{
    public class TaskEventArgs : TaskEventArgsBase
    {
        internal TaskEventArgs(string tag)
            : base(tag)
        {

        }
    }
}
