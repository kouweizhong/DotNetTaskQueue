using Sundstrom.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sundstrom.Tasks
{

    /// <summary>
    /// Simple task queue
    /// </summary>
    public class TaskQueue<TTaskInfo> : ITaskQueue<TTaskInfo>
        where TTaskInfo : TaskInfo
    {
        internal ISchedulerContext<TTaskInfo> _schedulerContext;

        private bool _cancelOnException = true;
        private TimeSpan _delay;

        /// <summary>
        /// Initializes a TaskQueue.
        /// </summary>
        public TaskQueue()
        {
            Scheduler = new DefaultScheduler();
        }

        /// <summary>
        /// Initializes a TaskQueue with a tag.
        /// </summary>
        /// <param name="tag"></param>
        public TaskQueue(string tag) : this()
        {
            Tag = tag;
        }

        /// <summary>
        /// Initializes a TaskQueue with a tag and some data.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        public TaskQueue(string tag, object data)
            : this(tag)
        {
            Data = data;
        }

        /// <summary>
        /// Initializes a TaskQueue.
        /// </summary>
        public TaskQueue(IScheduler scheduler)
        {
            Scheduler = scheduler;
        }

        /// <summary>
        /// Initializes a TaskQueue with a tag.
        /// </summary>
        /// <param name="tag"></param>
        public TaskQueue(IScheduler scheduler, string tag)
        : this(scheduler)
        {
            Tag = tag;
        }

        /// <summary>
        /// Initializes a TaskQueue with a tag and some data.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        public TaskQueue(IScheduler scheduler, string tag, object data)
        : this(scheduler, tag)
        {
            Data = data;
        }

        /// <summary>
        /// Gets or sets the identifier that is associated with the queue. (if any)
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets some data that is associated with the queue.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Gets the scheduler.
        /// </summary>
        /// <value>The scheduler.</value>
        public IScheduler Scheduler { get; }

        /// <summary>
        /// Starts the queue.
        /// </summary>
        /// <returns>The start.</returns>
        public TaskQueue<TTaskInfo> Start()
        {
            EnsureSchedulerContextIsSet();

            Scheduler.Start(_schedulerContext);

            return this;
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <returns>The schedule.</returns>
        /// <param name="task">Task.</param>
        public ITaskQueue<TTaskInfo> Schedule(TTaskInfo task)
        {
            EnsureSchedulerContextIsSet();

            if(task.Queue == null)
            {
                task.Queue = this;
            }

            Scheduler.Schedule(_schedulerContext, task);
            return this;
        }

        /// <summary>
        /// Stops the queue.
        /// </summary>
        public TaskQueue<TTaskInfo> Stop()
        {
            _schedulerContext.Invalidate();

            Scheduler.Stop(_schedulerContext);

            return this;
        }

        /// <summary>
        /// Deschedule the task.
        /// <param name="task"></param>
        /// </summary>
        public ITaskQueue<TTaskInfo> Deschedule(TTaskInfo task)
        {
            Scheduler.Deschedule(_schedulerContext, task);

            return this;
        }

        /// <summary>
        /// Clear the queue and cancel all unexecuted tasks.
        /// </summary>
        public TaskQueue<TTaskInfo> Clear()
        {
            Scheduler.Clear(_schedulerContext);

            return this;
        }

        public TimeSpan Delay
        {
            get => _delay;
            set
            {
                if (_schedulerContext != null && _schedulerContext.IsStarted)
                {
                    throw new InvalidOperationException("Can not be set when the queue is running.");
                }
                _delay = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether or not the queue should cancel on exception. (Default: true)
        /// </summary>
        public bool CancelOnException
        {
            get => _cancelOnException;
            set
            {
                if (_schedulerContext != null && _schedulerContext.IsStarted)
                {
                    throw new InvalidOperationException("Can not be set when the queue is running.");
                }
                _cancelOnException = value;
            }
        }

        /// <summary>
        /// Occurs when the queue is empty.
        /// </summary>
        public event EventHandler<QueueEventArgs> Empty;

        /// <summary>
        /// Occurs when the queue is started.
        /// </summary>
        public event EventHandler<QueueEventArgs> Started;

        /// <summary>
        /// Occurs when the queue is stopped.
        /// </summary>
        public event EventHandler<QueueEventArgs> Stopped;

        /// <summary>
        /// Occurs when an exception is thrown in a executed task.
        /// </summary>
        public event EventHandler<TaskExceptionEventArgs> TaskException;

        /// <summary>
        /// Occurs when a task is scheduled.
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskScheduled;

        /// <summary>
        /// Occurs when a task is canceling.
        /// </summary>
        public event EventHandler<TaskCancelingEventArgs> TaskCanceling;

        /// <summary>
        /// Occurs when a task is canceled.
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskCanceled;

        /// <summary>
        /// Occurs when a task is about to be executed.
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskExecuting;

        /// <summary>
        /// Occurs when a task is has been executed.
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskExecuted;

        /// <summary>
        /// Gets a value that indicates whether this queue is started or not.
        /// </summary>
        public bool IsStarted => _schedulerContext.IsStarted;

        /// <summary>
        /// Gets a value that indicates whether this queue has been stopped or not.
        /// </summary>
        public bool IsStopped => _schedulerContext.IsStopped;

        /// <summary>
        /// Gets a value that indicates whether this queue is running or not.
        /// </summary>
        public bool IsRunning => _schedulerContext.IsRunning;

        /// <summary>
        /// Gets the number of tasks currently in the queue.
        /// </summary>
        public int Count => _schedulerContext.Queue.Count;

        /// <summary>
        /// Gets a value that indicates whether this queue is empty of not.
        /// </summary>
        public bool IsEmpty => _schedulerContext.Queue.Count == 0;
 
        #region Internals

        private void EnsureSchedulerContextIsSet()
        {
            if (_schedulerContext == null || _schedulerContext.IsInvalid)
            {
                SetSchedulerContext();
            }
        }

        private void SetSchedulerContext()
        {
            var cts = new CancellationTokenSource();

            ITaskCollection collection = null;

            if (_schedulerContext != null)
            {
                // Restore existing queue context.

                collection = _schedulerContext.Queue;
            }
            else
            {
                collection = Scheduler.CreateCollection();
            }

            var queueData = new SchedulerContextData()
            {
                _queueEmpty = RaiseQueueEmpty,

                _queueStarted = RaiseQueueStarted,
                _queueStopped = RaiseQueueStopped,

                _taskScheduled = RaiseTaskScheduled,
                _taskCanceled = RaiseTaskCanceled,
                _taskCanceling = RaiseTaskCanceling,
                _taskExecuting = RaiseTaskExecuting,
                _taskExecuted = RaiseTaskExecuted,
                _taskException = RaiseTaskException
            };

            _schedulerContext = (ISchedulerContext<TTaskInfo>)Scheduler.GetContext(this, collection, cts, queueData);
        }

        private void RaiseQueueEmpty(QueueEventArgs e) => Empty?.Invoke(this, e);

        private void RaiseQueueStarted(QueueEventArgs e) => Started?.Invoke(this, e);

        private void RaiseQueueStopped(QueueEventArgs e) => Stopped?.Invoke(this, e);

        private void RaiseTaskScheduled(TaskEventArgs e) => TaskScheduled?.Invoke(this, e);

        private void RaiseTaskCanceled(TaskEventArgs e) => TaskCanceled?.Invoke(this, e);

        private void RaiseTaskCanceling(TaskCancelingEventArgs e) => TaskCanceling?.Invoke(this, e);

        private void RaiseTaskExecuting(TaskEventArgs e) => TaskExecuting?.Invoke(this, e);

        private void RaiseTaskExecuted(TaskEventArgs e) => TaskExecuted?.Invoke(this, e);

        private void RaiseTaskException(TaskExceptionEventArgs e) => TaskException?.Invoke(this, e);

        ITaskQueue ITaskQueue.Clear()
        {
            return Clear();
        }

        ITaskQueue ITaskQueue.Deschedule(TaskInfo task)
        {
            return Deschedule((TTaskInfo)task);
        }

        ITaskQueue ITaskQueue.Schedule(TaskInfo task)
        {
            return Schedule((TTaskInfo)task);
        }

        ITaskQueue ITaskQueue.Start()
        {
            return Start();
        }

        ITaskQueue ITaskQueue.Stop()
        {
            return Stop();
        }

        #endregion
    }
}