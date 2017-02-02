using System;

namespace Sundstrom.Tasks.Scheduling
{
    public sealed class SchedulerContextData
    {
        internal Action<QueueEventArgs> _queueEmpty;
        internal Action<QueueEventArgs> _queueStarted;
        internal Action<QueueEventArgs> _queueStopped;

        internal Action<TaskEventArgs> _taskScheduled;
        internal Action<TaskCancelingEventArgs> _taskCanceling;
        internal Action<TaskEventArgs> _taskCanceled;
        internal Action<TaskEventArgs> _taskExecuting;
        internal Action<TaskEventArgs> _taskExecuted;
        internal Action<TaskExceptionEventArgs> _taskException;

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