using System;
using Sundstrom.Tasks.Scheduling;

namespace Sundstrom.Tasks
{
    public interface ITaskQueue
    {
        bool CancelOnException { get; set; }
        int Count { get; }
        object Data { get; set; }
        TimeSpan Delay { get; set; }
        bool IsEmpty { get; }
        bool IsRunning { get; }
        bool IsStarted { get; }
        bool IsStopped { get; }
        IScheduler Scheduler { get; }
        string Tag { get; set; }

        event EventHandler<QueueEventArgs> Empty;
        event EventHandler<QueueEventArgs> Started;
        event EventHandler<QueueEventArgs> Stopped;
        event EventHandler<TaskEventArgs> TaskCanceled;
        event EventHandler<TaskCancelingEventArgs> TaskCanceling;
        event EventHandler<TaskExceptionEventArgs> TaskException;
        event EventHandler<TaskEventArgs> TaskExecuted;
        event EventHandler<TaskEventArgs> TaskExecuting;
        event EventHandler<TaskEventArgs> TaskScheduled;

        ITaskQueue Clear();
        ITaskQueue Deschedule(TaskInfo task);
        ITaskQueue Schedule(TaskInfo task);
        ITaskQueue Start();
        ITaskQueue Stop();
    }
}