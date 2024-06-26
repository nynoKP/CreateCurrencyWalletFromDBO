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
using static CreateCurrencyWalletFromDBO.Extensions.RabbitMqHelper;
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
            })
            .ConfigureLogging(logging =>
            {
                logging.AddSerilog();
            });

            new ProcessDeployer().AutoDeploy();

            try
            {
                var task = Task.Run(async () =>
                {
                    var rabbitSettings = ConfigurationManager.GetSection<RabbitMQSettings>("RabbitMQ");
                    var factory = new ConnectionFactory()
                    {
                        HostName = rabbitSettings.HostName,
                        Port = rabbitSettings.Port,
                        UserName = rabbitSettings.UserName,
                        Password = rabbitSettings.Password
                    };

                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: rabbitSettings.ConsumerQueueName,
                                             durable: false,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);

                        channel.ExchangeDeclare(exchange: rabbitSettings.ConsumerExchange, type: "direct");

                        channel.QueueBind(queue: rabbitSettings.ConsumerQueueName,
                                          exchange: rabbitSettings.ConsumerExchange,
                                          routingKey: "");

                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += async (model, ea) =>
                        {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);
                            await StartProcessWithMessageContent(message);
                            await Task.Yield();
                        };
                        channel.BasicConsume(queue: rabbitSettings.ConsumerQueueName,
                                             autoAck: true,
                                             consumer: consumer);

                        await Task.Delay(Timeout.Infinite);
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error("Cannot start rabbit listener!", ex);
            }

            await builder.RunConsoleAsync();
        }


        private static async Task StartProcessWithMessageContent(string messageContent)
        {
            CamundaClient camunda = CamundaClient.Create(ConfigurationManager.Configuration.GetSection("CamundaConnection")["url"] ?? "");
            await camunda.ProcessDefinitions.ByKey(ConfigurationManager.Configuration.GetSection("CamundaConnection")["ProcessKey"] ?? "").StartProcessInstance(new Camunda.Api.Client.ProcessDefinition.StartProcessInstance
            {
                Variables = new Dictionary<string, VariableValue>
                {
                    ["MessageContent"] = VariableValue.FromObject(messageContent)
                }
            });
        }
    }
}
