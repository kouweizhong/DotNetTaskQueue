using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Sundstrom.Tasks.Tests
{
    public class Test1
    {
        [Fact]
        public void Synchronous()
        {
            Console.WriteLine("SYNCHRONOUS");

            TaskQueue.Default.Schedule((queue, ct) =>
            {
                Console.WriteLine("Item 1");
            }).Schedule((queue, ct) =>
            {
                Console.WriteLine("Item 2");
            }).Schedule((queue, ct) =>
            {
                Console.WriteLine("Item 3");
            });
        }

        [Fact]
        public async Task Asynchronous()
        {
            Console.WriteLine("ASYNCHRONOUS");

            TaskQueue.Default.Schedule(async (queue, ct) =>
            {
                Console.WriteLine("Item 1");

                await Task.Delay(2000);

            }).Schedule(async  (queue, ct) =>
            {
                Console.WriteLine("Item 2");

                await Task.Delay(2000);
            }).Schedule(async (queue, ct) =>
            {
                Console.WriteLine("Item 3");

                await Task.Delay(2000);
            });

            await TaskQueue.Default.AwaitIsEmpty();
        }

        [Fact]
        public async Task Combo()
        {
            Console.WriteLine("COMBO");

            TaskQueue.Default.Schedule(async (queue, ct) =>
            {
                Console.WriteLine("Item 1: Async");

                await Task.Delay(2000);

            }).Schedule((queue, ct) =>
            {
                Console.WriteLine("Item 2: Sync");
            }).Schedule(async (queue, ct) =>
            {
                Console.WriteLine("Item 3: Async");

                await Task.Delay(2000);
            });

            await TaskQueue.Default.AwaitIsEmpty();
        }
    }
}
