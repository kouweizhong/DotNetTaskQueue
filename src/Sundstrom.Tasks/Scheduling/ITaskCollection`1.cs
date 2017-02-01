using System.Collections.Generic;

namespace Sundstrom.Tasks.Scheduling
{

    public interface ITaskCollection<TTaskInfo> : ITaskCollection
        where TTaskInfo : TaskInfo
    {

        new TTaskInfo Dequeue();
        void Enqueue(TTaskInfo item);
        new TTaskInfo Peek();
        bool Remove(TTaskInfo item);
    }
}