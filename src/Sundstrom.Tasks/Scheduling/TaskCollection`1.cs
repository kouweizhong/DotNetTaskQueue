using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sundstrom.Tasks.Scheduling
{

    public class TaskCollection<TTaskInfo> : TaskCollection, ITaskCollection<TTaskInfo>
        where TTaskInfo : TaskInfo
    {
        private Queue<TTaskInfo> _internalQueue = new Queue<TTaskInfo>();

        public new IEnumerator<TTaskInfo> GetEnumerator()
        {
            return _internalQueue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enqueue(TTaskInfo item)
        {
            _internalQueue.Enqueue(item);
        }

        public new TTaskInfo Peek()
        {
            return _internalQueue.Peek();
        }

        public new TTaskInfo Dequeue()
        {
            return _internalQueue.Dequeue();
        }

        public bool Remove(TTaskInfo item)
        {
            var result = _internalQueue.SingleOrDefault(x => x.Equals(item));
            if (result != null)
            {
                var newList = _internalQueue.ToList();
                newList.Remove(item);
                _internalQueue = new Queue<TTaskInfo>(newList);
                return true;
            }
            return false;
        }

        TaskInfo ITaskCollection.Dequeue()
        {
            return Dequeue();
        }

        void ITaskCollection.Enqueue(TaskInfo item)
        {
            Enqueue((TTaskInfo)item);
        }

        TaskInfo ITaskCollection.Peek()
        {
            return Peek();
        }

        bool ITaskCollection.Remove(TaskInfo item)
        {
            return Remove((TTaskInfo)item);
        }

        public new int Count => _internalQueue.Count;
    }
}
