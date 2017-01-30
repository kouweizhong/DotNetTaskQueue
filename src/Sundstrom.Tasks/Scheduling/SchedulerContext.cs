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

        internal Action<QueueEventArgs> _queueStarted;
        internal Action<QueueEventArgs> _queueStopped;

        internal Action<TaskEventArgs> _taskScheduled;
        internal Action<TaskCancelingEventArgs> _taskCanceling;
        internal Action<TaskEventArgs> _taskCanceled;
        internal Action<TaskEventArgs> _taskExecuting;
        internal Action<TaskEventArgs> _taskExecuted;
        internal Action<TaskExceptionEventArgs> _taskException;

        public SchedulerContext(Queue<TaskInfo> queue, CancellationTokenSource cts)
        {
            this.cts = cts;
            
            this.Queue = queue;
            this.IsInvalid = false;
        }

        internal CancellationTokenSource CancellationTokenSource => cts;

        public CancellationToken CancellationToken => cts.Token;

        public TimeSpan Delay { get; internal set; }

        public bool CancelOnException { get; internal set; }

        public Queue<TaskInfo> Queue { get; private set; }

        internal bool IsInvalid { get; private set; }

        internal void Invalidate() 
        {
            IsInvalid = true;
        }

        public void Remove(TaskInfo task) 
        {
            Queue = new Queue<TaskInfo>(Queue.Where(x => x != task));
        }

        public void RaiseQueueStarted(QueueEventArgs e)
        {
            this._queueStarted(e);
        }

        public void RaiseQueueStopped(QueueEventArgs e)
        {
            this._queueStopped(e);
        }

        public void RaiseTaskScheduled(TaskEventArgs e)
        {
            this._taskScheduled(e);
        }

        public void RaiseTaskCanceling(TaskCancelingEventArgs e)
        {
            this._taskCanceling(e);
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
