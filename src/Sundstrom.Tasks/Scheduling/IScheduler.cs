using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sundstrom.Tasks.Scheduling
{
    public interface IScheduler
    {
        TaskInfo Current { get; }
        bool IsRunning { get; }
        bool IsStarted { get; }
        bool IsStopped { get; }

        void Clear(ISchedulerContext context);
        void Deschedule(ISchedulerContext context, TaskInfo task);
        Task Next(ISchedulerContext context);
        void Schedule(ISchedulerContext context, TaskInfo task);
        void Start(ISchedulerContext context);
        void Stop(ISchedulerContext context);

        ISchedulerContext GetContext(TaskQueue taskQueue, ITaskCollection items, CancellationTokenSource cts, QueueData queueData);
        ITaskCollection CreateCollection();
    }
}