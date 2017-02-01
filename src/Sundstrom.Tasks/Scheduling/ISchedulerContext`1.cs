﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Sundstrom.Tasks.Scheduling
{

    public interface ISchedulerContext<TTaskInfo> : ISchedulerContext
        
        where TTaskInfo : TaskInfo
    {
        new ITaskCollection<TTaskInfo> Queue { get; }
        void Remove(TTaskInfo task);
    }
}