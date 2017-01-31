namespace Sundstrom.Tasks.Scheduling
{
    public static class SchedulingExtensions
    {
        /// <summary>
        /// Get SchedulerContext.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static SchedulerContext GetSchedulerContext(this TaskQueue source) => source._context;

        public static TaskInfo GetCurrentTask(this TaskQueue source) => source.Scheduler.Current;
    }
}