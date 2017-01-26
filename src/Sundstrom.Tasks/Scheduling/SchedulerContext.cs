using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sundstrom.Tasks;

namespace Sundstrom.Tasks.Scheduling
{
    public class SchedulerContext
    {
        private CancellationTokenSource cts = new CancellationTokenSource();

        internal Action<TaskEventArgs> _taskScheduled;
        internal Action<TaskEventArgs> _taskCanceled;
        internal Action<TaskEventArgs> _taskExecuting;
        internal Action<TaskEventArgs> _taskExecuted;
        internal Action<TaskExceptionEventArgs> _taskException;

        public SchedulerContext(CancellationTokenSource cts)
        {
            this.cts = cts;
        }

        internal CancellationTokenSource CancellationTokenSource => cts;

        public CancellationToken CancellationToken => cts.Token;

        public TimeSpan Delay { get; internal set; }

        public bool CancelOnException { get; internal set; }

        public void RaiseTaskScheduled(TaskEventArgs e)
        {
            this._taskScheduled(e);
        }

        public void RaiseTaskCanceled(TaskEventArgs e)
        {
            this._taskCanceled(e);
        }

        public void RaiseTaskExecuting(TaskEventArgs e)
        {
            this._taskExecuting(e);
        }

        public void RaiseTaskExecuted(TaskEventArgs e)
        {
            this._taskExecuted(e);
        }

        public void RaiseTaskException(TaskExceptionEventArgs e)
        {
            this._taskException(e);
        }
    }
}
