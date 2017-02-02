using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sundstrom.Tasks.Scheduling
{
    public interface IScheduler
    {
        void Clear(ISchedulerContext context);
        void Deschedule(ISchedulerContext context, TaskInfo task);
        Task Next(ISchedulerContext context);
        void Schedule(ISchedulerContext context, TaskInfo task);
        void Start(ISchedulerContext context);
        void Stop(ISchedulerContext context);

        ISchedulerContext GetContext(ITaskQueue taskQueue, ITaskCollection items, CancellationTokenSource cts, SchedulerContextData queueData);
        ITaskCollection CreateCollection();
    }
}