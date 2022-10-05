using CoolMessages.App.Models;
using CoolMessages.App.Options;
using CoolMessages.App.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoolMessages.App.Consumers
{
    public class ProcessMessageConsumer : BackgroundService
    {
        private readonly RabbitMqConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;
        public ProcessMessageConsumer(IOptions<RabbitMqConfiguration> option, IServiceProvider serviceProvider)
        {
            _configuration = option.Value;
            _serviceProvider = serviceProvider;

      //Definindo uma conexão com um nó do rabbitmq
      var factory = new ConnectionFactory
            {
                HostName = _configuration.Host
            };

      //Abrindo uma conexão com um nó do RabbitMQ
      _connection = factory.CreateConnection();

      //Criação de um canal onde vamos definir uma fila, mensagem e consumir a mensagem.
      _channel = _connection.CreateModel();
            _channel.QueueDeclare(
                        queue: _configuration.Queue, //~~> nome da fila
                        durable: false,//~~> se igual true a fila permanece ativa após o servidor ser reiniciado
                        exclusive: false,//~~> se igual a true ela só pode ser acessada via conexão atual e são excluídas ao fechar a conexão.
                        autoDelete: false,//~~> se igual a true será deletada automaticamente após os consumidores usar a fila.
                        arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
      //Solicita a entrega das mensagens de forma assíncrona e fornece um retorno de chamada.
            var consumer = new EventingBasicConsumer(_channel);

      //Recebe a mensagem da fila
            consumer.Received += (sender, eventArgs) =>
            {
                var contentArray = eventArgs.Body.ToArray(); //~~> Obten um array de bytes.
                var contentString = Encoding.UTF8.GetString(contentArray); //~~> Converte para string.
              var message = JsonConvert.DeserializeObject<MessageInputModel>(contentString); //~~> Deserializa para json.

              NotifyUser(message); //~~> Notifica um usuário ou mostra no console.

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            _channel.BasicConsume(_configuration.Queue, false, consumer);

            return Task.CompletedTask;
        }

        public void NotifyUser(MessageInputModel message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                notificationService.NotifyUser(message.FromId, message.ToId, message.Content); 
            }
        }
    }
}
