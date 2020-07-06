using Csissors.Middleware;
using Csissors.Redis;
using Csissors.Schedule;
using Csissors.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using StackExchange.Redis;
using System;
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

    class TaskContainer
    {
        private readonly ILogger<TaskContainer> _log;

        public TaskContainer(ILogger<TaskContainer> log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }
        /*
                [CsissorsTask(Seconds = 2)]
                public static async Task EveryTwoSeconds(ILogger<Program> anotherLog)
                {
                    anotherLog.LogInformation("MUAHAHAAHA");
                    throw new Exception("oops");
                }

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
        [CsissorsTask(Name = "foobar", Schedule = "* * * * *", Dynamic = true)]
        public async Task EveryMinuteDynamic(ITaskContext context)
        {
            _log.LogInformation($"Hello, I am {context.Task.Name}");
            if (context.Task.Name == "foobar:hello2")
            {
                await context.AppContext.UnscheduleTask(((IDynamicTask)context.Task).ParentTask, "foobar:hello2", CancellationToken.None);
            }
        }
    }

    class Program
    {
        static async Task MainAsync()
        {
            var csissors = new CsissorsBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging(configure => configure.AddConsole(configure =>
                    {
                        configure.Format = ConsoleLoggerFormat.Systemd;
                        configure.IncludeScopes = true;
                    }));
                })
                .AddTaskContainer<TaskContainer>()
                .AddMiddleware(services =>
                    new LoggingMiddleware("1", services.GetRequiredService<ILogger<LoggingMiddleware>>()))
                .AddMiddleware<RetryMiddleware>()
                .AddMiddleware(async (context, next) =>
                {
                    await next();
                })
                .AddInMemoryRepository()
                /*.AddRedisRepository(options =>
                {
                    options.ConfigurationOptions = ConfigurationOptions.Parse("localhost");
                })*/
                ;

            await using (var context = await csissors.BuildAsync())
            {
                var cts = new CancellationTokenSource();
                //cts.CancelAfter(5000);
                var task = context.GetTask("foobar");
                await context.ScheduleTask(task, "hello1", new TaskConfiguration(
                        new IntervalSchedule(TimeSpan.FromSeconds(2), false),
                        FailureMode.None,
                        ExecutionMode.AtLeastOnce,
                        false
                ), cts.Token);
                await context.ScheduleTask(task, "hello2", new TaskConfiguration(
                        new IntervalSchedule(TimeSpan.FromSeconds(4), false),
                        FailureMode.None,
                        ExecutionMode.AtLeastOnce,
                        false
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