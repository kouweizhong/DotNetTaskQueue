using System;
using System.Collections.Generic;
using System.Threading;

namespace Sundstrom.Tasks.Scheduling
{
    public interface ISchedulerContext
    {
        CancellationToken CancellationToken { get; }
        bool CancelOnException { get; }
        TimeSpan Delay { get; }

        ITaskCollection Queue { get; }
        TaskInfo Current { get; }
        bool IsRunning { get; }
        bool IsStarted { get; }
        bool IsStopped { get; }

        void RaiseQueueEmpty(QueueEventArgs e);
        void RaiseQueueStarted(QueueEventArgs e);
        void RaiseQueueStopped(QueueEventArgs e);
        void RaiseTaskCanceled(TaskEventArgs e);
        void RaiseTaskCanceling(TaskCancelingEventArgs e);
        void RaiseTaskException(TaskExceptionEventArgs e);
        void RaiseTaskExecuted(TaskEventArgs e);
        void RaiseTaskExecuting(TaskEventArgs e);
        void RaiseTaskScheduled(TaskEventArgs e);
        void Remove(TaskInfo task);

        bool IsInvalid { get; }
        void Invalidate();
    }
}