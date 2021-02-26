using Csissors.Attributes;
using Csissors.Middleware;
using Csissors.Postgres;
using Csissors.Schedule;
using Csissors.Tasks;
using Csissors.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Benchmark
{
    [CsissorsTaskContainer]
    class TaskContainer
    {
        public Instant _lastSample = Instant.Now;
        private long _samples = 0;

        [CsissorsDynamicTask]
        public async Task Benchmark(ITaskContext context)
        {
            Interlocked.Increment(ref _samples);
        }

        [CsissorsTask(Seconds=5)]
        public async Task ReportMetrics()
        {
            Interlocked.MemoryBarrier();
            var oldTime = _lastSample;
            _lastSample = Instant.Now;

            var difference = Interlocked.Exchange(ref _samples, 0);
            Console.WriteLine($"{difference} samples ({(double)difference / (_lastSample - oldTime).TotalSeconds}/s)");
        }
    }

    class Program
    {
        static async Task MainAsync(string[] args)
        {
            var csissors = new CsissorsBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging(configure => configure.AddConsole(configure =>
                    {
                        configure.Format = ConsoleLoggerFormat.Default;
                        configure.IncludeScopes = true;
                    }));
                })
                .AddTaskContainer<TaskContainer>()
                .AddPostgresRepository(options =>
                {
                    options.ConnectionString = "Host=localhost;Username=postgres;Password=postgres;Database=postgres;Enlist=false";
                    options.TableName = "pyncette" + args[1];
                })
                ;//.AddInMemoryRepository();
            var rand = new Random();
            await using (var context = await csissors.BuildAsync(CancellationToken.None))
            {
                switch (args[0])
                {
                    case "populate":
                        var taskInstance = context.Tasks.DynamicTasks[0];
                        for (int i = 0; i < 100000; ++i)
                        {
                            await context.ScheduleTask(taskInstance,
                                Guid.NewGuid().ToString(),
                                TaskConfiguration.Default
                                    .WithSchedule(new IntervalSchedule(TimeSpan.FromSeconds(rand.Next(60, 3600)), false))
                                    .WithExecutionMode(ExecutionMode.AtMostOnce),
                                CancellationToken.None
                            );
                        }
                        Console.WriteLine("Scheduled 1000 tasks");
                        break;//goto case "run";
                    case "run":
                        await context.RunAsync(CancellationToken.None);
                        break;
                }
            }
        }


        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }
    }
}