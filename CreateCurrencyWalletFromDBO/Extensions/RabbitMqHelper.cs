using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCurrencyWalletFromDBO.Extensions
{
    public class RabbitMqHelper
    {
        public static async Task StartListening(Func<Task> function)
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
                    await function();
                    await Task.Yield();
                };
                channel.BasicConsume(queue: rabbitSettings.ConsumerQueueName,
                                     autoAck: true,
                                     consumer: consumer);

                await Task.Delay(Timeout.Infinite);
            }
        }
        public static void SendMessage(string message)
        {
            var rabbitSettings = ConfigurationManager.GetSection<RabbitMQSettings>("RabbitMQ");
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = rabbitSettings.HostName,
                    UserName = rabbitSettings.UserName,
                    Password = rabbitSettings.Password
                };

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: rabbitSettings.SenderExchange, type: ExchangeType.Direct);

                    channel.QueueDeclare(queue: rabbitSettings.SenderQueueName,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);
                    // Используем пустую строку для routingKey вместо null
                    channel.QueueBind(queue: rabbitSettings.SenderQueueName,
                                      exchange: rabbitSettings.SenderExchange,
                                      routingKey: "");

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: rabbitSettings.SenderExchange,
                                         routingKey: "", // Используем пустую строку для routingKey
                                         basicProperties: null,
                                         body: body);

                    Log.ForContext<RabbitMqHelper>().Information(string.Format(" [{0}] Sent {1}", rabbitSettings.SenderQueueName, message));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while sending the message to rabbit mq queue ( " + rabbitSettings.SenderQueueName + "): " + ex.Message);
            }
        }

        public class RabbitMQSettings
        {
            public string HostName { get; set; }
            public int Port { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string ConsumerQueueName { get; set; }
            public string ConsumerExchange { get; set; }
            public string SenderQueueName { get; set; }
            public string SenderExchange { get; set; }
        }
    }
}
