using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CreateCurrencyWalletFromDBO.Extensions.QueueSenderService;
using System.Threading.Channels;
using Camunda.Api.Client;

namespace CreateCurrencyWalletFromDBO.Extensions
{
    public class QueueListenerService : BackgroundService
    {
        private readonly ILogger<QueueListenerService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly RabbitMQSettings rabbitSettings;
        private IConnection _connection;
        private IModel _channel;

        public QueueListenerService(ILogger<QueueListenerService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            rabbitSettings = ConfigurationManager.GetSection<RabbitMQSettings>("RabbitMQ");
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = rabbitSettings.HostName,
                Port = rabbitSettings.Port,
                UserName = rabbitSettings.UserName,
                Password = rabbitSettings.Password
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: rabbitSettings.ConsumerQueueName,
                                              durable: false,
                                              exclusive: false,
                                              autoDelete: false,
                                              arguments: null);
            _channel.ExchangeDeclare(exchange: rabbitSettings.ConsumerExchange, type: "direct");
            _channel.QueueBind(queue: rabbitSettings.ConsumerQueueName,
                                          exchange: rabbitSettings.ConsumerExchange,
                                          routingKey: "");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"Received message: {message}");

                using (var scope = _serviceScopeFactory.CreateScope())
                {

                    await StartProcessWithMessageContent(message);
                }
            };

            _channel.BasicConsume(queue: rabbitSettings.ConsumerQueueName,
                                 autoAck: true,
                                 consumer: consumer);

            return Task.CompletedTask;
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
        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
