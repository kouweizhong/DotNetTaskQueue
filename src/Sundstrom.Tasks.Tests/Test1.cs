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
            Console.WriteLine("SYNCHRONOUS");

            int taskExecutedCount = 0;

            await TaskQueue.Default.Schedule("Task 1", (context, ct) =>
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

            Assert.Equal(taskExecutedCount, 3);
        }

        [Fact]
        public async Task Asynchronous()
        {
            Console.WriteLine("ASYNCHRONOUS");

            int taskExecutedCount = 0;

            await TaskQueue.Default.Schedule("Task 1", async (context, ct) =>
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

            Assert.Equal(taskExecutedCount, 3);
        }

        [Fact]
        public async Task Combo()
        {
            Console.WriteLine("COMBO");

            int taskExecutedCount = 0;

            await TaskQueue.Default.Schedule("Task 1", async (context, ct) =>
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

            Assert.Equal(taskExecutedCount, 3);
        }

        [Fact]
        public async Task Exception_EventHandler()
        {
            Console.WriteLine("EXCEPTION");

            string tag = null;
            int taskExecutedCount = 0;

            TaskQueue.Default.CancelOnException = true;

            TaskQueue.Default.Exception += (sender, args) =>
            {
                tag = args.Tag;
                Console.WriteLine($"Exception in \"{args.Tag}\":\n\n{args.Exception}");
            };

            await TaskQueue.Default.Schedule("Task 1", async (context, ct) =>
            {
                Console.WriteLine("Item 1: Async");

                await Task.Delay(2000);

                taskExecutedCount++;
            }).Schedule("Task 2", (context, ct) =>
            {
                Console.WriteLine("Item 2: Sync");
                throw new Exception();

                taskExecutedCount++;
            }).Schedule("Task 3", async (context, ct) =>
            {
                Console.WriteLine("Item 3: Async");

                await Task.Delay(2000);

                taskExecutedCount++;
            }).AwaitIsEmpty();

            Assert.Equal(taskExecutedCount, 1);
            Assert.Equal(tag, "Task 2");
        }
    }
}
