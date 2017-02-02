using Sundstrom.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sundstrom.Tasks
{
    public static class TaskQueueFactory<TTaskInfo>
        where TTaskInfo : TaskInfo
    {
        private static volatile TaskQueue<TaskInfo> _default;
        private static object syncRoot = new Object();

        /// <summary>
        /// Gets the default queue.
        /// </summary>
        public static TaskQueue<TaskInfo> Default
        {
            get
            {
                if (_default == null)
                {
                    lock (syncRoot)
                    {
                        _default = new TaskQueue<TaskInfo>(string.Empty, null);
                        _queues[string.Empty] = _default;
                    }
                }
                return _default;
            }
        }

        private static Dictionary<string, ITaskQueue> _queues = new Dictionary<string, ITaskQueue>();

        /// <summary>
        /// Gets the existing queues.
        /// </summary>
        public static IEnumerable<ITaskQueue> Queues => _queues.Select(x => x.Value).ToArray();

        /// <summary>
        /// Creates a queue.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TaskQueue<TTaskInfo> Create(string tag, object data = null) => Create(new DefaultScheduler(), tag, data);

        /// <summary>
        /// Creates a queue with a specified Scheduler.
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TaskQueue<TTaskInfo> Create<TScheduler>(TScheduler scheduler, string tag, object data = null)
            where TScheduler : IScheduler
        {
            if (scheduler == null)
                throw new ArgumentNullException(nameof(scheduler));

            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException(nameof(tag), "Null, whitespaces or an empty string is not allowed for tags.");

            try
            {
                var queue = new TaskQueue<TTaskInfo>(scheduler, tag, data);
                _queues[tag] = queue;
                return queue;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Already exists a queue with the specified tag.");
            }
        }

        /// <summary>
        /// Removes the specified queue from the queue collection.
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public static bool Remove(TaskQueue<TTaskInfo> queue) => _queues.Remove(queue.Tag);
    }
}