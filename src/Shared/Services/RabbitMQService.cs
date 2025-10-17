using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Shared.Services
{
    /// <summary>
    /// Serviço singleton que encapsula a comunicação com o RabbitMQ.
    /// Gerencia uma única conexão e canal para toda a aplicação.
    /// </summary>
    public class RabbitMQService : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQService> _logger;

        public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
        {
            _logger = logger;
            
            try
            {
                // Lê as configurações de conexão do appsettings.json.
                var factory = new ConnectionFactory()
                {
                    HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                    Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                    UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                    Password = configuration["RabbitMQ:Password"] ?? "guest"
                };

                // Cria a conexão e o canal que serão reutilizados durante o ciclo de vida do serviço.
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _logger.LogInformation("Conexão com RabbitMQ estabelecida com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Não foi possível conectar ao RabbitMQ. Verifique as configurações e a disponibilidade do serviço.");
                throw; // Lança a exceção para impedir que a aplicação inicie sem o RabbitMQ.
            }
        }

        /// <summary>
        /// Publica uma mensagem em uma fila específica.
        /// </summary>
        /// <param name="queueName">O nome da fila.</param>
        /// <param name="message">O objeto da mensagem a ser serializado e enviado.</param>
        public void PublishMessage<T>(string queueName, T message)
        {
            // Garante que a fila exista antes de publicar.
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var jsonMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
            
            _logger.LogInformation("Mensagem publicada na fila '{QueueName}': {Message}", queueName, jsonMessage);
        }

        /// <summary>
        /// Registra um consumidor para uma fila que executa uma ação ao receber uma mensagem.
        /// </summary>
        /// <param name="queueName">O nome da fila a ser consumida.</param>
        /// <param name="onMessage">A ação (callback) a ser executada com a mensagem deserializada.</param>
        public void ConsumeMessage<T>(string queueName, Action<T> onMessage)
        {
            // Garante que a fila exista antes de consumir.
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var jsonMessage = Encoding.UTF8.GetString(body);
                
                try
                {
                    var deserializedMessage = JsonSerializer.Deserialize<T>(jsonMessage);
                    if (deserializedMessage != null)
                    {
                        // Executa a ação fornecida com a mensagem.
                        onMessage(deserializedMessage);
                        _logger.LogInformation("Mensagem da fila '{QueueName}' processada com sucesso.", queueName);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Falha ao deserializar a mensagem da fila '{QueueName}'. Conteúdo: {Message}", queueName, jsonMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro inesperado ao processar a mensagem da fila '{QueueName}'. Conteúdo: {Message}", queueName, jsonMessage);
                }
            };

            // Inicia o consumo da fila.
            // ATENÇÃO: autoAck=true significa que a mensagem é removida da fila assim que é entregue.
            // Se o processamento falhar, a mensagem será perdida. Para maior confiabilidade,
            // use autoAck=false e faça o acknowledgement manual (channel.BasicAck).
            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }

        /// <summary>
        /// Fecha e libera os recursos de conexão e canal do RabbitMQ.
        /// </summary>
        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
                _logger.LogInformation("Conexão com RabbitMQ fechada.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fechar a conexão com o RabbitMQ.");
            }
        }
    }
}