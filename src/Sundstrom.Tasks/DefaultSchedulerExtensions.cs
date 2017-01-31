using Sundstrom.Tasks.Scheduling;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        /// <returns>The current queue.</returns>
        public static async Task<TaskQueue> AwaitIsEmpty(this TaskQueue source)
        {
            if(source.IsEmpty)
            {
                return source;
            }
            EventHandler<QueueEventArgs> handler = null;
            var tcs = new TaskCompletionSource<TaskQueue>();
            handler = (s, e) => {
                source.Empty -= handler;
                tcs.SetResult(source);
            };
            source.Empty += handler;
            return await tcs.Task;
        }

        /// <summary>
        /// Waits for the queue to be stopped.
        /// </summary>
        /// <returns>The current queue.</returns>
        public static async Task<TaskQueue> AwaitIsStopped(this TaskQueue source)
        {
            if(source.IsStopped)
            {
                return source;
            }
            EventHandler<QueueEventArgs> handler = null;
            var tcs = new TaskCompletionSource<TaskQueue>();
            handler = (s, e) => {
                source.Stopped -= handler;
                tcs.SetResult(source);
            };
            source.Stopped += handler;
            return await tcs.Task;
        }
    }
}
