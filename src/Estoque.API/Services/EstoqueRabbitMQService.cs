using Shared.Models;
using Shared.Services;
using Estoque.API.Data;

namespace Estoque.API.Services
{
    /// <summary>
    /// Serviço de background que escuta a fila do RabbitMQ por notificações de vendas
    /// para realizar a baixa no estoque dos produtos vendidos.
    /// </summary>
    public class EstoqueRabbitMQService : BackgroundService
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EstoqueRabbitMQService> _logger;

        public EstoqueRabbitMQService(RabbitMQService rabbitMQService, IServiceScopeFactory scopeFactory, ILogger<EstoqueRabbitMQService> logger)
        {
            _rabbitMQService = rabbitMQService;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// Método principal do serviço de background. Registra o consumidor da fila.
        /// </summary>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviço de consumidor de estoque (RabbitMQ) iniciando.");
            
            // Registra o método que processará as mensagens da fila 'venda_notifications'.
            _rabbitMQService.ConsumeMessage<VendaNotification>("venda_notifications", ProcessVendaNotification);
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Processa a notificação de venda recebida. Este é o método de callback.
        /// ATENÇÃO: Este método é 'async void' por ser um manipulador de eventos.
        /// A lógica aqui deve ser robusta e lidar com suas próprias exceções.
        /// </summary>
        private async void ProcessVendaNotification(VendaNotification notification)
        {
            _logger.LogInformation("Processando notificação de venda para o Pedido ID: {PedidoId}", notification.PedidoId);
            
            // Cria um novo escopo de injeção de dependência para usar o DbContext,
            // pois este método é executado em uma thread separada e o DbContext é Scoped.
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<EstoqueDbContext>();

                foreach (var item in notification.Itens)
                {
                    var produto = await context.Produtos.FindAsync(item.ProdutoId);
                    if (produto != null)
                    {
                        // Verifica novamente se há estoque suficiente antes de dar baixa.
                        if (produto.Quantidade >= item.Quantidade)
                        {
                            produto.Quantidade -= item.Quantidade;
                            await context.SaveChangesAsync();
                            _logger.LogInformation("Estoque do produto {ProdutoId} atualizado. Nova quantidade: {NovaQuantidade}", item.ProdutoId, produto.Quantidade);
                        }
                        else
                        {
                            // Este cenário indica uma possível inconsistência, já que a venda foi aprovada.
                            _logger.LogWarning("Tentativa de baixa de estoque para o produto {ProdutoId} falhou. Estoque insuficiente. Disponível: {Disponivel}, Solicitado: {Solicitado}", item.ProdutoId, produto.Quantidade, item.Quantidade);
                        }
                    }
                    else
                    {
                        _logger.LogError("Produto {ProdutoId} da notificação de venda não foi encontrado no banco de dados de estoque.", item.ProdutoId);
                    }
                }
            }
        }
    }
}