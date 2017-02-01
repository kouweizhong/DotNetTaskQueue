using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sundstrom.Tasks.Scheduling
{
    public class TaskCollection : IEnumerable<TaskInfo>, ITaskCollection
    {
        private Queue<TaskInfo> _internalQueue = new Queue<TaskInfo>();

        public IEnumerator<TaskInfo> GetEnumerator()
        {
            return _internalQueue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enqueue(TaskInfo item)
        {
            _internalQueue.Enqueue(item);
        }

        public TaskInfo Peek()
        {
            return _internalQueue.Peek();
        }

        public TaskInfo Dequeue()
        {
            return _internalQueue.Dequeue();
        }

        public bool Remove(TaskInfo item)
        {
            var result = _internalQueue.SingleOrDefault(x => x == item);
            if(result != null)
            {
                var newList = _internalQueue.ToList();
                newList.Remove(item);
                _internalQueue = new Queue<TaskInfo>(newList);
                return true;
            }
            return false;
        }

        public int Count => _internalQueue.Count;
    }
}
