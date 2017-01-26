using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sundstrom.Tasks;

namespace Sundstrom.Tasks.Scheduling
{
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
            if (!_isStarted)
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
            if (_isStarted)
            {
                throw new InvalidCastException("Queue has already been started.");
            }
            _isStarted = true;

            Next(context);
        }
    }
}
