using Csissors.Attributes;
using Csissors.Middleware;
using Csissors.Postgres;
using Csissors.Schedule;
using Csissors.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Demo
{
    class LoggingMiddleware : IMiddleware
    {
        private readonly string id;
        private readonly ILogger<LoggingMiddleware> _log;

        public LoggingMiddleware(string id, ILogger<LoggingMiddleware> log)
        {
            this.id = id;
            _log = log;
        }

        public async Task ExecuteAsync(ITaskContext context, Func<Task> next)
        {
            using (_log.BeginScope(context.Task.Name))
            {
                _log.LogInformation($"{id} Start");
                try
                {
                    await next();
                }
                catch (Exception e)
                {
                    _log.LogError(e, $"{id} Something went wrong");
                }
                finally
                {
                    _log.LogInformation($"{id} Done");
                }
            }
        }
    }

    class RetryMiddleware : IMiddleware
    {
        public async Task ExecuteAsync(ITaskContext context, Func<Task> next)
        {
            for (int i = 0; i < 3; ++i)
            {
                await next();
            }
        }
    }

    [CsissorsTaskContainer]
    class TaskContainer
    {
        private readonly ILogger<TaskContainer> _log;

        public TaskContainer(ILogger<TaskContainer> log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        [CsissorsTask(Seconds = 3)]
        public static async Task LongRunningTask(ITaskContext taskContext)
        {
            await Task.Delay(10000);
        }

        [CsissorsTask(Seconds = 2)]
        public static async Task EveryTwoSeconds(ILogger<Program> anotherLog)
        {
            anotherLog.LogInformation("MUAHAHAAHA");
            throw new Exception("oops");
        }
        /*
                [CsissorsTask(Seconds = 5)]
                public async Task EveryFiveSeconds(ITaskContext context, [FromTaskData("hello")] string hello)
                {
                    _log.LogInformation($"Hello, I am {context.Task.Name}");
                }

                [CsissorsTask(Schedule = "* * * * *")]
                public async Task EveryMinute(ITaskContext context)
                {
                    _log.LogInformation($"Hello, I am {context.Task.Name}");
                }
        */

        [CsissorsDynamicTask]
        public async Task EveryMinuteDynamic(ITaskContext context, [FromTaskData] string username, [FromTaskData(Optional = true)] string nonExistent)
        {
            _log.LogInformation($"Hello, I am {username}, {nonExistent}");
            if (username == "liza")
            {
                await context.AppContext.UnscheduleTask(context.Task.ParentTask, context.Task.Name, CancellationToken.None);
            }
        }
    }

    class Program
    {
        static async Task MainAsync()
        {
            var conf = new TaskConfiguration(
                        new IntervalSchedule(TimeSpan.FromSeconds(1), false),
                        FailureMode.None,
                        ExecutionMode.AtLeastOnce,
                        TimeSpan.FromMinutes(1),
                        new Dictionary<string, object?> { { "username", "tibor" } }
                );

            var csissors = new CsissorsBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging(configure => configure.AddConsole(configure =>
                    {
                        configure.Format = ConsoleLoggerFormat.Default;
                        configure.IncludeScopes = true;
                    }));
                })
                //.AddTaskContainer<TaskContainer>()
                .AddAssembly()
                .AddDynamicTask("yupee", async (ITaskContext context, ILoggerFactory logger) =>
                {
                    logger.CreateLogger("yuhuhu").LogInformation("Hello", context.Task.Configuration.Data);
                    await Task.Yield();
                })
                .AddTask("whipee", conf, async (ITaskContext context, ILoggerFactory logger) =>
                {
                    logger.CreateLogger("yuhuhu").LogInformation("Hello1", context.Task.Configuration.Data);
                    await Task.Yield();
                })
                //.AddMiddleware<RetryMiddleware>()
                .AddPostgresRepository(options =>
                {
                    options.ConnectionString = "Host=localhost;Username=postgres;Password=postgres;Database=postgres";
                })
                /*.AddRedisRepository(options =>
                {
                    options.ConfigurationOptions = ConfigurationOptions.Parse("localhost");
                })*/
                ;

            await using (var context = await csissors.BuildAsync())
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(5000);
                var task = context.Tasks.DynamicTasks[0];

                await context.ScheduleTask(task, "hello1", new TaskConfiguration(
                        new IntervalSchedule(TimeSpan.FromSeconds(2), false),
                        FailureMode.None,
                        ExecutionMode.AtLeastOnce,
                        TimeSpan.FromMinutes(1),
                        new Dictionary<string, object?> { { "username", "tibor" } }
                ), cts.Token);
                await context.ScheduleTask(task, "hello2", new TaskConfiguration(
                        new IntervalSchedule(TimeSpan.FromSeconds(4), false),
                        FailureMode.None,
                        ExecutionMode.AtLeastOnce,
                        TimeSpan.FromMinutes(1),
                        new Dictionary<string, object?> { { "username", "liza" }, { "nonExistent", "liza" } }
                ), cts.Token);

                await context.RunAsync(cts.Token);
            }
        }


        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }
    }
}