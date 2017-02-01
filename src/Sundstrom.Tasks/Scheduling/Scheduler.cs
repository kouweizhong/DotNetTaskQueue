using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sundstrom.Tasks.Scheduling
{
    public abstract class Scheduler<TTaskInfo, TSchedulerContext> : IScheduler<TTaskInfo, TSchedulerContext>
        where TTaskInfo : TaskInfo
        where TSchedulerContext : ISchedulerContext<TTaskInfo>
    {
        public abstract void Start(TSchedulerContext context);

        public abstract void Schedule(TSchedulerContext context, TTaskInfo task);

        public abstract Task Next(TSchedulerContext context);

        public abstract void Clear(TSchedulerContext context);

        public abstract void Stop(TSchedulerContext context);

        public abstract void Deschedule(TSchedulerContext context, TTaskInfo task);

        void IScheduler.Start(ISchedulerContext context)
        {
            Start((TSchedulerContext)context);
        }

        void IScheduler.Stop(ISchedulerContext context)
        {
            Stop((TSchedulerContext)context);
        }

        void IScheduler.Clear(ISchedulerContext context)
        {
            Clear((TSchedulerContext)context);
        }

        Task IScheduler.Next(ISchedulerContext context)
        {
            return Next((TSchedulerContext)context);
        }

        void IScheduler.Deschedule(ISchedulerContext context, TaskInfo task)
        {
            Deschedule((TSchedulerContext)context, (TTaskInfo)task);
        }

        void IScheduler.Schedule(ISchedulerContext context, TaskInfo task)
        {
            Schedule((TSchedulerContext)context, (TTaskInfo)task);
        }

        public abstract TSchedulerContext GetContext(TaskQueue taskQueue, ITaskCollection<TTaskInfo> items, CancellationTokenSource cts, QueueData queueData);

        ISchedulerContext IScheduler.GetContext(TaskQueue taskQueue, ITaskCollection items, CancellationTokenSource cts, QueueData queueData)
        {
            return GetContext(taskQueue, (ITaskCollection<TTaskInfo>)items, cts, queueData);
        }

        public abstract ITaskCollection<TTaskInfo> CreateCollection();

        ITaskCollection IScheduler.CreateCollection()
        {
            return CreateCollection();
        }

        public abstract bool IsStarted { get; }

        public abstract bool IsRunning { get; }

        public abstract bool IsStopped { get; }

        public abstract TTaskInfo Current { get; }

        TaskInfo IScheduler.Current => Current;
    }
}
