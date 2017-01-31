using System.Threading.Tasks;

namespace Sundstrom.Tasks.Scheduling
{
    public abstract class Scheduler
    {
        public abstract void Start(SchedulerContext context);

        public abstract void Schedule(SchedulerContext context, TaskInfo task);

        public abstract Task Next(SchedulerContext context);

        public abstract void Clear(SchedulerContext context);

        public abstract void Stop(SchedulerContext context);

        public abstract void Deschedule(SchedulerContext context, TaskInfo task);

        public abstract bool IsStarted { get; }

        public abstract bool IsRunning { get; }

        public abstract bool IsStopped { get; }

        public abstract TaskInfo Current { get; }
    }
}
