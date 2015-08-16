using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sundstrom.Tasks
{
    public class TaskQueueExceptionEventArgs : EventArgs
    {
        internal TaskQueueExceptionEventArgs(TaskQueue taskQueue, Exception exception, string tag)
        {
            TaskQueue = taskQueue;
            Exception = exception;
            Tag = tag;
        }

        /// <summary>
        /// Gets the tag associated with this task. (if any)
        /// </summary>
        public string Tag
        {
            get;
        }

        /// <summary>
        /// Gets the queue in this context.
        /// </summary>
        public TaskQueue TaskQueue
        {
            get;
        }

        /// <summary>
        /// Gets the thrown exception.
        /// </summary>
        public Exception Exception
        {
            get;
        }
    }

    /// <summary>
    /// Simple task queue
    /// </summary>
    public class TaskQueue
    {
        private CancellationTokenSource cts = new CancellationTokenSource();

        private Queue<TaskContext> queue = new Queue<TaskContext>();

        private bool _isBusy = false;
        private bool _isRunning;

        private static TaskQueue _default;

        /// <summary>
        /// Initializes a queue.
        /// </summary>
        public TaskQueue()
        {

        }

        /// <summary>
        /// Initializes a queue with a tag.
        /// </summary>
        /// <param name="tag"></param>
        public TaskQueue(string tag)
        {
            Tag = tag;
        }

        /// <summary>
        /// Initializes a queue with a tag and some data.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        public TaskQueue(string tag, object data)
        {
            Tag = tag;
            Data = data;
        }

        /// <summary>
        /// Gets or sets an identifier that is associated with the queue.
        /// </summary>
        public string Tag { get; private set; }

        /// <summary>
        /// Gets or sets some data that is associated with the queue.
        /// </summary>
        public object Data { get; private set; }

        /// <summary>
        /// Schedules a task based on a given function.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TaskQueue Schedule(Func<TaskContext, CancellationToken, Task> action)
        {
            queue.Enqueue(new TaskContext(this, action));
            Next(cts.Token);
            return this;
        }

        /// <summary>
        /// Schedules a task based on a given function.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public TaskQueue Schedule(string tag, Func<TaskContext, CancellationToken, Task> action)
        {
            queue.Enqueue(new TaskContext(this, action, tag));
            Next(cts.Token);
            return this;
        }

        /// <summary>
        /// Schedules a task based on a given action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TaskQueue Schedule(Action<TaskContext, CancellationToken> action)
        {
            queue.Enqueue(new TaskContext(this, async (q, ct) => action(q, ct)));
            Next(cts.Token);
            return this;
        }

        /// <summary>
        /// Schedules a task with the specified key based on a given action.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public TaskQueue Schedule(string tag, Action<TaskContext, CancellationToken> action)
        {
            queue.Enqueue(new TaskContext(this, async (q, ct) => action(q, ct), tag));
            Next(cts.Token);
            return this;
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public TaskQueue Schedule(Task task)
        {
            queue.Enqueue(new TaskContext(this, async (q, ct) => await task));
            Next(cts.Token);
            return this;
        }


        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public TaskQueue Schedule(string tag, Task task)
        {
            queue.Enqueue(new TaskContext(this, async (q, ct) => await task));
            Next(cts.Token);
            return this;
        }

        /// <summary>
        /// Cancels the queue.
        /// </summary>
        public void Cancel()
        {
            cts.Cancel();
            _isRunning = false;
            _isBusy = false;
        }

        /// <summary>
        /// Clear the queue.
        /// </summary>
        public void Clear()
        {
            if (_isBusy) throw new InvalidOperationException();
            queue.Clear();
        }

        /// <summary>
        /// Gets or sets a value that indicates whether or not the queue should cancel on exception.
        /// </summary>
        public bool CancelOnException { get; set; } = false;

        /// <summary>
        /// Raises when an exception is thrown in a scheduled task.
        /// </summary>
        public event EventHandler<TaskQueueExceptionEventArgs> Exception;

        private async Task Next(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (!_isBusy)
            {
                if (queue.Count > 0)
                {
                    // Optional delay.

                    if (Delay > default(TimeSpan))
                    {
                        await Task.Delay(Delay);
                    }

                    // Peek the current task.

                    var context = queue.Peek();

                    try
                    {
                        _isRunning = true;
                        _isBusy = true;

                        // Execute the current task.

                        await context.Action(context, cts.Token);
                        Debug.WriteLine("Task finished");
                    }
                    catch (Exception exc)
                    {
                        // Handling any exception thrown inside a task.
                        // Invoke the Exception event handlers.

                        Exception?.Invoke(this, new TaskQueueExceptionEventArgs(this, exc, context.Tag));

                        if (CancelOnException)
                        {
                            // Cancel the queue.

                            ClearInternal();
                        }
                    }

                    // Dequeue the currently finished task and request the next.

                    if (queue.Count > 0)
                    {
                        queue.Dequeue();
                    }
                    _isBusy = false;
                    await Next(cts.Token);
                }
            }
            else
            {
                _isRunning = false;
            }
        }

        private void ClearInternal()
        {
            _isBusy = false;
            Clear();
        }

        /// <summary>
        /// Gets a value that indicates whether this queue is running or not.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether this queue is empty of not.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return queue.Count == 0;
            }
        }

        /// <summary>
        /// Gets or sets the the delay between each task.
        /// </summary>
        public TimeSpan Delay { get; set; }

        /// <summary>
        /// Waits for the queue to be empty.
        /// </summary>
        /// <param name="checkRate"></param>
        /// <returns>The current queue.</returns>
        public Task<TaskQueue> AwaitIsEmpty(int checkRate = 200)
        {
            return Task.Run(async () =>
            {
                while (!IsEmpty)
                {
                    await Task.Delay(checkRate);
                }

                return this;
            });
        }

        /// <summary>
        /// Gets the default queue.
        /// </summary>
        public static TaskQueue Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new TaskQueue(string.Empty, null);
                    _queues[string.Empty] = _default;
                }
                return _default;
            }
        }

        private static Dictionary<string, TaskQueue> _queues = new Dictionary<string, TaskQueue>();

        /// <summary>
        /// Gets the existing queues.
        /// </summary>
        public static IEnumerable<TaskQueue> Queues
        {
            get
            {
                return _queues.Select(x => x.Value);
            }
        }

        /// <summary>
        /// Creates a queue.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TaskQueue Create(string tag, object data = null)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException("tag", "Null, whitespaces or an empty string is not allowed for tags.");

            try
            {
                var queue = new TaskQueue(tag, data);
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
        public static bool Remove(TaskQueue queue)
        {
            return _queues.Remove(queue.Tag);
        }
    }

    /// <summary>
    /// Holds the context of a task.
    /// </summary>
    public class TaskContext
    {
        internal Func<TaskContext, CancellationToken, Task> Action;

        public TaskContext(TaskQueue queue, Func<TaskContext, CancellationToken, Task> action)
        {
            this.Queue = queue;
            this.Action = action;
        }

        public TaskContext(TaskQueue queue, Func<TaskContext, CancellationToken, Task> action, string taskTag)
        {
            this.Queue = queue;
            this.Action = action;
            this.Tag = taskTag;
        }

        /// <summary>
        /// Gets the queue.
        /// </summary>
        public TaskQueue Queue { get; }

        /// <summary>
        /// Gets the tag. (if any)
        /// </summary>
        public string Tag { get; }
    }
}