using System;
using Sundstrom.Tasks.Scheduling;

namespace Sundstrom.Tasks
{

    public interface ITaskQueue<TTaskInfo> : ITaskQueue
        where TTaskInfo : TaskInfo
    {
        new ITaskQueue<TTaskInfo> Clear();
        ITaskQueue<TTaskInfo> Deschedule(TTaskInfo task);
        ITaskQueue<TTaskInfo> Schedule(TTaskInfo task);
        new ITaskQueue<TTaskInfo> Start();
        new ITaskQueue<TTaskInfo> Stop();
    }
}