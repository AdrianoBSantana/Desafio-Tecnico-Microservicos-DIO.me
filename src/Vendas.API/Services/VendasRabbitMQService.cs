using Shared.Models;
using Shared.Services;

namespace Vendas.API.Services
{
    /// <summary>
    /// Serviço de background que escuta a fila do RabbitMQ por notificações de vendas.
    /// Este consumidor atua em paralelo ao do serviço de Estoque, seguindo um padrão Pub/Sub.
    /// </summary>
    public class VendasRabbitMQService : BackgroundService
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly ILogger<VendasRabbitMQService> _logger;

        public VendasRabbitMQService(RabbitMQService rabbitMQService, ILogger<VendasRabbitMQService> logger)
        {
            _rabbitMQService = rabbitMQService;
            _logger = logger;
        }

        /// <summary>
        /// Registra o consumidor na fila 'venda_notifications' quando o serviço inicia.
        /// </summary>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviço de consumidor de vendas (RabbitMQ) iniciando.");
            _rabbitMQService.ConsumeMessage<VendaNotification>("venda_notifications", ProcessVendaNotification);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Processa a notificação de venda recebida.
        /// </summary>
        /// <remarks>
        /// ATENÇÃO: Atualmente, este método serve como um placeholder.
        /// Ele demonstra como o serviço de Vendas também pode reagir a um evento de venda,
        /// mas não executa nenhuma ação crítica.
        /// Em um cenário real, a lógica para enviar e-mails de confirmação,
        /// notificar sistemas de logística, ou atualizar painéis de análise poderia ser adicionada aqui.
        /// </remarks>
        private void ProcessVendaNotification(VendaNotification notification)
        {
            _logger.LogInformation(
                "Notificação de venda para o Pedido ID: {PedidoId} recebida e processada no serviço de Vendas.",
                notification.PedidoId
            );
            
            // Lógica futura pode ser implementada aqui.
            // Exemplo:
            // using (var scope = _scopeFactory.CreateScope())
            // {
            //     var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            //     await emailService.SendOrderConfirmationEmailAsync(notification.PedidoId);
            // }
        }
    }
}