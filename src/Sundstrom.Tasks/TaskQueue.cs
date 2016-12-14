using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sundstrom.Tasks
{
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
        /// Initializes a TaskQueue.
        /// </summary>
        public TaskQueue()
        {

        }

        /// <summary>
        /// Initializes a TaskQueue with a tag.
        /// </summary>
        /// <param name="tag"></param>
        public TaskQueue(string tag)
        {
            Tag = tag;
        }

        /// <summary>
        /// Initializes a TaskQueue with a tag and some data.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        public TaskQueue(string tag, object data)
        {
            Tag = tag;
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

        private void ScheduleInternal(TaskContext context)
        {
            queue.Enqueue(context);
            TaskScheduled?.Invoke(this, new TaskEventArgs(context.Tag));
            Next(cts.Token);
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TaskQueue Schedule(Func<TaskContext, CancellationToken, Task> action)
        {
            var context = new TaskContext(this, action);
            ScheduleInternal(context);
            return this;
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public TaskQueue Schedule(string tag, Func<TaskContext, CancellationToken, Task> action)
        {
            var context = new TaskContext(this, action, tag);
            ScheduleInternal(context);
            return this;
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TaskQueue Schedule(Action<TaskContext, CancellationToken> action)
        {
            var context = new TaskContext(this, async (q, ct) => action(q, ct));
            ScheduleInternal(context);
            return this;
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public TaskQueue Schedule(string tag, Action<TaskContext, CancellationToken> action)
        {
            var context = new TaskContext(this, async (q, ct) => action(q, ct), tag);
            ScheduleInternal(context);
            return this;
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public TaskQueue Schedule(Task task)
        {
            var context = new TaskContext(this, async (q, ct) => await task);
            ScheduleInternal(context);
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
            var context = new TaskContext(this, async (q, ct) => await task);
            ScheduleInternal(context);
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
            
            while (queue.Count > 0)
            {
                var context = queue.Dequeue();
                TaskCanceled?.Invoke(this, new TaskEventArgs(context.Tag));
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether or not the queue should cancel on exception. (Default: true)
        /// </summary>
        public bool CancelOnException { get; set; } = true;

        /// <summary>
        /// Raises when an exception is thrown in a executed task.
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskException;

        /// <summary>
        /// Raises when a task is scheduled.
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskScheduled;

        /// <summary>
        /// Raises when a task is canceled.
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskCanceled;

        /// <summary>
        /// Raises when a task is about to be executed.
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskExecuting;

        /// <summary>
        /// Raises when a task is has been executed.
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskExecuted;

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

                        TaskExecuting?.Invoke(this, new TaskEventArgs(context.Tag));

                        await context.Action(context, cts.Token);
                    }
                    catch (Exception exc)
                    {
                        // Handle any exception thrown inside a task.
                        // Invoke the Exception event handlers.

                        TaskException?.Invoke(this, new TaskEventArgs(context.Tag, exc));

                        if (CancelOnException)
                        {
                            // Cancel the queue.

                            ClearInternal();
                        }
                    }

                    TaskExecuted?.Invoke(this, new TaskEventArgs(context.Tag));

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
        /// Gets the number of tasks currently in the queue.
        /// </summary>
        public int Count
        {
            get
            {
                return queue.Count;
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
        /// <param name="value"></param>
        /// <returns>The current queue.</returns>
        public Task<TaskQueue> AwaitIsEmpty(int checkRate = 200, bool value = true)
        {
            return Task.Run(async () =>
            {
                while (IsEmpty != value)
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
}