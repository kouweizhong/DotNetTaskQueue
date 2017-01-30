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

        queue.Empty += (s, e) => {
            Console.WriteLine("Empty");
        };

        await queue.Schedule("Task 1", async (context, ct) =>
        {
            Console.WriteLine("Task 1");

            await Task.Delay(2000);

        }).Schedule("Task 2", async (context, ct) =>
        {
            Console.WriteLine("Task 2");

            await Task.Delay(2000);
        }).Schedule("Task 3", async (context, ct) =>
        {
            Console.WriteLine("Task 3");

            await Task.Delay(2000);
        })
        .Start()
        .AwaitIsEmpty();
    } 
}
