using Csissors.Redis;
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

    class Controller
    {
        private readonly ILogger<Controller> _log;

        public Controller(ILogger<Controller> log)
        {
            _log = log;
        }

        [CsissorsTask(Seconds = 2)]
        public static async Task EveryTwoSeconds(ITaskContext context)
        {
            throw new Exception("oops");
        }

        [CsissorsTask(Schedule = "*/5 * * * *")]
        public async Task EveryFiveMinutes(ITaskContext context)
        {
            _log.LogInformation($"Hello, I am {context.Task.Name}");
        }


        [CsissorsTask(Schedule = "@every_minute")]
        public async Task EveryMinute(ITaskContext context)
        {
            _log.LogInformation($"Hello, I am {context.Task.Name}");
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
                        //configure.Format = ConsoleLoggerFormat.Systemd;
                        configure.IncludeScopes = true;
                    }));
                })
                .AddController<Controller>()
                .AddMiddleware<LoggingMiddleware>(services =>
                    new LoggingMiddleware("1", services.GetRequiredService<ILogger<LoggingMiddleware>>()))
                .AddMiddleware<LoggingMiddleware>(services =>
                    new LoggingMiddleware("2", services.GetRequiredService<ILogger<LoggingMiddleware>>()))
                .AddMiddleware<LoggingMiddleware>(services =>
                    new LoggingMiddleware("3", services.GetRequiredService<ILogger<LoggingMiddleware>>()))
                .AddMiddleware<RetryMiddleware>()
                .AddRedisRepository(options =>
                {
                    options.ConfigurationOptions = ConfigurationOptions.Parse("localhost");
                })
                ;

            await using (var context = await csissors.BuildAsync())
            {
                await context.RunAsync(CancellationToken.None);
            }
        }


        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }
    }
}