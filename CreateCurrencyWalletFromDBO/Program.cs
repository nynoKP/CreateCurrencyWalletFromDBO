using Camunda.Api.Client;
using Camunda.Worker;
using Camunda.Worker.Client;
using CreateCurrencyWalletFromDBO.Extensions;
using CreateCurrencyWalletFromDBO.Workers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Serilog;
using Serilog.Filters;
using static CreateCurrencyWalletFromDBO.Extensions.QueueSenderService;
using System.Text;

namespace CreateCurrencyWalletFromDBO
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning)
                .CreateLogger();

            Log.Information("\r\n" +
                " ██████ ██████  ███████  █████  ████████ ██ ███    ██  ██████          \r\n" +
                "██      ██   ██ ██      ██   ██    ██    ██ ████   ██ ██               \r\n" +
                "██      ██████  █████   ███████    ██    ██ ██ ██  ██ ██   ███         \r\n" +
                "██      ██   ██ ██      ██   ██    ██    ██ ██  ██ ██ ██    ██         \r\n" +
                " ██████ ██   ██ ███████ ██   ██    ██    ██ ██   ████  ██████          \r\n" +
                "                                                                       \r\n" +
                "                                                                       \r\n" +
                " ██████ ██    ██ ██████  ██████  ███████ ███    ██  ██████ ██    ██    \r\n" +
                "██      ██    ██ ██   ██ ██   ██ ██      ████   ██ ██       ██  ██     \r\n" +
                "██      ██    ██ ██████  ██████  █████   ██ ██  ██ ██        ████      \r\n" +
                "██      ██    ██ ██   ██ ██   ██ ██      ██  ██ ██ ██         ██       \r\n" +
                " ██████  ██████  ██   ██ ██   ██ ███████ ██   ████  ██████    ██       \r\n" +
                "                                                                       \r\n" +
                "                                                                       \r\n" +
                "██     ██  █████  ██      ██      ███████ ████████                     \r\n" +
                "██     ██ ██   ██ ██      ██      ██         ██                        \r\n" +
                "██  █  ██ ███████ ██      ██      █████      ██                        \r\n" +
                "██ ███ ██ ██   ██ ██      ██      ██         ██                        \r\n" +
                " ███ ███  ██   ██ ███████ ███████ ███████    ██                        \r\n" +
                "                                                                       \r\n" +
                "                                                                       \r\n" +
                "███████ ██████   ██████  ███    ███       ██████  ██████   ██████      \r\n" +
                "██      ██   ██ ██    ██ ████  ████       ██   ██ ██   ██ ██    ██     \r\n" +
                "█████   ██████  ██    ██ ██ ████ ██ █████ ██   ██ ██████  ██    ██     \r\n" +
                "██      ██   ██ ██    ██ ██  ██  ██       ██   ██ ██   ██ ██    ██     \r\n" +
                "██      ██   ██  ██████  ██      ██       ██████  ██████   ██████      \r\n");


            

            var builder = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddExternalTaskClient(client =>
                {
                    client.BaseAddress = new Uri(ConfigurationManager.Configuration.GetSection("CamundaConnection")["url"] ?? "");
                });

                services.AddCamundaWorker(Environment.MachineName)
                    .AddHandler<ParseMessageHandler>()
                    .AddHandler<CheckUnquenessHandler>()
                    .AddHandler<AccountGetHandler>()
                    .AddHandler<AccountCreateHandler>()
                    .AddHandler<SendResultHandler>()
                    .ConfigurePipeline(pipeline =>
                    {
                        pipeline.Use(next => async context =>
                        {
                            var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
                            logger.LogInformation("Started processing of task {Id}", context.Task.Id);
                            await next(context);
                            logger.LogInformation("Finished processing of task {Id}", context.Task.Id);
                        });
                    });
                services.AddHostedService<QueueListenerService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.AddSerilog();
            });

            new ProcessDeployer().AutoDeploy();

            await builder.RunConsoleAsync();
        }
    }
}
