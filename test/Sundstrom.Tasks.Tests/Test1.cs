using Sundstrom.Tasks.Scheduling;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Sundstrom.Tasks.Tests
{
    public class Test1
    {
        [Fact]
        public async Task Synchronous()
        {
            Console.WriteLine(nameof(Synchronous));

            var queue = TaskQueueFactory.Create<TaskInfo>("1").Start();

            int taskExecutedCount = 0;

            await queue.Schedule("Task 1", (context, ct) =>
            {
                Console.WriteLine("Item 1");

                taskExecutedCount++;
            }).Schedule("Task 2", (context, ct) =>
            {
                Console.WriteLine("Item 2");

                taskExecutedCount++;
            }).Schedule("Task 3", (context, ct) =>
            {
                Console.WriteLine("Item 3");

                taskExecutedCount++;
            }).AwaitIsEmpty();

            Assert.Equal(3, taskExecutedCount);
        }

        [Fact]
        public async Task Asynchronous()
        {
            Console.WriteLine(nameof(Asynchronous));

            var queue = TaskQueueFactory.Create<TaskInfo>("2").Start();

            int taskExecutedCount = 0;

            await queue.Schedule("Task 1", async (context, ct) =>
            {
                Console.WriteLine("Item 1");

                await Task.Delay(2000);

                taskExecutedCount++;
            }).Schedule("Task 2", async (context, ct) =>
            {
                Console.WriteLine("Item 2");

                await Task.Delay(2000);

                taskExecutedCount++;
            }).Schedule("Task 3", async (context, ct) =>
            {
                Console.WriteLine("Item 3");

                await Task.Delay(2000);

                taskExecutedCount++;
            }).AwaitIsEmpty();

            Assert.Equal(3, taskExecutedCount);
        }

        [Fact]
        public async Task Combo()
        {
            Console.WriteLine(nameof(Combo));

            var queue = TaskQueueFactory.Create<TaskInfo>("3").Start();

            int taskExecutedCount = 0;

            await queue.Schedule("Task 1", async (context, ct) =>
            {
                Console.WriteLine("Item 1: Async");

                await Task.Delay(2000);

                taskExecutedCount++;
            }).Schedule("Task 2", (context, ct) =>
            {
                Console.WriteLine("Item 2: Sync");

                taskExecutedCount++;
            }).Schedule("Task 3", async (context, ct) =>
            {
                Console.WriteLine("Item 3: Async");

                await Task.Delay(2000);

                taskExecutedCount++;
            }).AwaitIsEmpty();

            Assert.Equal(3, taskExecutedCount);
        }

        [Fact]
        public async Task Exception_EventHandler()
        {
            Console.WriteLine(nameof(Exception_EventHandler));

            string tag = null;
            int taskExecutedCount = 0;

            var queue = TaskQueueFactory.Create<TaskInfo>("4");

            queue.CancelOnException = true;

            queue.Start();

            queue.TaskException += (sender, args) =>
            {
                tag = args.Task.Tag;
                Console.WriteLine($"Exception in \"{args.Task.Tag}\":\n\n{args.Exception}");
            };

            await queue.Schedule("Task 1", async (context, ct) =>
            {
                Console.WriteLine("Item 1: Async");

                await Task.Delay(2000);

                taskExecutedCount++;
            }).Schedule("Task 2", (context, ct) =>
            {
                Console.WriteLine("Item 2: Sync");

                taskExecutedCount++;

                throw new Exception();
            }).Schedule("Task 3", async (context, ct) =>
            {
                Console.WriteLine("Item 3: Async");

                await Task.Delay(2000);

                taskExecutedCount++;
            }).AwaitIsEmpty();

            Assert.Equal(2, taskExecutedCount);
            Assert.Equal("Task 2", tag);
        }

        [Fact]
        public async Task Exception_EventHandler_CancelOverride()
        {
            Console.WriteLine(nameof(Exception_EventHandler_CancelOverride));

            string tag = null;
            int taskExecutedCount = 0;

            var queue = TaskQueueFactory.Create<TaskInfo>("5");

            queue.CancelOnException = true;

            queue.Start();

            queue.TaskException += (sender, args) =>
            {
                tag = args.Task.Tag;
                Console.WriteLine($"Exception in \"{args.Task.Tag}\":\n\n{args.Exception}");

                args.Cancel = false;
            };

            queue.Schedule("Task 1", async (context, ct) =>
            {
                Console.WriteLine("Item 1: Async");

                await Task.Delay(2000);

                taskExecutedCount++;
            }).Schedule("Task 2", (context, ct) =>
            {
                Console.WriteLine("Item 2: Sync");

                taskExecutedCount++;

                throw new Exception();
            }).Schedule("Task 3", async (context, ct) =>
            {
                Console.WriteLine("Item 3: Async");

                await Task.Delay(2000);

                taskExecutedCount++;
            });

            await queue.AwaitIsEmpty();

            Assert.Equal(3, taskExecutedCount);
            Assert.Equal("Task 2", tag);
        }

        [Fact]
        public async Task Run()
        {
            Console.WriteLine(nameof(Run));

            var queue = TaskQueueFactory.Create<TaskInfo>("6");

            queue.CancelOnException = true;

            queue.Start();

            queue.TaskException += (s, e) =>
            {
                Console.WriteLine(e.Task.Tag);
                e.Cancel = false;
            };

            for (int i = 1; i <= 5; i++)
            {
                int x = i;
                queue.Schedule($"Task: {x}", async (context, ct) =>
                {
                    if (x == 2)
                    {
                        throw new Exception();
                    }

                    Console.WriteLine($"Task: {x} ({queue.Count} tasks left)");

                    var task = queue.GetCurrentTask();

                    Console.WriteLine($"Task!!! {task.Tag}");

                    await Task.Delay(2000);
                });
            }

            await queue.AwaitIsEmpty();
        }

        [Fact]
        public async Task Run2()
        {
            Console.WriteLine(nameof(Run2));

            var queue = TaskQueueFactory.Create<TaskInfo>("7");

            queue.CancelOnException = true;

            await queue
            .Schedule((context, ct) => Console.WriteLine("Hello World!"))
            .Start()
            .AwaitIsEmpty();
        }

        [Fact]
        public async Task Run3()
        {
            Console.WriteLine(nameof(Run3));

            var queue = TaskQueueFactory.Create<TaskInfo>("8");

            queue.CancelOnException = true;

            queue.Started += (s, e) => Console.WriteLine("Started");

            queue.Stopped += (s, e) => Console.WriteLine("Stopped");

            await queue.Schedule((context, ct) => Console.WriteLine("Task 1")).Schedule((context, ct) =>
            {
                queue.Stop();

                Console.WriteLine("Task 2");
            })
            .Schedule((context, ct) => Console.WriteLine("Task 3"))
            .Start()
            .AwaitIsStopped();
        }

        [Fact]
        public async Task Run4()
        {
            Console.WriteLine(nameof(Run4));

            var queue = TaskQueueFactory.Default;

            queue.CancelOnException = true;

            queue.TaskException += (s, e) => Console.WriteLine(e.Exception);
            queue.Empty += (s, e) => Console.WriteLine("Queue is empty.");

            await queue
            .Schedule("Task 1", (t, ct) => Console.WriteLine(t.Tag))
            .Schedule("Task 2", (t, ct) => Console.WriteLine(t.Tag))
            .Schedule("Task 3", (t, ct) => Console.WriteLine(t.Tag))
            .Start()
            .AwaitIsEmpty();
        }

        [Fact]
        public async Task Run5()
        {
            Console.WriteLine(nameof(Run5));

            ITaskQueue queue = new TaskQueue<TaskInfo>();

            queue.CancelOnException = true;

            queue.TaskException += (s, e) => Console.WriteLine(e.Exception);
            queue.Empty += (s, e) => Console.WriteLine("Queue is empty.");

            await queue
            .Schedule(new TaskInfo(async (t, ct) => Console.WriteLine(t.Tag)))
            .Start()
            .AwaitIsEmpty();
        }
    }
}
