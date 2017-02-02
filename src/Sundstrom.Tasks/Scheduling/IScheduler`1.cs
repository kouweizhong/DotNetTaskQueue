using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sundstrom.Tasks.Scheduling
{
    public interface IScheduler<TTaskInfo, TSchedulerContext> : IScheduler
        where TTaskInfo : TaskInfo
        where TSchedulerContext : ISchedulerContext<TTaskInfo>
    {
        void Deschedule(TSchedulerContext context, TTaskInfo task);
        void Schedule(TSchedulerContext context, TTaskInfo task);
        void Clear(TSchedulerContext context);
        Task Next(TSchedulerContext context);
        void Start(TSchedulerContext context);
        void Stop(TSchedulerContext context);

        TSchedulerContext GetContext(ITaskQueue<TTaskInfo> taskQueue, ITaskCollection<TTaskInfo> items, CancellationTokenSource cts, SchedulerContextData queueData);
        new ITaskCollection<TTaskInfo> CreateCollection();
    }
}