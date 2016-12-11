# DotNetTaskQueue
Simple task queue for .NET Standard.

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

## NuGet Package

Feed:

```
https://www.myget.org/F/roberts-core-feed/
```

Install the package:

```
PM> Install-Package Sundstrom.Tasks -Version 1.0.0
```
   

## Build the project
###Prerequisites

Get the latest version of .NET SDK here: https://github.com/dotnet/home

### Steps

To build this solution:

```shell
cd src
dnu restore
dnu build
```

This results in a NuGet package.

The package can be consumed by applications that target the full .NET Framework.

## Test

To run the tests (from the Sundstrom.Tasks.Test project):
    
```shell
dotnet test
```
