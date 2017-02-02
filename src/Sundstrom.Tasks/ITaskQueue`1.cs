using System;
using Sundstrom.Tasks.Scheduling;

namespace Sundstrom.Tasks
{

    public interface ITaskQueue<TTaskInfo> : ITaskQueue
        where TTaskInfo : TaskInfo
    {
        new TaskQueue<TTaskInfo> Clear();
        ITaskQueue<TTaskInfo> Deschedule(TTaskInfo task);
        ITaskQueue<TTaskInfo> Schedule(TTaskInfo task);
        new TaskQueue<TTaskInfo> Start();
        new TaskQueue<TTaskInfo> Stop();
    }
}