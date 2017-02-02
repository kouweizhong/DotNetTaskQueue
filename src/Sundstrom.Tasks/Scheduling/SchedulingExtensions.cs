namespace Sundstrom.Tasks.Scheduling
{
    public static class SchedulingExtensions
    {
        /// <summary>
        /// Get SchedulerContext.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ISchedulerContext GetSchedulerContext(this TaskQueue source) => source._schedulerContext;

        //public static TSchedulerContext GetSchedulerContext<TScheduler, TSchedulerContext, TTaskInfo>(this TaskQueue<TTaskInfo, TScheduler> source)
        //    where TScheduler : IScheduler<TTaskInfo, TSchedulerContext>
        //    where TSchedulerContext : ISchedulerContext<TTaskInfo>
        //    where TTaskInfo : TaskInfo
        //    => (TSchedulerContext)source._schedulerContext;

        public static TaskInfo GetCurrentTask(this TaskQueue source) => source._schedulerContext.Current;
    }
}