using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sundstrom.Tasks;

namespace Sundstrom.Tasks.Scheduling
{
    public abstract class Scheduler
    {
        public abstract void Start(SchedulerContext context);

        public abstract void Schedule(SchedulerContext context, TaskInfo task);

        public abstract Task Next(SchedulerContext context);

        public abstract void Clear(SchedulerContext context);

        public abstract void Cancel(SchedulerContext context);

        public abstract bool IsStarted { get; }

        public abstract bool IsRunning { get; }
    }
}
