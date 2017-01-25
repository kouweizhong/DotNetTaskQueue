using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Sundstrom.Tasks.Tests
{
    public class Test1
    {
        [Fact]
        public async Task Synchronous()
        {
            var queue = TaskQueue.Create("1").Start();

            Console.WriteLine("SYNCHRONOUS");

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
            var queue = TaskQueue.Create("2").Start();

            Console.WriteLine("ASYNCHRONOUS");

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
            var queue = TaskQueue.Create("3").Start();

            Console.WriteLine("COMBO");

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
            Console.WriteLine("EXCEPTION");

            string tag = null;
            int taskExecutedCount = 0;

            var queue = TaskQueue.Create("4");

            queue.CancelOnException = true;

			queue.Start();

            queue.TaskException += (sender, args) =>
            {
                tag = args.Tag;
                Console.WriteLine($"Exception in \"{args.Tag}\":\n\n{args.Exception}");
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
            Console.WriteLine("EXCEPTION");

            string tag = null;
            int taskExecutedCount = 0;

            var queue = TaskQueue.Create("5");

            queue.CancelOnException = true;

			queue.Start();

            queue.TaskException += (sender, args) =>
            {
                tag = args.Tag;
                Console.WriteLine($"Exception in \"{args.Tag}\":\n\n{args.Exception}");

                args.Cancel = false;
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

            Assert.Equal(3, taskExecutedCount);
            Assert.Equal("Task 2", tag);
        }

         [Fact]
        public async Task Run()
        {
			var queue = TaskQueue.Create("6");

            queue.CancelOnException = true;

			queue.Start();

            queue.TaskException += (s, e) => {
                Console.WriteLine(e.Tag);
                e.Cancel = false;
            };

            for(int i = 0; i < 20; i++) 
            {
                int x = i;
                queue.Schedule($"Task: {x}", async (context, ct) =>
                {
                    if(x == 8) 
                    {
                        throw new Exception();
                    }

                    Console.WriteLine($"Task: {x} ({queue.Count} tasks left)");

                    await Task.Delay(2000);
                });
            }

            await queue.AwaitIsEmpty();
        }
    }
}
