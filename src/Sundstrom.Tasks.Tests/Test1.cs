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

            await TaskQueue.Default.Schedule(async (queue, ct) =>
            {
                Console.WriteLine("Item 1");

                await Task.Delay(2000);

            }).Schedule(async (queue, ct) =>
            {
                Console.WriteLine("Item 2");

                await Task.Delay(2000);
            }).Schedule(async (queue, ct) =>
            {
                Console.WriteLine("Item 3");

                await Task.Delay(2000);
            }).AwaitIsEmpty();
        }

        [Fact]
        public async Task Combo()
        {
            Console.WriteLine("COMBO");

            await TaskQueue.Default.Schedule(async (queue, ct) =>
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
            }).AwaitIsEmpty();
        }

        [Fact]
        public async Task Exception()
        {
            // This does not work.

            Console.WriteLine("EXCEPTION");

            TaskQueue.Default.ThrowOnException = true;

            try
            {
                await TaskQueue.Default.Schedule(async (queue, ct) =>
                {
                    Console.WriteLine("Item 1: Async");

                    await Task.Delay(2000);

                }).Schedule((queue, ct) =>
                {
                    Console.WriteLine("Item 2: Sync");
                    throw new Exception();
                }).Schedule(async (queue, ct) =>
                {
                    Console.WriteLine("Item 3: Async");

                    await Task.Delay(2000);
                }).AwaitIsEmpty();
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc);
            }
        }

        [Fact]
        public async Task Exception_EventHandler()
        {
            Console.WriteLine("EXCEPTION");

            TaskQueue.Default.CancelOnException = true;

            TaskQueue.Default.Exception += (sender, args) =>
            {
                Debug.WriteLine(args.Exception);
            };

            await TaskQueue.Default.Schedule(async (queue, ct) =>
            {
                Console.WriteLine("Item 1: Async");

                await Task.Delay(2000);

            }).Schedule((queue, ct) =>
            {
                Console.WriteLine("Item 2: Sync");
                throw new Exception();
            }).Schedule(async (queue, ct) =>
            {
                Console.WriteLine("Item 3: Async");

                await Task.Delay(2000);
            }).AwaitIsEmpty();
        }
    }
}
