using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sundstrom.Tasks.Scheduling
{
    public sealed class DefaultScheduler : Scheduler<TaskInfo, SchedulerContext<TaskInfo>>
    {
        public override async Task Next(SchedulerContext<TaskInfo> context)
        {
            if (context.CancellationToken.IsCancellationRequested)
                return;

            if (!context.IsBusy)
            {
                // Do not fetch next task if the queue has not been started.
                if (!context.IsStarted)
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

                    context._current = context.Queue.Peek();

                    try
                    {
                        context.IsRunning = true;
                        context.IsBusy = true;

                        // Execute the current task.

                        context.RaiseTaskExecuting(new TaskEventArgs(context._current));

                        await context._current.Action(context._current, context.CancellationToken);
                    }
                    catch (Exception exc)
                    {
                        // Handle any exception thrown inside a task.
                        // Invoke the Exception event handlers.

                        var eventArgs = new TaskExceptionEventArgs(context._current, exc, context.CancelOnException);

                        context.RaiseTaskException(eventArgs);

                        if (eventArgs.Cancel)
                        {
                            // Cancel the queue.

                            ClearCore(context);
                        }
                    }

                    context.RaiseTaskExecuted(new TaskEventArgs(context._current));

                    context._current = null;

                    // Dequeue the currently finished task and request the next.

                    if (context.Queue.Count > 0)
                    {
                        context.Queue.Dequeue();
                    }
                    context.IsBusy = false;
                    await Next(context);
                }
                else
                {
                    context.RaiseQueueEmpty(new QueueEventArgs());
                }
            }
            else
            {
                context.IsRunning = false;
            }
        }

        public override void Schedule(SchedulerContext<TaskInfo> context, TaskInfo task)
        {
            context.Queue.Enqueue(task);
            context.RaiseTaskScheduled(new TaskEventArgs(task));

            Next(context);
        }

        public override void Stop(SchedulerContext<TaskInfo> context)
        {
            if (!context.IsStarted)
            {
                throw new InvalidCastException("Queue has not been started.");
            }

            context.CancellationTokenSource.Cancel();

            context.IsStarted = false;
            context.IsStopped = true;
            context.IsRunning = false;
            context.IsBusy = false;

            context.RaiseQueueStopped(new QueueEventArgs());
        }

        public override void Deschedule(SchedulerContext<TaskInfo> context, TaskInfo task)
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

        public override void Clear(SchedulerContext<TaskInfo> context)
        {
            if (context.IsBusy) throw new InvalidOperationException();

            while (context.Queue.Count > 0)
            {
                var task = context.Queue.Dequeue();
                context.RaiseTaskCanceled(new TaskEventArgs(task));
            }
        }

        private void ClearCore(SchedulerContext<TaskInfo> context)
        {
            context.IsBusy = false;
            Clear(context);
        }

        public override void Start(SchedulerContext<TaskInfo> context)
        {
            if (context.IsStarted)
            {
                throw new InvalidCastException("Queue has already been started.");
            }
            context.IsStarted = true;
            context.IsStopped = false;

            Next(context);

            context.RaiseQueueStarted(new QueueEventArgs());
        }

        public override SchedulerContext<TaskInfo> GetContext(ITaskQueue<TaskInfo> taskQueue, ITaskCollection<TaskInfo> items, CancellationTokenSource cts, SchedulerContextData queueData)
        {
            return new SchedulerContext<TaskInfo>(items, cts)
            {
                Delay = taskQueue.Delay,
                CancelOnException = taskQueue.CancelOnException,

                _queueEmpty = queueData._queueEmpty,

                _queueStarted = queueData._queueStarted,
                _queueStopped = queueData._queueStopped,

                _taskScheduled = queueData._taskScheduled,
                _taskCanceled = queueData._taskCanceled,
                _taskCanceling = queueData._taskCanceling,
                _taskExecuting = queueData._taskExecuting,
                _taskExecuted = queueData._taskExecuted,
                _taskException = queueData._taskException
            };
        }

        public override ITaskCollection<TaskInfo> CreateCollection()
        {
            return new TaskCollection<TaskInfo>();
        }
    }
}
