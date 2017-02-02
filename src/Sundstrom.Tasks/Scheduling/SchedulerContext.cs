using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sundstrom.Tasks.Scheduling
{
    public class SchedulerContext<TTaskInfo> : ISchedulerContext<TTaskInfo>
        where TTaskInfo : TaskInfo
    {
        private CancellationTokenSource cts = new CancellationTokenSource();

        internal TTaskInfo _current;

        internal Action<QueueEventArgs> _queueEmpty;
        internal Action<QueueEventArgs> _queueStarted;
        internal Action<QueueEventArgs> _queueStopped;

        internal Action<TaskEventArgs> _taskScheduled;
        internal Action<TaskCancelingEventArgs> _taskCanceling;
        internal Action<TaskEventArgs> _taskCanceled;
        internal Action<TaskEventArgs> _taskExecuting;
        internal Action<TaskEventArgs> _taskExecuted;
        internal Action<TaskExceptionEventArgs> _taskException;

        public SchedulerContext(ITaskCollection<TTaskInfo> queue, CancellationTokenSource cts)
        {
            this.cts = cts;

            Queue = queue;
            IsInvalid = false;
        }

        internal CancellationTokenSource CancellationTokenSource => cts;

        public CancellationToken CancellationToken => cts.Token;

        public TimeSpan Delay { get; internal set; }

        public bool CancelOnException { get; internal set; }

        public ITaskCollection<TTaskInfo> Queue { get; private set; }

        ITaskCollection ISchedulerContext.Queue => Queue;

        public bool IsInvalid { get; private set; }

        public void Invalidate() => IsInvalid = true;

        public bool IsStarted { get; internal set; }

        public bool IsRunning { get; internal set; }

        public bool IsStopped { get; internal set; }

        public TTaskInfo Current => _current;

        TaskInfo ISchedulerContext.Current => _current;

        internal bool IsBusy { get; set; }

        public void Remove(TTaskInfo task) => Queue.Remove(task);

        void ISchedulerContext.Remove(TaskInfo task) => Queue.Remove((TTaskInfo)task);

        public void RaiseQueueEmpty(QueueEventArgs e) => _queueEmpty?.Invoke(e);

        public void RaiseQueueStarted(QueueEventArgs e) => _queueStarted?.Invoke(e);

        public void RaiseQueueStopped(QueueEventArgs e) => _queueStopped?.Invoke(e);

        public void RaiseTaskScheduled(TaskEventArgs e) => _taskScheduled?.Invoke(e);

        public void RaiseTaskCanceling(TaskCancelingEventArgs e) => _taskCanceling?.Invoke(e);

        public void RaiseTaskCanceled(TaskEventArgs e) => _taskCanceled?.Invoke(e);

        public void RaiseTaskExecuting(TaskEventArgs e) => _taskExecuting?.Invoke(e);

        public void RaiseTaskExecuted(TaskEventArgs e) => _taskExecuted?.Invoke(e);

        public void RaiseTaskException(TaskExceptionEventArgs e) => _taskException?.Invoke(e);
    }
}
