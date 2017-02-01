using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sundstrom.Tasks.Scheduling
{
    public interface IScheduler<TTaskInfo, TSchedulerContext> : IScheduler
        where TTaskInfo : TaskInfo
        where TSchedulerContext : ISchedulerContext<TTaskInfo>
    {
        new TTaskInfo Current { get; }

        void Deschedule(TSchedulerContext context, TTaskInfo task);
        void Schedule(TSchedulerContext context, TTaskInfo task);
        void Clear(TSchedulerContext context);
        Task Next(TSchedulerContext context);
        void Start(TSchedulerContext context);
        void Stop(TSchedulerContext context);

        TSchedulerContext GetContext(TaskQueue taskQueue, ITaskCollection<TTaskInfo> items, CancellationTokenSource cts, QueueData queueData);
        new ITaskCollection<TTaskInfo> CreateCollection();
    }
}