using System.Collections.Generic;

namespace Sundstrom.Tasks.Scheduling
{
    public interface ITaskCollection : IEnumerable<TaskInfo>
    {
        int Count { get; }

        TaskInfo Dequeue();
        void Enqueue(TaskInfo item);
        TaskInfo Peek();
        bool Remove(TaskInfo item);
    }
}