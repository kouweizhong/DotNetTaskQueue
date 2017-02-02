using Sundstrom.Tasks;
using System;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        MainAsync(args).Wait();
    }

    static async Task MainAsync(string[] args) 
    {
        var queue = TaskQueueFactory.Default;

        queue.CancelOnException = true;

        queue.TaskScheduled += (s, e) => Console.WriteLine($"Task {e.Task.Tag} was scheduled.");

        queue.TaskCanceled += (s, e) => Console.WriteLine($"Task {e.Task.Tag} was canceled.");

        queue.TaskExecuted += (s, e) => Console.WriteLine($"Task {e.Task.Tag} was executed.");

        queue.Empty += (s, e) => Console.WriteLine("The queue is empty.");

        queue.Start();

        int id = 1;

        while(true) 
        {
            var key = Console.Read();

            var k = key;
            if(k == 'q')
            {
                // Deschedule next
                queue.Deschedule($"Task {id - 1}");
            }

            queue.Schedule($"Task {id++}", async (context, ct) =>
            {
                await Task.Delay(2000);
            });
        }
    } 
}
