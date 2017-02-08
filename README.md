# DotNetTaskQueue
Simple task queue for .NET Standard.

## Description

TaskQueue for x-plat .NET that supports queueing and chaining of sequential tasks - both synchronous and asynchronous code.

```csharp
await TaskQueueFactory.Default.Schedule(async (context, ct) =>
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
})
.Start()
.AwaitIsEmpty();
```

Handle exceptions:

```csharp
// Cancels the rest of the tasks in queue on exception (optional)
var queue = TaskQueueFactory.Default;

queue.CancelOnException = true;

queue.TaskException += (sender, args) =>
{
    Console.WriteLine(args.Exception);
};

queue.Start();
```

Also supports:

* Creation of additional queues (with tagging).
* Task cancellation.

See tests for more examples.

## NuGet Package

Feed:

```
https://www.myget.org/F/roberts-core-feed/
```

Install the package:

```
PM> Install-Package Sundstrom.Tasks
```
   

## Build the project
###Prerequisites

Get the latest version of .NET SDK here: https://github.com/dotnet/home

### Steps

To build this solution:

```shell
cd src
dotnet restore
dotnet build
dotnet pack
```

This results in a NuGet package.

The package can be consumed by applications that target the full .NET Framework.

## Test

To run the tests (from the Sundstrom.Tasks.Test project):
    
```shell
dotnet test
```
