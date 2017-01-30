using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sundstrom.Tasks;
using Sundstrom.Tasks.Scheduling;

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
            var taskInfo = new TaskInfo(source, action);
            return source.Schedule(taskInfo);
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TaskQueue Schedule(this TaskQueue source, string tag, Func<TaskInfo, CancellationToken, Task> action)
        {
            var taskInfo = new TaskInfo(source, action, tag);
            return source.Schedule(taskInfo);
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TaskQueue Schedule(this TaskQueue source, Action<TaskInfo, CancellationToken> action)
        {
            var taskInfo = new TaskInfo(source, async (q, ct) => action(q, ct));
            return source.Schedule(taskInfo);
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TaskQueue Schedule(this TaskQueue source, string tag, Action<TaskInfo, CancellationToken> action)
        {
            var taskInfo = new TaskInfo(source, async (q, ct) => action(q, ct), tag);
            return source.Schedule(taskInfo);
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static TaskQueue Schedule(this TaskQueue source, Task task)
        {
            var taskInfo = new TaskInfo(source, async (q, ct) => await task);
            return source.Schedule(taskInfo);
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public static TaskQueue Schedule(this TaskQueue source, string tag, Task task)
        {
            var taskInfo = new TaskInfo(source, async (q, ct) => await task);
            return source.Schedule(taskInfo);
        }

        /// <summary>
        /// Cancel the task with the specified tag.
        /// <param name="tag"></param>
        /// </summary>
        public static TaskQueue Deschedule(this TaskQueue source, string tag)
        {
            var context = source.GetSchedulerContext();
            var queue = context.Queue;
            var item = queue.FirstOrDefault(x => x.Tag == tag);
            if (item == null)
            {
                throw new InvalidOperationException($"Task with tag \"{tag}\" does not exist.");
            }
            return source.Deschedule(item);
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

        /// <summary>
        /// Waits for the queue to be stopped.
        /// </summary>
        /// <returns>The current queue.</returns>
        public static Task<TaskQueue> AwaitIsStopped(this TaskQueue source, int checkRate = 200)
        {
            return Task.Run(async () =>
            {
                while (!source.IsStopped)
                {
                    await Task.Delay(checkRate);
                }

                return source;
            });

            /*
            // NOT WORKING //

            EventHandler<QueueEventArgs> handler = null;
            var tcs = new TaskCompletionSource<TaskQueue>();
            handler = (s, e) => {
                // source.Stopped -= handler;
                tcs.SetResult(source);
            };
            source.Stopped += handler;
            return tcs.Task;
            */
        }
    }
}
