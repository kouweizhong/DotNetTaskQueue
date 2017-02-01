using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sundstrom.Tasks.Scheduling
{
    public sealed class DefaultScheduler : Scheduler
    {
        public DefaultScheduler()
        {
        }

        private bool _isStarted;

        private bool _isStopped;

        private bool _isRunning;

        private bool _isBusy;

        private TaskInfo _current;

        public override bool IsStarted => _isStarted;

        public override bool IsRunning => _isRunning;

        public override bool IsStopped => _isStopped;

        public override TaskInfo Current => _current;

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

                if (context.Queue.Count > 0)
                {
                    // Optional delay.

                    if (context.Delay > default(TimeSpan))
                    {
                        await Task.Delay(context.Delay);
                    }

                    // Peek the current task.

                    _current = context.Queue.Peek();

                    try
                    {
                        _isRunning = true;
                        _isBusy = true;

                        // Execute the current task.

                        context.RaiseTaskExecuting(new TaskEventArgs(_current));

                        await _current.Action(_current, context.CancellationToken);
                    }
                    catch (Exception exc)
                    {
                        // Handle any exception thrown inside a task.
                        // Invoke the Exception event handlers.

                        var eventArgs = new TaskExceptionEventArgs(_current, exc, context.CancelOnException);

                        context.RaiseTaskException(eventArgs);

                        if (eventArgs.Cancel)
                        {
                            // Cancel the queue.

                            ClearCore(context);
                        }
                    }

                    context.RaiseTaskExecuted(new TaskEventArgs(_current));

                    _current = null;

                    // Dequeue the currently finished task and request the next.

                    if (context.Queue.Count > 0)
                    {
                        context.Queue.Dequeue();
                    }
                    _isBusy = false;
                    await Next(context);
                }
                else
                {
                    context.RaiseQueueEmpty(new QueueEventArgs());
                }
            }
            else
            {
                _isRunning = false;
            }
        }

        public override void Schedule(SchedulerContext context, TaskInfo task)
        {
            context.Queue.Enqueue(task);
            context.RaiseTaskScheduled(new TaskEventArgs(task));

            Next(context);
        }

        public override void Stop(SchedulerContext context)
        {
            if (!_isStarted)
            {
                throw new InvalidCastException("Queue has not been started.");
            }

            context.CancellationTokenSource.Cancel();

            _isStarted = false;
            _isStopped = true;
            _isRunning = false;
            _isBusy = false;

            context.RaiseQueueStopped(new QueueEventArgs());
        }

        public override void Deschedule(SchedulerContext context, TaskInfo task)
        {
            var item = context.Queue.FirstOrDefault(x => x == task);
            if (item == null)
            {
                throw new InvalidOperationException("Not part of this queue.");
            }

            var e = new TaskCancelingEventArgs(task, true);
            context.RaiseTaskCanceling(e);
            if (e.Cancel)
            {
                context.Remove(item);

                var e2 = new TaskEventArgs(task);
                context.RaiseTaskCanceled(e2);
            }
        }

        public override void Clear(SchedulerContext context)
        {
            if (_isBusy) throw new InvalidOperationException();

            while (context.Queue.Count > 0)
            {
                var task = context.Queue.Dequeue();
                context.RaiseTaskCanceled(new TaskEventArgs(task));
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
            _isStopped = false;

            Next(context);

            context.RaiseQueueStarted(new QueueEventArgs());
        }
    }
}
