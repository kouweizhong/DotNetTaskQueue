using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sundstrom.Tasks
{
    public class SchedulerContext
    {
        private CancellationTokenSource cts = new CancellationTokenSource();

        internal Action<TaskEventArgs> _taskScheduled;
        internal Action<TaskEventArgs> _taskCanceled;
        internal Action<TaskEventArgs> _taskExecuting;
        internal Action<TaskEventArgs> _taskExecuted;
        internal Action<TaskExceptionEventArgs> _taskException;

        public SchedulerContext(CancellationTokenSource cts)
        {
            this.cts = cts;
        }

        internal CancellationTokenSource CancellationTokenSource => cts;

        public CancellationToken CancellationToken => cts.Token;

        public TimeSpan Delay { get; internal set; }

        public bool CancelOnException { get; internal set; }

        public void RaiseTaskScheduled(TaskEventArgs e) 
        {
            this._taskScheduled(e);
        }

        public void RaiseTaskCanceled(TaskEventArgs e) 
        {
            this._taskCanceled(e);
        }

        public void RaiseTaskExecuting(TaskEventArgs e) 
        {
            this._taskExecuting(e);
        }

        public void RaiseTaskExecuted(TaskEventArgs e) 
        {
            this._taskExecuted(e);
        }

        public void RaiseTaskException(TaskExceptionEventArgs e) 
        {
            this._taskException(e);
        }
    }

    public abstract class Scheduler
    {
        public abstract void Start(SchedulerContext context);

        public abstract void Schedule(SchedulerContext context, TaskInfo task);

        public abstract Task Next(SchedulerContext context);

        public abstract void Clear(SchedulerContext context);

        public abstract void Cancel(SchedulerContext context);

        public abstract int Count { get; }

        public abstract bool IsEmpty { get; }

        public abstract bool IsStarted { get; }

        public abstract bool IsRunning { get; }
    }

    public sealed class DefaultScheduler : Scheduler
    { 
        private Queue<TaskInfo> queue = new Queue<TaskInfo>();

        private bool _isStarted;

        private bool _isRunning;

        private bool _isBusy;

        public override int Count => queue.Count;

        public override bool IsEmpty => queue.Count == 0;

        public override bool IsStarted
        {
            get 
            {
                return _isStarted;
            }
        }

        public override bool IsRunning
        {
            get 
            {
                return _isRunning;
            }
        }

        public override async Task Next(SchedulerContext context) 
        {
           if (context.CancellationToken.IsCancellationRequested)
                return;

            if (!_isBusy)
            {
                // Do not fetch next task if the queue has not been started.
                if (!_isStarted) 
                {
                    return;
                }

                if (queue.Count > 0)
                {
                    // Optional delay.

                    if (context.Delay > default(TimeSpan))
                    {
                        await Task.Delay(context.Delay);
                    }

                    // Peek the current task.

                    var task = queue.Peek();

                    try
                    {
                        _isRunning = true;
                        _isBusy = true;

                        // Execute the current task.

                        context.RaiseTaskExecuting(new TaskEventArgs(task.Tag));

                        await task.Action(task, context.CancellationToken);
                    }
                    catch (Exception exc)
                    {
                        // Handle any exception thrown inside a task.
                        // Invoke the Exception event handlers.

                        var eventArgs = new TaskExceptionEventArgs(task.Tag, exc, context.CancelOnException);

                        context.RaiseTaskException(eventArgs);

                        if (eventArgs.Cancel)
                        {
                            // Cancel the queue.

                            ClearCore(context);
                        }
                    }

                    context.RaiseTaskExecuted(new TaskEventArgs(task.Tag));

                    // Dequeue the currently finished task and request the next.

                    if (queue.Count > 0)
                    {
                        queue.Dequeue();
                    }
                    _isBusy = false;
                    await Next(context);
                }
            }
            else
            {
                _isRunning = false;
            }
        }

        public override void Schedule(SchedulerContext context, TaskInfo task)
        {
            queue.Enqueue(task);
            context.RaiseTaskScheduled(new TaskEventArgs(task.Tag));

            Next(context);
        }

        public override void Cancel(SchedulerContext context)
        {
            if(!_isStarted)
            {
                throw new InvalidCastException("Queue has not been started.");
            }

            context.CancellationTokenSource.Cancel();

             _isStarted = false;
            _isRunning = false;
            _isBusy = false;
        }

        public override void Clear(SchedulerContext context)
        {
            if (_isBusy) throw new InvalidOperationException();
            
            while (queue.Count > 0)
            {
                var task = queue.Dequeue();
                context.RaiseTaskCanceled(new TaskEventArgs(task.Tag));
            }
        }

        private void ClearCore(SchedulerContext context) 
        {
            _isBusy = false;
            Clear(context);
        }

        public override void Start(SchedulerContext context)
        {
            if(_isStarted)
            {
                throw new InvalidCastException("Queue has already been started.");
            }
            _isStarted = true;

            Next(context);
        }
    }

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
