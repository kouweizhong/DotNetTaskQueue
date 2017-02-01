using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sundstrom.Tasks.Scheduling
{
    public class SchedulerContext
    {
        private CancellationTokenSource cts = new CancellationTokenSource();

        internal Action<QueueEventArgs> _queueEmpty;
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

            Queue = queue;
            IsInvalid = false;
        }

        internal CancellationTokenSource CancellationTokenSource => cts;

        public CancellationToken CancellationToken => cts.Token;

        public TimeSpan Delay { get; internal set; }

        public bool CancelOnException { get; internal set; }

        public Queue<TaskInfo> Queue { get; private set; }

        internal bool IsInvalid { get; private set; }

        internal void Invalidate() => IsInvalid = true;

        public void Remove(TaskInfo task) => Queue = new Queue<TaskInfo>(Queue.Where(x => x != task));

        public void RaiseQueueEmpty(QueueEventArgs e) => _queueEmpty(e);

        public void RaiseQueueStarted(QueueEventArgs e) => _queueStarted(e);

        public void RaiseQueueStopped(QueueEventArgs e) => _queueStopped(e);

        public void RaiseTaskScheduled(TaskEventArgs e) => _taskScheduled(e);

        public void RaiseTaskCanceling(TaskCancelingEventArgs e) => _taskCanceling(e);

        public void RaiseTaskCanceled(TaskEventArgs e) => _taskCanceled(e);

        public void RaiseTaskExecuting(TaskEventArgs e) => _taskExecuting(e);

        public void RaiseTaskExecuted(TaskEventArgs e) => _taskExecuted(e);

        public void RaiseTaskException(TaskExceptionEventArgs e) => _taskException(e);
    }
}
