using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sundstrom.Tasks;

namespace Sundstrom.Tasks
{
    public static class DefaultSchedulerExtensions
    {
        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TaskQueue Schedule(this TaskQueue source, Func<TaskInfo, CancellationToken, Task> action)
        {
            var context = new TaskInfo(source, action);
            return source.Schedule(context);
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TaskQueue Schedule(this TaskQueue source, string tag, Func<TaskInfo, CancellationToken, Task> action)
        {
            var context = new TaskInfo(source, action, tag);
            return source.Schedule(context);
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TaskQueue Schedule(this TaskQueue source, Action<TaskInfo, CancellationToken> action)
        {
            var context = new TaskInfo(source, async (q, ct) => action(q, ct));
            return source.Schedule(context);
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TaskQueue Schedule(this TaskQueue source, string tag, Action<TaskInfo, CancellationToken> action)
        {
            var context = new TaskInfo(source, async (q, ct) => action(q, ct), tag);
            return source.Schedule(context);
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static TaskQueue Schedule(this TaskQueue source, Task task)
        {
            var context = new TaskInfo(source, async (q, ct) => await task);
            return source.Schedule(context);
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public static TaskQueue Schedule(this TaskQueue source, string tag, Task task)
        {
            var context = new TaskInfo(source, async (q, ct) => await task);
            return source.Schedule(context);
        }

           /// <summary>
        /// Waits for the queue to be empty.
        /// </summary>
        /// <param name="checkRate"></param>
        /// <param name="value"></param>
        /// <returns>The current queue.</returns>
        public static Task<TaskQueue> AwaitIsEmpty(this TaskQueue source, int checkRate = 200, bool value = true)
        {
            return Task.Run(async () =>
            {
                while (source.IsEmpty != value)
                {
                    await Task.Delay(checkRate);
                }

                return source;
            });
        }

    }
}
