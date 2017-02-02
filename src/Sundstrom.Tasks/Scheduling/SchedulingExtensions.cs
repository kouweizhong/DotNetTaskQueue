namespace Sundstrom.Tasks.Scheduling
{
    public static class SchedulingExtensions
    {
        /// <summary>
        /// Get SchedulerContext.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ISchedulerContext GetSchedulerContext<TTaskInfo>(this ITaskQueue<TTaskInfo> source)
            where TTaskInfo : TaskInfo
            => (source as TaskQueue<TTaskInfo>)._schedulerContext;

        public static TSchedulerContext GetSchedulerContext<TScheduler, TSchedulerContext, TTaskInfo>(this ITaskQueue<TTaskInfo> source)
            where TScheduler : IScheduler<TTaskInfo, TSchedulerContext>
            where TSchedulerContext : ISchedulerContext<TTaskInfo>
            where TTaskInfo : TaskInfo
            => (TSchedulerContext)(source as TaskQueue<TTaskInfo>)._schedulerContext;

        public static TTaskInfo GetCurrentTask<TTaskInfo>(this ITaskQueue<TTaskInfo> source)
            where TTaskInfo : TaskInfo
            => (source as TaskQueue<TTaskInfo>)._schedulerContext.Current;
    }
}