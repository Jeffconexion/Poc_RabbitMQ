using CoolMessages.App.Models;
using CoolMessages.App.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace CoolMessages.App.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ConnectionFactory _factory;
        private readonly RabbitMqConfiguration _config;
        public MessagesController(IOptions<RabbitMqConfiguration> options)
        {
            _config = options.Value;
        
            //Definindo uma conexão com um nó do rabbitmq
            _factory = new ConnectionFactory
            {
                HostName = _config.Host
            };
        }

        [HttpPost]
        public IActionResult PostMessage([FromBody] MessageInputModel message)
        {
            //Abrindo uma conexão com um nó do RabbitMQ
            using (var connection = _factory.CreateConnection())
            {
            //Criação de um canal onde vamos definir uma fila, mensagem e publicar a mensagem.
                using (var channel = connection.CreateModel())
                {
                  //Criando fila e configurando
                    channel.QueueDeclare(
                        queue: _config.Queue, //~~> nome da fila
                        durable: false, //~~> se igual true a fila permanece ativa após o servidor ser reiniciado
                        exclusive: false, //~~> se igual a true ela só pode ser acessada via conexão atual e são excluídas ao fechar a conexão.
                        autoDelete: false, //~~> se igual a true será deletada automaticamente após os consumidores usar a fila.
                        arguments: null); 

                  //A mensagem pode ser de qualquer tipo.
                    var stringfiedMessage = JsonConvert.SerializeObject(message);

                  //Independente da mensagem, vai ser convertido para um array de bytes.
                    var bytesMessage = Encoding.UTF8.GetBytes(stringfiedMessage);

                  //publica uma mensagem na fila.
                    channel.BasicPublish(
                        exchange: "",
                        routingKey: _config.Queue, //~~> o nome da fila que eu irei publicar.
                        basicProperties: null,
                        body: bytesMessage);
                }
            }

            return Accepted();
        }
    }
}
