using System;
using System.Threading.Tasks;
using Sundstrom.Tasks;

class Program
{
    static void Main(string[] args)
    {
        MainAsync(args).Wait();
    }

    static async Task MainAsync(string[] args) 
    {
        var queue = TaskQueue.Default;

        queue.CancelOnException = true;

        queue.TaskExecuted += (s, e) => {
            Console.WriteLine($"Task {e.Tag} was executed.");
        };

        queue.Empty += (s, e) => {
            Console.WriteLine("The queue is empty.");
        };

        queue.Start();

        int id = 1;

        while(true) 
        {
            Console.Read();

            var id2 = id;

            queue.Schedule($"Task {id2}", async (context, ct) =>
            {
                await Task.Delay(2000);
            });

            Console.WriteLine($"Task {id2} was added to queue.");

            id++;
        }
    } 
}
