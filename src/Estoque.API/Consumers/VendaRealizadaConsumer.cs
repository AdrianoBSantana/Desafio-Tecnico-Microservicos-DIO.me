using MassTransit;
using Shared.Events;
using Estoque.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Estoque.API.Consumers;

public class VendaRealizadaConsumer : IConsumer<VendaRealizadaEvent>
{
    private readonly EstoqueDbContext _context;
    private readonly ILogger<VendaRealizadaConsumer> _logger;

    public VendaRealizadaConsumer(EstoqueDbContext context, ILogger<VendaRealizadaConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<VendaRealizadaEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Consumindo VendaRealizadaEvent PedidoId={PedidoId}", message.PedidoId);

        // Idempotency: skip if already processed
        var already = await _context.ProcessedOrders.AnyAsync(p => p.PedidoId == message.PedidoId);
        if (already)
        {
            _logger.LogWarning("Pedido {PedidoId} já processado — pulando.", message.PedidoId);
            return;
        }

        foreach (var item in message.Itens)
        {
            var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == item.ProdutoId);
            if (produto == null)
            {
                _logger.LogWarning("Produto {ProdutoId} não encontrado ao processar evento de venda {PedidoId}", item.ProdutoId, message.PedidoId);
                continue;
            }

            if (produto.Quantidade >= item.Quantidade)
            {
                _logger.LogInformation("Decrementando estoque do Produto {ProdutoId}: -{Quantidade} (antes: {Antes}) for Pedido {PedidoId}", produto.Id, item.Quantidade, produto.Quantidade, message.PedidoId);
                produto.Quantidade -= item.Quantidade;
            }
            else
            {
                _logger.LogWarning("Estoque insuficiente para produto {ProdutoId} ao processar evento de venda {PedidoId}. Disponível: {Disponivel}, necessário: {Necessario}", produto.Id, message.PedidoId, produto.Quantidade, item.Quantidade);
            }
        }

        // Mark as processed
        _context.ProcessedOrders.Add(new Estoque.API.Models.ProcessedOrder
        {
            PedidoId = message.PedidoId,
            ProcessedOn = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        _logger.LogInformation("Pedido {PedidoId} processado e registrado em ProcessedOrders.", message.PedidoId);
    }
}
