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
    public class TaskQueue
    {
        internal SchedulerContext _context;
        private bool _cancelOnException = true;
        private TimeSpan _delay;

        private static volatile TaskQueue _default;
        private static object syncRoot = new Object();

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
        public TaskQueue(Scheduler scheduler)
        {
            Scheduler = scheduler;
        }

        /// <summary>
        /// Initializes a TaskQueue with a tag.
        /// </summary>
        /// <param name="tag"></param>
        public TaskQueue(Scheduler scheduler, string tag)
        : this(scheduler)
        {
            Tag = tag;
        }

        /// <summary>
        /// Initializes a TaskQueue with a tag and some data.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        public TaskQueue(Scheduler scheduler, string tag, object data)
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
        public Scheduler Scheduler { get; }

        /// <summary>
        /// Starts the queue.
        /// </summary>
        /// <returns>The start.</returns>
        public TaskQueue Start()
        {
            EnsureContextIsCreated();

            Scheduler.Start(_context);
            return this;
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <returns>The schedule.</returns>
        /// <param name="task">Task.</param>
        public TaskQueue Schedule(TaskInfo task)
        {
            EnsureContextIsCreated();

            Scheduler.Schedule(_context, task);
            return this;
        }

        /// <summary>
        /// Stops the queue.
        /// </summary>
        public TaskQueue Stop()
        {
            _context.Invalidate();

            Scheduler.Stop(_context);

            return this;
        }

        /// <summary>
        /// Deschedule the task.
        /// <param name="task"></param>
        /// </summary>
        public TaskQueue Deschedule(TaskInfo task)
        {
            Scheduler.Deschedule(_context, task);

            return this;
        }

        /// <summary>
        /// Clear the queue and cancel all unexecuted tasks.
        /// </summary>
        public TaskQueue Clear()
        {
            Scheduler.Clear(_context);

            return this;
        }

        public TimeSpan Delay
        {
            get => _delay;
            set
            {
                if (Scheduler.IsStarted)
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
                if (Scheduler.IsStarted)
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
        public bool IsStarted => Scheduler.IsStarted;

        /// <summary>
        /// Gets a value that indicates whether this queue has been stopped or not.
        /// </summary>
        public bool IsStopped => Scheduler.IsStopped;

        /// <summary>
        /// Gets a value that indicates whether this queue is running or not.
        /// </summary>
        public bool IsRunning => Scheduler.IsRunning;

        /// <summary>
        /// Gets the number of tasks currently in the queue.
        /// </summary>
        public int Count => _context.Queue.Count;

        /// <summary>
        /// Gets a value that indicates whether this queue is empty of not.
        /// </summary>
        public bool IsEmpty => _context.Queue.Count == 0;

        /// <summary>
        /// Gets the default queue.
        /// </summary>
        public static TaskQueue Default
        {
            get
            {
                if (_default == null)
                {
                    lock(syncRoot)
                    {
                        _default = new TaskQueue(string.Empty, null);
                        _queues[string.Empty] = _default;
                    }
                }
                return _default;
            }
        }

        private static Dictionary<string, TaskQueue> _queues = new Dictionary<string, TaskQueue>();

        /// <summary>
        /// Gets the existing queues.
        /// </summary>
        public static IEnumerable<TaskQueue> Queues => _queues.Select(x => x.Value).ToArray();

        /// <summary>
        /// Creates a queue.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TaskQueue Create(string tag, object data = null) => Create(new DefaultScheduler(), tag, data);

        /// <summary>
        /// Creates a queue with a specified Scheduler.
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TaskQueue Create(Scheduler scheduler, string tag, object data = null)
        {
            if (scheduler == null)
                throw new ArgumentNullException(nameof(scheduler));

            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException(nameof(tag), "Null, whitespaces or an empty string is not allowed for tags.");

            try
            {
                var queue = new TaskQueue(scheduler, tag, data);
                _queues[tag] = queue;
                return queue;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Already exists a queue with the specified tag.");
            }
        }

        /// <summary>
        /// Removes the specified queue from the queue collection.
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public static bool Remove(TaskQueue queue) => _queues.Remove(queue.Tag);

        #region Internals

        private void EnsureContextIsCreated()
        {
            if (_context == null || _context.IsInvalid)
            {
                CreateContext();
            }
        }

        private void CreateContext()
        {
            var cts = new CancellationTokenSource();

            Queue<TaskInfo> queue;

            if (_context != null)
            {
                // Restore existing queue context.

                queue = _context.Queue;
            }
            else
            {
                queue = new Queue<TaskInfo>();
            }
            _context = new SchedulerContext(queue, cts)
            {
                Delay = Delay,
                CancelOnException = CancelOnException,

                _queueEmpty = RaiseQueueEmpty,

                _queueStarted = RaiseQueueStarted,
                _queueStopped = RaiseQueueStopped,

                _taskScheduled = RaiseTaskScheduled,
                _taskCanceled = RaiseTaskCanceled,
                _taskCanceling = RaiseTaskCanceling,
                _taskExecuting = RaiseTaskExecuting,
                _taskExecuted = RaiseTaskExecuted,
                _taskException = RaiseTaskException,
            };
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

        #endregion
    }
}