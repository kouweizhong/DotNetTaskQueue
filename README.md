# DotNetTaskQueue
Simple task queue for .NET (DNX)

## Description

TaskQueue for x-plat .NET that supports queueing and chaining of sequential tasks - both synchronous and asynchronous code.

```csharp
await TaskQueue.Default.Schedule(async (context, ct) =>
{
    Console.WriteLine("Item 1: Async");

    await Task.Delay(2000);

}).Schedule((context, ct) =>
{
    Console.WriteLine("Item 2: Sync");
}).Schedule(async (context, ct) =>
{
    Console.WriteLine("Item 3: Async");

    await Task.Delay(2000);
}).AwaitIsEmpty();
```

Handle exceptions:

```csharp
// Cancels the rest of the tasks in queue on exception (optional)
TaskQueue.Default.CancelOnException = true;

TaskQueue.Default.TaskException += (sender, args) =>
{
    Console.WriteLine(args.Exception);
};
```

Also supports:

* Creation of additional queues (with tagging).
* Task cancellation.

See tests for more examples.

## Prerequisites

* DNX (.NET Execution Environment) - currently in Beta

Get the latest version of DNX here: https://github.com/aspnet/home

## Build

To build this solution:

```shell
cd src
dnu restore
dnu build
```

## Test

To run the tests:
    
```shell
cd Sundstrom.Tasks.Tests
dnx . test
```

In the next-coming versions (Beta 7 +) you should be able to just run:

```shell
dnx test
```