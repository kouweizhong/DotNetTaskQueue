using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sundstrom.Tasks
{
    public class TaskContext
    {
        internal Func<TaskQueue, CancellationToken, Task> Action;

        public TaskContext(TaskQueue queue, Func<TaskQueue, CancellationToken, Task> action)
        {
            this.Queue = queue;
            this.Action = action;
        }

        public TaskQueue Queue { get; private set; }

        public event EventHandler Queued;

        public event EventHandler Executing;

        internal void RaiseQueued()
        {
            Queued?.Invoke(this, new EventArgs());
        }

        internal void RaiseExecuting()
        {
            Executing?.Invoke(this, new EventArgs());
        }
    }

    public class TaskQueueExceptionEventArgs : EventArgs
    {
        internal TaskQueueExceptionEventArgs(TaskQueue taskQueue, Exception exception)
        {
            TaskQueue = taskQueue;
            Exception = exception;
        }

        public TaskQueue TaskQueue
        {
            get;
        }

        public Exception Exception
        {
            get;
        }
    }

    public class TaskQueue
    {
        private CancellationTokenSource cts = new CancellationTokenSource();

        private Queue<TaskContext> queue = new Queue<TaskContext>();

        private bool _isBusy = false;
        private bool _isRunning;

        private static TaskQueue _default;

        public TaskQueue(string tag, object data)
        {
            Tag = tag;
            Data = data;
        }

        public string Tag { get; private set; }

        public object Data { get; private set; }

        public TaskQueue Schedule(Func<TaskQueue, CancellationToken, Task> action)
        {
            queue.Enqueue(new TaskContext(this, action));
            Next(cts.Token);
            return this;
        }

        public TaskQueue Schedule(Action<TaskQueue, CancellationToken> action)
        {
            queue.Enqueue(new TaskContext(this, async (q, ct) => action(q, ct)));
            Next(cts.Token);
            return this;
        }

        public TaskQueue Schedule(Task task)
        {
            queue.Enqueue(new TaskContext(this, async (q, ct) => await task));
            Next(cts.Token);
            return this;
        }

        public void Cancel()
        {
            cts.Cancel();
            _isRunning = false;
            _isBusy = false;
        }

        public void Clear()
        {
            if (_isBusy) throw new InvalidOperationException();
            queue.Clear();
        }

        public bool CancelOnException { get; set; } = false;

        public event EventHandler<TaskQueueExceptionEventArgs> Exception;

        private async Task Next(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (!_isBusy)
            {
                if (queue.Count > 0)
                {
                    // Peek the current task.

                    var context = queue.Peek();

                    try
                    {
                        _isRunning = true;
                        _isBusy = true;

                        // Execute the current task.

                        await context.Action(context.Queue, cts.Token);
                        Debug.WriteLine("Task finished");
                    }
                    catch (Exception exc)
                    {
                        // Handling any exception thrown inside a task.
                        // Invoke the Exception event handlers.

                        Exception?.Invoke(this, new TaskQueueExceptionEventArgs(this, exc));

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

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return queue.Count == 0;
            }
        }

        public Task AwaitIsEmpty(int frequency = 200)
        {
            return Task.Run(async () =>
            {
                while (!IsEmpty)
                {
                    await Task.Delay(frequency);
                }
            });
        }

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

        public static IEnumerable<TaskQueue> Queues
        {
            get
            {
                return _queues.Select(x => x.Value);
            }
        }

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

        public static bool Delete(TaskQueue queue)
        {
            return _queues.Remove(queue.Tag);
        }
    }
}