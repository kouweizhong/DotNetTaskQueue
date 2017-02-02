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
        public static ITaskQueue<TTaskInfo> Schedule<TTaskInfo>(this ITaskQueue<TTaskInfo> source, Func<TaskInfo, CancellationToken, Task> action)
            where TTaskInfo : TaskInfo
        {
            var taskInfo = new TaskInfo(source, action);
            return (ITaskQueue<TTaskInfo>)source.Schedule(taskInfo);
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static ITaskQueue<TTaskInfo> Schedule<TTaskInfo>(this ITaskQueue<TTaskInfo> source, string tag, Func<TaskInfo, CancellationToken, Task> action)
            where TTaskInfo : TaskInfo
        {
            var taskInfo = new TaskInfo(source, action, tag);
            return (ITaskQueue<TTaskInfo>)source.Schedule(taskInfo);
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static ITaskQueue<TTaskInfo> Schedule<TTaskInfo>(this ITaskQueue<TTaskInfo> source, Action<TaskInfo, CancellationToken> action)
            where TTaskInfo : TaskInfo
        {
            var taskInfo = new TaskInfo(source, async (q, ct) => action(q, ct));
            return (ITaskQueue<TTaskInfo>)source.Schedule(taskInfo);
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static ITaskQueue<TTaskInfo> Schedule<TTaskInfo>(this ITaskQueue<TTaskInfo> source, string tag, Action<TaskInfo, CancellationToken> action)
            where TTaskInfo : TaskInfo
        {
            var taskInfo = new TaskInfo(source, async (q, ct) => action(q, ct), tag);
            return (ITaskQueue<TTaskInfo>)source.Schedule(taskInfo);
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static ITaskQueue<TTaskInfo> Schedule<TTaskInfo>(this ITaskQueue<TTaskInfo> source, Task task)
            where TTaskInfo : TaskInfo
        {
            var taskInfo = new TaskInfo(source, async (q, ct) => await task);
            return (ITaskQueue<TTaskInfo>)source.Schedule(taskInfo);
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public static ITaskQueue<TTaskInfo> Schedule<TTaskInfo>(this ITaskQueue<TTaskInfo> source, string tag, Task task)
            where TTaskInfo : TaskInfo
        {
            var taskInfo = new TaskInfo(source, async (q, ct) => await task);
            return (ITaskQueue<TTaskInfo>)source.Schedule(taskInfo);
        }

        /// <summary>
        /// Cancel the task with the specified tag.
        /// <param name="tag"></param>
        /// </summary>
        public static ITaskQueue<TTaskInfo> Deschedule<TTaskInfo>(this ITaskQueue<TTaskInfo> source, string tag)
            where TTaskInfo : TaskInfo
        {
            var context = source.GetSchedulerContext();
            var queue = context.Queue;
            var item = queue.FirstOrDefault(x => x.Tag == tag);
            if (item == null)
            {
                throw new InvalidOperationException($"Task with tag \"{tag}\" does not exist.");
            }
            return (ITaskQueue<TTaskInfo>)source.Deschedule(item);
        }

        /// <summary>
        /// Waits for the queue to be empty.
        /// </summary>
        /// <returns>The current queue.</returns>
        public static async Task<ITaskQueue<TTaskInfo>> AwaitIsEmpty<TTaskInfo>(this ITaskQueue<TTaskInfo> source)
            where TTaskInfo : TaskInfo
        {
            if(source.IsEmpty)
            {
                return source;
            }
            EventHandler<QueueEventArgs> handler = null;
            var tcs = new TaskCompletionSource<ITaskQueue<TTaskInfo>>();
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
        public static async Task<ITaskQueue<TTaskInfo>> AwaitIsStopped<TTaskInfo>(this ITaskQueue<TTaskInfo> source)
            where TTaskInfo : TaskInfo
        {
            if(source.IsStopped)
            {
                return source;
            }
            EventHandler<QueueEventArgs> handler = null;
            var tcs = new TaskCompletionSource<ITaskQueue<TTaskInfo>>();
            handler = (s, e) => {
                source.Stopped -= handler;
                tcs.SetResult(source);
            };
            source.Stopped += handler;
            return await tcs.Task;
        }
    }
}
